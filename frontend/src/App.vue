<template>
  <v-app>
    <v-app-bar v-if="authStore.isLoggedIn" color="primary" elevation="2">
      <v-app-bar-nav-icon @click="drawer = !drawer" />

      <v-app-bar-title>
        <v-icon class="mr-2">mdi-school</v-icon>
        Training Service
      </v-app-bar-title>

      <v-spacer />

      <!-- User chip -->
      <v-chip color="white" variant="tonal" class="mr-3" prepend-icon="mdi-account-circle">
        {{ authStore.username }}
        <template v-if="authStore.isTrainer">
          <v-icon end size="14" color="yellow">mdi-star</v-icon>
        </template>
      </v-chip>

      <v-btn icon @click="authStore.logout">
        <v-icon>mdi-logout</v-icon>
        <v-tooltip activator="parent">Logout</v-tooltip>
      </v-btn>
    </v-app-bar>

    <!-- Navigation Drawer -->
    <v-navigation-drawer v-if="authStore.isLoggedIn" v-model="drawer" temporary>
      <v-list-item
        prepend-icon="mdi-school"
        title="Training Service"
        :subtitle="authStore.username"
        class="bg-primary text-white py-4"
      />

      <v-divider />

      <v-list density="compact" nav>
        <v-list-item prepend-icon="mdi-book-open-page-variant" title="Courses" to="/courses" />
        <v-list-item v-if="authStore.isTrainer"
          prepend-icon="mdi-teach" title="My Courses" to="/my-courses" />
      </v-list>

      <template #append>
        <v-divider />
        <v-list density="compact" nav>
          <v-list-item prepend-icon="mdi-logout" title="Logout" @click="authStore.logout" />
        </v-list>
      </template>
    </v-navigation-drawer>

    <v-main>
      <router-view />
    </v-main>
  </v-app>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
const authStore = useAuthStore()
const drawer    = ref(false)
</script>
