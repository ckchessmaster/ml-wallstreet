# -*- coding: utf-8 -*-

# Set the working directory before we do anything else
import os
os.chdir(os.path.dirname(__file__) + '/..')

# Imports
import services.logger as logger
from flask import Flask
from routes.health import health_api
from routes.sentiment import sentiment_api

import middleware.authmiddleware as middleware

# Configure the API
app = Flask(__name__)
app.config['MAX_CONTENT_LENGTH'] = 100 * 1024 * 1024 # 100MB

app.wsgi_app = middleware.AuthMiddleware(app, app.wsgi_app)

app.register_blueprint(health_api, url_prefix='/api/health')
app.register_blueprint(sentiment_api, url_prefix='/api/sentiment')

# Run the app
if __name__ == '__main__':
    app.run()
