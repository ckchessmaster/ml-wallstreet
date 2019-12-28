# Imports
import re
import config
import services.logger as logger
import services.model_service as model_service
import services.data_service as data_service
import time

from services.model_service import Model

from sklearn.feature_extraction.text import CountVectorizer
from sklearn.cluster import KMeans
from sklearn.metrics import silhouette_score
from sklearn.model_selection import train_test_split, GridSearchCV
from sklearn.model_selection import cross_val_score
from sklearn.preprocessing import OneHotEncoder, LabelEncoder
from sklearn.neighbors import KNeighborsClassifier
from sklearn.naive_bayes import GaussianNB
from sklearn.ensemble import RandomForestClassifier

from nltk.stem.porter import PorterStemmer
from nltk.corpus import stopwords

from services.exception import CleaningInProgressError
from services.exception import TrainingInProgressError
from services.exception import ClassifierNotReadyError
from utility.ann import ANN

from multiprocessing import Pool
from uuid import uuid4
from keras.layers import Dense

# Globals
classifier_ready = False
is_training = False
is_cleaning = False
MODEL_TYPE = 'CATEGORY'

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

# Begin category functions --------------------------------------------------------------------------------------------------
def clean_text_single(data):
    text = data['text']
    value = data['value']

    clean_text = re.sub('[^a-zA-Z]', ' ', text) # Replace all non letters with spaces
    clean_text = clean_text.lower() # Set the entire text to lower case
    clean_text = clean_text.split() # Split the text into it's individual words

    # Remove words that don't contribute anything (like the, a, this, etc.)
    # Also apply stemming aka getting the root of words
    ps = PorterStemmer()
    clean_text = [ps.stem(word) for word in clean_text if not word in set(stopwords.words('english'))]
    clean_text = ' '.join(clean_text) # Put the string back together

    return (clean_text, value)
# end clean_single()

def clean(data):
    global is_cleaning

    if is_cleaning == True:
        raise CleaningInProgressError()

    # Clean the text
    if len(data) > config.SENTIMENT_SINGLE_THREAD_CUTOFF:
        logger.log('Cleaning text.')
        is_cleaning = True

        pool = Pool(processes=8)
        clean_data = pool.map(clean_text_single, data)
        
        is_cleaning = False
        logger.log('Cleaning complete.')
    else:
        clean_data = map(clean_text_single, data)

    return list(clean_data)
# clean()

def predict_single(text):
    global classifier_ready

    if not classifier_ready:
        raise ClassifierNotReadyError()

    clean_data = clean_text_single({'text': text, 'value': None})

    vectorized_text = vectorizer.transform([clean_data[0]]).toarray()

    predictions = classifier.predict(vectorized_text)

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

    text, values = zip(*dataset.data)

    X_train, X_test, y_train, y_test = train_test_split(text, values, train_size=config.TRAINING_SET_SIZE)

    logger.log('Encoding the data.')
    label_encoder = LabelEncoder()
    y_train_labeled = label_encoder.fit_transform(y_train)
    y_test_labeled = label_encoder.transform(y_test)

    one_hot_encoder = OneHotEncoder(drop='first', handle_unknown='error', categories='auto')
    y_train_encoded = one_hot_encoder.fit_transform(y_train_labeled.reshape(-1, 1)).toarray()
    y_test_encoded = one_hot_encoder.transform(y_test_labeled.reshape(-1, 1)).toarray()

    logger.log('Vectorizing the data.')
    vectorizer = CountVectorizer(max_features=config.BAG_OF_WORDS_SIZE)
    X_train = vectorizer.fit_transform(X_train).toarray()
    y_train = y_train_encoded

    num_categories = len(y_train[0])

    X_test = vectorizer.transform(X_test).toarray()
    y_test = y_test_encoded
    # y_train = y_labeled # Note this is only for classifiers that do not support multilabel

    logger.log('Fitting the model.')
    start = time.time()

    # classifier = KNeighborsClassifier(n_neighbors = 3, metric = 'minkowski', p = 1, weights='distance') # acc: 44%, 25%
    # classifier = RandomForestClassifier(n_estimators=100, criterion='gini', n_jobs=-1) # acc: 59%, 35%
    # classifier = GaussianNB() # acc: 30%, 3%

    # ANN classifier
    classifier = ANN('SEQUENTIAL', 10) # acc: 60%, 35%
    classifier.add(Dense(output_dim=250, activation='relu', input_dim=config.BAG_OF_WORDS_SIZE))
    classifier.add(Dense(output_dim=num_categories, activation='softmax'))
    classifier.compile(optimizer='rmsprop', loss='categorical_crossentropy', metrics=['accuracy'])

    classifier.fit(X_train, y_train)

    end = time.time()
    final = end - start
    
    logger.log(f'Fitting completed in {final}s')
    
    # logger.log('Finding the best parameters')

    # parameters for KNeighborsClassifier
    # parameters = {
    #     'n_neighbors': range(3, 20), # best: 3
    #     'weights': ['uniform', 'distance'], # best: distance
    #     'metric': ['euclidean', 'minkowski', 'manhattan'], # best: minkowski
    #     'p': range(1,5) # best: 1
    # }

    # parameters for RandomForestClassifier
    # parameters = {
    #     'n_estimators': [100], # best: 100
    #     'criterion': ['gini', 'entropy'] # best: gini
    # }

    # start = time.time()
    # grid_search = GridSearchCV(estimator = RandomForestClassifier(), 
    #                         param_grid = parameters,
    #                         scoring = 'accuracy',
    #                         cv = 10,
    #                         pre_dispatch=8,
    #                         n_jobs=-1)

    # grid_search = grid_search.fit(X, y)
    # end = time.time()

    # best_accuracy = grid_search.best_score_
    # best_parameters = grid_search.best_params_
    # logger.log(f'Best accuracy: {best_accuracy}\nBest Parameters: {best_parameters}\nTraining complete. In order to save model please re-run with the given parameters.')

    logger.log('Determining accuracy.')
    # accuracies = cross_val_score(estimator=classifier, X=X, y=y_labeled, cv=10, n_jobs=-1)

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
        'is_current_model': True,
        'acc': avg_accuracy,
        'std_dev': std_dev,
        'use_keras_save': True
    }

    model = Model(model_info, classifier, vectorizer)
    model_service.save_model(model)

    is_training = False
    classifier_ready = True
    logger.log(f'Training completed.\nResults:\nAverage: {avg_accuracy}\nStandard Deviation: {std_dev}')
    
def is_busy():
    global is_cleaning
    global is_training

    return is_cleaning or is_training
# end is_busy()
