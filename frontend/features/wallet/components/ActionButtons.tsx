'use client'

import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { depositSchema, withdrawSchema, transferSchema, DepositFormData, WithdrawFormData, TransferFormData } from '../schemas/wallet.schema'
import { useDeposit, useWithdraw, useTransfer } from '../hooks/useWallet'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog'
import { ArrowDownLeft, ArrowUpRight, ArrowRightLeft } from 'lucide-react'

export function ActionButtons({ walletId, walletNumber }: { walletId: number, walletNumber: string }) {
  const [depositOpen, setDepositOpen] = useState(false)
  const [withdrawOpen, setWithdrawOpen] = useState(false)
  const [transferOpen, setTransferOpen] = useState(false)

  // Deposit
  const depositForm = useForm<DepositFormData>({ resolver: zodResolver(depositSchema) })
  const depositMutation = useDeposit(walletId)
  const onDeposit = (data: DepositFormData) => {
    depositMutation.mutate(data, {
      onSuccess: () => { setDepositOpen(false); depositForm.reset() }
    })
  }

  // Withdraw
  const withdrawForm = useForm<WithdrawFormData>({ resolver: zodResolver(withdrawSchema) })
  const withdrawMutation = useWithdraw(walletId)
  const onWithdraw = (data: WithdrawFormData) => {
    withdrawMutation.mutate(data, {
      onSuccess: () => { setWithdrawOpen(false); withdrawForm.reset() }
    })
  }

  // Transfer
  const transferForm = useForm<TransferFormData>({ resolver: zodResolver(transferSchema) })
  const transferMutation = useTransfer(walletId, walletNumber)
  const onTransfer = (data: TransferFormData) => {
    transferMutation.mutate({ ...data, description: data.description || null }, {
      onSuccess: () => { setTransferOpen(false); transferForm.reset() }
    })
  }

  return (
    <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
      {/* Deposit */}
      <Dialog open={depositOpen} onOpenChange={setDepositOpen}>
        <DialogTrigger asChild>
          <button className="flex items-center justify-start p-3 rounded-xl border border-transparent bg-green-500/10 hover:bg-green-500/20 transition-colors group text-left cursor-pointer">
            <div className="p-2 rounded-full bg-green-500/20 text-green-700 dark:text-green-400 mr-3 group-hover:scale-110 transition-transform">
              <ArrowDownLeft size={20} />
            </div>
            <div>
              <div className="font-semibold text-sm text-green-800 dark:text-green-400">Deposit</div>
              <div className="text-[11px] text-green-600/70 dark:text-green-400/70">Add funds</div>
            </div>
          </button>
        </DialogTrigger>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Deposit Funds</DialogTitle>
            <DialogDescription>Add funds to your wallet balance.</DialogDescription>
          </DialogHeader>
          <form onSubmit={depositForm.handleSubmit(onDeposit)} className="space-y-4">
            <div className="space-y-2">
              <Label>Amount</Label>
              <Input type="number" step="0.01" {...depositForm.register('amount', { valueAsNumber: true })} />
              {depositForm.formState.errors.amount && <p className="text-sm text-red-500">{depositForm.formState.errors.amount.message}</p>}
            </div>
            <div className="space-y-2">
              <Label>Description (Optional)</Label>
              <Input {...depositForm.register('description')} />
            </div>
            <Button type="submit" disabled={depositMutation.isPending} className="w-full cursor-pointer">
              {depositMutation.isPending ? 'Processing...' : 'Confirm Deposit'}
            </Button>
          </form>
        </DialogContent>
      </Dialog>

      {/* Withdraw */}
      <Dialog open={withdrawOpen} onOpenChange={setWithdrawOpen}>
        <DialogTrigger asChild>
          <button className="flex items-center justify-start p-3 rounded-xl border border-transparent bg-red-500/10 hover:bg-red-500/20 transition-colors group text-left cursor-pointer">
            <div className="p-2 rounded-full bg-red-500/20 text-red-700 dark:text-red-400 mr-3 group-hover:scale-110 transition-transform">
              <ArrowUpRight size={20} />
            </div>
            <div>
              <div className="font-semibold text-sm text-red-800 dark:text-red-400">Withdraw</div>
              <div className="text-[11px] text-red-600/70 dark:text-red-400/70">Cash out</div>
            </div>
          </button>
        </DialogTrigger>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Withdraw Funds</DialogTitle>
            <DialogDescription>Withdraw funds from your wallet.</DialogDescription>
          </DialogHeader>
          <form onSubmit={withdrawForm.handleSubmit(onWithdraw)} className="space-y-4">
            <div className="space-y-2">
              <Label>Amount</Label>
              <Input type="number" step="0.01" {...withdrawForm.register('amount', { valueAsNumber: true })} />
              {withdrawForm.formState.errors.amount && <p className="text-sm text-red-500">{withdrawForm.formState.errors.amount.message}</p>}
            </div>
            <div className="space-y-2">
              <Label>Description (Optional)</Label>
              <Input {...withdrawForm.register('description')} />
            </div>
            <Button type="submit" disabled={withdrawMutation.isPending} className="w-full cursor-pointer">
              {withdrawMutation.isPending ? 'Processing...' : 'Confirm Withdrawal'}
            </Button>
          </form>
        </DialogContent>
      </Dialog>

      {/* Transfer */}
      <Dialog open={transferOpen} onOpenChange={setTransferOpen}>
        <DialogTrigger asChild>
          <button className="flex items-center justify-start p-3 rounded-xl border border-transparent bg-blue-500/10 hover:bg-blue-500/20 transition-colors group text-left cursor-pointer">
            <div className="p-2 rounded-full bg-blue-500/20 text-blue-700 dark:text-blue-400 mr-3 group-hover:scale-110 transition-transform">
              <ArrowRightLeft size={20} />
            </div>
            <div>
              <div className="font-semibold text-sm text-blue-800 dark:text-blue-400">Transfer</div>
              <div className="text-[11px] text-blue-600/70 dark:text-blue-400/70">Send money</div>
            </div>
          </button>
        </DialogTrigger>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Transfer Funds</DialogTitle>
            <DialogDescription>Send funds to another wallet.</DialogDescription>
          </DialogHeader>
          <form onSubmit={transferForm.handleSubmit(onTransfer)} className="space-y-4">
            <div className="space-y-2">
              <Label>To Wallet Number</Label>
              <Input type="text" placeholder="e.g. 6574548275" {...transferForm.register('toWalletNumber')} />
              {transferForm.formState.errors.toWalletNumber && <p className="text-sm text-red-500">{transferForm.formState.errors.toWalletNumber.message}</p>}
            </div>
            <div className="space-y-2">
              <Label>Amount</Label>
              <Input type="number" step="0.01" {...transferForm.register('amount', { valueAsNumber: true })} />
              {transferForm.formState.errors.amount && <p className="text-sm text-red-500">{transferForm.formState.errors.amount.message}</p>}
            </div>
            <div className="space-y-2">
              <Label>Description (Optional)</Label>
              <Input {...transferForm.register('description')} />
            </div>
            <Button type="submit" disabled={transferMutation.isPending} className="w-full cursor-pointer">
              {transferMutation.isPending ? 'Processing...' : 'Confirm Transfer'}
            </Button>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
