const utilityService = {
    csvToJson (csv, expectedHeaders=null) {
        let lines = csv.split('\r\n')
        let result = []
        let headers = expectedHeaders === null ? lines[0].split(',') : expectedHeaders

        // Skip the header row
        for (let i=1; i<lines.length; i++) {
            let obj = {}
            let currentLine = lines[i].split(',')

            // Make sure the data is valid
            if (currentLine == "") {
                continue
            }

            for (let j=0; j<headers.length; j++) {
                obj[headers[j]] = currentLine[j]
            }

            result.push(obj)
        }

        return result
    }
}

export default utilityService