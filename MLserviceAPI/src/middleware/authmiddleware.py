# -*- coding: utf-8 -*-
import config
import json
import http.client
import ssl

# TODO: We need to make the bad requests actually return a body. WSGI is a pain...
class AuthMiddleware(object):
    def __init__(self, app):
        self.app = app
        
    def __call__(self, environ, start_response):
        # The health route is the only route which does not need authentication
        if environ['PATH_INFO'].startswith('/api/health'):
            return self.app(environ, start_response)
        
        # TODO: mabye make this configurable? Or just rely on the requested content type.
        content_type = 'application/json'
        
        # If there was no token at all then this is a bad request
        if not 'HTTP_AUTHORIZATION' in environ:
            body = json.dumps({"message":"Bad Request. No token provided."})
            
            response = self.app.make_response(body)
            response.content_type = content_type
            response.status_code = 400
            
            return response(environ, start_response)
            
        # Validate the token with the security service
        url = config.MLWSECURITYSERVICE_URL

        # TODO: context should not allow self signed certs, this is only for dev
        try:
            con = http.client.HTTPSConnection(url, timeout=10, context = ssl._create_unverified_context())
            requestHeaders = {'Content-type': 'application/json'}
            requestPayload = environ['HTTP_AUTHORIZATION']
            json_data = json.dumps({"token":requestPayload})
            con.request('POST', '/api/auth/validateToken', json_data, requestHeaders)
            response = con.getresponse()
            result = json.loads(response.read().decode())
            con.close()
        except Exception as e:
            # TODO: Add proper logging
            print(e)

            body = json.dumps({"message":"Something went wrong. Please try again later."})
            
            response = self.app.make_response(body)
            response.content_type = content_type
            response.status_code = 500

            return response(environ, start_response)
        
        if (response.status != 200 or result['result'] == False):
            body = json.dumps({"message":"Access denied. Invalid token."})
            
            response = self.app.make_response(body)
            response.content_type = content_type
            response.status_code = 401

            return response(environ, start_response)
        
        # Token is valid
        return self.app(environ, start_response)