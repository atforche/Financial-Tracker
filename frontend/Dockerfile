FROM node:22-alpine AS build
WORKDIR /frontend

COPY . ./
RUN npm install
RUN npx next build
ENTRYPOINT ["sh", "-c", "npx next start --port $FRONTEND_PORT"]