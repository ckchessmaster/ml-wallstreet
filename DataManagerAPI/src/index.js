const config = require('./config')
const express = require('express')
const app = express()

var routes = require('./routes')
app.use('/', routes)

app.listen(config.port, () => console.log(`DataManager is listening on port ${config.port}!`))
