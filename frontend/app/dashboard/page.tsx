"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useSession, signOut } from "next-auth/react";
import { useTotalBalance, useWalletTransactions, useWallets, useCreateWallet } from "@/features/wallet/hooks/useWallet";
import { ActionButtons } from "@/features/wallet/components/ActionButtons";
import { formatCurrency } from "@/lib/utils";
import {
  Wallet,
  CreditCard,
  LogOut,
  Activity,
  ArrowDownLeft,
  ArrowUpRight,
  ArrowRightLeft,
  ChevronLeft,
  ChevronRight,
  ChevronDown,
  User
} from "lucide-react";
import { Transaction } from "@/features/wallet/types";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { Button } from '@/components/ui/button'

export default function DashboardPage() {
  const router = useRouter();
  const { data: session, status } = useSession();
  const [mounted, setMounted] = useState(false);
  const [pageNumber, setPageNumber] = useState(1);
  const pageSize = 10;

  const [selectedWalletId, setSelectedWalletId] = useState<number | null>(null);
  const [showAddWalletModal, setShowAddWalletModal] = useState(false);
  const [newWalletCurrency, setNewWalletCurrency] = useState<'THB' | 'USD'>('THB');

  const { data: walletsData, isLoading: isWalletsLoading } = useWallets();
  const createWalletMutation = useCreateWallet();

  useEffect(() => {
    if (walletsData && walletsData.length > 0 && !selectedWalletId) {
      setSelectedWalletId(walletsData[0].walletId);
    }
  }, [walletsData, selectedWalletId]);

  const { data: totalBalanceData, isLoading: isTotalBalanceLoading } = useTotalBalance();
  const { data: txData, isLoading: isTxLoading, isFetching: isTxFetching } = useWalletTransactions(selectedWalletId || 0, pageNumber, pageSize);

  const selectedWallet = walletsData?.find(w => w.walletId === selectedWalletId);

  useEffect(() => {
    setMounted(true);
    if (status === "unauthenticated") {
      router.push("/login");
    }
  }, [status, router]);

  if (!mounted || status === "loading" || status === "unauthenticated") return null;

  const transactions: Transaction[] = Array.isArray(txData) ? txData : txData?.items || [];
  const hasNextPage = txData?.hasNextPage ?? (transactions.length === pageSize);
  const hasPreviousPage = txData?.hasPreviousPage ?? (pageNumber > 1);

  const getTxIcon = (type: string) => {
    switch (type) {
      case "Deposit": return <ArrowDownLeft className="w-5 h-5 text-green-500" />;
      case "Withdraw": return <ArrowUpRight className="w-5 h-5 text-red-500" />;
      case "Transfer": return <ArrowRightLeft className="w-5 h-5 text-blue-500" />;
      default: return <Activity className="w-5 h-5 text-muted-foreground" />;
    }
  };

  const getTxLabel = (type: string) => {
    switch (type) {
      case "Deposit": return "Deposit";
      case "Withdraw": return "Withdrawal";
      case "Transfer": return "Transfer";
      default: return "Unknown";
    }
  };

  return (
    <div className="min-h-screen bg-background">
      <nav className="fixed top-0 left-0 right-0 z-50 bg-card/80 backdrop-blur-xl border-b border-border">
        <div className="flex items-center justify-between px-4 py-3">
          <div className="flex items-center gap-3">
            <div className="flex items-center gap-2">
              <Wallet className="w-6 h-6 text-primary" />
              <span className="text-xl font-bold bg-linear-to-r from-primary to-accent bg-clip-text text-transparent">
                MicroWallet
              </span>
            </div>
          </div>
          <div className="flex items-center gap-3">
            {session?.user?.name && (
              <div className="hidden sm:flex items-center gap-2">
                <User size={16} />
                <span className="text-sm font-bold">
                  {session.user.name}
                </span>
              </div>
            )}
            <button onClick={() => { signOut({ callbackUrl: '/login' }); }} className="p-2 hover:bg-muted rounded-lg">
              <LogOut className="w-5 h-5" />
            </button>
          </div>
        </div>
      </nav>

      <main className="p-4 pt-24 lg:p-8 lg:pt-24">
        <div className="max-w-6xl mx-auto space-y-6">
          <div className="flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
            <div>
              <h1 className="text-2xl font-bold">
                Welcome
                {session?.user?.name && (
                  <span className="ml-1">
                    {session.user.name}
                  </span>
                )}
              </h1>
              <p className="text-muted-foreground">Here&apos;s your financial overview</p>
            </div>
            <div className="text-left md:text-right">
              <p className="text-sm text-muted-foreground">Total Balance</p>
              {isTotalBalanceLoading ? (
                <div className="h-9 w-32 bg-muted animate-pulse rounded-md mt-1" />
              ) : (
                <p className="text-3xl font-bold">{formatCurrency(totalBalanceData?.totalBalanceTHB || 0, 'THB')}</p>
              )}
            </div>
          </div>

          {/* Quick Actions */}
          <div className="p-4 rounded-2xl bg-card border border-border shadow-sm">
            <h2 className="text-lg font-bold mb-4">Quick Actions</h2>
            {selectedWalletId && selectedWallet ? (
              <ActionButtons walletId={selectedWalletId} walletNumber={selectedWallet.walletNumber || ''} />
            ) : (
              <p className="text-muted-foreground">Please select or create a wallet first.</p>
            )}
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className={`p-6 rounded-2xl bg-linear-to-br from-blue-500 to-purple-600 text-white relative overflow-hidden`}>
              <div className="absolute right-[-10%] top-[-10%] opacity-10 pointer-events-none">
                <Wallet size={150} />
              </div>
              <div className="flex items-center justify-between mb-4 relative z-10">
                {isWalletsLoading ? (
                  <span className="text-white/80">Loading wallets...</span>
                ) : (
                  <div className="relative inline-flex items-center">
                    <select
                      className="bg-transparent text-white font-medium border-b border-white/30 focus:outline-none appearance-none cursor-pointer pr-6 pb-1"
                      value={selectedWalletId || ''}
                      onChange={(e) => {
                        if (e.target.value === 'add_new') {
                          setShowAddWalletModal(true);
                        } else {
                          setSelectedWalletId(Number(e.target.value));
                          setPageNumber(1); // Reset pagination on wallet change
                        }
                      }}
                    >
                      {walletsData?.map(w => (
                        <option key={w.walletId} value={w.walletId} className="text-black">
                          {w.walletNumber || `Wallet #${w.walletId}`} ({w.currency})
                        </option>
                      ))}
                      <option value="add_new" className="text-black font-bold">+ Add New Wallet</option>
                    </select>
                    <ChevronDown className="absolute right-0 top-1/2 -translate-y-1/2 mb-1 w-4 h-4 text-white/80 pointer-events-none" />
                  </div>
                )}
                <div className="flex items-center gap-2">
                  <span className="px-2 py-0.5 bg-white/20 rounded text-[10px] font-bold tracking-wider uppercase backdrop-blur-sm">
                    {selectedWallet?.currency || 'THB'}
                  </span>
                  <CreditCard className="w-5 h-5 text-white/80" />
                </div>
              </div>
              {isWalletsLoading ? (
                <div className="h-8 w-40 bg-white/20 animate-pulse rounded-md mb-1 relative z-10" />
              ) : (
                <p className="text-3xl font-bold mb-1 relative z-10">{formatCurrency(selectedWallet?.balance || 0, selectedWallet?.currency || 'THB')}</p>
              )}
            </div>

            <div className={`p-6 rounded-2xl bg-linear-to-br from-emerald-500 to-teal-600 text-white relative overflow-hidden`}>
              <div className="flex items-center justify-between mb-4 relative z-10">
                <span className="text-white/80">Transactions</span>
                <Activity className="w-5 h-5 text-white/80" />
              </div>
              {isTxLoading ? (
                <div className="h-8 w-16 bg-white/20 animate-pulse rounded-md mb-1 relative z-10" />
              ) : (
                <p className="text-3xl font-bold mb-1 relative z-10">{txData?.totalCount || 0}</p>
              )}
              <p className="text-sm text-white/60 relative z-10">Total operations</p>
            </div>
          </div>

          <div className="bg-card border border-border rounded-2xl p-4 shadow-sm">
            <h2 className="text-lg font-bold mb-4">Recent Transactions</h2>
            <div className="space-y-4">
              {isTxLoading ? (
                <p className="text-muted-foreground">Loading transactions...</p>
              ) : transactions.length === 0 ? (
                <p className="text-muted-foreground text-sm">No recent transactions.</p>
              ) : (
                transactions.map((tx: Transaction, index: number) => (
                  <div key={index} className="flex items-center justify-between p-3 hover:bg-muted/50 rounded-lg border border-transparent hover:border-border transition-colors">
                    <div className="flex items-center gap-3">
                      <div className={`p-2 rounded-lg ${tx.type === "Deposit" ? "bg-green-500/10" : tx.type === "Withdraw" ? "bg-red-500/10" : "bg-blue-500/10"}`}>
                        {getTxIcon(tx.type)}
                      </div>
                      <div>
                        <p className="font-medium">
                          {getTxLabel(tx.type)}
                          {tx.type === "Transfer" && tx.counterpartyName && (
                            <span className="font-normal text-muted-foreground ml-1">
                              {tx.direction === "Sent" ? `to ${tx.counterpartyName}` : `from ${tx.counterpartyName}`}
                            </span>
                          )}
                        </p>
                        {tx.description && (
                          <p className="text-sm text-muted-foreground italic">&quot;{tx.description}&quot;</p>
                        )}
                        <p className="text-xs text-muted-foreground mt-0.5">{new Date(tx.createdAt || new Date()).toLocaleString()}</p>
                      </div>
                    </div>
                    <p className={`font-bold ${tx.amount > 0 ? "text-green-500" : tx.amount < 0 ? "text-red-500" : ""}`}>
                      {tx.amount > 0 ? "+" : ""}{formatCurrency(tx.amount, selectedWallet?.currency)}
                    </p>
                  </div>
                ))
              )}
            </div>

            {/* Pagination Controls */}
            {transactions.length > 0 && (
              <div className="flex items-center justify-between mt-6 pt-4 border-t border-border">
                <button
                  disabled={!hasPreviousPage || isTxFetching}
                  onClick={() => setPageNumber(p => Math.max(1, p - 1))}
                  className="flex items-center gap-1 text-sm font-medium text-muted-foreground hover:text-foreground disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <ChevronLeft size={16} /> Previous
                </button>
                <span className="text-sm text-muted-foreground">
                  Page {pageNumber} {txData?.totalPages ? `of ${txData.totalPages}` : ''}
                </span>
                <button
                  disabled={!hasNextPage || isTxFetching}
                  onClick={() => setPageNumber(p => p + 1)}
                  className="flex items-center gap-1 text-sm font-medium text-muted-foreground hover:text-foreground disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Next <ChevronRight size={16} />
                </button>
              </div>
            )}
          </div>
        </div>
      </main>

      <Dialog open={showAddWalletModal} onOpenChange={setShowAddWalletModal}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Create New Wallet</DialogTitle>
            <DialogDescription>
              Select a currency for your new wallet. You can have multiple wallets in different currencies.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4 py-4">
            <label className="text-sm font-medium text-muted-foreground">Select Currency</label>
            <div className="grid grid-cols-2 gap-3 mt-2">
              <button
                onClick={() => setNewWalletCurrency('THB')}
                className={`p-4 rounded-xl border transition-all flex flex-col items-center gap-2 cursor-pointer ${newWalletCurrency === 'THB'
                  ? 'border-primary bg-primary/5 text-primary ring-1 ring-primary'
                  : 'border-border hover:bg-muted'
                  }`}
              >
                <span className="text-2xl font-bold">฿</span>
                <span className="font-semibold">THB</span>
              </button>
              <button
                onClick={() => setNewWalletCurrency('USD')}
                className={`p-4 rounded-xl border transition-all flex flex-col items-center gap-2 cursor-pointer ${newWalletCurrency === 'USD'
                  ? 'border-primary bg-primary/5 text-primary ring-1 ring-primary'
                  : 'border-border hover:bg-muted'
                  }`}
              >
                <span className="text-2xl font-bold">$</span>
                <span className="font-semibold">USD</span>
              </button>
            </div>
          </div>

          <div className="flex justify-end gap-3 mt-4">
            <Button
              variant="ghost"
              onClick={() => setShowAddWalletModal(false)}
              className="cursor-pointer"
              disabled={createWalletMutation.isPending}
            >
              Cancel
            </Button>
            <Button
              onClick={() => {
                createWalletMutation.mutate(newWalletCurrency, {
                  onSuccess: (newWallet) => {
                    setShowAddWalletModal(false);
                    setSelectedWalletId(newWallet.walletId);
                  }
                });
              }}
              disabled={createWalletMutation.isPending}
              className="cursor-pointer"
            >
              {createWalletMutation.isPending ? 'Creating...' : 'Create Wallet'}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}