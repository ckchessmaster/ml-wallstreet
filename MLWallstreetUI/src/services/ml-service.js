import axios from 'axios'

const mlService = {
    async test (baseRoute, token, inputData) {
        try {
            var response = await axios({
                method: 'GET',
                headers: {
                    Authorization: 'Bearer ' + token
                },
                url: baseRoute + '/predict',
                params: {
                    inputText: inputData
                }
            })

            return {
                status: 200,
                result: response.data.Result
            }
        } catch (e) {
            console.error(e)
            return { status: 500 }
        }
    },
    async trainNew(baseRoute, token, trainingSet, trainingSetInfo) {
        console.log(trainingSet)

        let body = {
            name: trainingSetInfo.name,
            data: trainingSet
        }

        try {
            var response = await axios({
                method: 'POST',
                headers: {
                    Authorization: 'Bearer ' + token
                },
                url: baseRoute + '/train',
                data: body
            })

            return {
                status: 200,
                result: response.data
            }
        } catch (e) {
            console.error(e)
            return { status: 500 }
        }
    },
    async trainExisting(baseRoute, token, trainingSet) {
        console.log(baseRoute)
        console.log(token)
        console.log(trainingSet)

        return { status: 200 }
    },
    async getDataSets(baseRoute, token) {
        console.log(baseRoute)
        console.log(token)

        return [
            {
                text: "None",
                value: "0"
            },
            {
                text: "Test01",
                value: "123456"
            },
            {
                text: "Test02",
                value: "456789"
            }
        ]
    }
}

export default mlService