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
