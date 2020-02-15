# -*- coding: utf-8 -*-
if __name__ == '__main__':
    # Set the working directory and other environment vars before we do anything else
    import os
    os.chdir(os.path.dirname(__file__) + '/..')
    # os.environ["KERAS_BACKEND"] = "plaidml.keras.backend"
    # os.environ["PLAIDML_USE_STRIPE"] = "1"

    import nltk
    nltk.download('stopwords')

    # Imports
    import services.logger as logger
    from flask import Flask
    from flask_cors import CORS
    from routes.health_route import health_api
    from routes.model_route import model_api
    import config

    import middleware.auth_middleware as middleware

    # Configure the API
    app = Flask(__name__)
    app.config['MAX_CONTENT_LENGTH'] = 100 * 1024 * 1024 # 100MB

    app.wsgi_app = middleware.AuthMiddleware(app, app.wsgi_app)

    app.register_blueprint(health_api, url_prefix='/api/health')
    app.register_blueprint(model_api, url_prefix='/api/models')

    CORS(app, origins=config.ALLOWED_ORIGINS, supports_credentials=True)

    # Run the app
    app.run(host='127.0.0.1')
