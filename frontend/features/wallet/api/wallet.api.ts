import { apiClient } from '@/lib/api-client'
import { v4 as uuidv4 } from 'uuid'
import {
  DepositCommand,
  WithdrawCommand,
  TransferCommand,
  TransactionType,
  PaginatedTransactionsResponse,
  WalletResponse,
  TotalBalanceResponse
} from '../types'

export const getWallets = async (): Promise<WalletResponse[]> => {
  const response = await apiClient.get<WalletResponse[]>('Wallet')
  return response.data
}

export const getTotalBalance = async (): Promise<TotalBalanceResponse> => {
  const response = await apiClient.get<TotalBalanceResponse>('Wallet/balances')
  return response.data
}

export const getTransactions = async (walletId: number, pageNumber = 1, pageSize = 10, transactionType?: TransactionType): Promise<PaginatedTransactionsResponse> => {
  const params = new URLSearchParams()
  params.append('pageNumber', pageNumber.toString())
  params.append('pageSize', pageSize.toString())
  if (transactionType) {
    params.append('transactionType', transactionType.toString())
  }

  const response = await apiClient.get(`Wallet/${walletId}/transactions?${params.toString()}`)
  return response.data
}

export const deposit = async (data: DepositCommand): Promise<void> => {
  await apiClient.post('Wallet/deposit', data, {
    headers: { 'Idempotency-Key': uuidv4() }
  })
}

export const withdraw = async (data: WithdrawCommand): Promise<void> => {
  await apiClient.post('/Wallet/withdraw', data, {
    headers: { 'Idempotency-Key': uuidv4() }
  })
}

export const transfer = async (data: TransferCommand): Promise<void> => {
  await apiClient.post('/Wallet/transfer', data, {
    headers: { 'Idempotency-Key': uuidv4() }
  })
}

export const createWallet = async (currency: 'THB' | 'USD'): Promise<WalletResponse> => {
  const response = await apiClient.post<WalletResponse>('/Wallet', { currency })
  return response.data
}
