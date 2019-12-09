import config

import threading
import services.logger as logger
import services.sentiment_service as sentiment_service
import services.data_service as data_service
import bson.json_util as json_util

from collections import namedtuple
from services.data_service import Dataset
from services.data_service import DatasetInfo
from uuid import uuid4

from services.sentiment_service import ClassifierNotReadyError
from services.sentiment_service import CleaningInProgressError
from services.sentiment_service import TrainingInProgressError

from flask import Blueprint
from flask import jsonify
from flask import request

sentiment_api = Blueprint('sentiment_api', __name__)

@sentiment_api.route('/predict', methods=['GET'])
def predict_single():
    input_text = request.args.get('inputText')
    
    if input_text is None:
        return jsonify({"message":"Missing required query parameter: InputText"}), 400

    try:
        result = sentiment_service.predict_single(input_text)
    except ClassifierNotReadyError as e:
        logger.log_error('Error trying to predict sentiment. ' + str(e.message))
        return jsonify({"message":e.client_message}), 500
    except CleaningInProgressError as e:
        logger.log_error('Error trying to predict sentiment. ' + str(e.message))
        return jsonify({"message":e.client_message}), 500
    except Exception as e:
        logger.log_error('Error trying to predict sentiment. ' + str(e.args))
        return jsonify({"message":config.INTERNAL_ERROR_MESSAGE}), 500

    return jsonify({ "Result": str(result[0]) })

@sentiment_api.route('/train', methods=['POST'])
def train_new():
    json_body = request.get_json()

    if sentiment_service.is_busy():
        return jsonify({"message":"System is busy. Please try again later."}), 500

    # Validate required parameters
    if json_body is None:
        return jsonify({"message":"Missing required training data."}), 400
    if 'name' not in json_body:
        return jsonify({"message":"Missing required query parameter: name"}), 400
    if 'data' not in json_body:
        return jsonify({"message":"Missing required query parameter: data"}), 400

    dataset_info = DatasetInfo(str(uuid4()), json_body['name'], 'SENTIMENT')
    dataset = Dataset(dataset_info, json_body['data'])

    thread = threading.Thread(target=sentiment_service.train_dirty, args=(dataset,))
    thread.start()

    return { 'message': 'Training started.' }
# end train_new()

@sentiment_api.route('/train/<dataset_id>', methods=['POST'])
def train_existing(dataset_id):
    if data_service.get_dataset(dataset_id) is None:
        return jsonify({"message":"Dataset not found."}), 400

    thread = threading.Thread(target=sentiment_service.train_clean, args=(dataset_id,))
    thread.start()

    return jsonify({ 'message': 'Training started.' })

@sentiment_api.route('/isBusy', methods=['GET'])
def is_sentiment_busy():
    return jsonify(sentiment_service.is_busy())
# end is_sentiment_busy()



