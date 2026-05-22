# Stage 1: Base
FROM node:24-alpine AS base
RUN apk add --no-cache libc6-compat
WORKDIR /app

# -------------------------
# Stage 2: Development
# -------------------------
FROM base AS development
ENV NODE_ENV=development

# install deps
COPY package*.json ./
RUN npm install

# source will be mounted in docker-compose
CMD ["npm", "run", "dev"]

# -------------------------
# Stage 3: Builder
# -------------------------
FROM base AS builder
COPY package*.json ./
RUN npm ci
COPY . .

ENV NEXT_TELEMETRY_DISABLED=1

ARG NEXT_PUBLIC_API_URL=/api/
ENV NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL}

RUN npm run build

# -------------------------
# Stage 4: Production (Standalone)
# -------------------------
FROM node:24-alpine AS production
WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1

# install runtime dependencies
RUN apk add --no-cache libc6-compat

RUN addgroup --system --gid 1001 nodejs \
    && adduser --system --uid 1001 nextjs

# copy standalone build
COPY --from=builder /app/public ./public
COPY --from=builder /app/.next/static ./.next/static
COPY --from=builder /app/.next/standalone .

USER nextjs

EXPOSE 3000
ENV PORT=3000
ENV HOSTNAME="0.0.0.0"

# IMPORTANT: standalone uses server.js
CMD ["node", "server.js"]