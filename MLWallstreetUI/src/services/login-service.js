import axios from 'axios'
import config from '../config'
import jsonwebtoken from 'jsonwebtoken'

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
                token: response.data.token,
                refreshToken: response.data.refreshToken
            }
        } catch(e) {
            if(e.response) {
                return { status: e.response.status }
            }
            
            //console.log(e)
            return { status: 500 }
        }
    },
    // Validate the current token
    // If it's invalid then try and refresh the token
    // Otherwise logout
    async tryValidateCurrentToken() {
        var currentToken = sessionStorage.getItem('token')

        if (!currentToken) {
            // Logout
            return false
        }

        var expiration = jsonwebtoken.decode(currentToken).exp
        
        if (Date.now() < expiration * 1000) {
            return true
        }

        var currentRefreshToken = sessionStorage.getItem('refreshToken')

        if (!currentRefreshToken) {
            // Logout
            return false
        }

        expiration = jsonwebtoken.decode(currentRefreshToken).exp

        if (Date.now() < expiration * 1000) {
            // Get new tokens
            try {
                var response = await axios({
                    method: 'POST',
                    url: config.security_service_base_url + 'auth/refreshToken',
                    data: {
                        token: currentRefreshToken
                    }
                })

                sessionStorage.setItem('token', response.data.token)
                sessionStorage.setItem('refreshToken', response.data.refreshToken)
    
                return true
            } catch(e) {
                if(e.response) {
                    //console.log(e.response)
                    return { status: e.response.status }
                }
                
                // Logout
                //console.log(e)
                return false
            }
        }

        // Logout
        return false
    }
}

export default loginService