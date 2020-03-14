import nltk
nltk.download('stopwords')

import re

from multiprocessing import Pool

import services.logger as logger
import config

from nltk.stem.porter import PorterStemmer
from nltk.corpus import stopwords

def clean_text_single(data):
    text = data['Body']
    value = 1 if float(data['price_diff']) > 0 else 0

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
    # Clean the text
    if len(data) > config.CLEANER_SINGLE_THREAD_CUTOFF:
        logger.log('Cleaning text.')
        pool = Pool(processes=8)
        clean_data = pool.map(clean_text_single, data)
        logger.log('Cleaning complete.')
    else:
        logger.log('Cleaning text.')
        clean_data = map(clean_text_single, data)
        logger.log('Cleaning complete.')
    #endif
    
    return list(clean_data)
# clean()