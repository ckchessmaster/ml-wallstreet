# Imports
import re
import config
import time
import numpy as np

import services.logger as logger
import services.model_service as model_service
import services.data_service as data_service
import services.text_service as text_service

from services.model_service import Model
from utility.ann import NeuralNetwork
from multiprocessing import Pool
from uuid import uuid4

from sklearn.feature_extraction.text import CountVectorizer, TfidfVectorizer
from sklearn.cluster import KMeans
from sklearn.metrics import silhouette_score
from sklearn.model_selection import train_test_split, GridSearchCV
from sklearn.model_selection import cross_val_score
from sklearn.preprocessing import OneHotEncoder, LabelEncoder, OrdinalEncoder
from sklearn.neighbors import KNeighborsClassifier
from sklearn.naive_bayes import GaussianNB
from sklearn.ensemble import RandomForestClassifier

from keras.preprocessing.sequence import pad_sequences
from keras.preprocessing.text import Tokenizer, one_hot
from keras.optimizers import SGD, RMSprop, Adam
from keras.layers import Dense, Flatten, LSTM
from keras.layers.convolutional import Conv1D, MaxPooling1D
from keras.layers.embeddings import Embedding

from services.exception import CleaningInProgressError
from services.exception import TrainingInProgressError
from services.exception import ClassifierNotReadyError

# Globals
classifier_ready = False
is_training = False
is_cleaning = False
MODEL_TYPE = 'STOCKV2'

# Try to load the vectorizer & classfier
classifier = None
vectorizer = None
tokenizer = None
label_encoder = None
one_hot_encoder = None

model_id = model_service.find_current_model_by_model_type(MODEL_TYPE)
if model_id is not None:
    model = model_service.get_model(model_id)

    classifier = model.predictor
    vectorizer = model.vectorizor
    tokenizer = model.tokenizer
    label_encoder = model.label_encoder
    one_hot_encoder = model.one_hot_encoder

    classifier_ready = True
# endif

# Begin stockv2 functions --------------------------------------------------------------------------------------------------


def predict_single(text):
    global classifier_ready

    if not classifier_ready:
        raise ClassifierNotReadyError()

    clean_data = text_service.clean_text_single({'Body': text, 'price_diff': 0.0})

    transformed_text = vectorizer.transform([clean_data[0]]).toarray() if vectorizer is not None else tokenizer.texts_to_sequences([clean_data[0]])

    prediction = classifier.predict(transformed_text)

    prediction = label_encoder.inverse_transform(np.array(one_hot_encoder.inverse_transform(prediction)).ravel())

    return prediction[0]
# end predict()

def predict_single_raw(text):
    global classifier_ready

    if not classifier_ready:
        raise ClassifierNotReadyError()

    clean_data = text_service.clean_text_single({'text': text, 'value': None})

    transformed_text = vectorizer.transform([clean_data[0]]).toarray() if vectorizer is not None else tokenizer.texts_to_sequences()

    prediction = classifier.predict(transformed_text)

    prediction = np.array(one_hot_encoder.inverse_transform(prediction)).ravel()

    return prediction[0]
# end predict()

def train_clean(dataset_id):
    # Load the dataset
    dataset = data_service.get_dataset(dataset_id)

    train(dataset)
# end train_clean()

def train_dirty(dataset):
    # Clean the dataset
    dataset.data = text_service.clean(dataset.data)

    # Save the cleaned dataset
    data_service.save_dataset(dataset)

    # Train
    train(dataset)
# end train_dirty()

def build_ann(epochs, batch_size):
    classifier = NeuralNetwork('SEQUENTIAL', epochs, batch_size) 
    classifier.add(Dense(units=500, activation='relu', input_dim=config.STOCK_V2_BAG_OF_WORDS_SIZE))
    classifier.add(Dense(units=250, activation='relu'))
    classifier.add(Dense(units=1, activation='sigmoid'))

    # Other optimizers: rmsprop, adagrad, adam, adadelta, adamax, nadam, SGD(lr=0.01)
    optimizer = 'adam' # Best: all about the same

    classifier.compile(optimizer=optimizer, loss='binary_crossentropy', metrics=['accuracy'])

    return classifier

# end build_ann()

def build_cnn(max_len, num_words):
    classifier = NeuralNetwork('SEQUENTIAL', 10) 
    classifier.add(Embedding(input_dim=num_words, output_dim=32, input_length=max_len)) # use bag of words size when applicable
    classifier.add(Conv1D(32, 3, padding='same', activation='relu'))
    classifier.add(MaxPooling1D())
    classifier.add(Flatten())
    classifier.add(Dense(250, activation='relu'))
    classifier.add(Dense(1, activation='sigmoid'))
    classifier.compile(loss='binary_crossentropy', optimizer=Adam(lr=0.001), metrics=['accuracy'])

    # Other optimizers: rmsprop, adagrad, adam, adadelta, adamax, nadam, SGD(lr=0.01)
    optimizer = 'adam' # Best: all about the same

    classifier.compile(optimizer=optimizer, loss='binary_crossentropy', metrics=['accuracy'])

    return classifier
# end build_cnn()
    
def build_lstm(sequence_length, n_words, batch_size=100):
    units1, units2 = int(n_words/4), int(n_words/8)

    classifier = NeuralNetwork('SEQUENTIAL', epochs=10, batch_size=batch_size) 
    classifier.add(Embedding(input_dim=n_words, output_dim=units1, input_length=sequence_length, trainable=True)) # Embed the text sequences
    classifier.add(LSTM(units=units2, return_sequences=False))
    classifier.add(Dense(units=1, activation='sigmoid'))

    # Other optimizers: rmsprop, adagrad, adam, adadelta, adamax, nadam, SGD(lr=0.01)
    classifier.compile(optimizer='adam', loss='binary_crossentropy', metrics=['accuracy'])

    return classifier
# end build_ltsm()

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

def max_length(array):
    if(not isinstance(array, list)): return(0)
    return(max([len(array),] + [len(subl) for subl in array if isinstance(subl, list)] + [max_length(subl) for subl in array]))
# end max_length

def train(dataset):
    global is_training
    global vectorizer
    global tokenizer
    global classifier
    global classifier_ready
    global label_encoder
    global one_hot_encoder

    if is_training == True:
        raise TrainingInProgressError()

    logger.log('Starting training...')
    is_training = True
    classifier_ready = False

    X, y = zip(*dataset.data)

    # Note: This should only be used for Bag of Words Models
    # logger.log('Vectorizing the data.') 
    # vectorizer = TfidfVectorizer(max_features=config.STOCK_V2_BAG_OF_WORDS_SIZE)
    # X = vectorizer.fit_transform(X).toarray()

    # Note: This should only be used for Embedded models
    logger.log('Encoding the data.')
    tokenizer = Tokenizer(num_words=config.STOCK_V2_BAG_OF_WORDS_SIZE)
    tokenizer.fit_on_texts(X)
    X = np.array([np.array(xi) for xi in tokenizer.texts_to_sequences(X)])
    X = pad_sequences(X, padding='post', value=0)
    sequence_length = len(X[0])
    tokenizer.max_length = sequence_length
    n_words = len(tokenizer.word_index)

    # Reduce the size of the array
    X = X.astype('uint16')

    X_train, X_test, y_train, y_test = train_test_split(X, y, train_size=config.TRAINING_SET_SIZE)
    
    logger.log('Fitting the model.')
    start = time.time()

    # classifier = KNeighborsClassifier(n_neighbors = 16, metric = 'minkowski', p = 1, weights='distance') # acc: 57.37% std_dev: 2.87%
    # classifier = RandomForestClassifier(n_estimators=1000, criterion='entropy', n_jobs=-1) # acc: 52.72% std_dev: 6.45%
    # classifier = GaussianNB() # acc: 49.51% std_dev: 5.78%
    # classifier = build_ann(epochs=10, batch_size=len(X_train)) # acc: Bag of Words - 55-60%
    # classifier = build_cnn() # acc: Bag of Words - 58.62%
    classifier = build_lstm(sequence_length, n_words, batch_size=int(len(X_train) / 8)) # acc: 

    classifier.fit(X_train, y_train)

    end = time.time()
    final = end - start
    logger.log('Fitting completed in ' + str(final) + 's')
    
    # find_best_params(X, y)

    logger.log('Determining accuracy.')

    # Acc for statistical models
    # accuracies = cross_val_score(estimator=classifier, X=X, y=y, cv=10, n_jobs=-1)
    # avg_accuracy = accuracies.mean() * 100
    # std_dev = accuracies.std() * 100

    # Acc for ANN:
    avg_accuracy = classifier.evaluate(X_test, y_test)
    avg_accuracy = avg_accuracy * 100
    std_dev = 0

    print('Saving results.')
    model_info = {
        '_id': str(uuid4()),
        'model_type': MODEL_TYPE,
        'has_vectorizor': True,
        'has_encoders': True,
        'is_current_model': True,
        'acc': avg_accuracy,
        'std_dev': std_dev,
        'use_keras_save': True
    }

    model = Model(model_info, classifier, vectorizor=None, tokenizer=tokenizer)
    model_service.save_model(model)

    is_training = False
    classifier_ready = True
    logger.log('Training completed.\nResults:\nAverage: ' + str(avg_accuracy) + '\nStandard Deviation: ' + str(std_dev))

    logger.log('Debugger')
#end train()
    
def is_busy():
    global is_cleaning
    global is_training

    return is_cleaning or is_training
# end is_busy()
