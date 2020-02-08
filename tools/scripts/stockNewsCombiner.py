import pandas as pd
import os
import re

news_data_location = '../datasets/StockTraining/NewsData_Final.csv'
stock_data_location = '../datasets/StockTraining/HistoricalQuotes-Googl.csv'

def map_stock_price(news_row, stock_data):
    date_regex = '(\d+\/\d+\/\d+)'

    date_published = re.search(date_regex, news_row['DatePublished']).group(1)
    stock_rows = stock_data.loc[stock_data['Date'] == date_published]

    if stock_rows.empty == False:
        money_regex = '(-{0,1}\d+.\d+)'
        open_price = float(re.search(money_regex, stock_rows.iloc[0][3]).group(1))
        close_price = float(re.search(money_regex, stock_rows.iloc[0][1]).group(1))

        stock_diff = open_price - close_price

        return stock_diff
    else:
        return 'NaN' 

# Main

news_data = pd.read_csv(news_data_location, encoding='windows-1252')
stock_data = pd.read_csv(stock_data_location, encoding='windows-1252')

# Drop duplicates
news_data.drop_duplicates(subset='Url', keep='first', inplace=True)

# Map stock prices
news_data['price_diff'] = news_data.apply(map_stock_price, axis=1, args=(stock_data,))

# Remove articles without a stock price
news_data = news_data[news_data['price_diff'] != 'NaN']

news_data.to_csv('final.csv')