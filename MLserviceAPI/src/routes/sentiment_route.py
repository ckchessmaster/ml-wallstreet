import config
import services.logger as logger
import services.sentiment_service as sentiment_service
from services.sentiment_service import ClassifierNotReadyError

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
        result = sentiment_service.try_predict(input_text)
    except ClassifierNotReadyError as e:
        return jsonify({"message":e.client_message}), 500
    except Exception as e:
        logger.log_error('Error trying to predict sentiment. ' + str(e.message))
        return jsonify({"message":config.INTERNAL_ERROR_MESSAGE}), 500

    return jsonify({ "Result": result[0] })

# @sentiment_api.route('/predict', methods=['POST'])
# def predict_many():
#     json = request.get_json()

#     if json is None or 'InputText' not in json:
#         return jsonify({"Message":"Missing required array: InputText"}), 400
    
#     inputText = json['InputText']

#     results = predict(inputText)

#     finalResults = list(map(lambda x: { "Text": x[0], "Prediction": x[1] }, zip(inputText, results)))

#     return jsonify({ "Results": finalResults })

# @sentiment_api.route('/train', methods=['POST'])
# def train_sentiment():
#     json = request.get_json()

#     if json is None or 'TrainingData' not in json:
#         trainingThread = threading.Thread(target=train, args=(None,))
#         trainingThread.start()

#         return jsonify({"Message":"Training started with pre-cleaned data..."})
#         #return jsonify({"Message":"Missing required array: InputText"}), 400


#     if is_training == True:
#         return jsonify({"Message":"Training already in progress."})

#     trainingThread = threading.Thread(target=train, args=(json,))
#     trainingThread.start()

#     return jsonify({ "Message": "Training Started." })

# # TODO: This route is broken!
# @sentiment_api.route('/isTraining', methods=['GET'])
# def is_sentiment_training():
#     return jsonify(is_training)