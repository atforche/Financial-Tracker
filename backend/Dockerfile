# check=skip=InvalidDefaultArgInFrom

# Base images are pinned in config/toolchain.toml and always passed as build args.
ARG DOTNET_SDK_IMAGE
ARG DOTNET_RUNTIME_IMAGE
FROM ${DOTNET_SDK_IMAGE} AS build
WORKDIR /backend

# Restore from the project files alone so source edits reuse the cached restore.
COPY Directory.Build.props Directory.Packages.props ./
COPY Data/Data.csproj Data/
COPY Domain/Domain.csproj Domain/
COPY Models/Models.csproj Models/
COPY Rest/Rest.csproj Rest/
COPY Migrator/Migrator.csproj Migrator/
RUN dotnet restore Rest/Rest.csproj \
    && dotnet restore Migrator/Migrator.csproj

# Analyzers and OpenAPI generation are enforced by the backend CI job, so the
# image build skips them.
COPY . ./
RUN dotnet publish Rest/Rest.csproj --configuration Release --no-restore \
        -p:RunAnalyzers=false -p:OpenApiGenerateDocuments=false \
    && dotnet publish Migrator/Migrator.csproj --configuration Release --no-restore \
        -p:RunAnalyzers=false

FROM ${DOTNET_RUNTIME_IMAGE} AS migrator
WORKDIR /migrator
COPY --from=build --chown=$APP_UID /backend/.artifacts/publish/Migrator .
USER $APP_UID
ENTRYPOINT ["dotnet", "./Migrator.dll"]

FROM ${DOTNET_RUNTIME_IMAGE} AS backend
WORKDIR /backend
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build --chown=$APP_UID /backend/.artifacts/publish/Rest ./Rest
USER $APP_UID
HEALTHCHECK --interval=10s --timeout=3s --start-period=15s --retries=3 \
    CMD curl --fail --silent --show-error http://127.0.0.1:8080/health/ready || exit 1
ENTRYPOINT ["dotnet", "./Rest/Rest.dll"]
