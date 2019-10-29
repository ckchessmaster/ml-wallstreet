const axios = require('axios')

module.exports = async function(req, res, next) {
    const token = req.headers["authorization"]

    if(!token) {
        return res.status(401).send("Access denied. No token provided.")
    }

    

    next()
}