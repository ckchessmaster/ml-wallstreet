# -*- coding: utf-8 -*-
import appconfig as config
import json
import http.client
import ssl

class AuthMiddleware(object):
    def __init__(self, app):
        self.app = app
        
    def __call__(self, environ, start_response):
        # The health route is the only route which does not need authentication
        if environ['PATH_INFO'].startswith('/api/health'):
            return self.app(environ, start_response)
        
        responseHeaders = [('Content-type', 'application/json')]
        body = '{}'
        
        # If there was no token at all then this is a bad request
        if not 'HTTP_AUTHORIZATION' in environ:
            status = '400 Bad Request'
            start_response(status, responseHeaders)
            return [body]
            
        # Validate the token with the security service
        url = config.MLWSECURITYSERVICE_URL
        # TODO: context should not allow self signed certs, this is only for dev
        con = http.client.HTTPSConnection(url, timeout=10, context = ssl._create_unverified_context())
        requestHeaders = {'Content-type': 'application/json'}
        requestPayload = environ['HTTP_AUTHORIZATION']
        json_data = json.dumps(requestPayload)
        con.request('POST', '/api/auth/validateToken', json_data, requestHeaders)
        response = con.getresponse()
        result = json.loads(response.read().decode())
        con.close()
        
        if (response.status != 200 or result['result'] == False):
            status = '401 Unauthorized'
            start_response(status, responseHeaders)
            return [body]
        
        return self.app(environ, start_response)