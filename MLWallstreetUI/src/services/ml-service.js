import axios from 'axios'

const mlService = {
    async test (baseRoute, token, inputData) {
        try {
            let response = await axios({
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
        let body = {
            name: trainingSetInfo.name,
            data: trainingSet
        }

        try {
            let response = await axios({
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
    async trainExisting(baseRoute, token, trainingSetId) {
        try {
            let response = await axios({
                method: 'POST',
                headers: {
                    Authorization: 'Bearer ' + token
                },
                url: baseRoute + '/train/' + trainingSetId
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
    async getDataSets(baseRoute, token) {
        try {
            let response = await axios({
                method: 'GET',
                headers: {
                    Authorization: 'Bearer ' + token
                },
                url: baseRoute + '/datasets'
            })

            let datasets = response.data.datasets
            datasets.unshift({
                name: 'None',
                _id: '0'
            })

            return datasets
        } catch (e) {
            console.error(e)
            return { status: 500 }
        }
    }
}

export default mlService