<template>
  <v-container max-width="820">
    <v-progress-linear v-if="store.loading" indeterminate color="primary" class="mb-4" rounded />

    <!-- ── Result screen ─────────────────────────────────────────────── -->
    <template v-if="submitted && result">
      <v-card rounded="xl" elevation="4" class="text-center pa-8"
        :color="isPassed ? 'success' : 'error'" theme="dark">
        <v-icon size="80" class="mb-4">
          {{ isPassed ? 'mdi-check-decagram' : 'mdi-close-octagon' }}
        </v-icon>
        <div class="text-h3 font-weight-bold mb-2">
          {{ isPassed ? 'Congratulations!' : 'Try Again' }}
        </div>
        <div class="text-h5 mb-1">{{ result.message }}</div>
        <div class="text-h6 opacity-80">{{ result.arMessage }}</div>

        <!-- Score chip -->
        <v-chip size="large" class="mt-6 mb-2" color="white" variant="outlined">
          <v-icon start>mdi-percent</v-icon>
          Score: {{ score }}%
        </v-chip>

        <div class="mt-6 d-flex justify-center gap-4 flex-wrap">
          <v-btn variant="outlined" color="white" to="/courses">
            <v-icon start>mdi-arrow-left</v-icon> Back to Courses
          </v-btn>

          <!-- Download Certificate — only shown when passed -->
          <v-btn v-if="isPassed" color="white" variant="flat"
            :href="`${apiUrl}Test/GenerateCertificate/${result.id}`" target="_blank">
            <v-icon start color="success">mdi-certificate</v-icon>
            <span class="text-success font-weight-bold">Download Certificate</span>
          </v-btn>

          <v-btn v-if="!isPassed" variant="outlined" color="white" @click="retake">
            <v-icon start>mdi-replay</v-icon> Retake Test
          </v-btn>
        </div>
      </v-card>
    </template>

    <!-- ── Test form ──────────────────────────────────────────────────── -->
    <template v-else-if="store.currentTest?.test">
      <!-- Header -->
      <v-card class="mb-6 pa-4 bg-primary text-white" rounded="lg">
        <div class="d-flex align-center">
          <div>
            <div class="text-h5 font-weight-bold">{{ store.currentTest.test.name }}</div>
            <div class="text-body-2 opacity-80 mt-1">
              Pass score: {{ store.currentTest.test.passScore }}% ·
              {{ store.currentTest.questions?.length ?? 0 }} questions
            </div>
          </div>
          <v-spacer />
          <v-chip color="white" variant="tonal">
            {{ answered }} / {{ store.currentTest.questions?.length ?? 0 }} answered
          </v-chip>
        </div>
        <v-progress-linear
          :model-value="(answered / (store.currentTest.questions?.length || 1)) * 100"
          color="white" bg-color="rgba(255,255,255,0.3)" height="4" rounded class="mt-3" />
      </v-card>

      <!-- Questions -->
      <v-form ref="formRef">
        <div v-for="(q, qi) in store.currentTest.questions" :key="q.id" class="mb-4">
          <v-card variant="outlined" rounded="lg" :class="answers[q.id] ? 'border-success' : ''">
            <v-card-text class="pa-4">
              <div class="d-flex align-start mb-3">
                <v-avatar :color="answers[q.id] ? 'success' : 'primary'" size="28" class="mr-3 mt-1 flex-shrink-0">
                  <span class="text-white text-caption font-weight-bold">{{ qi + 1 }}</span>
                </v-avatar>
                <div class="text-subtitle-1 font-weight-medium">{{ q.text }}</div>
              </div>

              <v-radio-group v-model="answers[q.id]" hide-details class="ml-10">
                <v-radio v-for="a in q.answers" :key="a.id"
                  :label="a.text" :value="a.id"
                  color="primary" density="comfortable" class="mb-1" />
              </v-radio-group>
            </v-card-text>
          </v-card>
        </div>

        <!-- Submit button -->
        <v-card rounded="lg" class="mt-4 pa-4 bg-grey-lighten-4">
          <div class="d-flex align-center">
            <div>
              <span v-if="unanswered > 0" class="text-warning font-weight-medium">
                <v-icon>mdi-alert</v-icon>
                {{ unanswered }} question{{ unanswered > 1 ? 's' : '' }} not answered
              </span>
              <span v-else class="text-success font-weight-medium">
                <v-icon>mdi-check-circle</v-icon>
                All questions answered
              </span>
            </div>
            <v-spacer />
            <v-btn color="primary" size="large" :loading="store.loading"
              :disabled="unanswered > 0" @click="submitTest">
              <v-icon start>mdi-send</v-icon> Submit Test
            </v-btn>
          </div>
        </v-card>
      </v-form>
    </template>

    <!-- Loading / not found -->
    <div v-else-if="!store.loading" class="text-center py-12 text-grey">
      <v-icon size="72" color="grey-lighten-2">mdi-help-circle-outline</v-icon>
      <div class="text-h6 mt-4">No test available for this course</div>
      <v-btn class="mt-4" to="/courses" variant="outlined">Back to Courses</v-btn>
    </div>
  </v-container>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTrainingStore } from '@/stores/training'

const route    = useRoute()
const router   = useRouter()
const store    = useTrainingStore()
const courseId  = Number(route.params.id)
const apiUrl    = import.meta.env.VITE_API_URL

const answers   = reactive<Record<number, number>>({})
const submitted = ref(false)
const result    = ref<any>(null)

const questions  = computed(() => store.currentTest?.questions ?? [])
const answered   = computed(() => Object.keys(answers).length)
const unanswered = computed(() => questions.value.length - answered.value)
const score      = computed(() => Number(result.value?.score ?? 0).toFixed(1))
const isPassed   = computed(() => result.value?.isValid === true ||
  (result.value?.message?.toLowerCase() === 'passed'))

function retake() {
  Object.keys(answers).forEach(k => delete (answers as any)[k])
  submitted.value = false
  result.value = null
}

async function submitTest() {
  const payload = {
    testId: store.currentTest.test.id,
    answers: Object.entries(answers).map(([qId, aId]) => ({
      questionId: Number(qId),
      selectedAnswerId: Number(aId)
    }))
  }
  result.value = await store.submitTest(payload)
  if (result.value) submitted.value = true
}

onMounted(() => store.getTest(courseId))
</script>

<style scoped>
.border-success { border-color: rgb(var(--v-theme-success)) !important; }
.gap-4 { gap: 16px; }
</style>
