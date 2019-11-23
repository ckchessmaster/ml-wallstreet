<template>
    <v-container class="fill-height" fluid>
        <v-row align="center" justify="center">
            <v-col cols="12" sm="8" md="4">
                <v-card class="elevation-12" :loading="loading">
                    <v-toolbar color="primary" dark flat>
                        <v-toolbar-title>Login</v-toolbar-title>
                    </v-toolbar>
                    <v-card-text>
                        <v-form v-model="valid" lazy-validation>
                            <v-text-field
                                label="Username" 
                                name="username" 
                                prepend-icon="person" 
                                type="text" 
                                v-model="username" 
                                :rules="usernameRules"
                                required />
                            <v-text-field 
                                id="password" 
                                label="Password" 
                                name="password" 
                                prepend-icon="lock" 
                                type="password" 
                                v-model="password" 
                                :rules="passwordRules"
                                required />
                        </v-form>
                        <div v-show="invalidLoginCredentials" class="error-message">Invalid username or password.</div>
                        <div v-show="internalServerError" class="error-message">Something went wrong. Please try again later.</div>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn :disabled="!valid" color="primary" @click="login">Login</v-btn>
                    </v-card-actions>
                </v-card>
            </v-col>
        </v-row>
    </v-container>
</template>

<script>
import loginService from '../services/login-service'
import { mapActions } from 'vuex'

export default {
  name: 'Login',
  data() {
      return {
          valid: true,
          loading: false,
          invalidLoginCredentials: false,
          internalServerError: false,
          username: "",
          usernameRules: [ v => !!v || 'Username is required.'],
          password: "",
          passwordRules: [v => !!v || 'Password is required.']
      }
  },
  methods: {
      ...mapActions({
          saveLogin: 'login'
      }),
      async login() {
          if (this.username != "" && this.password != "") {
              this.loading = true

              this.invalidLoginCredentials = false
              this.internalServerError = false
              
              var loginResult = await loginService.login(this.username, this.password)
              
              this.loading = false

              if (loginResult.status >= 200 && loginResult.status < 300) {
                  this.saveLogin(loginResult.token)
              } else if (loginResult.status >= 400 && loginResult.status < 500) {
                  this.invalidLoginCredentials = true
              } else if (loginResult.status >= 500) {
                  this.internalServerError = true
              }
          }
      }
  }
}
</script>

<style>

</style>