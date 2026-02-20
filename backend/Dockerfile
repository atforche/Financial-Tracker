FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /backend 

COPY . ./
RUN dotnet restore
RUN dotnet publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /backend
COPY --from=build /backend/.artifacts/publish .
ENTRYPOINT ["dotnet", "./Rest/Rest.dll"]