"use client";

import { useState, useRef, useEffect, useCallback } from "react";
import { useAiQuery } from "../hooks/useWallet";
import type { AiChatMessage } from "../types";
import { Sparkles, Send, X, Bot, User, Lightbulb } from "lucide-react";

const SUGGESTIONS = [
  "Monthly spending summary",
  "Top spending category",
  "Compare this month vs last month",
  "Unusual expenses",
  "Saving recommendations",
];

interface Props {
  walletId: number | null;
}

export function AIAssistantPanel({ walletId }: Props) {
  const [isOpen, setIsOpen] = useState(false);
  const [input, setInput] = useState("");
  const [messages, setMessages] = useState<AiChatMessage[]>([]);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const aiQueryMutation = useAiQuery();

  const scrollToBottom = useCallback(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, []);

  useEffect(() => {
    if (isOpen) {
      setTimeout(() => inputRef.current?.focus(), 300);
    }
  }, [isOpen]);

  useEffect(() => {
    scrollToBottom();
  }, [messages, scrollToBottom]);

  const handleSend = useCallback(async () => {
    const query = input.trim();
    if (!query || !walletId || aiQueryMutation.isPending) return;

    setInput("");

    const tempId = crypto.randomUUID();
    setMessages((prev) => [
      ...prev,
      { id: tempId, query, answer: "", timestamp: new Date() },
    ]);

    aiQueryMutation.mutate(
      { walletId, query },
      {
        onSuccess: (data) => {
          setMessages((prev) =>
            prev.map((msg) =>
              msg.id === tempId
                ? { ...msg, answer: data.answer, timestamp: new Date() }
                : msg
            )
          );
        },
        onError: () => {
          setMessages((prev) =>
            prev.map((msg) =>
              msg.id === tempId
                ? {
                  ...msg,
                  answer: "Sorry, I couldn't process your request. Please try again.",
                  timestamp: new Date(),
                }
                : msg
            )
          );
        },
      }
    );
  }, [input, walletId, aiQueryMutation]);

  const handleSuggestionClick = useCallback((suggestion: string) => {
    setInput(suggestion);
    inputRef.current?.focus();
  }, []);

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent) => {
      if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault();
        handleSend();
      }
    },
    [handleSend]
  );

  return (
    <div className="fixed bottom-6 right-6 z-50 flex flex-col items-end gap-3">
      {isOpen && (
        <>
          <div
            className="fixed inset-0 bg-black/40 z-40 md:hidden"
            onClick={() => setIsOpen(false)}
            aria-hidden="true"
          />
          <div
            className="relative z-50 w-[calc(100vw-2rem)] sm:w-100 max-h-150 bg-card/95 backdrop-blur-xl border border-border rounded-2xl shadow-2xl flex flex-col animate-slide-up"
            role="dialog"
            aria-label="AI Financial Assistant"
          >
            <div className="flex items-center justify-between px-4 py-3 border-b border-border shrink-0">
              <div className="flex items-center gap-2">
                <div className="p-1.5 rounded-lg bg-primary/10">
                  <Sparkles className="w-4 h-4 text-primary" />
                </div>
                <span className="font-semibold text-sm">AI Assistant</span>
              </div>
              <button
                onClick={() => setIsOpen(false)}
                className="p-1.5 rounded-lg hover:bg-muted transition-colors cursor-pointer"
                aria-label="Close AI Assistant"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            <div className="flex-1 overflow-y-auto px-4 py-3 space-y-4 min-h-0 max-h-90 scrollbar-hide">
              {messages.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-8 text-center">
                  <div className="p-3 rounded-full bg-primary/10 mb-3">
                    <Bot className="w-8 h-8 text-primary" />
                  </div>
                  <p className="text-sm font-medium text-foreground">
                    How can I help you?
                  </p>
                  <p className="text-xs text-muted-foreground mt-1">
                    Ask me anything about your finances
                  </p>
                </div>
              ) : (
                messages.map((msg) => (
                  <div key={msg.id} className="space-y-2">
                    <div className="flex items-start gap-2 justify-end">
                      <div className="bg-primary text-primary-foreground rounded-2xl rounded-tr-md px-3 py-2 max-w-[85%]">
                        <p className="text-sm">{msg.query}</p>
                      </div>
                      <div className="p-1.5 rounded-full bg-primary/10 shrink-0 mt-0.5">
                        <User className="w-3.5 h-3.5 text-primary" />
                      </div>
                    </div>
                    {msg.answer && (
                      <div className="flex items-start gap-2">
                        <div className="p-1.5 rounded-full bg-accent/10 shrink-0 mt-0.5">
                          <Bot className="w-3.5 h-3.5 text-accent" />
                        </div>
                        <div className="bg-muted rounded-2xl rounded-tl-md px-3 py-2 max-w-[85%]">
                          <p className="text-sm whitespace-pre-wrap">{msg.answer}</p>
                        </div>
                      </div>
                    )}
                    {!msg.answer && (
                      <div className="flex items-start gap-2">
                        <div className="p-1.5 rounded-full bg-accent/10 shrink-0 mt-0.5">
                          <Bot className="w-3.5 h-3.5 text-accent" />
                        </div>
                        <div className="bg-muted rounded-2xl rounded-tl-md px-3 py-2">
                          <div className="flex gap-1">
                            <span className="w-2 h-2 bg-muted-foreground/50 rounded-full animate-bounce" style={{ animationDelay: "0ms" }} />
                            <span className="w-2 h-2 bg-muted-foreground/50 rounded-full animate-bounce" style={{ animationDelay: "150ms" }} />
                            <span className="w-2 h-2 bg-muted-foreground/50 rounded-full animate-bounce" style={{ animationDelay: "300ms" }} />
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                ))
              )}
              <div ref={messagesEndRef} />
            </div>

            <div className="px-4 py-2 border-t border-border shrink-0">
              <div className="flex items-center gap-1 mb-2 overflow-x-auto">
                <Lightbulb className="w-3 h-3 text-muted-foreground shrink-0" />
                {SUGGESTIONS.map((suggestion) => (
                  <button
                    key={suggestion}
                    onClick={() => handleSuggestionClick(suggestion)}
                    className="whitespace-nowrap text-[11px] px-2 py-1 rounded-full bg-muted hover:bg-primary/10 hover:text-primary transition-colors text-muted-foreground shrink-0 cursor-pointer"
                  >
                    {suggestion}
                  </button>
                ))}
              </div>
              <div className="flex items-center gap-2 pb-2">
                <input
                  ref={inputRef}
                  type="text"
                  value={input}
                  onChange={(e) => setInput(e.target.value)}
                  onKeyDown={handleKeyDown}
                  placeholder="Ask anything about your finances..."
                  className="flex-1 bg-muted text-sm rounded-xl px-3 py-2 border border-border focus:outline-none focus:ring-1 focus:ring-primary placeholder:text-muted-foreground/60"
                  aria-label="Ask a question"
                  disabled={aiQueryMutation.isPending}
                />
                <button
                  onClick={handleSend}
                  disabled={!input.trim() || !walletId || aiQueryMutation.isPending}
                  className="p-2 rounded-xl bg-primary text-primary-foreground hover:bg-primary/90 transition-colors disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer shrink-0"
                  aria-label="Send message"
                >
                  <Send className="w-4 h-4" />
                </button>
              </div>
            </div>
          </div>
        </>
      )}

      <button
        onClick={() => setIsOpen((prev) => !prev)}
        className={`relative z-50 w-14 h-14 rounded-full shadow-lg flex items-center justify-center transition-all duration-300 cursor-pointer ${isOpen
          ? "bg-destructive text-destructive-foreground rotate-90"
          : "bg-primary text-primary-foreground hover:bg-primary/90 hover:scale-105"
          }`}
        aria-label={isOpen ? "Close AI Assistant" : "Open AI Assistant"}
      >
        {isOpen ? <X className="w-6 h-6" /> : <Sparkles className="w-6 h-6" />}
      </button>
    </div>
  );
}
