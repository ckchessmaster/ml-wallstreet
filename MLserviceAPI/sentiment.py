from flask import Blueprint
from flask import jsonify
from flask import request

from sklearn.naive_bayes import GaussianNB
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.model_selection import train_test_split
from sklearn.model_selection import cross_val_score

from nltk.corpus import stopwords
from nltk.stem.porter import PorterStemmer

from multiprocessing import Pool

import pickle
import re
import pandas
import threading

sentiment_api = Blueprint('sentiment_api', __name__)

vectorizer = pickle.load(open('sentiment.vec', 'rb'))
classifier = pickle.load(open('sentiment.mdl', 'rb'))

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

    testSetSize = float(data['TestSetSize']) if 'TestSetSize' in data else 0.20

    pool = Pool(processes=16)

    print('Cleaning the data')
    cleanData = pool.map(clean, data['TrainingData'])

    finalData = []
    results = []
    for item in cleanData:
        finalData.append(item[0])
        results.append(item[1])

    print('Vectorizing the data')
    # Creating the Bag of Words model
    vectorizer = CountVectorizer(max_features = 1500)
    X = vectorizer.fit_transform(finalData).toarray()
    y = results

    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size = testSetSize)
    
    # Freeing up some memory
    X = None
    y = None
    X_test = None
    y_test = None
    cleanText = None
    results = None

    print('Fitting the model.')
    # Fitting classifier to the Training set
    classifier = GaussianNB()
    classifier.fit(X_train, y_train)

    print('Getting accuracy.')
    # Predicting the Test set results so we can get the accuracy
    accuracies = cross_val_score(estimator = classifier, X = X_train, y = y_train, cv = 10)
    avgAccuracy = accuracies.mean()
    accuracyStdDeviation = accuracies.std()    

    print('Saving results.')
    # Save the classifier and vectorizer
    pickle.dump(vectorizer, open('sentiment.vec', 'wb'))
    pickle.dump(classifier, open('sentiment.mdl', 'wb'))

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
        return jsonify({"Message":"Missing required array: InputText"}), 400

    if is_training == True:
        return jsonify({"Message":"Training already in progress."})

    trainingThread = threading.Thread(target=train, args=(json,))
    trainingThread.start()

    return jsonify({ "Message": "Training Started." })

@sentiment_api.route('/isTraining', methods=['GET'])
def is_sentiment_training():
    return jsonify(is_training)