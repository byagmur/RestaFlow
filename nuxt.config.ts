import process from 'node:process'
import tailwindcss from '@tailwindcss/vite'
// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-05-15',
  devtools: { enabled: true },
  modules: [
    '@nuxt/fonts',
    '@nuxt/icon',
    '@nuxt/ui',
    '@pinia/nuxt',
    'nuxt-booster',
    '@nuxt/image',
  ],
  css: [
    '@/assets/css/main.css',
  ],
  vite: {
    plugins: [
      tailwindcss(),
      
    ],
  },
  components: [
    {
      path: '~/components',
      pathPrefix: false,
    },
  ],
  
 nitro: {
    // IIS options default
    iis: {
      // merges in a pre-existing web.config file to the nitro default file
      mergeConfig: true,
      // overrides the default nitro web.config file all together
      overrideConfig: false,
    },
  },
 colorMode: {
    preference: 'light'
  },
  runtimeConfig: {
    public: {
      apiBase: process.env.NUXT_PUBLIC_API_BASE,
    },
    // router: {
    //   middleware: ['auth'],
    // },
  },
  app: {
    head: {
      title: 'RestaFlow - Restoran Yönetiminde Akıllı Çözüm',
      link: [
        { rel: 'icon', type: 'image/png', href: '/favicon.ico' },
      ],
      meta: [
        { name: 'description', content: '' },
      ],
    },
  },
  devServer: {
    port: 3000, // Geliştirme sunucusu için port numarası
    host: '0.0.0.0',
  },

})
