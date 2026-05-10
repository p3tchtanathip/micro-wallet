'use client'

import { Wallet } from 'lucide-react'

export default function AuthLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <main className="min-h-screen flex w-full bg-background">
      {/* Left side: branding/imagery */}
      <div className="hidden lg:flex w-1/2 bg-slate-900 text-white flex-col justify-between p-12 relative overflow-hidden">
        <div className="absolute top-[-10%] left-[-10%] w-[40%] h-[40%] rounded-full bg-blue-500/20 blur-[100px]" />
        <div className="absolute bottom-[-10%] right-[-10%] w-[40%] h-[40%] rounded-full bg-purple-500/20 blur-[100px]" />

        <div className="relative z-10 flex items-center gap-2 text-2xl font-bold">
          <Wallet className="w-8 h-8 text-blue-500" />
          MicroWallet
        </div>

        <div className="relative z-10 max-w-md mt-20">
          <h1 className="text-5xl font-bold mb-6 leading-tight">Manage your finances with ease</h1>
          <p className="text-slate-400 text-lg leading-relaxed">
            Use MicroWallet for everyday transactions and simple financial management.
          </p>
        </div>

        <div className="relative z-10 text-slate-500 text-sm">
          &copy; {new Date().getFullYear()} MicroWallet Inc. All rights reserved.
        </div>
      </div>

      {/* Right side: form */}
      <div className="w-full lg:w-1/2 flex items-center justify-center p-8 sm:p-12 bg-white dark:bg-slate-950">
        <div className="w-full max-w-md space-y-8">
          {children}
        </div>
      </div>
    </main>
  );
}