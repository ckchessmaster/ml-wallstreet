# Imports
import services.logger as logger
import pickle
import re
import config

from sklearn.feature_extraction.text import CountVectorizer
from sklearn.model_selection import cross_val_score
from sklearn.naive_bayes import GaussianNB

from nltk.stem.porter import PorterStemmer
from nltk.corpus import stopwords

from multiprocessing import Pool

# Custom exceptions
class CleaningInProgressError(Exception):
    def __init__(self, message=''):
        self.message = message + 'Cleaning already in progress.'
        self.client_message = 'System is currently busy. Please try again later.'

class TrainingInProgressError(Exception):
    def __init__(self, message=''):
        self.message = message + 'Training already in progress.'
        self.client_message = 'System is currently busy. Please try again later.'

class ClassifierNotReadyError(Exception):
    def __init__(self, message=''):
        self.message = message + 'Classifier not ready.'
        self.client_message = 'Classifier not ready. Please train your model first.'

# Globals
classifier_ready = True
is_training = False
is_cleaning = False

# Try to load the vectorizer & classfier
try:
    vectorizer = vectorizer = pickle.load(open('models/sentiment.vec', 'rb'))
except Exception as e:
    logger.log_error('Error loading vectorizer! ' + str(e.args))
    classifier_ready = False

try:
    classifier = pickle.load(open('models/sentiment.mdl', 'rb'))
except Exception as e:
    logger.log_error('Error loading classifier! ' + str(e.args))
    classifier_ready = False

# Begin sentiment functions --------------------------------------------------------------------------------------------------

def clean_single(text):
    clean_text = re.sub('[^a-zA-Z]', ' ', text) # Replace all non letters with spaces
    clean_text = clean_text.lower() # Set the entire text to lower case
    clean_text = clean_text.split() # Split the text into it's individual words

    # Remove words that don't contribute anything (like the, a, this, etc.)
    # Also apply stemming aka getting the root of words
    ps = PorterStemmer()
    clean_text = [ps.stem(word) for word in clean_text if not word in set(stopwords.words('english'))]
    clean_text = ' '.join(clean_text) # Put the string back together

    return clean_text

def clean(text):
    global is_cleaning

    if is_cleaning == True:
        raise CleaningInProgressError()

    if len(text) > config.SENTIMENT_SINGLE_THREAD_CUTOFF:
        logger.log('Cleaning text.')
        is_cleaning = True

        pool = Pool(processes=8)
        clean_text = pool.map(clean_single, text)
        
        is_cleaning = False
        logger.log('Cleaning complete.')
    else:
        clean_text = map(clean_single, text)

    return clean_text

def predict(text):
    global classifier_ready

    if not classifier_ready:
        raise ClassifierNotReadyError()

    clean_text = clean(text)
    vectorized_text = vectorizer.transform(clean_text).toarray()
    predictions = classifier.predict(vectorized_text)

    return predictions

def train(dataset):
    global is_training
    global vectorizer
    global classifier

    if is_training == True:
        raise TrainingInProgressError()

    logger.log('Starting training...')
    is_training = True

    logger.log('Vectorizing the data.')
    vectorizer = CountVectorizer(max_features=config.BAG_OF_WORDS_SIZE)
    X = vectorizer.fit_transform(dataset.X)
    y = dataset.y

    logger.log('Fitting the model.')
    classifier = GaussianNB()
    classifier.fit(X, y)

    logger.log('Determining accuracy.')
    accuracies = cross_val_score(estimator=classifier, X=X, y=y, cv=10, pre_dispatch=8)
    avg_accuracy = accuracies.mean() * 100
    std_dev = accuracies.std() * 100

    print('Saving results.')
    pickle.dump(vectorizer, open('models/sentiment.vec', 'wb'))
    pickle.dump(classifier, open('models/sentiment.mdl', 'wb'))

    is_training = False
    logger.log(f'Training completed.\nResults:\nAverage: {avg_accuracy}\nStandard Deviation: {std_dev}')
    
# Use the following section when training to choose between classifiers and their parameters
# parameters = [{'C': [1, 10, 100, 1000], 'kernel': ['linear']},
#             {'C': [1, 10, 100, 1000], 'kernel': ['rbf'], 'gamma': [0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9]}]
# grid_search = GridSearchCV(estimator = classifier,
#                         param_grid = parameters,
#                         scoring = 'accuracy',
#                         cv = 10,
#                         pre_dispatch=8)
# grid_search = grid_search.fit(X_train, y_train)
# best_accuracy = grid_search.best_score_
# best_parameters = grid_search.best_params_