import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '@/utils/api'

export const useTrainingStore = defineStore('training', () => {
  const courses       = ref<any[]>([])
  const currentCourse = ref<any>(null)
  const currentTest   = ref<any>(null)
  const testResults   = ref<any[]>([])
  const loading       = ref(false)
  const error         = ref<string | null>(null)

  // ── Courses ───────────────────────────────────────────────────────────────

  async function getCourses() {
    loading.value = true; error.value = null
    try { const { data } = await api.get('Course/Courses'); courses.value = data.courses ?? [] }
    catch (e: any) { error.value = e.message }
    finally { loading.value = false }
  }

  async function getMyCourses() {
    loading.value = true; error.value = null
    try { const { data } = await api.get('Course/MyCourses'); courses.value = data.courses ?? [] }
    catch (e: any) { error.value = e.message }
    finally { loading.value = false }
  }

  async function getCourseDetails(id: number) {
    loading.value = true; error.value = null
    try { const { data } = await api.get(`Course/CourseDetails/${id}`); currentCourse.value = data }
    catch (e: any) { error.value = e.message }
    finally { loading.value = false }
  }

  async function addUpdateCourse(model: any) {
    const { data } = await api.post('Course/AddUpdateCourse', model)
    return data
  }

  async function addUpdateSection(model: any) {
    const { data } = await api.post('Course/AddUpdateSection', model)
    return data
  }

  async function deleteSection(id: number) {
    const { data } = await api.delete(`Course/Section/${id}`)
    return data
  }

  // ── Tests ─────────────────────────────────────────────────────────────────

  async function getTest(courseId: number) {
    loading.value = true; error.value = null
    try { const { data } = await api.get(`Test/Test/${courseId}`); currentTest.value = data }
    catch (e: any) { error.value = e.message }
    finally { loading.value = false }
  }

  async function getMyTest(courseId: number) {
    loading.value = true; error.value = null
    try { const { data } = await api.get(`Test/MyTest/${courseId}`); currentTest.value = data }
    catch (e: any) { error.value = e.message }
    finally { loading.value = false }
  }

  async function addUpdateTest(model: any) {
    const { data } = await api.post('Test/AddUpdateTest', model)
    return data
  }

  async function addUpdateQuestion(model: any) {
    const { data } = await api.post('Test/AddUpdateQuestion', model)
    return data
  }

  async function deleteQuestion(id: number) {
    const { data } = await api.delete(`Test/Question/${id}`)
    return data
  }

  async function submitTest(payload: { testId: number; answers: { questionId: number; selectedAnswerId: number }[] }) {
    loading.value = true
    try {
      const { data } = await api.post('Test/submit', {
        TestId: payload.testId,
        Answers: payload.answers.map(a => ({ QuestionId: a.questionId, SelectedAnswerId: a.selectedAnswerId })),
        ExamDate: new Date()
      })
      return data
    } catch (e: any) { error.value = e.message; return null }
    finally { loading.value = false }
  }

  async function getTestResults(testId: number) {
    loading.value = true
    try { const { data } = await api.get(`Test/TestResults/${testId}`); testResults.value = data }
    catch (e: any) { error.value = e.message }
    finally { loading.value = false }
  }

  function getCertificateUrl(resultId: number) {
    return `${import.meta.env.VITE_API_URL}Test/GenerateCertificate/${resultId}`
  }

  // ── Files ─────────────────────────────────────────────────────────────────

  async function uploadFile(file: File): Promise<{ id: string; name: string; url: string } | null> {
    const form = new FormData()
    form.append('file', file)
    try {
      const { data } = await api.post('File/upload', form, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      return data
    } catch (e: any) {
      error.value = e.response?.data?.message ?? 'Upload failed'
      return null
    }
  }

  function getFileUrl(id: string | null | undefined) {
    if (!id) return null
    return `${import.meta.env.VITE_API_URL}File/${id}`
  }

  async function getFileInfo(id: string) {
    try {
      const { data } = await api.get(`File/${id}/info`)
      return data
    } catch { return null }
  }

  return {
    courses, currentCourse, currentTest, testResults, loading, error,
    getCourses, getMyCourses, getCourseDetails,
    addUpdateCourse, addUpdateSection, deleteSection,
    getTest, getMyTest, addUpdateTest, addUpdateQuestion, deleteQuestion,
    submitTest, getTestResults, getCertificateUrl,
    uploadFile, getFileUrl, getFileInfo
  }
})
