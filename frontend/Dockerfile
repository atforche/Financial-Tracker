ARG NODE_IMAGE
FROM ${NODE_IMAGE} AS dependencies
WORKDIR /app

COPY package.json package-lock.json ./
RUN --mount=type=cache,target=/root/.npm npm ci

FROM ${NODE_IMAGE} AS build
WORKDIR /app

COPY --from=dependencies /app/node_modules ./node_modules
COPY . ./
RUN --mount=type=cache,target=/app/.next/cache npm run build

FROM build AS production-dependencies
RUN npm prune --omit=dev

FROM ${NODE_IMAGE} AS runtime
WORKDIR /app
ENV NODE_ENV=production

RUN apk upgrade --no-cache

COPY --from=production-dependencies --chown=node:node /app/node_modules ./node_modules
COPY --from=build --chown=node:node /app/.next ./.next
RUN rm -rf /usr/local/lib/node_modules/npm /usr/local/bin/npm /usr/local/bin/npx

USER node
HEALTHCHECK --interval=10s --timeout=3s --start-period=15s --retries=3 \
    CMD ["node", "-e", "fetch('http://127.0.0.1:3000/login').then(response => { if (!response.ok) process.exit(1) }).catch(() => process.exit(1))"]
ENTRYPOINT ["./node_modules/.bin/next", "start", "--port", "3000"]
