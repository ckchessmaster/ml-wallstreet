# Imports
import services.logger as logger
import services.model_service as model_service
from services.model_service import Model
import services.data_service as data_service
import services.category_service as category_service
import services.sentiment_service as sentiment_service

from services.exception import ClassifierNotReadyError
from services.exception import CleaningInProgressError
from services.exception import TrainingInProgressError

from sklearn.preprocessing import OneHotEncoder
from sklearn.model_selection import train_test_split, GridSearchCV
from sklearn.model_selection import cross_val_score

from sklearn.neighbors import KNeighborsClassifier
from sklearn.naive_bayes import GaussianNB
from sklearn.ensemble import RandomForestClassifier

from tensorflow.keras.layers import Dense
from tensorflow.keras.optimizers import SGD

from utility.ann import NeuralNetwork
from multiprocessing import Pool
import config
import time
import pandas as pd
from uuid import uuid4

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
def find_best_params(X, y):
    logger.log('Finding the best parameters')

    # parameters for KNeighborsClassifier
    # parameters = {
    #     'n_neighbors': range(3, 20), # best: 16
    #     'weights': ['uniform', 'distance'], # best: distance
    #     'metric': ['euclidean', 'minkowski', 'manhattan'], # best: minkowski
    #     'p': range(1,5) # best: 1
    # }

    # parameters for RandomForestClassifier
    # parameters = {
    #     'n_estimators': [1, 5, 10, 100, 250, 500], # best: 500
    #     'criterion': ['gini', 'entropy'] # best: entropy
    # }

    parameters = {
        'n_estimators': [500, 750, 1000, 2000], # best: 500
        'criterion': ['entropy'] # best: entropy
    }

    start = time.time()
    grid_search = GridSearchCV(estimator = RandomForestClassifier(), 
                            param_grid = parameters,
                            scoring = 'accuracy',
                            cv = 10,
                            pre_dispatch=8,
                            n_jobs=-1)

    grid_search = grid_search.fit(X, y)
    end = time.time()
    final_time = end - start

    best_accuracy = grid_search.best_score_
    best_parameters = grid_search.best_params_
    logger.log('Best accuracy: ' + best_accuracy + '\nBest Parameters: ' + best_parameters + '\nTraining complete. In order to save model please re-run with the given parameters.')
    logger.log('Debug here')
# end find_best_params()

def clean_single(data):

    sentiment = sentiment_service.predict_single(data['Body'])
    category = category_service.predict_single_raw(data['Body'])
    result = 1 if float(data['price_diff']) > 0 else 0

    return (sentiment, category, result)
# end clean_single()

def clean(data):
    global is_cleaning

    if is_cleaning == True:
        raise CleaningInProgressError()

    # Clean the text
    if len(data) > config.STOCK_SINGLE_THREAD_CUTOFF:
        logger.log('Cleaning text.')
        is_cleaning = True

        pool = Pool(processes=8)
        clean_data = pool.map(clean_single, data)
        
        is_cleaning = False
        logger.log('Cleaning complete.')
    else:
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

def build_ann():
    classifier = NeuralNetwork('SEQUENTIAL', 10) 
    classifier.add(Dense(units=2, activation='relu', input_dim=2))
    classifier.add(Dense(units=1, activation='sigmoid'))

    # Other optimizers: rmsprop, adagrad, adam, adadelta, adamax, nadam, SGD(lr=0.01)
    optimizer = 'nadam' # Best: nadam

    classifier.compile(optimizer=optimizer, loss='binary_crossentropy', metrics=['accuracy'])

    return classifier
# end build_ann()

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

    df = pd.DataFrame(dataset.data, columns=['sentiment', 'category', 'result'])

    X = df.iloc[:, :-1].values
    y = df.iloc[:, -1].values

    X_train, X_test, y_train, y_test = train_test_split(X, y, train_size=config.TRAINING_SET_SIZE)

    logger.log('Fitting the model.')
    start = time.time()

    # find_best_params(X, y)

    # classifier = KNeighborsClassifier(n_neighbors = 9, metric = 'euclidean', p = 1, weights='uniform') # acc: 51.91% std_dev: 3.76%
    # classifier = GaussianNB() # acc: 57.44% std_dev: 0.07%
    classifier = RandomForestClassifier(n_estimators=10, criterion='gini', n_jobs=-1) # acc: 57.56% std_dev: 0.88%
    # classifier = build_ann() # acc: 56.79% 

    classifier.fit(X_train, y_train)

    end = time.time()
    final = end - start
    
    logger.log('Fitting completed in ' + final + 's')

    logger.log('Determining accuracy.')

    # Acc for statistical models
    accuracies = cross_val_score(estimator=classifier, X=X_train, y=y_train, cv=10, n_jobs=-1)
    avg_accuracy = accuracies.mean() * 100
    std_dev = accuracies.std() * 100

    # Acc for ANN:
    # avg_accuracy = classifier.evaluate(X_test, y_test)
    # avg_accuracy = avg_accuracy * 100
    # std_dev = 0

    print('Saving results.')
    model_info = {
        '_id': str(uuid4()),
        'model_type': MODEL_TYPE,
        'has_vectorizor': True,
        'has_encoders': False,
        'is_current_model': True,
        'acc': avg_accuracy,
        'std_dev': std_dev,
        'use_keras_save': False
    }

    model = Model(model_info, classifier, vectorizer)
    model_service.save_model(model)

    is_training = False
    classifier_ready = True
    logger.log('Training completed.\nResults:\nAverage: ' + avg_accuracy + '\nStandard Deviation: ' + std_dev)
    logger.log('Debug')
# end train()

def is_busy():
    global is_cleaning
    global is_training

    return is_cleaning or is_training
# end is_busy()