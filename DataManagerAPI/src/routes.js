const express = require('express')
const router = express.Router()
const auth = require('./middleware/auth')

router.get('/health', (req, res) => res.send({Healthy:true}))
router.get('/hello', auth, (req, res) => res.send("hello world!"))


module.exports = router