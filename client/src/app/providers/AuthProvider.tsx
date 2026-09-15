import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createContext, useContext, useEffect, type ReactNode } from 'react'
import { authApi } from '@/features/auth/api/authApi'
import type { LoginPayload, RegisterPayload, UserProfile } from '@/features/auth/types'
import { UNAUTHORIZED_EVENT } from '@/lib/api/client'

const AUTH_QUERY_KEY = ['auth', 'me'] as const

interface AuthContextValue {
  currentUser: UserProfile | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (payload: LoginPayload) => Promise<void>
  logout: () => Promise<void>
  register: (payload: RegisterPayload) => Promise<void>
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()

  const meQuery = useQuery({
    queryKey: AUTH_QUERY_KEY,
    queryFn: authApi.getCurrentUser,
    staleTime: 5 * 60_000,
    retry: false,
  })

  useEffect(() => {
    const handleUnauthorized = () => queryClient.setQueryData(AUTH_QUERY_KEY, null)
    window.addEventListener(UNAUTHORIZED_EVENT, handleUnauthorized)
    return () => window.removeEventListener(UNAUTHORIZED_EVENT, handleUnauthorized)
  }, [queryClient])

  const loginMutation = useMutation({
    mutationFn: authApi.login,
    onSuccess: (user) => queryClient.setQueryData(AUTH_QUERY_KEY, user),
  })

  const registerMutation = useMutation({
    mutationFn: authApi.register,
    onSuccess: (user) => queryClient.setQueryData(AUTH_QUERY_KEY, user),
  })

  const logoutMutation = useMutation({
    mutationFn: authApi.logout,
    onSuccess: () => queryClient.setQueryData(AUTH_QUERY_KEY, null),
  })

  const value: AuthContextValue = {
    currentUser: meQuery.data ?? null,
    isAuthenticated: Boolean(meQuery.data),
    isLoading: meQuery.isLoading,
    login: async (payload) => {
      await loginMutation.mutateAsync(payload)
    },
    logout: async () => {
      await logoutMutation.mutateAsync()
    },
    register: async (payload) => {
      await registerMutation.mutateAsync(payload)
    },
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
