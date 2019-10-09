from flask import Blueprint
from flask import jsonify
from flask import request

from sklearn.naive_bayes import GaussianNB
from sklearn.feature_extraction.text import CountVectorizer

from nltk.corpus import stopwords
from nltk.stem.porter import PorterStemmer

import pickle
import re
import pandas

sentiment_api = Blueprint('sentiment_api', __name__)

vectorizer = pickle.load(open('sentiment.vec', 'rb'))
classifier = pickle.load(open('sentiment.mdl', 'rb'))

def clean(text):
    cleanText = re.sub('[^a-zA-Z]', ' ', text) # Replace all non letters with spaces
    cleanText = cleanText.lower() # Set the entire text to lower case
    cleanText = cleanText.split() # Split the text into it's individual words

    # Remove words that don't contribute anything (like the, a, this, etc.)
    # Also apply stemming aka getting the root of words
    ps = PorterStemmer()
    cleanText = [ps.stem(word) for word in cleanText if not word in set(stopwords.words('english'))]
    cleanText = ' '.join(cleanText) # Put the string back together

    return cleanText

def predict(text):
    cleanText = list(map(clean, text))

    vectorizedText = vectorizer.transform(cleanText).toarray()
    predictions = list(map(int, classifier.predict(vectorizedText)))

    return predictions

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
