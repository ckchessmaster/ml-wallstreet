<template>
     <v-container>
         <v-card class="model-handler">
            <v-row>
                <v-col cols="9" class="model-input">
                    <h2>{{title}}</h2>
                    <v-row>
                        <v-col cols="6"><v-file-input label="Training Set" accept=".csv"></v-file-input></v-col>
                        <v-col cols="2"><v-btn color="primary">Train</v-btn></v-col>
                    </v-row>
                    <v-row>
                        <v-col cols="6">
                            <v-text-field 
                                label="Input Text"
                                name="inputText"
                                prepend-icon="assessment"/>
                        </v-col>
                        <v-col cols="2"><v-btn color="secondary">Test</v-btn></v-col>
                    </v-row>
                </v-col>
                <v-col cols="3" class="model-result">
                    <h3>Results</h3>
                    <div>Accuracy: {{accuracy * 100}}%</div>
                    <div>Standard Deviation: {{stdDev * 100}}%</div>
                </v-col>
            </v-row>
         </v-card>
     </v-container>
</template>

<script>
export default {
    name: 'ModelHandler',
    props: {
        title: String,
        baseRoute: String
    },
    data: () => ({
        accuracy: 0.5,
        stdDev: 0.2
    })
}
</script>

<style scoped>
.model-handler {
    padding: 10px;
}

.model-result {
    border-left: solid 2px;
}
</style>