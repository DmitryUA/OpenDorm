import { defineConfig } from 'orval'

export default defineConfig({
  openapi: {
    input: {
      target: '../docs/openapi.json',
    },
    output: {
      mode: 'split', // Плоская структура, каждая функция экспортируется отдельно
      target: 'src/shared/api/generated/endpoints.ts',
      schemas: 'src/shared/api/generated/models',
      client: 'axios',
      override: {
        mutator: {
          path: './src/shared/api/axiosInstance.ts',
          name: 'customInstance',
        },
      },
    },
  },
})
