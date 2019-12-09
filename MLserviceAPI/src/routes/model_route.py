import json

import services.model_service as model_service
import services.data_service as data_service
import bson.json_util as json_util

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