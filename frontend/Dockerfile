FROM node:22-alpine AS build
WORKDIR /frontend

ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL
COPY . ./
RUN npm install
RUN npx vite build

FROM nginx:alpine
COPY --from=build /frontend/dist /usr/share/nginx/html
EXPOSE $NGINX_PORT
CMD ["nginx", "-g", "daemon off;"]