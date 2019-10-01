from flask import Blueprint
from flask import jsonify
from flask import request

from sklearn.naive_bayes import GaussianNB
from sklearn.feature_extraction.text import CountVectorizer

from nltk.corpus import stopwords
from nltk.stem.porter import PorterStemmer

import pickle
import re

sentiment_api = Blueprint('sentiment_api', __name__)

@sentiment_api.route('/predict', methods=['GET'])
def predict_single():
    inputText = request.args.get('inputText')
    
    if inputText is None:
        return jsonify({"Message":"Missing required query parameter: InputText"}), 400

    # Clean the input text
    cleanText = re.sub('[^a-zA-Z]', ' ', inputText) # Replace all non letters with spaces
    cleanText = cleanText.lower() # Set the entire text to lower case
    cleanText = cleanText.split() # Split the text into it's individual words

    # Remove words that don't contribute anything (like the, a, this, etc.)
    # Also apply stemming aka getting the root of words
    ps = PorterStemmer()
    cleanText = [ps.stem(word) for word in cleanText if not word in set(stopwords.words('english'))]
    cleanText = ' '.join(cleanText) # Put the string back together

    # Predict
    vectorizer = pickle.load(open('sentiment.vec', 'rb'))
    textFinal = vectorizer.transform([cleanText]).toarray()
    classifier = pickle.load(open('sentiment.mdl', 'rb'))
    result = classifier.predict(textFinal).tolist()
    finalResult = False

    if result[0] == 1:
        finalResult = True

    return jsonify({ "Result": finalResult })
