ARG DOTNET_SDK_IMAGE
ARG DOTNET_RUNTIME_IMAGE
FROM ${DOTNET_SDK_IMAGE} AS build
WORKDIR /backend

COPY . ./
RUN dotnet restore Migrator/Migrator.csproj
RUN dotnet publish Migrator/Migrator.csproj --configuration Release --no-restore

FROM ${DOTNET_RUNTIME_IMAGE}
WORKDIR /migrator
COPY --from=build --chown=$APP_UID /backend/.artifacts/publish/Migrator .
USER $APP_UID
ENTRYPOINT ["dotnet", "./Migrator.dll"]
