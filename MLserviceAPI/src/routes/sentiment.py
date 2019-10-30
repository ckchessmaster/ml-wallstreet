from flask import Blueprint
from flask import jsonify
from flask import request

from sklearn.naive_bayes import GaussianNB
from sklearn.svm import SVC
from sklearn.model_selection import GridSearchCV
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.feature_extraction.text import HashingVectorizer
from sklearn.decomposition import PCA
from sklearn.model_selection import train_test_split
from sklearn.model_selection import cross_val_score

from nltk.corpus import stopwords
from nltk.stem.porter import PorterStemmer

from multiprocessing import Pool

import pickle
import re
import pandas
import threading
import gc

sentiment_api = Blueprint('sentiment_api', __name__)
vectorizer = pickle.load(open('models/sentiment.vec', 'rb'))
classifier = pickle.load(open('models/sentiment.mdl', 'rb'))

is_training = False

def clean(data):
    text = data['Text']
    result = data['Result']

    cleanText = re.sub('[^a-zA-Z]', ' ', text) # Replace all non letters with spaces
    cleanText = cleanText.lower() # Set the entire text to lower case
    cleanText = cleanText.split() # Split the text into it's individual words

    # Remove words that don't contribute anything (like the, a, this, etc.)
    # Also apply stemming aka getting the root of words
    ps = PorterStemmer()
    cleanText = [ps.stem(word) for word in cleanText if not word in set(stopwords.words('english'))]
    cleanText = ' '.join(cleanText) # Put the string back together

    return (cleanText, result)

def predict(text):
    cleanText = list(map(clean, text))

    vectorizedText = vectorizer.transform(cleanText).toarray()
    predictions = list(map(int, classifier.predict(vectorizedText)))

    return predictions

def train(data):
    print('Starting training...')
    is_training = True

    testSetSize = float(data['TestSetSize']) if data != None and 'TestSetSize' in data else 0.20

    finalData = []
    results = []

    if data != None:
        print('Cleaning the data')
        pool = Pool(processes=16)
        cleanData = pool.map(clean, data['TrainingData'])

        for item in cleanData:
            finalData.append(item[0])
            results.append(item[1])

        pickle.dump((finalData, results), open('CleanedSentimentData.dat', 'wb'))
    else:
        print('Loading cleaned data')
        cleanedData = pickle.load(open('CleanedSentimentData.dat', 'rb'))
        finalData = cleanedData[0]
        results = cleanedData[1]

    print('Vectorizing the data')
    # Creating the Bag of Words model
    vectorizer = CountVectorizer(max_features = 1500)
    # vectorizer = HashingVectorizer(n_features=50)
    X = vectorizer.fit_transform(finalData).toarray()
    y = results

    # Lets see if PCA can help reduce things for us
    # print('Attempting to reduce features')
    # pca = PCA(n_components = 200)
    # X = pca.fit_transform(X)
    # print('Explained varience' + str(pca.explained_variance_))

    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size = testSetSize)
    
    # Freeing up some memory
    X = None
    y = None
    X_test = None
    y_test = None
    finalData = None
    results = None
    pool = None

    gc.collect()

    print('Fitting the model.')
    # Fitting classifier to the Training set
    classifier = GaussianNB()
    # classifier = SVC(kernel = 'rbf', gamma=0.2, C=10)
    classifier.fit(X_train, y_train)

    # Use the following section when choosing between classifiers and their parameters
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

    # Predicting the Test set results so we can get the accuracy
    print('Getting accuracy.')
    accuracies = cross_val_score(estimator = classifier, X = X_train, y = y_train, cv = 10, pre_dispatch=8)
    avgAccuracy = accuracies.mean() * 100
    accuracyStdDeviation = accuracies.std() * 100

    print('Saving results.')
    # Save the classifier and vectorizer
    pickle.dump(vectorizer, open('sentiment.vec', 'wb'))
    pickle.dump(classifier, open('sentiment.mdl', 'wb'))

    # print('Training complete!\nResults:\nAverage: ' + str(best_accuracy) + '\nSVM Params: ' + str(best_parameters))
    print('Training complete!\nResults:\nAverage: ' + str(avgAccuracy) + '\nStandard Deviation: ' + str(accuracyStdDeviation))
    is_training = False

@sentiment_api.route('/predict', methods=['GET'])
def predict_single():
    inputText = request.args.get('inputText')
    
    if inputText is None:
        return jsonify({"Message":"Missing required query parameter: InputText"}), 400

    result = predict([inputText])
    
    return jsonify({ "Result": result[0] })

@sentiment_api.route('/predict', methods=['POST'])
def predict_many():
    json = request.get_json()

    if json is None or 'InputText' not in json:
        return jsonify({"Message":"Missing required array: InputText"}), 400
    
    inputText = json['InputText']

    results = predict(inputText)

    finalResults = list(map(lambda x: { "Text": x[0], "Prediction": x[1] }, zip(inputText, results)))

    return jsonify({ "Results": finalResults })

@sentiment_api.route('/train', methods=['POST'])
def train_sentiment():
    json = request.get_json()

    if json is None or 'TrainingData' not in json:
        trainingThread = threading.Thread(target=train, args=(None,))
        trainingThread.start()

        return jsonify({"Message":"Training started with pre-cleaned data..."})
        #return jsonify({"Message":"Missing required array: InputText"}), 400


    if is_training == True:
        return jsonify({"Message":"Training already in progress."})

    trainingThread = threading.Thread(target=train, args=(json,))
    trainingThread.start()

    return jsonify({ "Message": "Training Started." })

# TODO: This route is broken!
@sentiment_api.route('/isTraining', methods=['GET'])
def is_sentiment_training():
    return jsonify(is_training)