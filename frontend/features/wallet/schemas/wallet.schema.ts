import { z } from 'zod'

export const depositSchema = z.object({
  amount: z.number().positive("Amount must be positive"),
  description: z.string().optional(),
})
export type DepositFormData = z.infer<typeof depositSchema>

export const withdrawSchema = z.object({
  amount: z.number().positive("Amount must be positive"),
  description: z.string().optional(),
})
export type WithdrawFormData = z.infer<typeof withdrawSchema>

export const transferSchema = z.object({
  toWalletNumber: z.string().min(1, "Valid wallet number is required"),
  amount: z.number().positive("Amount must be positive"),
  description: z.string().optional(),
})
export type TransferFormData = z.infer<typeof transferSchema>
