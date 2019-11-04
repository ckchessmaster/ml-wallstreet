<template>
    <v-container class="fill-height" fluid>
        <v-row align="center" justify="center">
            <v-col cols="12" sm="8" md="4">
                <v-card class="elevation-12">
                    <v-toolbar color="primary" dark flat>
                        <v-toolbar-title>Login</v-toolbar-title>
                    </v-toolbar>
                    <v-card-text>
                        <v-form>
                            <v-text-field label="Login" name="login" prepend-icon="person" type="text" v-model="input.username" />
                            <v-text-field id="password" label="Password" name="password" prepend-icon="lock" type="password" v-model="input.password" />
                        </v-form>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn color="primary" v-on:click="login()">Login</v-btn>
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
          input: {
              username: "",
              password: ""
          }
      }
  },
  methods: {
      async login() {
          if (this.input.username != "" && this.input.password != "") {
              var loginResult = await loginService.login(this.input.username, this.input.password)

              if (loginResult == true) {
                  console.log('logged in!!!')
              }
          }
      }
  }
}
</script>