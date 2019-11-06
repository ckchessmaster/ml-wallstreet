<template>
    <v-container class="fill-height" fluid>
        <v-row align="center" justify="center">
            <v-col cols="12" sm="8" md="4">
                <v-card class="elevation-12">
                    <v-toolbar color="primary" dark flat>
                        <v-toolbar-title>Login</v-toolbar-title>
                    </v-toolbar>
                    <v-card-text>
                        <v-form v-model="valid" lazy-validation>
                            <v-text-field
                                label="Login" 
                                name="login" 
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

export default {
  name: 'Login',
  data() {
      return {
          valid: true,
          username: "",
          usernameRules: [ v => !!v || 'Username is required.'],
          password: "",
          passwordRules: [v => !!v || 'Password is required.']
      }
  },
  methods: {
      async login() {
          if (this.input.username != "" && this.input.password != "") {
              var loginResult = await loginService.login(this.input.username, this.input.password)

              if (loginResult.status >= 200 && loginResult.status < 300) {
                  console.log('Success!')
              } else if (loginResult.status >= 400 && loginResult.status < 500) {
                  console.log('Invalid Username or Password!')
              } else if (loginResult.status >= 500) {
                  console.log('Internal server error!')
              }
          }
      }
  }
}
</script>