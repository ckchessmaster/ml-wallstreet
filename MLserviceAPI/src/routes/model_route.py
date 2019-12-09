import json
import threading
import config

from uuid import uuid4

import services.model_service as model_service
import services.data_service as data_service
import services.sentiment_service as sentiment_service
import services.logger as logger
import bson.json_util as json_util

from services.data_service import DatasetInfo
from services.data_service import Dataset

from flask import Blueprint
from flask import jsonify
from flask import request

model_api = Blueprint('model_api', __name__)

@model_api.route('/<model_type>/current', methods=['GET'])
def get_model_info(model_type):
    model_id = model_service.find_current_model_by_model_type(model_type)

    model = model_service.get_model(model_id)

    return jsonify(model.info.__dict__)
# end get_model_info()

@model_api.route('/<model_type>/datasets', methods=['GET'])
def list_datasets(model_type):
    datasets = data_service.get_datasets(model_type)

    sanatized_result = json.loads(json_util.dumps(datasets))

    return jsonify({'datasets': sanatized_result })
# end list_datasets()

@model_api.route('/<model_type>/train', methods=['POST'])
def train_new(model_type):
    json_body = request.get_json()

    # Validate required parameters
    if json_body is None:
        return jsonify({"message":"Missing required training data."}), 400
    if 'name' not in json_body:
        return jsonify({"message":"Missing required query parameter: name"}), 400
    if 'data' not in json_body:
        return jsonify({"message":"Missing required query parameter: data"}), 400

    dataset_info = DatasetInfo(str(uuid4()), json_body['name'], 'SENTIMENT')
    dataset = Dataset(dataset_info, json_body['data'])

    if model_type == 'SENTIMENT':
        if sentiment_service.is_busy():
            return jsonify({"message":"System is busy. Please try again later."}), 500
        # endif

        thread = threading.Thread(target=sentiment_service.train_dirty, args=(dataset,))
        thread.start()
    # endif

    return jsonify({ 'message': 'Training started.' })
# end train_new()

@model_api.route('/<model_type>/train/<dataset_id>', methods=['POST'])
def train_existing(model_type, dataset_id):
    if data_service.get_dataset(dataset_id) is None:
        return jsonify({"message":"Dataset not found."}), 400

    if model_type == 'SENTIMENT':
        if sentiment_service.is_busy():
            return jsonify({"message":"System is busy. Please try again later."}), 500
        # endif

        thread = threading.Thread(target=sentiment_service.train_clean, args=(dataset_id,))
        thread.start()
    # endif

    return jsonify({ 'message': 'Training started.' })
# end train_existing

@model_api.route('/<model_type>/predict', methods=['GET'])
def predict_single(model_type):
    input_text = request.args.get('inputText')
    
    if input_text is None:
        return jsonify({"message":"Missing required query parameter: InputText"}), 400

    result = {}

    if model_type == 'SENTIMENT':
        try:
            result = sentiment_service.predict_single(input_text)
        except Exception as e:
            logger.log_error('Error trying to predict sentiment. ' + str(e.args))
            return jsonify({"message": config.INTERNAL_ERROR_MESSAGE}), 500
    # endif

    return jsonify({ "Result": str(result[0]) })
# end predict_single()