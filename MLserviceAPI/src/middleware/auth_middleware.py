# -*- coding: utf-8 -*-
import config
import json
import http.client
import ssl
import services.logger as logger
import sys

class AuthMiddleware(object):
    def __init__(self, flask, app):
        self.flask = flask
        self.app = app
        
    def getInvalidLoginAttemptMessage(self, environ):
        request_method = environ['REQUEST_METHOD']
        request_uri = environ['REQUEST_URI']
        remote_addr = environ['REMOTE_ADDR']

        return (f'Invalid access attempt! REQUEST_METHOD: {request_method}, REQUEST_URI: {request_uri}, REMOTE_ADDR: {remote_addr}')

    def __call__(self, environ, start_response):
        # If we are in a dev environment don't worry about security
        # If this is a preflight CORS request then go ahead and let it through as well
        if config.ENVIRONMNET == 'DEV' or environ['REQUEST_METHOD'] == 'OPTIONS':
            return self.app(environ, start_response)

        # The health route is the only route which does not need authentication
        if environ['PATH_INFO'].startswith('/api/health'):
            return self.flask(environ, start_response)
        
        # TODO: mabye make this configurable? Or just rely on the requested content type.
        content_type = 'application/json'
        
        # If there was no token at all then this is a bad request
        if not 'HTTP_AUTHORIZATION' in environ:
            logger.log_event(self.getInvalidLoginAttemptMessage(environ))
            body = json.dumps({"message":"Bad Request. No token provided."})
            
            response = self.flask.make_response(body)
            response.content_type = content_type
            response.status_code = 400
            
            return response(environ, start_response)
            
        # Validate the token with the security service
        url = config.MLWSECURITYSERVICE_URL
        port = config.MLWSECURITYSERVICE_PORT

        # TODO: context should not allow self signed certs, this is only for dev
        try:
            con = http.client.HTTPSConnection(url, port=port, timeout=30, context=ssl._create_unverified_context())
            requestHeaders = {'Content-type': 'application/json'}
            requestPayload = environ['HTTP_AUTHORIZATION']
            json_data = json.dumps({"token":requestPayload})
            con.request('POST', '/api/auth/validateToken', json_data, requestHeaders)
            response = con.getresponse()
            result = json.loads(response.read().decode())
            con.close()
        except Exception as e:
            logger.log_error('Error while trying to validate token: ' + str(e.args))

            body = json.dumps({"message":"Something went wrong. Please try again later."})
            
            response = self.flask.make_response(body)
            response.content_type = content_type
            response.status_code = 500

            return response(environ, start_response)
        
        if (response.status != 200 or result['result'] == False):
            logger.log_event(self.getInvalidLoginAttemptMessage(environ))
            body = json.dumps({"message":"Access denied. Invalid token."})
            
            response = self.flask.make_response(body)
            response.content_type = content_type
            response.status_code = 401

            return response(environ, start_response)
        
        # Token is valid
        return self.app(environ, start_response)