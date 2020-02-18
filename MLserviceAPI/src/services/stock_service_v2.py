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

from sklearn.model_selection import train_test_split
from sklearn.model_selection import cross_val_score

from tensorflow.keras.preprocessing.sequence import pad_sequences
from tensorflow.keras.preprocessing.text import Tokenizer
from tensorflow.keras.optimizers import SGD, RMSprop, Adam
from tensorflow.keras.layers import Dense, Flatten, LSTM, Conv1D, MaxPooling1D, Embedding

from services.exception import CleaningInProgressError
from services.exception import TrainingInProgressError
from services.exception import ClassifierNotReadyError

# Globals
classifier_ready = False
is_training = False
is_cleaning = False
MODEL_TYPE = 'STOCKV2'

# Try to load the tokenizer & classfier
classifier = None
tokenizer = None

model_id = model_service.find_current_model_by_model_type(MODEL_TYPE)
if model_id is not None:
    model = model_service.get_model(model_id)

    classifier = model.predictor
    tokenizer = model.tokenizer

    classifier_ready = True
# endif

# Begin stockv2 functions --------------------------------------------------------------------------------------------------
def predict(texts):
    global classifier_ready

    if not classifier_ready:
        raise ClassifierNotReadyError()

    data = list(map(lambda text: { 'Body': text, 'price_diff': 0.0 }, texts))
    clean_data = text_service.clean(data)

    transformed_texts, _ = zip(*clean_data)
    transformed_texts = np.array([np.array(xi) for xi in tokenizer.texts_to_sequences(transformed_texts)])
    transformed_texts = pad_sequences(transformed_texts, maxlen=tokenizer.max_length, padding='post', value=0)

    predictions = classifier.predict(transformed_texts)

    return predictions
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
    
def build_lstm(sequence_length, n_words, starting_output_dim, batch_size=100):
    mem_size_GB = ((starting_output_dim * sequence_length * batch_size * 8) / 1000000000)
    if mem_size_GB > config.MAX_MEMORY_GB:
        raise Exception('Network is configured to use more than available RAM. Total size of current network: ' + str(mem_size_GB) + 'GB')

    classifier = NeuralNetwork('SEQUENTIAL', epochs=10, batch_size=batch_size) 
    classifier.add(Embedding(input_dim=n_words, output_dim=starting_output_dim, input_length=sequence_length, trainable=True)) # Embed the text sequences
    classifier.add(LSTM(units=int(starting_output_dim / 2), return_sequences=False))
    classifier.add(Dense(units=1, activation='sigmoid'))

    # Other optimizers: rmsprop, adagrad, adam, adadelta, adamax, nadam, SGD(lr=0.01)
    classifier.compile(optimizer='adam', loss='binary_crossentropy', metrics=['accuracy'])

    return classifier
# end build_ltsm()

def train(dataset):
    global is_training
    global tokenizer
    global classifier
    global classifier_ready

    if is_training == True:
        raise TrainingInProgressError()

    logger.log('Starting training...')
    is_training = True
    classifier_ready = False

    X, y = zip(*dataset.data)

    y = np.asarray(y)

    # Note: This should only be used for Embedded models
    logger.log('Encoding the data.')
    tokenizer = Tokenizer(num_words=config.STOCK_V2_BAG_OF_WORDS_SIZE)
    tokenizer.fit_on_texts(X)
    X = np.array([np.array(xi) for xi in tokenizer.texts_to_sequences(X)])
    X = pad_sequences(X, padding='post', value=0, maxlen=500) # Only ~3% of articles are greater than 500 words
    sequence_length = len(X[0])
    tokenizer.max_length = sequence_length
    n_words = config.STOCK_V2_BAG_OF_WORDS_SIZE

    X_train, X_test, y_train, y_test = train_test_split(X, y, train_size=config.TRAINING_SET_SIZE)
    
    logger.log('Fitting the model.')
    start = time.time()

    classifier = build_lstm(sequence_length, n_words, starting_output_dim=256, batch_size=1024) # acc: 

    classifier.fit(X_train, y_train)

    end = time.time()
    final = end - start
    logger.log('Fitting completed in ' + str(final) + 's')
    
    # find_best_params(X, y)

    logger.log('Determining accuracy.')

    # Acc for ANN:
    avg_accuracy = classifier.evaluate(X_test, y_test)
    avg_accuracy = avg_accuracy * 100
    std_dev = 0

    print('Saving results.')
    model_info = {
        '_id': str(uuid4()),
        'model_type': MODEL_TYPE,
        'is_current_model': True,
        'acc': avg_accuracy,
        'std_dev': std_dev,
        'use_keras_save': True
    }

    model = Model(model_info, classifier, tokenizer=tokenizer)
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
