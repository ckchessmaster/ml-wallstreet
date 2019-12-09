import pickle
import os
import os.path as path

from pymongo import MongoClient

class DatasetInfo():
    def __init__(self, _id, name, model_type):
        self._id = _id
        self.name = name
        self.model_type = model_type

class Dataset():
    def __init__(self, info, data):
        self.info = info
        self.data = data
# end Datset

collection = MongoClient().MLService.datasets

def get_datasets(model_type):
    datasets = collection.find({'model_type': model_type}, {'_id': 1, 'name': 1})

    return datasets

def get_dataset(dataset_id):
    if collection.find_one({'_id': dataset_id}) is None:
        return None

    dataset = pickle.load(open('datasets/' + dataset_id + '.dat', 'rb'))

    return dataset
# end get_dataset()

def save_dataset(dataset):
    # Insert record into mongo for tracking
    dataset_id = collection.insert_one(dataset.info.__dict__).inserted_id

    # Write the dataset to file
    folder_path = 'datasets/'

    # make sure directory exists
    if not os.path.exists(folder_path):
        os.makedirs(folder_path)

    pickle.dump(dataset, open(folder_path + dataset_id + '.dat', 'wb'))

    return dataset_id
# end save_dataset()