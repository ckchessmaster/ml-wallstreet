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
                                prepend-icon="assessment"
                                v-model="testData"/>
                        </v-col>
                        <v-col cols="2"><v-btn color="secondary" @click="test">Test</v-btn></v-col>
                    </v-row>
                </v-col>
                <v-col cols="3" class="model-result">
                    <h3>Results</h3>
                    <div>Accuracy: {{accuracy * 100}}%</div>
                    <div>Standard Deviation: {{stdDev * 100}}%</div>
                    <div v-if="testResult !== ''">Test Result: {{testResult}}</div>
                </v-col>
            </v-row>
         </v-card>
     </v-container>
</template>

<script>
import mlService from '../../../services/ml-service'
import { mapGetters } from 'vuex'

export default {
    name: 'ModelHandler',
    props: {
        title: String,
        baseRoute: String
    },
    data: () => ({
        accuracy: 0.5,
        stdDev: 0.2,
        testData: '',
        trainingSet: '',
        testResult: ''
    }),
    computed: {
      ...mapGetters([
        'token'
      ])
  },
    methods: {
        async test() {
            if (this.testData !== '') {
                var result = await mlService.test(this.baseRoute, this.token, this.testData)
                
                if (result.status === 200) {
                    this.testResult = result.result == 1 ? 'True' : 'False'
                }
            }
        },
        async train() {
            if (this.trainingSet !== '') {
                var result = await mlService.train(this.baseRoute, this.token,this.trainingSet)
                console.log(result)
            }
        }
    }
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