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
      "@accounts": "/src/accounts",
      "@accounting-periods": "/src/accounting-periods",
      "@data": "/src/data",
      "@framework": "/src/framework",
      "@funds": "/src/funds",
      "@navigation": "/src/navigation",
      "@src": "/src",
      "@transactions": "/src/transactions"
    }
  }
})
