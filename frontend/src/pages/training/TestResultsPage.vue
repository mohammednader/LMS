<template>
  <v-container>
    <div class="text-h5 font-weight-bold text-primary mb-4">Test Results</div>
    <v-progress-linear v-if="store.loading" indeterminate color="primary" />

    <v-data-table :headers="headers" :items="store.testResults">
      <template #item.score="{ item }">
        <v-chip :color="item.isPassed ? 'success' : 'error'" size="small">
          {{ item.score?.toFixed(1) }}%
        </v-chip>
      </template>
      <template #item.examDate="{ item }">
        {{ new Date(item.examDate).toLocaleDateString() }}
      </template>
      <template #item.certificate="{ item }">
        <v-btn v-if="item.isPassed" icon size="small" variant="text"
          :href="store.getCertificateUrl(item.id)" target="_blank">
          <v-icon color="primary">mdi-certificate</v-icon>
        </v-btn>
      </template>
    </v-data-table>
  </v-container>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useTrainingStore } from '@/stores/training'

const route = useRoute()
const store = useTrainingStore()
const headers = [
  { title: 'User', key: 'userId' },
  { title: 'Date', key: 'examDate' },
  { title: 'Score', key: 'score' },
  { title: 'Status', key: 'isPassed' },
  { title: 'Certificate', key: 'certificate', sortable: false }
]
onMounted(() => store.getTestResults(Number(route.params.testId)))
</script>
