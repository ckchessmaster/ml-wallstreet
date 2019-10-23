# -*- coding: utf-8 -*-

from flask import Flask
from health import health_api
from sentiment import sentiment_api

import authmiddleware as middleware

app = Flask(__name__)
app.config['MAX_CONTENT_LENGTH'] = 100 * 1024 * 1024 # 100MB

app.wsgi_app = middleware.AuthMiddleware(app.wsgi_app)

app.register_blueprint(health_api, url_prefix='/api/health')
app.register_blueprint(sentiment_api, url_prefix='/api/sentiment')

if __name__ == '__main__':
    app.run()
