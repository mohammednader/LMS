<template>
  <v-container>
    <!-- Breadcrumb -->
    <v-btn variant="text" color="grey" size="small" to="/my-courses" class="mb-2">
      <v-icon start>mdi-arrow-left</v-icon> My Courses
    </v-btn>
    <div class="text-h5 font-weight-bold text-primary mb-1">
      {{ store.currentCourse?.courses?.[0]?.nameAr || 'Manage Course' }}
    </div>
    <div class="text-body-2 text-grey mb-4">
      {{ store.currentCourse?.courses?.[0]?.name }}
    </div>

    <v-tabs v-model="tab" color="primary" class="mb-1">
      <v-tab value="sections" prepend-icon="mdi-book">Sections</v-tab>
      <v-tab value="test"     prepend-icon="mdi-help-circle">Test</v-tab>
      <v-tab value="results"  prepend-icon="mdi-chart-bar">Results</v-tab>
    </v-tabs>

    <v-window v-model="tab">

      <!-- ── Sections Tab ─────────────────────────────────────────────────── -->
      <v-window-item value="sections">
        <v-card class="mt-3" rounded="lg">
          <v-card-title class="d-flex align-center pa-4">
            <span>Course Sections</span>
            <v-spacer />
            <v-btn color="primary" size="small" prepend-icon="mdi-plus" @click="openSectionDialog()">
              Add Section
            </v-btn>
          </v-card-title>

          <v-list v-if="sections.length" lines="two">
            <v-list-item v-for="(s, i) in sections" :key="s.id" class="border-b">
              <template #prepend>
                <v-avatar color="primary" size="36">
                  <span class="text-white text-caption">{{ i + 1 }}</span>
                </v-avatar>
              </template>
              <template #title>{{ s.nameAr }}</template>
              <template #subtitle>{{ s.name }}</template>
              <template #append>
                <div class="d-flex align-center gap-2">
                  <v-chip v-if="s.attachRecordId" color="success" size="x-small">
                    <v-icon start size="12">mdi-paperclip</v-icon> File
                  </v-chip>
                  <v-btn icon size="small" variant="text" color="primary" @click="openSectionDialog(s)">
                    <v-icon>mdi-pencil</v-icon>
                  </v-btn>
                </div>
              </template>
            </v-list-item>
          </v-list>
          <v-card-text v-else class="text-center py-8 text-grey">
            <v-icon size="48" color="grey-lighten-2">mdi-book-plus-multiple</v-icon>
            <div class="mt-2">No sections yet. Add the first one.</div>
          </v-card-text>
        </v-card>
      </v-window-item>

      <!-- ── Test Tab ─────────────────────────────────────────────────────── -->
      <v-window-item value="test">
        <v-card class="mt-3" rounded="lg">
          <v-card-title class="d-flex align-center pa-4">
            <template v-if="currentTest">
              <span>{{ currentTest.name }}</span>
              <v-chip size="small" color="primary" class="ml-3">
                Pass: {{ currentTest.passScore }}%
              </v-chip>
            </template>
            <span v-else>No Test Created</span>
            <v-spacer />
            <v-btn color="secondary" size="small" class="mr-2" prepend-icon="mdi-cog"
              @click="openTestDialog">
              {{ currentTest ? 'Edit Test' : 'Create Test' }}
            </v-btn>
            <v-btn v-if="currentTest" color="primary" size="small" prepend-icon="mdi-plus"
              @click="openQuestionDialog()">
              Add Question
            </v-btn>
          </v-card-title>

          <template v-if="questions.length">
            <v-expansion-panels variant="accordion" flat>
              <v-expansion-panel v-for="(q, qi) in questions" :key="q.id">
                <v-expansion-panel-title>
                  <div class="d-flex align-center w-100">
                    <v-chip size="x-small" color="primary" class="mr-3">{{ qi + 1 }}</v-chip>
                    <span class="flex-grow-1">{{ q.text }}</span>
                    <v-btn icon size="x-small" variant="text" color="primary" class="mr-1"
                      @click.stop="openQuestionDialog(q)">
                      <v-icon>mdi-pencil</v-icon>
                    </v-btn>
                    <v-btn icon size="x-small" variant="text" color="error"
                      @click.stop="deleteQ(q.id)">
                      <v-icon>mdi-delete</v-icon>
                    </v-btn>
                  </div>
                </v-expansion-panel-title>
                <v-expansion-panel-text>
                  <v-list density="compact">
                    <v-list-item v-for="a in q.answers" :key="a.id"
                      :prepend-icon="a.isCorrect ? 'mdi-check-circle' : 'mdi-circle-outline'"
                      :base-color="a.isCorrect ? 'success' : 'grey'"
                      :title="a.text" />
                  </v-list>
                </v-expansion-panel-text>
              </v-expansion-panel>
            </v-expansion-panels>
          </template>

          <v-card-text v-else-if="currentTest" class="text-center py-8 text-grey">
            <v-icon size="48" color="grey-lighten-2">mdi-help-circle-outline</v-icon>
            <div class="mt-2">No questions yet. Add the first question.</div>
          </v-card-text>

          <v-card-text v-else class="text-center py-8 text-grey">
            <v-icon size="48" color="grey-lighten-2">mdi-clipboard-plus-outline</v-icon>
            <div class="mt-2">Create a test first, then add questions.</div>
          </v-card-text>
        </v-card>
      </v-window-item>

      <!-- ── Results Tab ──────────────────────────────────────────────────── -->
      <v-window-item value="results">
        <v-card class="mt-3" rounded="lg">
          <v-card-title class="d-flex align-center pa-4">
            Student Results
            <v-chip v-if="store.testResults.length" class="ml-2" size="small" color="primary">
              {{ store.testResults.length }}
            </v-chip>
            <v-spacer />
            <v-btn icon size="small" variant="text" color="primary" :loading="store.loading"
              @click="refreshResults">
              <v-icon>mdi-refresh</v-icon>
              <v-tooltip activator="parent">Refresh</v-tooltip>
            </v-btn>
          </v-card-title>

          <v-data-table v-if="store.testResults.length"
            :headers="resultHeaders" :items="store.testResults" density="comfortable">
            <template #item.userId="{ item }">
              <v-chip size="small" color="primary" variant="tonal" prepend-icon="mdi-account">
                {{ item.userId }}
              </v-chip>
            </template>
            <template #item.score="{ item }">
              <v-chip :color="item.isPassed ? 'success' : 'error'" size="small">
                {{ Number(item.score).toFixed(1) }}%
              </v-chip>
            </template>
            <template #item.isPassed="{ item }">
              <v-icon :color="item.isPassed ? 'success' : 'error'">
                {{ item.isPassed ? 'mdi-check-circle' : 'mdi-close-circle' }}
              </v-icon>
            </template>
            <template #item.examDate="{ item }">
              {{ new Date(item.examDate).toLocaleDateString() }}
            </template>
            <template #item.certificate="{ item }">
              <v-btn v-if="item.isPassed" size="small" color="success" variant="tonal"
                :href="store.getCertificateUrl(item.id)" target="_blank" prepend-icon="mdi-certificate">
                Download
              </v-btn>
              <span v-else class="text-grey text-caption">—</span>
            </template>
          </v-data-table>

          <v-card-text v-else class="text-center py-10 text-grey">
            <v-icon size="56" color="grey-lighten-2">mdi-chart-bar</v-icon>
            <div class="text-h6 mt-3">No results yet</div>
            <div class="text-body-2 mt-1">Results will appear here after students submit the test.</div>
          </v-card-text>
        </v-card>
      </v-window-item>
    </v-window>

    <!-- ── Section Dialog ─────────────────────────────────────────────────── -->
    <v-dialog v-model="sectionDialog" max-width="560" persistent>
      <v-card rounded="lg">
        <v-card-title class="bg-primary text-white pa-4">
          {{ sectionForm.id ? 'Edit Section' : 'Add Section' }}
        </v-card-title>
        <v-card-text class="pa-6">
          <v-text-field v-model="sectionForm.name" label="Name (English)" variant="outlined" class="mb-3" />
          <v-text-field v-model="sectionForm.nameAr" label="الاسم (عربي)" variant="outlined"
            dir="rtl" class="mb-4" />

          <!-- File upload -->
          <div class="text-subtitle-2 mb-2">Material / Attachment</div>
          <v-file-input v-if="!sectionForm.attachRecordId"
            v-model="sectionFile" label="Upload PDF, Image, Video, Doc..." variant="outlined"
            accept=".pdf,.jpg,.jpeg,.png,.gif,.webp,.mp4,.webm,.ppt,.pptx,.doc,.docx,.txt"
            prepend-icon="mdi-paperclip" density="comfortable" :loading="uploading"
            @update:model-value="handleFileSelect" />

          <div v-else class="d-flex align-center pa-3 bg-grey-lighten-4 rounded">
            <v-icon color="success" class="mr-2">mdi-check-circle</v-icon>
            <span class="text-body-2 flex-grow-1">File attached</span>
            <v-btn icon size="small" variant="text" color="error" @click="removeFile">
              <v-icon>mdi-close</v-icon>
            </v-btn>
          </div>
        </v-card-text>
        <v-card-actions class="pa-4 pt-0">
          <v-spacer />
          <v-btn variant="text" @click="sectionDialog = false">Cancel</v-btn>
          <v-btn color="primary" :loading="saving" @click="saveSection">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Test Dialog ────────────────────────────────────────────────────── -->
    <v-dialog v-model="testDialog" max-width="480" persistent>
      <v-card rounded="lg">
        <v-card-title class="bg-secondary text-white pa-4">
          {{ currentTest ? 'Edit Test Settings' : 'Create Test' }}
        </v-card-title>
        <v-card-text class="pa-6">
          <v-text-field v-model="testForm.name" label="Test Name" variant="outlined" class="mb-3" />
          <v-row dense>
            <v-col cols="6">
              <v-text-field v-model.number="testForm.passScore" label="Pass Score (%)" type="number"
                variant="outlined" :rules="[v => (v >= 1 && v <= 100) || '1–100']" />
            </v-col>
            <v-col cols="6">
              <v-text-field v-model.number="testForm.expiryDuration" label="Expiry (days)" type="number"
                variant="outlined" />
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions class="pa-4 pt-0">
          <v-spacer />
          <v-btn variant="text" @click="testDialog = false">Cancel</v-btn>
          <v-btn color="secondary" :loading="saving" @click="saveTest">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── Question Dialog ────────────────────────────────────────────────── -->
    <v-dialog v-model="questionDialog" max-width="640" persistent>
      <v-card rounded="lg">
        <v-card-title class="bg-primary text-white pa-4">
          {{ questionForm.id ? 'Edit Question' : 'Add Question' }}
        </v-card-title>
        <v-card-text class="pa-6">
          <v-textarea v-model="questionForm.text" label="Question Text" variant="outlined"
            rows="2" auto-grow class="mb-4" />

          <div class="text-subtitle-2 mb-3">Answers (check the correct one)</div>
          <div v-for="(ans, i) in questionForm.answers" :key="i" class="d-flex align-center mb-2">
            <v-checkbox v-model="ans.isCorrect" hide-details class="mr-2 flex-shrink-0"
              color="success" @change="onCorrectChange(i)" />
            <v-text-field v-model="ans.text" :label="`Answer ${i + 1}`" variant="outlined"
              density="compact" hide-details class="flex-grow-1 mr-2" />
            <v-btn icon size="small" variant="text" color="error" :disabled="questionForm.answers.length <= 2"
              @click="removeAnswer(i)">
              <v-icon>mdi-minus</v-icon>
            </v-btn>
          </div>
          <v-btn variant="text" color="primary" size="small" class="mt-1" @click="addAnswer"
            :disabled="questionForm.answers.length >= 6">
            <v-icon start>mdi-plus</v-icon> Add Answer
          </v-btn>
        </v-card-text>
        <v-card-actions class="pa-4 pt-0">
          <v-spacer />
          <v-btn variant="text" @click="questionDialog = false">Cancel</v-btn>
          <v-btn color="primary" :loading="saving" @click="saveQuestion">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useTrainingStore } from '@/stores/training'

const route    = useRoute()
const store    = useTrainingStore()
const courseId = Number(route.params.id)

const tab      = ref('sections')
const saving   = ref(false)
const uploading = ref(false)

const sections    = computed(() => store.currentCourse?.courseSections ?? [])
const currentTest = computed(() => store.currentTest?.test ?? null)
const questions   = computed(() => store.currentTest?.questions ?? [])

// Result table headers
const resultHeaders = [
  { title: 'Student', key: 'userId' },
  { title: 'Date', key: 'examDate' },
  { title: 'Score', key: 'score' },
  { title: 'Status', key: 'isPassed' },
  { title: 'Certificate', key: 'certificate', sortable: false }
]

async function refreshResults() {
  if (currentTest.value?.id) await store.getTestResults(currentTest.value.id)
}

// ── Section ────────────────────────────────────────────────────────────────
const sectionDialog = ref(false)
const sectionFile   = ref<File | undefined>()
const sectionForm   = ref({ id: 0, courseId, name: '', nameAr: '', isActive: true, organizationId: 1, attachRecordId: null as string | null })

function openSectionDialog(s?: any) {
  sectionForm.value = s
    ? { id: s.id, courseId, name: s.name ?? '', nameAr: s.nameAr ?? '', isActive: s.isActive, organizationId: 1, attachRecordId: s.attachRecordId ?? null }
    : { id: 0, courseId, name: '', nameAr: '', isActive: true, organizationId: 1, attachRecordId: null }
  sectionFile.value = undefined
  sectionDialog.value = true
}

async function handleFileSelect(file: File | File[] | undefined) {
  if (!file || Array.isArray(file)) return
  uploading.value = true
  const result = await store.uploadFile(file)
  if (result) sectionForm.value.attachRecordId = result.id
  uploading.value = false
}

function removeFile() { sectionForm.value.attachRecordId = null; sectionFile.value = undefined }

async function saveSection() {
  if (!sectionForm.value.name && !sectionForm.value.nameAr) return
  saving.value = true
  try {
    await store.addUpdateSection(sectionForm.value)
    sectionDialog.value = false
    await store.getCourseDetails(courseId)
  } finally { saving.value = false }
}

// ── Test ───────────────────────────────────────────────────────────────────
const testDialog = ref(false)
const testForm   = ref({ id: 0, name: '', trainingCourseId: courseId, passScore: 60, expiryDuration: 365, isActive: true })

function openTestDialog() {
  testForm.value = currentTest.value
    ? { id: currentTest.value.id, name: currentTest.value.name, trainingCourseId: courseId, passScore: currentTest.value.passScore, expiryDuration: currentTest.value.expiryDuration, isActive: true }
    : { id: 0, name: '', trainingCourseId: courseId, passScore: 60, expiryDuration: 365, isActive: true }
  testDialog.value = true
}

async function saveTest() {
  if (!testForm.value.name) return
  saving.value = true
  try {
    await store.addUpdateTest(testForm.value)
    testDialog.value = false
    await store.getMyTest(courseId)
  } finally { saving.value = false }
}

// ── Question ───────────────────────────────────────────────────────────────
const questionDialog = ref(false)
const questionForm   = ref({
  id: 0, testId: 0, text: '', isActive: true, sorting: 1,
  answers: [
    { text: '', isCorrect: false, isActive: true },
    { text: '', isCorrect: false, isActive: true },
    { text: '', isCorrect: false, isActive: true },
    { text: '', isCorrect: false, isActive: true },
  ]
})

function openQuestionDialog(q?: any) {
  if (q) {
    questionForm.value = {
      id: q.id, testId: q.testId, text: q.text ?? '', isActive: q.isActive, sorting: q.sorting ?? 1,
      answers: q.answers?.map((a: any) => ({ text: a.text ?? '', isCorrect: a.isCorrect, isActive: a.isActive })) ?? []
    }
  } else {
    questionForm.value = {
      id: 0, testId: currentTest.value?.id ?? 0, text: '', isActive: true, sorting: questions.value.length + 1,
      answers: [
        { text: '', isCorrect: false, isActive: true }, { text: '', isCorrect: false, isActive: true },
        { text: '', isCorrect: false, isActive: true }, { text: '', isCorrect: false, isActive: true }
      ]
    }
  }
  questionDialog.value = true
}

function addAnswer() {
  questionForm.value.answers.push({ text: '', isCorrect: false, isActive: true })
}

function removeAnswer(i: number) {
  questionForm.value.answers.splice(i, 1)
}

function onCorrectChange(checkedIndex: number) {
  // Only one correct answer allowed
  questionForm.value.answers.forEach((a, i) => { if (i !== checkedIndex) a.isCorrect = false })
}

async function saveQuestion() {
  if (!questionForm.value.text) return
  saving.value = true
  try {
    await store.addUpdateQuestion(questionForm.value)
    questionDialog.value = false
    await store.getMyTest(courseId)
  } finally { saving.value = false }
}

async function deleteQ(id: number) {
  if (!confirm('Delete this question?')) return
  await store.deleteQuestion(id)
  await store.getMyTest(courseId)
}

// ── Init ───────────────────────────────────────────────────────────────────
onMounted(async () => {
  await store.getCourseDetails(courseId)
  await store.getMyTest(courseId)
  if (store.currentTest?.test) await store.getTestResults(store.currentTest.test.id)
})
</script>
