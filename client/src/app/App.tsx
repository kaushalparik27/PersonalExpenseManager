import { RouterProvider } from 'react-router-dom'
import { Toaster } from '@/components/ui/sonner'
import { AuthProvider } from '@/app/providers/AuthProvider'
import { QueryProvider } from '@/app/providers/QueryProvider'
import { ThemeProvider } from '@/app/providers/ThemeProvider'
import { router } from '@/app/router'

export function App() {
  return (
    <ThemeProvider>
      <QueryProvider>
        <AuthProvider>
          <RouterProvider router={router} />
          <Toaster richColors closeButton position="top-right" />
        </AuthProvider>
      </QueryProvider>
    </ThemeProvider>
  )
}
