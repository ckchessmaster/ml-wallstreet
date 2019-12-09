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

@sentiment_api.route('/isBusy', methods=['GET'])
def is_sentiment_busy():
    return jsonify(sentiment_service.is_busy())
# end is_sentiment_busy()



