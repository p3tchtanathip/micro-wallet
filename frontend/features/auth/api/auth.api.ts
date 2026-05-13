import { apiClient } from '@/lib/api-client'
import { RegisterFormData } from '../schemas/auth.schema'

export const register = async (data: RegisterFormData): Promise<number> => {
  const response = await apiClient.post<number>('Auth/register', data)
  return response.data
}
