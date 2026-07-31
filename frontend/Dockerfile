FROM node:22-alpine AS dependencies
WORKDIR /app

COPY package.json package-lock.json ./
RUN --mount=type=cache,target=/root/.npm npm ci

FROM node:22-alpine AS build
WORKDIR /app

COPY --from=dependencies /app/node_modules ./node_modules
COPY . ./
RUN --mount=type=cache,target=/app/.next/cache npm run build

FROM build AS production-dependencies
RUN npm prune --omit=dev

FROM node:22-alpine AS runtime
WORKDIR /app
ENV NODE_ENV=production

COPY --from=production-dependencies --chown=node:node /app/node_modules ./node_modules
COPY --from=build --chown=node:node /app/.next ./.next
RUN rm -rf /usr/local/lib/node_modules/npm /usr/local/bin/npm /usr/local/bin/npx

USER node
ENTRYPOINT ["./node_modules/.bin/next", "start", "--port", "3000"]
