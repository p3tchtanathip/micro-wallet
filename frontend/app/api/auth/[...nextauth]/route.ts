import NextAuth, { NextAuthOptions } from "next-auth";
import GoogleProvider from "next-auth/providers/google";
import CredentialsProvider from "next-auth/providers/credentials";
import { JWT } from "next-auth/jwt";

const API_BASE_URL = process.env.BACKEND_API_URL || process.env.NEXT_PUBLIC_API_URL;

function parseJwt(token: string) {
  try {
    return JSON.parse(Buffer.from(token.split('.')[1], 'base64').toString());
  } catch (e) {
    return null;
  }
}

async function refreshAccessToken(token: JWT) {
  try {
    const res = await fetch(`${API_BASE_URL}/Auth/refresh`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        accessToken: token.accessToken,
        refreshToken: token.refreshToken,
      }),
    });

    const refreshedTokens = await res.json();

    if (!res.ok) {
      throw refreshedTokens;
    }

    const decoded = parseJwt(refreshedTokens.accessToken);

    return {
      ...token,
      accessToken: refreshedTokens.accessToken,
      refreshToken: refreshedTokens.refreshToken ?? token.refreshToken, // Fall back to old refresh token
      accessTokenExpires: decoded?.exp ? decoded.exp * 1000 : Date.now() + 60 * 60 * 1000,
    };
  } catch (error) {
    console.error("Error refreshing access token", error);
    return {
      ...token,
      error: "RefreshAccessTokenError",
    };
  }
}

export const authOptions: NextAuthOptions = {
  providers: [
    GoogleProvider({
      clientId: process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID || "",
      clientSecret: process.env.GOOGLE_CLIENT_SECRET || "",
      idToken: true,
    }),
    CredentialsProvider({
      name: "Credentials",
      credentials: {
        email: { label: "Email", type: "email" },
        password: { label: "Password", type: "password" }
      },
      async authorize(credentials) {
        if (!credentials?.email || !credentials?.password) {
          throw new Error("Missing credentials");
        }

        try {
          const loginUrl = `${API_BASE_URL}/Auth/login`;
          console.log(`Attempting login at: ${loginUrl}`);
          
          const res = await fetch(loginUrl, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({
              email: credentials.email,
              password: credentials.password,
            }),
          });

          if (!res.ok) {
            console.error(`Login failed with status: ${res.status}`);
            throw new Error("Invalid credentials");
          }

          const data = await res.json();
          console.log("Login successful, token received");

          if (data && data.accessToken) {
            // We return an object that will be passed to the jwt callback as `user`
            return {
              id: "0", // placeholder
              email: credentials.email,
              accessToken: data.accessToken,
              refreshToken: data.refreshToken,
            };
          }

          return null;
        } catch (error) {
          console.error("Auth error:", error);
          return null;
        }
      }
    })
  ],
  callbacks: {
    async jwt({ token, user, account }) {
      // Initial sign in
      if (account && user) {
        if (account.provider === "google") {
          // Send id_token to backend
          try {
            const res = await fetch(`${API_BASE_URL}/Auth/google-login`, {
              method: "POST",
              headers: {
                "Content-Type": "application/json",
              },
              body: JSON.stringify(account.id_token),
            });

            if (res.ok) {
              const data = await res.json();
              token.accessToken = data.accessToken;
              token.refreshToken = data.refreshToken;
              const decoded = parseJwt(data.accessToken);
              token.accessTokenExpires = decoded?.exp ? decoded.exp * 1000 : Date.now() + 60 * 60 * 1000;
              if (decoded?.name) {
                token.name = decoded.name;
              }
            } else {
              console.error("Failed to authenticate Google token with backend");
            }
          } catch (error) {
            console.error("Error calling backend Google login:", error);
          }
        } else if (account.provider === "credentials") {
          // The user object contains the tokens we returned from authorize
          token.accessToken = (user as any).accessToken;
          token.refreshToken = (user as any).refreshToken;
          const decoded = parseJwt((user as any).accessToken);
          token.accessTokenExpires = decoded?.exp ? decoded.exp * 1000 : Date.now() + 60 * 60 * 1000;
          if (decoded?.name) {
            token.name = decoded.name;
          }
        }
        return token;
      }

      // Return previous token if the access token has not expired yet
      // Adding a buffer (e.g., 5 seconds) to prevent edge cases
      if (token.accessTokenExpires && Date.now() < (token.accessTokenExpires as number) - 5000) {
        return token;
      }

      // Access token has expired, try to update it
      return refreshAccessToken(token);
    },
    async session({ session, token }) {
      // Send properties to the client
      (session as any).accessToken = token.accessToken;
      (session as any).error = token.error;
      if (token.name && session.user) {
        session.user.name = token.name;
      }
      return session;
    }
  },
  pages: {
    signIn: "/login",
  },
  session: {
    strategy: "jwt",
  },
};

const handler = NextAuth(authOptions);

export { handler as GET, handler as POST };
