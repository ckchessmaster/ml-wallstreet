import os
import re
import time
import numpy as np
import pandas as pd

from multiprocessing import Pool
from functools import partial

news_data_location = '../datasets/StockTraining/Microsoft-ContexWeb.csv'
stock_data_location = '../datasets/StockTraining/MSFT.csv'

news_date_string = 'DatePublished'

def get_formatted_date(date):
    # Exctract just the date:
    date_regex = '(\d+\/\d+\/\d+)'
    date = re.search(date_regex, date).group(1)

    # Convert to yyy-mm-dd
    split_date = date.split('/')
    date = split_date[2] + '-' + split_date[0] + '-' + split_date[1]

    return date

def map_stock_price(news_row, stock_data):
    date_published = get_formatted_date(news_row[news_date_string])

    stock_row = stock_data.loc[stock_data['Date'] == date_published]

    if stock_row is not None and stock_row.empty == False:
        money_regex = '(-{0,1}\d+.\d+)'
        open_price = float(stock_row['Open']) #float(re.search(money_regex, stock_rows.iloc[0][3]).group(1))
        close_price = float(stock_row['Close']) #float(re.search(money_regex, stock_rows.iloc[0][1]).group(1))

        stock_diff = open_price - close_price

        return stock_diff
    else:
        return 'NaN' 
# end map_stock_price()

def parallelize(data, func, num_of_processes=8):
    if __name__ == '__main__':    
        data_split = np.array_split(data, num_of_processes)
        pool = Pool(num_of_processes)
        data = pd.concat(pool.map(func, data_split))
        pool.close()
        pool.join()
        return data
# end parallelize

def run_on_subset(func, data_subset, args):
    return data_subset.apply(func, axis=1, args=args)
# end run_on_subset

def parallelize_on_rows(data, func, stock_data, num_of_processes=8):
    return parallelize(data, partial(run_on_subset, func, args=(stock_data,)), num_of_processes)
# end parallelize_on_rows

# Main

news_data = pd.read_csv(news_data_location, encoding='utf-8')
stock_data = pd.read_csv(stock_data_location, encoding='windows-1252')

# Drop duplicates
# news_data.drop_duplicates(subset='Url', keep='first', inplace=True)

# Map stock prices
start = time.time()
news_data['price_diff'] = parallelize_on_rows(news_data, map_stock_price, stock_data) #news_data.apply(map_stock_price, axis=1, args=(stock_data,))
end = time.time()
total_time = end - start

# Remove articles without a stock price
news_data = news_data[news_data['price_diff'] != 'NaN']

news_data.to_csv('final.csv')