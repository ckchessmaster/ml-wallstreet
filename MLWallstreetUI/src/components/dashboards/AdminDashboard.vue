<template>
     <v-container class="fill-height" fluid>
         <v-row align="center" justify="center">
             <v-col cols="12">
                <ModelHandler title="Sentiment Analysis Model" :baseRoute="baseRoute + 'sentiment'"></ModelHandler>
                <ModelHandler title="test" :baseRoute="baseRoute + ''"></ModelHandler>
                <ModelHandler title="test" :baseRoute="baseRoute + ''"></ModelHandler>
             </v-col>
         </v-row>
     </v-container>
</template>

<script>
import ModelHandler from './parts/ModelHandler'
import config from '../../config'

export default {
    name: 'AdminDashboard',
    components: {
        ModelHandler
    },
    data: () => ({
        baseRoute: config.ml_service_base_url
    })
}
</script>
