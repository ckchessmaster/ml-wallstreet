<template>
     <v-container>
         <v-card class="model-handler">
            <v-row>
                <v-col cols="9" class="model-input">
                    <h2>{{title}}</h2>
                    <v-row align="center">
                        <v-col cols="4">
                            <v-file-input
                                label="Upload a new dataset"
                                accept=".csv,application/vnd.ms-excel"
                                v-model="trainingSetInfo"
                                @change="onFileChange"
                                :loading="isFileUploading"/>
                        </v-col>
                        <v-col cols="1">
                            <div class="dataset-seperator">Or</div>
                        </v-col>
                        <v-col cols="4">
                            <v-select 
                                label="Select an existing dataset" 
                                :items="availableDataSets"
                                item-text="name"
                                item-value="_id"
                                v-model="selectedTrainingSet" />
                        </v-col>
                        <v-col cols="2">
                            <v-btn color="primary" @click="train" :disabled="!trainingReady">Train</v-btn>
                        </v-col>
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
import utilityService from '../../../services/utility-service'
import { mapGetters } from 'vuex'

export default {
    name: 'ModelHandler',
    props: {
        title: String,
        baseRoute: String
    },
    data: () => ({
        // Results
        accuracy: 0.5,
        stdDev: 0.2,
        testResult: '',
        // Training Info
        isFileUploading: false,
        trainingSetInfo: null,
        trainingSet: null,
        availableDataSets: [],
        selectedTrainingSet: null,
        // Other
        testData: ''
    }),
    computed: {
      ...mapGetters([
        'token'
      ]),
      trainingReady() {
          return this.trainingSet !== null || (this.selectedTrainingSet !== null && this.selectedTrainingSet != 0)
      }
    },
    methods: {
        async test() {
            if (this.testData !== '') {
                let result = await mlService.test(this.baseRoute, this.token, this.testData)
                
                if (result.status === 200) {
                    this.testResult = result.result == 1 ? 'True' : 'False'
                }
            }
        },
        async onFileChange() {
            if (this.trainingSetInfo) {
                this.isFileUploading = true

                let reader = new FileReader()
                reader.onload = (event) => {
                    this.trainingSet = utilityService.csvToJson(event.target.result, ['value', 'text'])
                    this.isFileUploading = false
                }
                reader.readAsText(this.trainingSetInfo)
            } else {
                this.trainingSet = null
            }
        },
        async train() {
            if (this.trainingReady === true) {
                let result = {}

                if (this.selectedTrainingSet !== null && this.selectedTrainingSet != 0) {
                    result = await mlService.trainExisting(this.baseRoute, this.token, this.selectedTrainingSet)
                } else {
                    result = await mlService.trainNew(this.baseRoute, this.token, this.trainingSet, this.trainingSetInfo)
                }
                
                console.log(result)
            }
        }
    },
    async created() {
        // Load the existing datasets
        this.availableDataSets = await mlService.getDataSets(this.baseRoute, this.token)
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

.dataset-seperator {
    text-align: center;
    vertical-align: middle;
}
</style>