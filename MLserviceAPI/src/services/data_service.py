import pickle

from pymongo import MongoClient
from enum import Enum

class Dataset():
    def __init__(self, name, model_type, data):
        self.name = name
        self.model_type = model_type
        self.data = data
# end Datset

client = MongoClient()
db = client.ml_wallstreet
collection = db.datasets

def get_datasets(model_type):
    datasets = collection.find({'model_type': model_type}, {'_id': 1, 'name': 1})

    return list(datasets)

def get_dataset(name):
    dataset = collection.find_one({'name': name})

    return dataset

def save_dataset(dataset):
    result = collection.replace_one({'name': dataset.name}, dataset.__dict__, upsert=True)

    return result