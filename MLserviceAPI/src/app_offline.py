if __name__ == '__main__':
    import sys
    import os
    os.chdir(os.path.dirname(__file__) + '/..')
    # os.environ["KERAS_BACKEND"] = "plaidml.keras.backend"
    # os.environ["PLAIDML_USE_STRIPE"] = "1"

    import csv
    maxInt = sys.maxsize

    while True:
        # decrease the maxInt value by factor 10 
        # as long as the OverflowError occurs.

        try:
            csv.field_size_limit(maxInt)
            break
        except OverflowError:
            maxInt = int(maxInt/10)
        # end try/catch
    # end while

    import services.stock_service_v2 as stock_service_v2

    # Vars
    dataset_path = 'C:\\Users\\ckche\Desktop\\ML Stock Project\\datasets\\StockTraining\\all-the-news\\all-the-news.csv'
    dataset_id = None

    # MAIN ------------------------------------------------------------------------------------------------------
    if dataset_id is not None:
        stock_service_v2.train_clean(dataset_id)
    else:
        # load the data into a dict
        dataset = type('',(object,),{})()

        dataset.data = []
        with open(dataset_path, encoding='utf-8') as infile:
            reader = csv.DictReader(infile)
            for row in reader:
                dataset.data.append(row)
        # end with
        
        # Remove row 0


        stock_service_v2.train_dirty(dataset)
    #endif