from flask import Blueprint
from flask import jsonify

health_api = Blueprint('health_api', __name__)

@health_api.route('/', methods=['GET'])
def health():
    return jsonify({ "Healthy": True })