# -*- coding: utf-8 -*-

# Set the working directory before we do anything else
import os
os.chdir(os.path.dirname(__file__) + '/..')

# Imports
import services.logger as logger
from flask import Flask
from flask_cors import CORS
from routes.health_route import health_api
from routes.sentiment_route import sentiment_api
from routes.model_route import model_api
import config

import middleware.auth_middleware as middleware

# Configure the API
app = Flask(__name__)
app.config['MAX_CONTENT_LENGTH'] = 100 * 1024 * 1024 # 100MB

app.wsgi_app = middleware.AuthMiddleware(app, app.wsgi_app)

app.register_blueprint(health_api, url_prefix='/api/health')
app.register_blueprint(sentiment_api, url_prefix='/api/sentiment')
app.register_blueprint(model_api, url_prefix='/api/models')

CORS(app, origins=config.ALLOWED_ORIGINS, supports_credentials=True)

# Run the app
if __name__ == '__main__':
    app.run()
