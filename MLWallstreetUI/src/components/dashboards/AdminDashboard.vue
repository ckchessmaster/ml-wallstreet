<template>
     <v-container class="fill-height" fluid>
         <v-row align="center" justify="center">
             <v-col cols="12">
                <ModelHandler 
                    title="Sentiment Analysis Model" 
                    :baseRoute="baseRoute" 
                    modelType="SENTIMENT" 
                />
                <ModelHandler 
                    title="Category Analysis Model" 
                    :baseRoute="baseRoute" 
                    modelType="CATEGORY" 
                />
                <ModelHandler 
                    title="Stock Analysis Model" 
                    :baseRoute="baseRoute" 
                    modelType="STOCK" 
                />
                <ModelHandler 
                    title="Stock Analysis Model V2" 
                    :baseRoute="baseRoute" 
                    modelType="STOCKV2" 
                />
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
