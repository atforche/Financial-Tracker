import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    host: true,
    port: 5000
  },
  resolve: {
    alias: {
      "@core": "/src/core",
      "@data": "/src/data",
      "@ui": "/src/ui",
    }
  }
})
