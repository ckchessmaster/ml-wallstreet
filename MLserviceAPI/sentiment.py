from flask import Blueprint
from flask import jsonify
from flask import request

import pickle

sentiment_api = Blueprint('sentiment_api', __name__)

@sentiment_api.route('/predict', methods=['GET'])
def predict_single():
    inputText = request.args.get('inputText')
    
    if inputText is None:
        return jsonify({"Message":"Missing required query parameter: InputText"}), 400

    classifier = pickle.load(open('sentiment.mdl', 'rb'))
    return jsonify({ "Result": classifier.predict(inputText) })
