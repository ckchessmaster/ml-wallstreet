<template>
     <v-container class="fill-height" fluid>
         <v-row align="center" justify="center">
             <v-col cols="12" sm="8" md="4">
                 <v-card class="elevation-12">
                     <v-card-text>
                         <div class="center">Welcome {{username}}!</div>
                     </v-card-text>
                 </v-card>
             </v-col>
         </v-row>
     </v-container>
</template>

<script>
import { mapGetters } from 'vuex'

export default {
    name: 'StatusDashboard',
    computed: {
        ...mapGetters([
            'username'
        ])
    }
}
</script>