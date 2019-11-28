import axios from 'axios'

const mlService = {
    async test (route, token, inputData) {
        try {
            var response = await axios({
                method: 'GET',
                headers: {
                    Authorization: 'Bearer ' + token
                },
                url: route + '/predict',
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
    async train(route, token, trainingSet) {
        console.log(route)
        console.log(token)
        console.log(trainingSet)

        return { status: 200 }
    }
}

export default mlService