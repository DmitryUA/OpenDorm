import axios from 'axios'

const genericTitles = new Set([
  'bad request',
  'domain rule violation',
  'forbidden',
  'internal server error',
  'not found',
  'one or more validation errors occurred.',
  'unauthorized',
  'an unexpected error occurred.',
  'validation error',
])

const getString = (value: unknown) =>
  typeof value === 'string' && value.trim() ? value.trim() : undefined

const getValidationMessages = (errors: unknown): string[] => {
  if (!errors || typeof errors !== 'object') return []

  return Object.values(errors).flatMap((messages) => {
    if (typeof messages === 'string') return getString(messages) ? [messages.trim()] : []
    if (!Array.isArray(messages)) return []
    return messages.flatMap((message) => (getString(message) ? [message.trim()] : []))
  })
}

export const getApiErrorMessage = (error: unknown, fallback: string): string => {
  if (!axios.isAxiosError<unknown>(error)) {
    return error instanceof Error ? error.message : fallback
  }

  const { status, data } = error.response ?? {}
  if (typeof data === 'string' && data.trim()) return data.trim()

  if (data && typeof data === 'object') {
    const problem = data as { detail?: unknown; errors?: unknown; title?: unknown }
    const detail = getString(problem.detail)
    if (detail && !genericTitles.has(detail.toLocaleLowerCase('en-US'))) return detail

    const validationMessages = getValidationMessages(problem.errors)
    if (validationMessages.length) return [...new Set(validationMessages)].join('; ')

    const title = getString(problem.title)
    if (title && !genericTitles.has(title.toLocaleLowerCase('en-US'))) return title
  }

  if (status === 403) return 'Недостаточно прав для выполнения операции.'
  if (status === 401 && !error.config?.url?.startsWith('/api/auth/login')) {
    return 'Сессия истекла. Войдите в систему снова.'
  }

  return fallback
}
