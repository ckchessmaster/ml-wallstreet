import axios from 'axios'
import config from '../config'

const loginService = {
     async login (username, password) {
        try {
            var response = await axios({
                method: 'POST',
                url: config.security_service_base_url + 'auth/login',
                data: {
                    username: username,
                    password: password
                }
            })

            return {
                status: response.status,
                token: response.data.token
            }
        }
        catch(e) {
            if(e.response) {
                return { status: e.response.status }
            }
            
            console.log(e)
            return { status: 500 }
        }
    }
}

export default loginService