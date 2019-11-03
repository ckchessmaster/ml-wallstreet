<template>
  <v-app id="mlwallstreet">
    <v-content>
      <Login />
    </v-content>
  </v-app>
</template>

<script>
import Login from './components/Login';

export default {
  name: 'MLWallstreet',
  components: {
    Login,
  },
  data: () => ({}),
  created() {
    this.$vuetify.theme.dark = true
  }
}
</script>
