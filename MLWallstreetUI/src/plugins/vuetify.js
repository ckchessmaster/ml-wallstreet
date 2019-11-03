import Vue from 'vue';
import Vuetify from 'vuetify/lib';

Vue.use(Vuetify);

export default new Vuetify({
  icons: {
    iconfont: 'mdi',
  },
  theme: {
    themes: {
      light: {
        primary: '#8bc34a',
        secondary: '#607d8b',
        accent: '#795548',
        error: '#f44336',
        warning: '#ff9800',
        info: '#009688',
        success: '#cddc39'
      },
      dark: {
        primary: '#8bc34a',
        secondary: '#607d8b',
        accent: '#795548',
        error: '#f44336',
        warning: '#ff9800',
        info: '#009688',
        success: '#cddc39'
      }
    }
  }
});
