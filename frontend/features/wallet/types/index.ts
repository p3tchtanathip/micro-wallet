export interface TotalBalanceResponse {
  totalBalanceTHB: number;
  exchangeRate: number;
  lastUpdated: string | null;
}

export interface WalletResponse {
  walletId: number;
  walletNumber: string | null;
  currency: 'THB' | 'USD';
  balance: number;
}

export enum TransactionType {
  Deposit = 1,
  Withdrawal = 2,
  Transfer = 3
}

export interface DepositCommand {
  walletId: number;
  amount: number;
  description: string | null;
}

export interface WithdrawCommand {
  walletId: number;
  amount: number;
  description: string | null;
}

export interface TransferCommand {
  fromWalletNumber: string;
  toWalletNumber: string;
  amount: number;
  description: string | null;
}

export interface Transaction {
  referenceNo: string;
  type: string;
  status: string;
  amount: number;
  description: string | null;
  createdAt: string;
  counterpartyName: string | null;
  counterpartyWalletId: number | null;
  direction: string;
}

export interface PaginatedTransactionsResponse {
  items: Transaction[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
