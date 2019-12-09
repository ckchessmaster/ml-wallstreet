import pickle
import os
import os.path as path

from pymongo import MongoClient

class ModelInfo():
    def __init__(self, _id, model_type, acc, std_dev, has_vectorizor, is_current_model):
        self._id = _id
        self.model_type = model_type
        self.acc = acc
        self.std_dev = std_dev
        self.has_vectorizor = has_vectorizor
        self.is_current_model = is_current_model
    # end __init__()
# end class ModelInfo

class Model():
    def __init__(self, info, predictor, vectorizor=None):
        self.info = info
        self.predictor = predictor
        self.vectorizor = vectorizor
    # end __init__()
# end class Model

collection = MongoClient().MLService.models

def get_model(model_id):
    model_info = collection.find_one({'_id': model_id})

    if model_info is None:
        return None

    model_info = ModelInfo(
        model_info['_id'],
        model_info['model_type'],
        model_info['acc'],
        model_info['std_dev'],
        model_info['has_vectorizor'],
        model_info['is_current_model'])

    predictor = pickle.load(open('models/' + model_id + '.mdl', 'rb'))
    vectorizor = None

    if model_info.has_vectorizor == True:
        vectorizor = pickle.load(open('models/' + model_id + '.vec', 'rb'))

    return Model(model_info, predictor, vectorizor)
# end get_model()

def save_model(model):
    # Insert record into mongo for tracking
    # If this model is going to be the new current we need to first update the current
    if model.info.is_current_model == True:
        collection.update_many({
            'model_type': model.info.model_type,
            'is_current_model': True
        },
        {
            '$set': { 'is_current_model': False }
        })
    # endif

    model_id = collection.insert_one(model.info.__dict__).inserted_id

    # Write the dataset to file
    folder_path = 'models/'

    # make sure directory exists
    if not os.path.exists(folder_path):
        os.makedirs(folder_path)

    pickle.dump(model.predictor, open(folder_path + model_id + '.mdl', 'wb'))

    if model.info.has_vectorizor:
        pickle.dump(model.vectorizor, open(folder_path + model_id + '.vec', 'wb'))

    return model_id
# end save_model()

def find_current_model_by_model_type(model_type):
    return collection.find_one({'model_type': model_type, 'is_current_model': True}, {'_id': 1})['_id']
# end find_current_model_by_model_type()

