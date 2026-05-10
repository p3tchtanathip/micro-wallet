import axios from 'axios'
import { getSession, signIn } from 'next-auth/react'

export const apiClient = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5168',
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request interceptor
apiClient.interceptors.request.use(
  async (config) => {
    // Only fetch session on the client
    if (typeof window !== 'undefined') {
      const session = await getSession();
      if (session && (session as any).accessToken && config.headers) {
        config.headers.Authorization = `Bearer ${(session as any).accessToken}`
      }
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config
    
    // Handle 401 Unauthorized globally
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true
      
      if (typeof window !== 'undefined') {
        // Since we removed manual refresh token logic in favor of NextAuth,
        // if we get a 401, we should probably force the user to sign in again.
        // Implementing NextAuth refresh token rotation is beyond the scope of this file.
        signIn();
      }
    }
    
    return Promise.reject(error)
  }
)
