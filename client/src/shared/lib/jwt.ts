import { jwtDecode } from 'jwt-decode'

export interface JwtPayload {
  sub: string
  unique_name: string
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': string[]
  permissions: string[]
  exp: number
  iss: string
  aud: string
}

export const decodeToken = (token: string): JwtPayload | null => {
  try {
    return jwtDecode<JwtPayload>(token)
  } catch {
    return null
  }
}

export const isTokenExpired = (token: string): boolean => {
  const payload = decodeToken(token)
  if (!payload) return true

  const currentTime = Math.floor(Date.now() / 1000)
  return payload.exp < currentTime
}
