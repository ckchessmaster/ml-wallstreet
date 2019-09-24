# -*- coding: utf-8 -*-
"""
Created on Mon Sep 23 21:28:51 2019

@author: ckche
"""

# Testing how to properly use http.client
''' 
import http.client
import appconfig as config
import json
import ssl

url = 'localhost:44373'
port = 44373

con = http.client.HTTPSConnection(url, timeout=10, context = ssl._create_unverified_context())
requestHeaders = {'Content-type': 'application/json'}
requestPayload = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkRhdGFNYW5hZ2VyQWNjZXNzVG9rZW4iLCJuYmYiOjE1NjkyODgwMDksImV4cCI6MTU2OTM3NDQwOSwiaWF0IjoxNTY5Mjg4MDA5LCJpc3MiOiJNTFdhbGxzdHJlZXQiLCJhdWQiOiJNTFdhbGxzdHJlZXQifQ.64Mn1IlIqgGFTT1Flw7uDZ_rHSpXJDm1RfYUX-_CFoc'
json_data = json.dumps(requestPayload)
con.request('POST', '/api/auth/validateToken', json_data, requestHeaders)
#con.request('GET', '/api/health')
result = con.getresponse()
test = result.read().decode()
con.close()
'''