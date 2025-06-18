import type { ServiceResponse } from '~/types'
import { defineStore } from 'pinia'
import { z } from 'zod'


export const useAuthStore = defineStore('auth', () => {
  const router = useRouter()
  const config = useRuntimeConfig()
  const baseUrl = config.public.apiBase

  const isAuthenticated = ref(false)
  const userInfo = ref({
    firstName: '',
    lastName: '',
    role: '',
    id: 0,
    isAuth: false,
    token: '',
  })
  
  const isLoading = ref(false)
  const errorMessage = ref('')
  const successMessage = ref('')
  
  const form = ref({
    username: '',
    email: '',
    password: '',
    passwordConfirm: '',
  })
  const schema = z.object({
    username: z.string().min(3, 'Kullanıcı adı en az 3 karakter olmalıdır'),
    email: z.string().email('Geçerli bir email adresi giriniz'),
    password: z.string().min(6, 'Şifre en az 6 karakter olmalıdır'),
    passwordConfirm: z.string(),
  }).refine(data => data.password === data.passwordConfirm, {
    message: 'Şifreler eşleşmiyor',
    path: ['passwordConfirm'],
  })
  const loginSchema = z.object({
    username: z.string().min(1, 'Kullanıcı adı alanı boş bırakılamaz'),
    password: z.string().min(1, 'Şifre alanı boş bırakılamaz'),
  })
  const changeAuth = () => {
    isAuthenticated.value = !isAuthenticated.value
  }
  const loadUserInfo = () => {
    if (typeof window !== 'undefined') { // Tarayıcıda çalıştığından emin ol
      const storedUserInfo = localStorage.getItem('userInfo')
      if (storedUserInfo) {
        userInfo.value = JSON.parse(storedUserInfo)
        isAuthenticated.value = true
      }
    }
  }
  const saveUserInfo = () => {
    if (typeof window !== 'undefined') { // Tarayıcıda çalıştığından emin ol
      localStorage.setItem('userInfo', JSON.stringify(userInfo.value))
    }
  }
  const clearUserInfo = () => {
    if (typeof window !== 'undefined') { // Tarayıcıda çalıştığından emin ol
      localStorage.removeItem('userInfo')
    }
  }
  async function register() {
    errorMessage.value = ''
    successMessage.value = ''

    const result = schema.safeParse(form.value)
    if (!result.success) {
      const error = result.error.errors[0]
      errorMessage.value = error.message
      return
    }
    if (!form.value.username || !form.value.email || !form.value.password || !form.value.passwordConfirm) {
      errorMessage.value = 'Lütfen tüm alanları doldurun'
      return
    }
    isLoading.value = true
    try {
      const response = await $fetch<ServiceResponse>(`${baseUrl}/Account/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          username: form.value.username,
          email: form.value.email,
          password: form.value.password,
        }),
      })
      if (response.success) {
        successMessage.value = 'Kayıt başarılı!'
        // toast.add({
        //   id: 'register-success',
        //   title: 'Başarılı',
        //   description: response.message || 'Kayıt başarılı!',
        //   color: 'green',
        //   icon: 'i-heroicons-check-circle-solid',
        //   timeout: 5000,
        // });
        setTimeout(() => {
          router.replace('/')
        }, 1000)
      }
      else {
        errorMessage.value = 'Kayıt başarısız oldu!'
        // toast.add({
        //   id: 'register-failed',
        //   title: 'Hata',
        //   description: response.message || 'Kayıt başarısız oldu!',
        //   color: 'red',
        //   icon: 'i-heroicons-x-circle-solid',
        //   timeout: 5000,
        // });
      }
    }
    catch (error: any) {
      errorMessage.value = 'Sunucu hatası oluştu!'
      // toast.add({
      //   id: 'server-error',
      //   title: 'Hata',
      //   description: error?.data?.message || 'Sunucu hatası oluştu!',
      //   color: 'red',
      //   icon: 'i-heroicons-x-circle-solid',
      //   timeout: 5000,
      // });
    }
    finally {
      isLoading.value = false
    }
  }
  const login = async () => {
    const result = loginSchema.safeParse(form.value)
    if (!result.success) {
      const error = result.error.errors[0]
      errorMessage.value = error.message
      return
    }

    isLoading.value = true
    errorMessage.value = ''
    try {
      const response = await $fetch<ServiceResponse>(`${baseUrl}/Account/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          username: form.value.username,
          password: form.value.password,
        }),
      })
      userInfo.value = {
        firstName: response.userFName || '',
        lastName: response.userLName || '',
        role: response.role || '',
        id: response.userId || 0,
        isAuth: response.isAuth || false,
        token: response.token || '',
      }
      changeAuth()
      console.log('yetkilendimre:', isAuthenticated.value)
      console.log('Kullanıcı bilgileri:', userInfo.value)

      console.log('Token:', response.token)
      // login işlemi başarılıysa:
      localStorage.setItem('token', response.token ? response.token : '')
      if (!response.success) {
        throw new Error(response.message || 'Kullanıcı adı veya şifre hatalı!')
        errorMessage.value = 'Kullanıcı adı veya şifre hatalı!'
      }
      // toast.add({
      //   title: 'Başarılı',
      //   description: response.message || 'Giriş başarılı!',
      //   color: 'green',
      //   icon: 'i-heroicons-check-circle',
      // });
      router.replace('/')
      // Başarılı girişte kullanıcı bilgilerini sakla
      saveUserInfo()
    }
    catch (error: any) {
      errorMessage.value = error.message || 'Giriş sırasında bir hata oluştu!'
    }
    finally {
      isLoading.value = false
    }
  }

  const logout = () => {
    isAuthenticated.value = false
    userInfo.value = {
      firstName: '',
      lastName: '',
      role: '',
      id: 0,
      isAuth: false,
      token: '',
    }
    clearUserInfo() // Kullanıcı bilgilerini temizle
    router.push('/login')
    localStorage.removeItem('token')
    console.log('Çıkış yapıldı, token silindi')
  }
  // Sayfa yüklendiğinde kullanıcı bilgilerini yükle
  loadUserInfo()
  return {
    loadUserInfo,
    userInfo,
    isLoading,
    errorMessage,
    successMessage,
    form,
    login,
    logout,
    isAuthenticated,
    register,
  }
}, {
  persist: true,
})
