# ----- Etapa 1: build -----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os .csproj primeiro (cache de restore)
COPY GenericStore.WebApi/GenericStore.WebApi.csproj GenericStore.WebApi/
COPY GenericStore.Application/GenericStore.Application.csproj GenericStore.Application/
COPY GenericStore.Domain/GenericStore.Domain.csproj GenericStore.Domain/
COPY GenericStore.Infrastructure/GenericStore.Infrastructure.csproj GenericStore.Infrastructure/

RUN dotnet restore GenericStore.WebApi/GenericStore.WebApi.csproj

# Copia o resto do código
COPY . .

RUN dotnet publish GenericStore.WebApi/GenericStore.WebApi.csproj -c Release -o /app/publish --no-restore

# ----- Etapa 2: runtime -----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "GenericStore.WebApi.dll"]