import pickle
import os
import os.path as path

from pymongo import MongoClient
from collections import namedtuple
from keras.models import load_model

from utility.ann import ANN

class Model():
    def __init__(self, info, predictor, vectorizor=None):
        self.info = info
        self.predictor = predictor
        self.vectorizor = vectorizor
    # end __init__()
# end class Model

collection = MongoClient().MLService.models

def get_model_info(model_id):
    return collection.find_one({'_id': model_id})
# get_model_info

def get_model(model_id):
    model_info = collection.find_one({'_id': model_id})

    if model_info is None:
        return None

    if 'use_keras_save' in model_info and model_info['use_keras_save'] == True:
        model = ANN()
        model.load('models/' + model_id + '.mdl')
    else:
        model = pickle.load(open('models/' + model_id + '.mdl', 'rb'))

    vectorizor = None
    if model_info['has_vectorizor'] == True:
        vectorizor = pickle.load(open('models/' + model_id + '.vec', 'rb'))

    return Model(model_info, model, vectorizor)
# end get_model()

def save_model(model):
    # Insert record into mongo for tracking
    # If this model is going to be the new current we need to first update the current
    if model.info['is_current_model'] == True:
        collection.update_many({
            'model_type': model.info['model_type'],
            'is_current_model': True
        },
        {
            '$set': { 'is_current_model': False }
        })
    # endif

    model_id = collection.insert_one(model.info).inserted_id

    # Write the dataset to file
    folder_path = 'models/'

    # make sure directory exists
    if not os.path.exists(folder_path):
        os.makedirs(folder_path)

    if 'use_keras_save' in model.info and model.info['use_keras_save'] == True:
        model.predictor.save(folder_path + model_id + '.mdl')
    else:
        pickle.dump(model.predictor, open(folder_path + model_id + '.mdl', 'wb'))

    if model.info['has_vectorizor']:
        pickle.dump(model.vectorizor, open(folder_path + model_id + '.vec', 'wb'))

    return model_id
# end save_model()

def find_current_model_by_model_type(model_type):
    model = collection.find_one({'model_type': model_type, 'is_current_model': True}, {'_id': 1})

    return model['_id'] if model is not None else None
# end find_current_model_by_model_type()

