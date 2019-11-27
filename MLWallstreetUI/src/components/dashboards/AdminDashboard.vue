<template>
     <v-container class="fill-height" fluid>
         <v-row align="center" justify="center">
             <v-col cols="12">
                <ModelHandler title="Sentiment Analysis Model" baseRoute="test"></ModelHandler>
                <ModelHandler title="test" baseRoute="test"></ModelHandler>
                <ModelHandler title="test" baseRoute="test"></ModelHandler>
             </v-col>
         </v-row>
     </v-container>
</template>

<script>
import ModelHandler from './parts/ModelHandler'

export default {
    name: 'AdminDashboard',
    components: {
        ModelHandler
    }
}
</script>
