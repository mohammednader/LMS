<template>
  <v-container>
    <v-row class="mb-4" align="center">
      <v-col>
        <div class="text-h5 font-weight-bold text-primary">
          <v-icon class="mr-2">mdi-teach</v-icon> My Courses
        </div>
        <div class="text-body-2 text-grey">Manage your courses, sections, tests and view results.</div>
      </v-col>
      <v-col cols="auto">
        <v-btn color="primary" prepend-icon="mdi-plus" @click="showAddDialog">New Course</v-btn>
      </v-col>
    </v-row>

    <v-progress-linear v-if="store.loading" indeterminate color="primary" class="mb-4" rounded />

    <v-row v-if="!store.loading && store.courses.length === 0">
      <v-col class="text-center py-12">
        <v-icon size="80" color="grey-lighten-2">mdi-book-plus-outline</v-icon>
        <div class="text-h6 text-grey mt-4">No courses yet</div>
        <v-btn color="primary" class="mt-4" @click="showAddDialog">Create First Course</v-btn>
      </v-col>
    </v-row>

    <v-row>
      <v-col v-for="course in store.courses" :key="course.id" cols="12" sm="6" md="4">
        <v-card elevation="2" rounded="lg" class="h-100 d-flex flex-column" hover>
          <v-card-title class="bg-secondary text-white pa-4">
            <div class="text-subtitle-1 font-weight-bold text-truncate">{{ course.nameAr }}</div>
            <div class="text-caption text-teal-lighten-3">{{ course.name }}</div>
          </v-card-title>
          <v-card-text class="flex-grow-1 pt-3">
            <v-chip :color="course.isActive ? 'success' : 'error'" size="small" class="mb-2">
              {{ course.isActive ? 'Active' : 'Inactive' }}
            </v-chip>
            <div v-if="course.targetedAudiance" class="text-body-2 text-grey">
              <v-icon size="16" class="mr-1">mdi-account-group</v-icon>
              {{ course.targetedAudiance }}
            </div>
          </v-card-text>
          <v-divider />
          <v-card-actions>
            <v-btn variant="tonal" color="secondary" :to="`/my-courses/${course.id}/manage`" flex>
              <v-icon start>mdi-cog</v-icon> Manage
            </v-btn>
            <v-btn icon variant="text" color="primary" @click="editCourse(course)">
              <v-icon>mdi-pencil</v-icon>
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>

    <!-- Add/Edit Dialog -->
    <v-dialog v-model="dialog" max-width="560" persistent>
      <v-card rounded="lg">
        <v-card-title class="bg-secondary text-white pa-4">
          <v-icon class="mr-2">mdi-book-plus</v-icon>
          {{ form.id ? 'Edit Course' : 'New Course' }}
        </v-card-title>
        <v-card-text class="pa-6">
          <v-row dense>
            <v-col cols="12">
              <v-text-field v-model="form.name" label="Course Name (English)" variant="outlined"
                prepend-inner-icon="mdi-translate" />
            </v-col>
            <v-col cols="12">
              <v-text-field v-model="form.nameAr" label="اسم الدورة (عربي)" variant="outlined"
                prepend-inner-icon="mdi-translate" dir="rtl" />
            </v-col>
            <v-col cols="12">
              <v-text-field v-model="form.targetedAudiance" label="Target Audience"
                variant="outlined" prepend-inner-icon="mdi-account-group" />
            </v-col>
            <v-col cols="12">
              <v-switch v-model="form.isActive" label="Active" color="secondary" hide-details />
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions class="pa-4 pt-0">
          <v-spacer />
          <v-btn variant="text" @click="dialog = false">Cancel</v-btn>
          <v-btn color="secondary" :loading="saving" @click="saveCourse">
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

const store  = useTrainingStore()
const dialog = ref(false)
const saving = ref(false)
const form   = ref({ id: 0, name: '', nameAr: '', targetedAudiance: '', isActive: true })

onMounted(() => store.getMyCourses())

function showAddDialog() {
  form.value = { id: 0, name: '', nameAr: '', targetedAudiance: '', isActive: true }
  dialog.value = true
}

function editCourse(c: any) {
  form.value = { id: c.id, name: c.name ?? '', nameAr: c.nameAr ?? '', targetedAudiance: c.targetedAudiance ?? '', isActive: c.isActive }
  dialog.value = true
}

async function saveCourse() {
  saving.value = true
  try {
    const r = await store.addUpdateCourse(form.value)
    if (r?.isValid) { dialog.value = false; await store.getMyCourses() }
  } finally { saving.value = false }
}
</script>
