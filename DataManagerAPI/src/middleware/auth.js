const config = require('../config')
const axios = require('axios')
const logger = require('../util/logger')

module.exports = async function(req, res, next) {
    const token = req.headers["authorization"]

    if(!token) {
        logger.log(req, logger.LogLevel.EVENT)
        return res.status(401).send("Access denied. No token provided.")
    }

    var url = config.security.securityServiceUrl + 'auth/validateToken'

    try {
        var response = await axios.post(url, {token: token})

        if (response.data.result == true) {
            next()
        } else {
            logger.log(req, logger.LogLevel.EVENT)
            return res.status(401).send({message: "Access denied. Invalid token."})
        }

    } catch (error) {
        logger.log(error, logger.LogLevel.ERROR)
        return res.status(500).send({message: "Error processing request."})
    }
}