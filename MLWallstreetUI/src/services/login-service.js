import axios from 'axios'
import config from '../config'

var loginService = {
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
        } catch(error) {
            console.log(error)
        }

        return true
    }
}

export default loginService