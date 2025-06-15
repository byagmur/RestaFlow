import { isClient } from '@vueuse/core'

export default defineNuxtRouteMiddleware((to) => {
  if (isClient) {
    const token = localStorage.getItem('token')
    if (!token && to.path !== '/login' && to.path !== '/register') {
      return navigateTo('/login')
    }
    if (token && (to.path === '/login' || to.path === '/register')) {
      return navigateTo('/')
    }
  }
})
