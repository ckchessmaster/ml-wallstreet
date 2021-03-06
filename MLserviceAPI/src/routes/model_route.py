import json
import config
import threading
from uuid import uuid4
from multiprocessing import Process


import services.model_service as model_service
import services.data_service as data_service
import services.sentiment_service as sentiment_service
import services.category_service as category_service
import services.stock_service as stock_service
import services.stock_service_v2 as stock_service_v2
import services.logger as logger
import bson.json_util as json_util

from flask import Blueprint
from flask import jsonify
from flask import request

from services.data_service import Dataset
from services.sentiment_service import MODEL_TYPE as SENTIMENT_MODEL_TYPE
from services.category_service import MODEL_TYPE as CATEGORY_MODEL_TYPE
from services.stock_service import MODEL_TYPE as STOCK_MODEL_TYPE
from services.stock_service_v2 import MODEL_TYPE as STOCK_V2_MODEL_TYPE

model_api = Blueprint('model_api', __name__)

@model_api.route('/<model_type>/current', methods=['GET'])
def get_model_info(model_type):
    model_id = model_service.find_current_model_by_model_type(model_type)

    model_info = model_service.get_model_info(model_id)

    if model_info is None:
        return jsonify({'acc': 0.0, 'std_dev': 0.0})

    return jsonify(model_info)
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

    dataset_info = {
        "_id": str(uuid4()),
        "name": json_body['name'],
        "model_type": model_type
    }
    dataset = Dataset(dataset_info, json_body['data'])

    if model_type == SENTIMENT_MODEL_TYPE:
        if sentiment_service.is_busy():
            return jsonify({"message":"System is busy. Please try again later."}), 500
        # endif

        if config.ADDITIONAL_DEBUGGING_SUPPORT == True:
            thread = threading.Thread(target=sentiment_service.train_dirty, args=(dataset,))
            thread.start()
        else:
            process = Process(target=sentiment_service.train_dirty, args=(dataset,))
            process.start()
    elif model_type == CATEGORY_MODEL_TYPE:
        if category_service.is_busy():
            return jsonify({"message":"System is busy. Please try again later."}), 500
        # endif
        if config.ADDITIONAL_DEBUGGING_SUPPORT == True:
            thread = threading.Thread(target=category_service.train_dirty, args=(dataset,))
            thread.start()
        else:
            process = Process(target=category_service.train_dirty, args=(dataset,))
            process.start()
    # endif

    elif model_type == STOCK_MODEL_TYPE:
        if stock_service.is_busy():
            return jsonify({"message":"System is busy. Please try again later."}), 500
        # endif
        if config.ADDITIONAL_DEBUGGING_SUPPORT == True:
            thread = threading.Thread(target=stock_service.train_dirty, args=(dataset,))
            thread.start()
        else:
            process = Process(target=stock_service.train_dirty, args=(dataset,))
            process.start()
    # endif

    elif model_type == STOCK_V2_MODEL_TYPE:
        if stock_service_v2.is_busy():
            return jsonify({"message":"System is busy. Please try again later."}), 500
        # endif
        if config.ADDITIONAL_DEBUGGING_SUPPORT == True:
            thread = threading.Thread(target=stock_service_v2.train_dirty, args=(dataset,))
            thread.start()
        else:
            process = Process(target=stock_service_v2.train_dirty, args=(dataset,))
            process.start()
    # endif

    return jsonify({ 'message': 'Training started.' })
# end train_new()

@model_api.route('/<model_type>/train/<dataset_id>', methods=['POST'])
def train_existing(model_type, dataset_id):
    if data_service.get_dataset(dataset_id) is None:
        return jsonify({"message":"Dataset not found."}), 400

    if model_type == SENTIMENT_MODEL_TYPE:
        if sentiment_service.is_busy():
            return jsonify({"message":"System is busy. Please try again later."}), 500
        # endif

        if config.ADDITIONAL_DEBUGGING_SUPPORT == True:
            thread = threading.Thread(target=sentiment_service.train_clean, args=(dataset_id,))
            thread.start()
        else:
            process = Process(target=sentiment_service.train_clean, args=(dataset_id,))
            process.start()
    elif model_type == CATEGORY_MODEL_TYPE:
        if category_service.is_busy():
            return jsonify({"message":"System is busy. Please try again later."}), 500
        # endif

        if config.ADDITIONAL_DEBUGGING_SUPPORT == True:
            thread = threading.Thread(target=category_service.train_clean, args=(dataset_id,))
            thread.start()
        else:
            process = Process(target=category_service.train_clean, args=(dataset_id,))
            process.start()

    elif model_type == STOCK_MODEL_TYPE:
        if stock_service.is_busy():
            return jsonify({"message":"System is busy. Please try again later."}), 500
        # endif

        if config.ADDITIONAL_DEBUGGING_SUPPORT == True:
            thread = threading.Thread(target=stock_service.train_clean, args=(dataset_id,))
            thread.start()
        else:
            process = Process(target=stock_service.train_clean, args=(dataset_id,))
            process.start()
    # endif

    elif model_type == STOCK_V2_MODEL_TYPE:
        if stock_service_v2.is_busy():
            return jsonify({"message":"System is busy. Please try again later."}), 500
        # endif

        if config.ADDITIONAL_DEBUGGING_SUPPORT == True:
            thread = threading.Thread(target=stock_service_v2.train_clean, args=(dataset_id,))
            thread.start()
        else:
            process = Process(target=stock_service_v2.train_clean, args=(dataset_id,))
            process.start()
    # endif

    return jsonify({ 'message': 'Training started.' })
# end train_existing

@model_api.route('/<model_type>/predict', methods=['POST'])
def predict_many(model_type):
    json_body = request.get_json()

    # Validate required parameters
    if json_body is None:
        return jsonify({"message":"Missing required training data."}), 400
    elif 'texts' not in json_body:
        return jsonify({"message":"Missing required texts."}), 400

    results = None

    if model_type == STOCK_V2_MODEL_TYPE:
        try:
            results = stock_service_v2.predict(json_body['texts'])
        except Exception as e:
            logger.log_error('Error trying to predict sentiment. ' + str(e.args))
            return jsonify({"message": config.INTERNAL_ERROR_MESSAGE}), 500
    else:
        return jsonify({"message": "Model not found."}), 400
    # endif

    return jsonify({ "results": results.tolist() })
# end predict_single()

@model_api.route('/<model_type>/predict', methods=['GET'])
def predict_single(model_type):
    input_text = request.args.get('inputText')
    
    if input_text is None:
        return jsonify({"message":"Missing required query parameter: InputText"}), 400

    result = {}

    if model_type == SENTIMENT_MODEL_TYPE:
        try:
            result = sentiment_service.predict_single(input_text)
        except Exception as e:
            logger.log_error('Error trying to predict sentiment. ' + str(e.args))
            return jsonify({"message": config.INTERNAL_ERROR_MESSAGE}), 500
    elif model_type == CATEGORY_MODEL_TYPE:
        try:
            result = category_service.predict_single(input_text)
        except Exception as e:
            logger.log_error('Error trying to predict sentiment. ' + str(e.args))
            return jsonify({"message": config.INTERNAL_ERROR_MESSAGE}), 500
    elif model_type == STOCK_MODEL_TYPE:
        try:
            result = stock_service.predict_single(input_text)
        except Exception as e:
            logger.log_error('Error trying to predict sentiment. ' + str(e.args))
            return jsonify({"message": config.INTERNAL_ERROR_MESSAGE}), 500
    elif model_type == STOCK_V2_MODEL_TYPE:
        try:
            result = stock_service_v2.predict(input_text)
        except Exception as e:
            logger.log_error('Error trying to predict sentiment. ' + str(e.args))
            return jsonify({"message": config.INTERNAL_ERROR_MESSAGE}), 500
    # endif

    return jsonify({ "Result": str(result[0]) })
# end predict_single()

@model_api.route('/<model_type>/isBusy', methods=['GET'])
def is_sentiment_busy(model_type):
    if model_type == SENTIMENT_MODEL_TYPE:
        return jsonify(sentiment_service.is_busy())
    elif model_type == CATEGORY_MODEL_TYPE:
        return jsonify(category_service.is_busy())
    # endif
# end is_sentiment_busy()