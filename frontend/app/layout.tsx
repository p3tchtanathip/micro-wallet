import type { Metadata, Viewport } from "next";
import localFont from "next/font/local";
import { QueryProvider } from "@/components/providers/QueryProvider";
import { NextAuthProvider } from "@/providers/NextAuthProvider";
import "./globals.css";

const inter = localFont({ src: "../public/fonts/Inter-VariableFont_opsz,wght.ttf", variable: "--font-inter" });

export const metadata: Metadata = {
  title: "MicroWallet",
  description: "Secure your finances with MicroWallet",
};

export const viewport: Viewport = {
  themeColor: "#0f172a",
  colorScheme: "dark",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" className={`${inter.variable} h-full dark`}>
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