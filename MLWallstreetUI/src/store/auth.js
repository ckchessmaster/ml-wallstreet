const auth = {
    state: {
        isLoggedIn: !!localStorage.getItem("token")
    },
    mutations: {
        LOGIN (state) {
            state.isLoggedIn = true
            state.pending = false
        },
        LOGOUT (state) {
            state.isLoggedIn = false
        }
    },
    actions: {
        login({ commit }, token, username) {
            localStorage.setItem("token", token);
            localStorage.setItem("username", username)
            commit('LOGIN');
        },
        logout({ commit }) {
            localStorage.removeItem("token");
            localStorage.removeItem("username")
            commit('LOGOUT');
        }
    },
    getters: {
        isLoggedIn: state => {
            return state.isLoggedIn
        }
    }
}

export default auth