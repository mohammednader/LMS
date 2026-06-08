import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore('auth', () => {
  const token    = ref<string>(localStorage.getItem('training_token') ?? '')
  const username = ref<string>(localStorage.getItem('training_user') ?? '')
  const roles    = ref<string[]>(JSON.parse(localStorage.getItem('training_roles') ?? '[]'))
  const orgId    = ref<number>(Number(localStorage.getItem('training_orgId') ?? '1'))

  const isLoggedIn  = computed(() => !!token.value)
  const isTrainer   = computed(() => roles.value.includes('Training') || roles.value.includes('Developer'))
  const isDeveloper = computed(() => roles.value.includes('Developer'))

  function setToken(t: string, user: string, r: string[] = [], org = 1) {
    token.value = t; username.value = user; roles.value = r; orgId.value = org
    localStorage.setItem('training_token', t)
    localStorage.setItem('training_user', user)
    localStorage.setItem('training_roles', JSON.stringify(r))
    localStorage.setItem('training_orgId', String(org))
  }

  function logout() {
    token.value = ''; username.value = ''; roles.value = []; orgId.value = 1
    localStorage.removeItem('training_token')
    localStorage.removeItem('training_user')
    localStorage.removeItem('training_roles')
    localStorage.removeItem('training_orgId')
    window.location.href = '/login'
  }

  return { token, username, roles, orgId, isLoggedIn, isTrainer, isDeveloper, setToken, logout }
})
