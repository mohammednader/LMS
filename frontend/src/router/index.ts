import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', component: () => import('@/pages/LoginPage.vue'), meta: { public: true } },
    { path: '/', redirect: '/courses' },
    { path: '/courses', component: () => import('@/pages/training/CoursesPage.vue') },
    { path: '/courses/:id', component: () => import('@/pages/training/CourseDetailPage.vue') },
    { path: '/courses/:id/test', component: () => import('@/pages/training/TestPage.vue') },
    { path: '/my-courses', component: () => import('@/pages/training/MyCoursesPage.vue') },
    { path: '/my-courses/:id/manage', component: () => import('@/pages/training/ManageCoursePage.vue') },
    { path: '/results/:testId', component: () => import('@/pages/training/TestResultsPage.vue') },
  ]
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (!to.meta.public && !auth.isLoggedIn) return '/login'
})

export default router
