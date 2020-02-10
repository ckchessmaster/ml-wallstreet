import jwt_decode from 'jwt-decode'

const auth = {
    state: {
        isLoggedIn: !!sessionStorage.getItem('token'),
    },
    mutations: {
        SET_USERNAME (state, username) {
            state.username = username
        },
        LOGIN (state) {
            state.isLoggedIn = true
            state.pending = false
        },
        LOGOUT (state) {
            state.isLoggedIn = false
        }
    },
    actions: {
        login({ commit }, tokens) {
            sessionStorage.setItem('token', tokens.token)
            sessionStorage.setItem('refreshToken', tokens.refreshToken)
            commit('LOGIN');
        },
        logout({ commit }) {
            sessionStorage.removeItem('token');
            commit('LOGOUT');
        }
    },
    getters: {
        isLoggedIn: state => {
            return state.isLoggedIn
        },
        username: () => {
            var token = sessionStorage.getItem('token')

            if (!token) return ''

            var decoded_token = jwt_decode(token)

            return decoded_token.username
        },
        token: () => {
            return sessionStorage.getItem('token')
        },
        refreshToken: () => {
            return sessionStorage.getItem('refreshToken')
        }
    }
}

export default auth