<template>
  <v-container class="fill-height" fluid>
    <v-row align="center" justify="center">
      <v-col cols="12" sm="8" md="5" lg="4">
        <v-card elevation="8" rounded="lg">
          <v-card-title class="text-center pa-6 bg-primary text-white">
            <v-icon size="48" class="mb-2">mdi-school</v-icon>
            <div class="text-h5">Training Service</div>
          </v-card-title>

          <v-card-text class="pa-6">
            <v-alert v-if="errorMsg" type="error" class="mb-4" density="compact" closable @click:close="errorMsg=''">
              {{ errorMsg }}
            </v-alert>

            <v-text-field v-model="form.username" label="Username" prepend-inner-icon="mdi-account"
              variant="outlined" class="mb-3" autofocus @keyup.enter="login" />

            <v-text-field v-model="form.password" label="Password" prepend-inner-icon="mdi-lock"
              :type="showPass ? 'text' : 'password'" variant="outlined" class="mb-5"
              :append-inner-icon="showPass ? 'mdi-eye-off' : 'mdi-eye'"
              @click:append-inner="showPass = !showPass"
              @keyup.enter="login" />

            <v-btn block color="primary" size="large" :loading="loading" @click="login">
              <v-icon start>mdi-login</v-icon> Login
            </v-btn>
          </v-card-text>

          <!-- Quick-access credentials hint -->
          <v-card-actions class="pa-4 pt-0">
            <v-expansion-panels variant="accordion" flat>
              <v-expansion-panel>
                <v-expansion-panel-title class="text-caption text-grey">
                  Available accounts
                </v-expansion-panel-title>
                <v-expansion-panel-text>
                  <v-table density="compact">
                    <thead><tr><th>Username</th><th>Password</th><th>Role</th></tr></thead>
                    <tbody>
                      <tr v-for="u in users" :key="u.username" class="cursor-pointer"
                        @click="form.username = u.username; form.password = u.password">
                        <td><code>{{ u.username }}</code></td>
                        <td><code>{{ u.password }}</code></td>
                        <td><v-chip size="x-small" :color="u.roles.includes('Developer') ? 'primary' : 'secondary'">
                          {{ u.roles.join(', ') || 'Trainee' }}
                        </v-chip></td>
                      </tr>
                    </tbody>
                  </v-table>
                  <div class="text-caption text-grey mt-1">Click a row to fill credentials.</div>
                </v-expansion-panel-text>
              </v-expansion-panel>
            </v-expansion-panels>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import api from '@/utils/api'

const router = useRouter()
const authStore = useAuthStore()
const form = ref({ username: '', password: '' })
const loading = ref(false)
const errorMsg = ref('')
const showPass = ref(false)
const users = ref<any[]>([])

onMounted(async () => {
  try {
    const { data } = await api.get('Auth/users')
    users.value = data
  } catch {}
})

async function login() {
  if (!form.value.username || !form.value.password) {
    errorMsg.value = 'Please enter username and password'
    return
  }
  loading.value = true
  errorMsg.value = ''
  try {
    const { data } = await api.post('Auth/login', form.value)
    authStore.setToken(data.token, data.username, data.roles, data.orgId)
    router.push('/')
  } catch (e: any) {
    errorMsg.value = e.response?.data?.message ?? 'Login failed'
  } finally {
    loading.value = false
  }
}
</script>
