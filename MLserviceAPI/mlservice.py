# -*- coding: utf-8 -*-

from flask import Flask
from flask import jsonify
import authmiddleware as middleware

app = Flask(__name__)

app.wsgi_app = middleware.AuthMiddleware(app.wsgi_app)

@app.route('/api/health')
def hello_world():
    return jsonify({ "Healthy": True })

if __name__ == '__main__':
    app.run()
