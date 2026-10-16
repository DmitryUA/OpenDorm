import { defineConfig } from 'orval'

export default defineConfig({
  openapi: {
    input: {
      // Путь к openapi.json
      target: '../docs/openapi.json', 
    },
    output: {
      mode: 'tags-split', // Генерирует файлы, разделенные по тегам из OpenAPI
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
