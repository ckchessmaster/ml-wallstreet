from colorama import Fore, Back, Style 

def log(message):
    print(message)

def log_error(message):
    print(Fore.RED)
    print(message)
    print(Style.RESET_ALL)

def log_event(message):
    print(Back.RED + message + Style.RESET_ALL)