<template>
  <v-container>
    <v-progress-linear v-if="store.loading" indeterminate color="primary" class="mb-4" rounded />

    <template v-if="course">
      <!-- Header -->
      <v-row class="mb-4" align="start">
        <v-col>
          <v-btn variant="text" color="grey" size="small" to="/courses" class="mb-1">
            <v-icon start>mdi-arrow-left</v-icon> Back
          </v-btn>
          <div class="text-h4 font-weight-bold text-primary">{{ course.nameAr }}</div>
          <div class="text-h6 text-grey-darken-1">{{ course.name }}</div>
          <v-chip v-if="course.targetedAudiance" color="secondary" size="small" class="mt-2">
            <v-icon start size="14">mdi-account-group</v-icon>
            {{ course.targetedAudiance }}
          </v-chip>
        </v-col>
        <v-col cols="auto" class="d-flex flex-column gap-2 align-end">
          <v-btn color="primary" size="large" :to="`/courses/${courseId}/test`" prepend-icon="mdi-pencil-box">
            Take Test
          </v-btn>
          <v-btn v-if="authStore.isTrainer" color="secondary" size="small"
            :to="`/my-courses/${courseId}/manage`" prepend-icon="mdi-cog">
            Manage Test & Results
          </v-btn>
        </v-col>
      </v-row>

      <!-- Sections header -->
      <v-row class="mb-3" align="center">
        <v-col>
          <div class="text-h6 font-weight-bold">
            <v-icon class="mr-1">mdi-book-open</v-icon>
            Course Material
            <v-chip size="small" color="primary" variant="tonal" class="ml-2">
              {{ sections.length }} sections
            </v-chip>
          </div>
        </v-col>
        <v-col cols="auto" v-if="authStore.isTrainer">
          <v-btn color="primary" prepend-icon="mdi-plus" @click="openSectionDialog()">
            Add Section
          </v-btn>
        </v-col>
      </v-row>

      <!-- Empty state -->
      <v-row v-if="sections.length === 0">
        <v-col class="text-center py-10">
          <v-icon size="72" color="grey-lighten-2">mdi-book-plus-multiple-outline</v-icon>
          <div class="text-h6 text-grey mt-3">No sections yet</div>
          <v-btn v-if="authStore.isTrainer" color="primary" class="mt-4"
            prepend-icon="mdi-plus" @click="openSectionDialog()">
            Add First Section
          </v-btn>
        </v-col>
      </v-row>

      <!-- Section cards -->
      <v-row>
        <v-col v-for="(section, i) in sections" :key="section.id" cols="12" md="6" lg="4">
          <v-card variant="outlined" rounded="lg" class="h-100">
            <!-- Card header -->
            <v-card-title class="pa-4 pb-2 d-flex align-start">
              <v-avatar color="primary" size="32" class="mr-3 mt-1 flex-shrink-0">
                <span class="text-white text-caption font-weight-bold">{{ i + 1 }}</span>
              </v-avatar>
              <div class="flex-grow-1 min-width-0">
                <div class="text-subtitle-2 font-weight-bold text-wrap">{{ section.nameAr }}</div>
                <div class="text-caption text-grey">{{ section.name }}</div>
              </div>
              <!-- Edit button for trainers -->
              <v-btn v-if="authStore.isTrainer" icon size="x-small" variant="text"
                color="primary" class="flex-shrink-0" @click="openSectionDialog(section)">
                <v-icon>mdi-pencil</v-icon>
              </v-btn>
            </v-card-title>

            <v-card-text class="pa-4 pt-0">
              <div v-if="section.description" class="text-body-2 text-grey mb-3">
                {{ section.description }}
              </div>

              <!-- Material badges -->
              <div class="d-flex flex-wrap gap-1">
                <v-chip v-if="section.materialUrl" color="success" size="x-small">
                  <v-icon start size="12">mdi-paperclip</v-icon>
                  Uploaded file
                </v-chip>
                <v-chip v-if="section.externalUrl" color="blue" size="x-small">
                  <v-icon start size="12">mdi-link</v-icon>
                  External link
                </v-chip>
                <v-chip v-if="!section.materialUrl && !section.externalUrl"
                  color="grey" size="x-small" variant="outlined">
                  No material
                </v-chip>
              </div>
            </v-card-text>

            <!-- Action buttons -->
            <v-card-actions class="pa-4 pt-0 d-flex flex-column gap-2">
              <v-btn v-if="section.materialUrl" color="primary" variant="tonal" size="small"
                block @click="viewFile(section)">
                <v-icon start>mdi-eye</v-icon> View Uploaded File
              </v-btn>
              <v-btn v-if="section.externalUrl" color="blue" variant="tonal" size="small"
                block :href="section.externalUrl" target="_blank">
                <v-icon start>mdi-open-in-new</v-icon> Open External Link
              </v-btn>
              <v-btn v-if="authStore.isTrainer && !section.materialUrl && !section.externalUrl"
                color="primary" variant="outlined" size="small" block
                @click="openSectionDialog(section)">
                <v-icon start>mdi-upload</v-icon> Add Material
              </v-btn>
            </v-card-actions>
          </v-card>
        </v-col>
      </v-row>
    </template>

    <!-- ── Section Add/Edit Dialog ─────────────────────────────────────── -->
    <v-dialog v-model="sectionDialog" max-width="580" persistent>
      <v-card rounded="lg">
        <v-card-title class="bg-primary text-white pa-4 d-flex align-center">
          <v-icon class="mr-2">{{ sectionForm.id ? 'mdi-pencil' : 'mdi-plus' }}</v-icon>
          {{ sectionForm.id ? 'Edit Section' : 'Add New Section' }}
          <v-spacer />
          <v-btn icon variant="text" color="white" size="small" @click="sectionDialog = false">
            <v-icon>mdi-close</v-icon>
          </v-btn>
        </v-card-title>

        <v-card-text class="pa-6">
          <v-row dense>
            <v-col cols="12">
              <v-text-field v-model="sectionForm.name" label="Section Name (English)"
                variant="outlined" prepend-inner-icon="mdi-translate"
                :rules="[v => !!v || 'Required']" />
            </v-col>
            <v-col cols="12">
              <v-text-field v-model="sectionForm.nameAr" label="اسم القسم (عربي)"
                variant="outlined" prepend-inner-icon="mdi-translate" dir="rtl"
                :rules="[v => !!v || 'Required']" />
            </v-col>
            <v-col cols="12">
              <v-textarea v-model="sectionForm.description" label="Description (optional)"
                variant="outlined" rows="2" auto-grow />
            </v-col>
          </v-row>

          <v-divider class="my-4" />
          <div class="text-subtitle-2 mb-3">
            <v-icon class="mr-1">mdi-attachment</v-icon>
            Material (optional — choose one)
          </div>

          <!-- Upload file -->
          <v-card variant="outlined" class="pa-4 mb-3" rounded="lg">
            <div class="text-body-2 font-weight-medium mb-2">
              <v-icon color="primary" size="18" class="mr-1">mdi-upload</v-icon>
              Upload File
            </div>
            <div v-if="sectionForm.attachRecordId" class="d-flex align-center pa-2 bg-green-lighten-5 rounded">
              <v-icon color="success" class="mr-2">mdi-check-circle</v-icon>
              <span class="text-body-2 flex-grow-1 text-success">File uploaded successfully</span>
              <v-btn icon size="x-small" variant="text" color="error" @click="removeFile">
                <v-icon>mdi-close</v-icon>
              </v-btn>
            </div>
            <v-file-input v-else v-model="fileInput"
              label="PDF, Image, Video, Word, PowerPoint..."
              variant="outlined" density="comfortable"
              accept=".pdf,.jpg,.jpeg,.png,.gif,.webp,.mp4,.webm,.ppt,.pptx,.doc,.docx,.txt"
              prepend-icon="mdi-paperclip" :loading="uploading"
              hide-details @update:model-value="uploadSelected" />
          </v-card>

          <!-- External URL -->
          <v-card variant="outlined" class="pa-4" rounded="lg">
            <div class="text-body-2 font-weight-medium mb-2">
              <v-icon color="blue" size="18" class="mr-1">mdi-link</v-icon>
              External Link (YouTube, SharePoint, etc.)
            </div>
            <v-text-field v-model="sectionForm.externalUrl"
              label="https://..." variant="outlined" density="comfortable"
              prepend-inner-icon="mdi-link" hide-details clearable
              :disabled="!!sectionForm.attachRecordId" />
            <div v-if="sectionForm.attachRecordId" class="text-caption text-grey mt-1">
              Remove the uploaded file first to use an external link.
            </div>
          </v-card>
        </v-card-text>

        <v-card-actions class="pa-4 pt-0">
          <v-btn v-if="sectionForm.id" color="error" variant="text"
            @click="deleteSection(sectionForm.id)">
            <v-icon start>mdi-delete</v-icon> Delete
          </v-btn>
          <v-spacer />
          <v-btn variant="text" @click="sectionDialog = false">Cancel</v-btn>
          <v-btn color="primary" :loading="saving" @click="saveSection">
            <v-icon start>mdi-content-save</v-icon>
            {{ sectionForm.id ? 'Update' : 'Add' }} Section
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- ── File Viewer Dialog ─────────────────────────────────────────── -->
    <v-dialog v-model="viewerDialog" max-width="920" scrollable>
      <v-card rounded="lg">
        <v-card-title class="bg-primary text-white pa-4 d-flex align-center">
          <v-icon class="mr-2">mdi-file-eye</v-icon>
          {{ selectedSection?.nameAr }}
          <v-spacer />
          <v-btn icon variant="text" color="white" size="small" @click="viewerDialog = false">
            <v-icon>mdi-close</v-icon>
          </v-btn>
        </v-card-title>

        <v-card-text class="pa-0" style="min-height: 420px">
          <v-progress-circular v-if="!fileInfo" indeterminate color="primary"
            class="d-block ma-auto mt-12" />

          <template v-else>
            <iframe v-if="fileInfo.contentType === 'application/pdf'"
              :src="fileUrl!" width="100%" height="600" style="border:0" />
            <v-img v-else-if="fileInfo.contentType?.startsWith('image/')"
              :src="fileUrl!" max-height="600" contain class="bg-grey-lighten-4" />
            <video v-else-if="fileInfo.contentType?.startsWith('video/')"
              :src="fileUrl!" controls width="100%" />
            <div v-else class="text-center pa-12">
              <v-icon size="72" color="primary">mdi-file-download</v-icon>
              <div class="text-h6 mt-4">{{ fileInfo.name }}</div>
              <div class="text-body-2 text-grey mb-6">
                {{ fileInfo.contentType }} · {{ formatSize(fileInfo.size) }}
              </div>
              <v-btn color="primary" :href="fileUrl!" target="_blank" download>
                <v-icon start>mdi-download</v-icon> Download
              </v-btn>
            </div>
          </template>
        </v-card-text>

        <v-card-actions class="pa-4">
          <v-btn v-if="fileInfo" :href="fileUrl!" target="_blank" variant="outlined" size="small">
            <v-icon start>mdi-open-in-new</v-icon> Open in New Tab
          </v-btn>
          <v-spacer />
          <v-btn @click="viewerDialog = false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useTrainingStore } from '@/stores/training'
import { useAuthStore } from '@/stores/auth'

const route    = useRoute()
const store    = useTrainingStore()
const authStore = useAuthStore()
const courseId  = Number(route.params.id)

// ── State ──────────────────────────────────────────────────────────────────
const saving    = ref(false)
const uploading = ref(false)
const fileInput = ref<File | undefined>()

const sectionDialog = ref(false)
const sectionForm   = ref({
  id: 0, courseId, name: '', nameAr: '', description: '',
  isActive: true, organizationId: 1,
  attachRecordId: null as string | null,
  externalUrl: '' as string | null
})

const viewerDialog    = ref(false)
const selectedSection = ref<any>(null)
const fileInfo        = ref<any>(null)
const fileUrl         = ref<string | null>(null)

// ── Computed ───────────────────────────────────────────────────────────────
const course   = computed(() => store.currentCourse?.courses?.[0])
const sections = computed(() => store.currentCourse?.courseSections ?? [])

// ── Section dialog ─────────────────────────────────────────────────────────
function openSectionDialog(s?: any) {
  if (s) {
    sectionForm.value = {
      id: s.id, courseId, name: s.name ?? '', nameAr: s.nameAr ?? '',
      description: s.description ?? '', isActive: s.isActive,
      organizationId: 1, attachRecordId: s.attachRecordId ?? null,
      externalUrl: s.externalUrl ?? ''
    }
  } else {
    sectionForm.value = {
      id: 0, courseId, name: '', nameAr: '', description: '',
      isActive: true, organizationId: 1, attachRecordId: null, externalUrl: ''
    }
  }
  fileInput.value = undefined
  sectionDialog.value = true
}

async function uploadSelected(file: File | File[] | undefined) {
  if (!file || Array.isArray(file)) return
  uploading.value = true
  const result = await store.uploadFile(file)
  if (result) sectionForm.value.attachRecordId = result.id
  uploading.value = false
}

function removeFile() {
  sectionForm.value.attachRecordId = null
  fileInput.value = undefined
}

async function saveSection() {
  if (!sectionForm.value.name && !sectionForm.value.nameAr) return
  saving.value = true
  try {
    const result = await store.addUpdateSection(sectionForm.value)
    if (result?.isValid !== false) {
      sectionDialog.value = false
      await store.getCourseDetails(courseId)
    }
  } finally { saving.value = false }
}

async function deleteSection(id: number) {
  if (!confirm('Delete this section?')) return
  await store.deleteSection(id)
  sectionDialog.value = false
  await store.getCourseDetails(courseId)
}

// ── File viewer ────────────────────────────────────────────────────────────
async function viewFile(section: any) {
  selectedSection.value = section
  fileInfo.value = null
  fileUrl.value = null
  viewerDialog.value = true
  const info = await store.getFileInfo(section.attachRecordId)
  if (info) {
    fileInfo.value = info
    fileUrl.value = store.getFileUrl(section.attachRecordId)
  }
}

function formatSize(bytes: number) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1048576).toFixed(1)} MB`
}

// ── Init ───────────────────────────────────────────────────────────────────
onMounted(() => store.getCourseDetails(courseId))
</script>

<style scoped>
.gap-2 { gap: 8px; }
.min-width-0 { min-width: 0; }
</style>
