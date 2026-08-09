ARG DOTNET_SDK_IMAGE
ARG DOTNET_RUNTIME_IMAGE
FROM ${DOTNET_SDK_IMAGE} AS build
WORKDIR /backend 

COPY . ./
RUN dotnet restore
RUN dotnet publish

FROM ${DOTNET_RUNTIME_IMAGE}
WORKDIR /backend
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build --chown=$APP_UID /backend/.artifacts/publish .
USER $APP_UID
HEALTHCHECK --interval=10s --timeout=3s --start-period=15s --retries=3 \
    CMD curl --fail --silent --show-error http://127.0.0.1:8080/health/ready || exit 1
ENTRYPOINT ["dotnet", "./Rest/Rest.dll"]
