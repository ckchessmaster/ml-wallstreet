# Imports
import services.logger as logger
import services.model_service as model_service
import services.data_service as data_service
import services.category_service as category_service
import services.sentiment_service as sentiment_service
import config

from services.model_service import Model

from multiprocessing import Pool

from services.exception import ClassifierNotReadyError
from services.exception import CleaningInProgressError
from services.exception import TrainingInProgressError

# Globals
classifier_ready = False
is_training = False
is_cleaning = False
MODEL_TYPE = 'STOCK'

# Try to load the vectorizer & classfier
classifier = None
vectorizer = None

model_id = model_service.find_current_model_by_model_type(MODEL_TYPE)
if model_id is not None:
    model = model_service.get_model(model_id)

    classifier = model.predictor
    vectorizer = model.vectorizor
    classifier_ready = True
# endif

# Begin stock functions --------------------------------------------------------------------------------------------------
def clean_single(data):

    sentiment = sentiment_service.predict_single(data['news'])
    category = category_service.predict_single(data['news'])
    
    start_price = 0
    end_price = 0
    result = end_price - start_price
    
    return (sentiment, category, result)
# end clean_single()

def clean(data):
    global is_cleaning

    if is_cleaning == True:
        raise CleaningInProgressError()

    clean_data = map(clean_single, data)

    return list(clean_data)
# clean()

def predict_single(data):
    global classifier_ready

    if not classifier_ready:
        raise ClassifierNotReadyError()

    clean_data = clean_single({'data': data, 'value': None})

    predictions = classifier.predict(clean_data)

    return predictions
# end predict()

def train_clean(dataset_id):
    # Load the dataset
    dataset = data_service.get_dataset(dataset_id)

    train(dataset)
# end train_clean()

def train_dirty(dataset):
    # Clean the dataset
    dataset.data = clean(dataset.data)

    # Save the cleaned dataset
    data_service.save_dataset(dataset)

    # Train
    train(dataset)
# end train_dirty()

def train(dataset):
    global is_training
    global vectorizer
    global classifier
    global classifier_ready

    if is_training == True:
        raise TrainingInProgressError()

    logger.log('Starting training...')
    is_training = True
    classifier_ready = False



    is_training = False
    classifier_ready = True
    logger.log(f'Training completed.')
# end train()

def is_busy():
    global is_cleaning
    global is_training

    return is_cleaning or is_training
# end is_busy()