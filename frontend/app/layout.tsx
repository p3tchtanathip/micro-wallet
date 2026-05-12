import type { Metadata } from "next";
import localFont from "next/font/local";
import { QueryProvider } from "@/components/providers/QueryProvider";
import { NextAuthProvider } from "@/providers/NextAuthProvider";
import "./globals.css";

const inter = localFont({ src: "../public/fonts/Inter-VariableFont_opsz,wght.ttf", variable: "--font-inter" });

export const metadata: Metadata = {
  title: "MicroWallet",
  description: "Secure your finances with MicroWallet",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" className={`${inter.variable} h-full`}>
      <body className={`${inter.className} h-full antialiased`}>
        <NextAuthProvider>
          <QueryProvider>
            {children}
          </QueryProvider>
        </NextAuthProvider>
      </body>
    </html>
  );
}