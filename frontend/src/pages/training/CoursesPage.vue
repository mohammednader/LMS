<template>
  <v-container>
    <!-- Header -->
    <v-row class="mb-4" align="center">
      <v-col>
        <div class="text-h5 font-weight-bold text-primary">
          <v-icon class="mr-2">mdi-book-open-page-variant</v-icon>
          Available Courses
        </div>
        <div class="text-body-2 text-grey">Select a course to view its material and take the test.</div>
      </v-col>
      <v-col cols="auto" v-if="authStore.isTrainer">
        <v-btn color="primary" prepend-icon="mdi-plus" @click="showAddDialog">
          Add Course
        </v-btn>
      </v-col>
    </v-row>

    <v-progress-linear v-if="store.loading" indeterminate color="primary" class="mb-4" rounded />

    <v-alert v-if="store.error" type="error" variant="tonal" class="mb-4" closable>
      {{ store.error }}
    </v-alert>

    <!-- Empty state -->
    <v-row v-if="!store.loading && store.courses.length === 0">
      <v-col class="text-center py-12">
        <v-icon size="80" color="grey-lighten-2">mdi-book-off-outline</v-icon>
        <div class="text-h6 text-grey mt-4">No courses available yet</div>
        <v-btn v-if="authStore.isTrainer" color="primary" class="mt-4" @click="showAddDialog">
          Create First Course
        </v-btn>
      </v-col>
    </v-row>

    <!-- Course cards -->
    <v-row>
      <v-col v-for="course in store.courses" :key="course.id" cols="12" sm="6" md="4">
        <v-card elevation="2" rounded="lg" class="h-100 d-flex flex-column" hover>
          <v-card-title class="bg-primary text-white pa-4">
            <div class="text-subtitle-1 font-weight-bold text-truncate">{{ course.nameAr }}</div>
            <div class="text-caption text-blue-lighten-3">{{ course.name }}</div>
          </v-card-title>

          <v-card-text class="flex-grow-1 pt-3">
            <v-chip v-if="course.targetedAudiance" color="secondary" size="small" class="mb-2">
              <v-icon start size="14">mdi-account-group</v-icon>
              {{ course.targetedAudiance }}
            </v-chip>
            <div class="text-body-2 text-grey">
              <v-icon size="16" class="mr-1">mdi-calendar</v-icon>
              {{ new Date(course.creatd).toLocaleDateString('ar-SA') }}
            </div>
          </v-card-text>

          <v-divider />
          <v-card-actions>
            <v-btn color="primary" variant="tonal" :to="`/courses/${course.id}`" block>
              <v-icon start>mdi-eye</v-icon>
              View Course
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>

    <!-- Add Course Dialog -->
    <v-dialog v-model="dialog" max-width="560" persistent>
      <v-card rounded="lg">
        <v-card-title class="bg-primary text-white pa-4">
          <v-icon class="mr-2">mdi-book-plus</v-icon>
          {{ form.id ? 'Edit Course' : 'New Course' }}
        </v-card-title>

        <v-card-text class="pa-6">
          <v-row dense>
            <v-col cols="12">
              <v-text-field v-model="form.name" label="Course Name (English)" variant="outlined"
                prepend-inner-icon="mdi-translate" :rules="[v => !!v || 'Required']" />
            </v-col>
            <v-col cols="12">
              <v-text-field v-model="form.nameAr" label="اسم الدورة (عربي)" variant="outlined"
                prepend-inner-icon="mdi-translate" dir="rtl" :rules="[v => !!v || 'Required']" />
            </v-col>
            <v-col cols="12">
              <v-text-field v-model="form.targetedAudiance" label="Target Audience"
                variant="outlined" prepend-inner-icon="mdi-account-group" />
            </v-col>
            <v-col cols="12">
              <v-switch v-model="form.isActive" label="Active" color="primary" hide-details />
            </v-col>
          </v-row>
        </v-card-text>

        <v-card-actions class="pa-4 pt-0">
          <v-spacer />
          <v-btn variant="text" @click="dialog = false">Cancel</v-btn>
          <v-btn color="primary" :loading="saving" @click="saveCourse">
            <v-icon start>mdi-content-save</v-icon>
            {{ form.id ? 'Update' : 'Create' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useTrainingStore } from '@/stores/training'
import { useAuthStore } from '@/stores/auth'

const store    = useTrainingStore()
const authStore = useAuthStore()

const dialog = ref(false)
const saving = ref(false)
const form   = ref({ id: 0, name: '', nameAr: '', targetedAudiance: '', isActive: true })

onMounted(() => store.getCourses())

function showAddDialog() {
  form.value = { id: 0, name: '', nameAr: '', targetedAudiance: '', isActive: true }
  dialog.value = true
}

async function saveCourse() {
  if (!form.value.name || !form.value.nameAr) return
  saving.value = true
  try {
    const result = await store.addUpdateCourse(form.value)
    if (result?.isValid) {
      dialog.value = false
      await store.getCourses()
    }
  } finally {
    saving.value = false
  }
}
</script>
