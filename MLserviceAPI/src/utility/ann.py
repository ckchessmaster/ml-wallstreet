import os

from tensorflow.keras.models import Sequential, load_model, save_model

import services.logger as logger

class NeuralNetwork:
    def __init__(self, ann_type='NONE', epochs=10, batch_size=100):
        # self.session = tf.compat.v1.Session()
        # self.graph = tf.compat.v1.get_default_graph()
        self.epochs = epochs
        self.batch_size = batch_size
        self.model = Sequential()

        # disable tensorflow warnings
        # os.environ['TF_CPP_MIN_LOG_LEVEL'] = '2' 
        #         
        # with self.graph.as_default():
        #     with self.session.as_default():
        #         if ann_type == 'SEQUENTIAL':
        #             self.model = Sequential()

        #         logger.log('ANN initialised.')
        #     # end with
        # # end with
    #end __init__()

    def load(self, file_path):
        try:
            self.model = load_model(file_path + '.h5')
            logger.log('Model loaded.')
            self.model.summary()
            return True
        except Exception as e:
            logger.log_error(e)
            return False
        # end try/except
        # with self.graph.as_default():
        #     with self.session.as_default():
        #         try:
        #             self.model = load_model(file_path)
        #             logger.log('ANN loaded.')
        #             return True
        #         except Exception as e:
        #             logger.log_error(e)
        #             return False
        #         # end try/except
        #     # end with
        # # end with
    # end load()

    def save(self, file_path):
        try:
            self.model.save(file_path + '.h5')
            logger.log('Model saved.')
            return True
        except Exception as e:
            logger.log_error(e)
            return False
        # end try/except
        # with self.graph.as_default():
        #     with self.session.as_default():
        #         try:
        #             self.model.save(file_path)
        #             logger.log('ANN saved.')
        #             return True
        #         except Exception as e:
        #             logger.log_error(e)
        #             return False
        #         # end try/except
        #     # end with
        # # end with
    # end save()

    def add(self, layer):
        self.model.add(layer)
        #  with self.graph.as_default():
        #     with self.session.as_default():
        #         self.model.add(layer)
    # end add

    def compile(self, optimizer, loss, metrics):
        self.model.compile(optimizer, loss, metrics)
        #  with self.graph.as_default():
        #     with self.session.as_default():
        #         self.model.compile(optimizer, loss, metrics)
    # end compile

    def fit(self, X, y):
        self.model.fit(X, y, batch_size=self.batch_size, epochs=self.epochs)
        # with self.graph.as_default():
        #     with self.session.as_default():
        #         self.model.fit(X, y, batch_size=self.batch_size, epochs=self.epochs)
    # end fit

    def predict(self, X):
        # with self.graph.as_default():
        #     with self.session.as_default():
        #         y = self.model.predict(X)

        y = self.model.predict(X)
        return y
    # end predict()

    def evaluate(self, X, y):
        # with self.graph.as_default():
        #     with self.session.as_default():
        #         _, acc = self.model.evaluate(X, y)

        _, acc = self.model.evaluate(X, y)
        return acc
    # end evaluate
# end ANN

