import axios from 'axios'

const mlService = {
    async test (baseRoute, inputData, modelType) {
        try {
            let response = await axios({
                method: 'GET',
                headers: {
                    Authorization: 'Bearer ' + sessionStorage.getItem('token')
                },
                url: baseRoute + 'models/' + modelType + '/predict',
                params: {
                    inputText: inputData
                }
            })

            return {
                status: 200,
                result: response.data.Result
            }
        } catch (e) {
            //console.error(e)
            return { status: 500 }
        }
    },
    async trainNew(baseRoute, trainingSet, trainingSetInfo, modelType) {
        let body = {
            name: trainingSetInfo.name,
            data: trainingSet
        }

        try {
            let response = await axios({
                method: 'POST',
                headers: {
                    Authorization: 'Bearer ' + sessionStorage.getItem('token')
                },
                url: baseRoute + 'models/' + modelType + '/train',
                data: body
            })

            return {
                status: 200,
                result: response.data
            }
        } catch (e) {
            //console.error(e)
            return { status: 500 }
        }
    },
    async trainExisting(baseRoute, trainingSetId, modelType) {
        try {
            let response = await axios({
                method: 'POST',
                headers: {
                    Authorization: 'Bearer ' + sessionStorage.getItem('token')
                },
                url: baseRoute + 'models/' + modelType + '/train/' + trainingSetId
            })

            return {
                status: 200,
                result: response.data
            }
        } catch (e) {
            //console.error(e)
            return { status: 500 }
        }
    },

    async getDataSets(baseRoute, modelType) {
        try {
            let response = await axios({
                method: 'GET',
                headers: {
                    Authorization: 'Bearer ' + sessionStorage.getItem('token')
                },
                url: baseRoute + 'models/' + modelType + '/datasets'
            })

            let datasets = response.data.datasets
            datasets.unshift({
                name: 'None',
                _id: '0'
            })

            return datasets
        } catch (e) {
            //console.error(e)
            return { status: 500 }
        }
    },
    async getCurrentModelInfo(baseRoute, modelType) {
        try {
            let response = await axios({
                method: 'GET',
                headers: {
                    Authorization: 'Bearer ' + sessionStorage.getItem('token')
                },
                url: baseRoute + 'models/' + modelType + '/current'
            })

            let model = response.data

            return model
        } catch (e) {
            //console.error(e)
            return { status: 500 }
        }
    }
}

export default mlService