<template>
  <v-app id="mlwallstreet">
    <Login v-show="!isLoggedIn" />
    <div v-show="isLoggedIn">
      <v-navigation-drawer v-model="drawer" app clipped>
        <v-list dense>
          <v-list-item link>
            <v-list-item-action>
              <v-icon>mdi-view-dashboard</v-icon>
            </v-list-item-action>
            <v-list-item-content v-on:click="currentDashboard = 'status'">
              <v-list-item-title>Dashboard</v-list-item-title>
            </v-list-item-content>
          </v-list-item>
          <v-list-item link>
            <v-list-item-action >
              <v-icon>mdi-settings</v-icon>
            </v-list-item-action>
            <v-list-item-content v-on:click="currentDashboard = 'admin'">
              <v-list-item-title>Admin</v-list-item-title>
            </v-list-item-content>
          </v-list-item>
        </v-list>
      </v-navigation-drawer>

      <v-app-bar
        app
        clipped-left
      >
        <v-app-bar-nav-icon @click.stop="drawer = !drawer" />
        <v-toolbar-title>ML-Wallstreet</v-toolbar-title>
      </v-app-bar>

      <v-content>
        <v-container class="fill-height" fluid>
          <StatusDashboard v-if="currentDashboard === 'status'"></StatusDashboard>
          <AdminDashboard v-if="currentDashboard === 'admin'"></AdminDashboard>
        </v-container>
      </v-content>
    </div>
    <v-footer app>
      <span>Christopher Kingdon &copy; 2019</span>
    </v-footer>
  </v-app>
</template>

<script>
import { mapGetters } from 'vuex'
import Login from './components/Login'
import StatusDashboard from './components/dashboards/StatusDashboard'
import AdminDashboard from './components/dashboards/AdminDashboard'

// import Config from './components/Config'

export default {
  name: 'MLWallstreet',
  components: {
    Login,
    StatusDashboard,
    AdminDashboard
  },
  props: {
    source: String,
  },
  data: () => ({
    drawer: null,
    currentDashboard: 'admin'
  }),
  computed: {
      ...mapGetters([
        'isLoggedIn',
        'username'
      ])
  },
  created() {
    this.$vuetify.theme.dark = true
  }
}
</script>

<style>
.error-message {
  color: var(--v-error-base);
}
</style>
