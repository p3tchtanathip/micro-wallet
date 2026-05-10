import { RegisterForm } from '@/features/auth/components/RegisterForm'
import Link from 'next/link'

export default function RegisterPage() {
  return (
    <>
      <div className="text-center lg:text-left">
        <h2 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white">Create an account</h2>
        <p className="text-slate-500 dark:text-slate-400 mt-2">Fill in your details below to get started</p>
      </div>
      
      <RegisterForm />
      
      <div className="text-center text-sm">
        <span className="text-slate-500">Already have an account? </span>
        <Link href="/login" className="font-semibold text-blue-600 hover:text-blue-500 dark:text-blue-400 transition-colors">
          Log in
        </Link>
      </div>
    </>
  )
}