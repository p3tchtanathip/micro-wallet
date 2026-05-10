import { LoginForm } from '@/features/auth/components/LoginForm'
import Link from 'next/link'

export default function LoginPage() {
  return (
    <>
      <div className="text-center lg:text-left">
        <h2 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white">Welcome back</h2>
        <p className="text-slate-500 dark:text-slate-400 mt-2">Enter your credentials to access your account</p>
      </div>

      <LoginForm />

      <div className="text-center text-sm">
        <span className="text-slate-500">Don't have an account? </span>
        <Link href="/register" className="font-semibold text-blue-600 hover:text-blue-500 dark:text-blue-400 transition-colors">
          Sign up
        </Link>
      </div>
    </>
  )
}