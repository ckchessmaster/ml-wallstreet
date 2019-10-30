module.exports.LogLevel = {
    INFO: 0,
    ERROR: 1,
    EVENT: 2
}

module.exports.log = function(message, logLevel) {
    switch (logLevel) {
        case this.LogLevel.INFO:
            console.log(message)
            break
        case this.LogLevel.ERROR:
            console.error(message)
        case this.LogLevel.EVENT:
            console.log(message)
    }
}