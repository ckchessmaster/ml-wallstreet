import axios from 'axios'
import config from '../config'

const loginService = {
     async login (username, password) {
        
        try {
            var response = await axios({
                method: 'post',
                url: config.security_service_base_url + 'auth/login',
                data: {
                    username: username,
                    password: password
                }
            });

            console.log(response)
        }
        catch(e) {
            if(e.response) {
                return { status: e.response.status }
            }
            
            console.log(e)
            return { status: 500 }
        }

        return true
    }
}

export default loginService