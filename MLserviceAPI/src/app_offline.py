if __name__ == '__main__':
    import os
    os.chdir(os.path.dirname(__file__) + '/..')
    # os.environ["KERAS_BACKEND"] = "plaidml.keras.backend"
    # os.environ["PLAIDML_USE_STRIPE"] = "1"

    import sys
    maxInt = sys.maxsize

    import csv
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

    from uuid import uuid4
    import services.stock_service_v2 as stock_service_v2
    import services.data_service as data_service

    # Vars
    dataset_path = 'C:\\Users\\ckche\Desktop\\ML Stock Project\\datasets\\StockTraining\\final.csv'
    dataset_id = 'fa1ba012-f6cf-4250-970b-9ba6db91e7ce'

    # MAIN ------------------------------------------------------------------------------------------------------
    if dataset_id is not None:
        stock_service_v2.train_clean(dataset_id)
    else:
        # load the dataset
        dataset_info = {
            "_id": str(uuid4()),
            "name": 'msft-context-web',
            "model_type": 'STOCKV2'
        }

        data = []
        with open(dataset_path, encoding='utf-8') as infile:
            reader = csv.DictReader(infile)
            for row in reader:
                data.append(row)
        # end with

        dataset = data_service.Dataset(dataset_info, data)
        
        # Train
        stock_service_v2.train_dirty(dataset)
    #endif
#endif