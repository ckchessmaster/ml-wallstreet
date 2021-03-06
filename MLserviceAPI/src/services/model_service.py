import pickle
import os
import os.path as path
import config

from pymongo import MongoClient
from collections import namedtuple

from utility.ann import NeuralNetwork

class Model():
    def __init__(self, info, predictor, vectorizor=None, label_encoder=None, one_hot_encoder=None, tokenizer=None):
        self.info = info
        self.predictor = predictor
        self.vectorizor = vectorizor
        self.label_encoder = label_encoder
        self.one_hot_encoder = one_hot_encoder
        self.tokenizer = tokenizer
    # end __init__()
# end class Model

collection = MongoClient(config.MONGO_CONNECTION_STRING).MLService.models

def get_model_info(model_id):
    return collection.find_one({'_id': model_id})
# get_model_info

def get_model(model_id):
    model_info = collection.find_one({'_id': model_id})

    if model_info is None:
        return None

    if 'use_keras_save' in model_info and model_info['use_keras_save'] == True:
        model = NeuralNetwork()
        model.load('models/' + model_id)
    else:
        model = pickle.load(open('models/' + model_id + '.mdl', 'rb'))

    vectorizor = None
    label_encoder = None
    one_hot_encoder = None
    tokenizer = None

    vectorizor_path = 'models/' + model_id + '.vec'
    if path.exists(vectorizor_path):
        vectorizor = pickle.load(open('models/' + model_id + '.vec', 'rb'))

    label_encoder_path = 'models/' + model_id + '.lbl'
    if path.exists(label_encoder_path):
        label_encoder = pickle.load(open(label_encoder_path, 'rb'))

    one_hot_encoder_path = 'models/' + model_id + '.ohe'
    if path.exists(one_hot_encoder_path):
        one_hot_encoder = pickle.load(open(one_hot_encoder_path, 'rb'))

    tokenizer_path  = 'models/' + model_id + '.tok'
    if path.exists(tokenizer_path):
        tokenizer = pickle.load(open(tokenizer_path, 'rb'))
    # endif

    return Model(model_info, model, vectorizor, label_encoder, one_hot_encoder, tokenizer)
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
        model.predictor.save(folder_path + model_id)
    else:
        pickle.dump(model.predictor, open(folder_path + model_id + '.mdl', 'wb'))
        
    if model.vectorizor is not None:
        pickle.dump(model.vectorizor, open(folder_path + model_id + '.vec', 'wb'))
    if model.label_encoder is not None:
        pickle.dump(model.label_encoder, open(folder_path + model_id + '.lbl', 'wb'))
    if model.one_hot_encoder is not None:
        pickle.dump(model.one_hot_encoder, open(folder_path + model_id + '.ohe', 'wb'))
    if model.tokenizer is not None:
        pickle.dump(model.tokenizer, open(folder_path + model_id + '.tok', 'wb'))
    # endif

    return model_id
# end save_model()

def find_current_model_by_model_type(model_type):
    model = collection.find_one({'model_type': model_type, 'is_current_model': True}, {'_id': 1})

    return model['_id'] if model is not None else None
# end find_current_model_by_model_type()

