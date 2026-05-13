# Stage 1: Base
FROM node:24-alpine AS base
RUN apk add --no-cache libc6-compat
WORKDIR /app

# Stage 2: Development
FROM base AS development
ENV NODE_ENV=development
# The source will be mounted via volumes in docker-compose
CMD ["npm", "run", "dev"]

# Stage 3: Build
FROM base AS builder
COPY package*.json ./
RUN npm ci
COPY . .
ENV NEXT_TELEMETRY_DISABLED=1

# NEXT_PUBLIC_ vars must be available at build time (they get inlined into JS)
ARG NEXT_PUBLIC_API_URL=/api/
ENV NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL}

RUN npm run build

# Stage 4: Production
FROM base AS production
WORKDIR /app
ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1

RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

COPY --from=builder /app/public ./public
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static

USER nextjs
EXPOSE 3000
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

CMD ["node", "server.js"]