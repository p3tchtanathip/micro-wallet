import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getWallets, getTotalBalance, getTransactions, deposit, withdraw, transfer, createWallet, aiQuery } from '../api/wallet.api'
import { TransferCommand, TransactionType, WalletResponse, AiQueryCommand } from '../types'

export const useWallets = (enabled = true) => {
  return useQuery({
    queryKey: ['wallets'],
    queryFn: getWallets,
    enabled,
  })
}

export const useTotalBalance = (enabled = true) => {
  return useQuery({
    queryKey: ['totalBalance'],
    queryFn: getTotalBalance,
    enabled,
  })
}

export const useWalletTransactions = (walletId: number, pageNumber = 1, pageSize = 10, transactionType?: TransactionType) => {
  return useQuery({
    queryKey: ['walletTransactions', walletId, pageNumber, pageSize, transactionType],
    queryFn: () => getTransactions(walletId, pageNumber, pageSize, transactionType),
    enabled: !!walletId,
  })
}

export const useDeposit = (walletId: number) => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: { amount: number; description?: string }) => 
      deposit({ walletId, amount: data.amount, description: data.description || null }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['totalBalance'] })
      queryClient.invalidateQueries({ queryKey: ['wallets'] })
      queryClient.invalidateQueries({ queryKey: ['walletTransactions', walletId] })
    },
  })
}

export const useWithdraw = (walletId: number) => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: { amount: number; description?: string }) => 
      withdraw({ walletId, amount: data.amount, description: data.description || null }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['totalBalance'] })
      queryClient.invalidateQueries({ queryKey: ['wallets'] })
      queryClient.invalidateQueries({ queryKey: ['walletTransactions', walletId] })
    },
  })
}

export const useTransfer = (walletId: number, fromWalletNumber: string) => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: Omit<TransferCommand, 'fromWalletNumber'>) => transfer({ fromWalletNumber, ...data }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['totalBalance'] })
      queryClient.invalidateQueries({ queryKey: ['wallets'] })
      queryClient.invalidateQueries({ queryKey: ['walletTransactions', walletId] })
    },
  })
}

export const useAiQuery = () => {
  return useMutation({
    mutationFn: (data: AiQueryCommand) => aiQuery(data),
  })
}

export const useCreateWallet = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (currency: 'THB' | 'USD') => createWallet(currency),
    onSuccess: (newWallet) => {
      // Update wallets list in cache
      queryClient.setQueryData<WalletResponse[]>(['wallets'], (old) => {
        return old ? [...old, newWallet] : [newWallet]
      })
      // Also invalidate total balance as it will change
      queryClient.invalidateQueries({ queryKey: ['totalBalance'] })
    },
  })
}
