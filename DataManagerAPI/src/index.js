// TODO: Fix this!!!!
process.env['NODE_TLS_REJECT_UNAUTHORIZED'] = '0'

const config = require('./config')
const logger = require('./util/logger')

const express = require('express')
const app = express()

var routes = require('./routes')
app.use('/', routes)

app.listen(config.port, () => logger.log(`DataManager is listening on port ${config.port}!`, logger.LogLevel.INFO))
