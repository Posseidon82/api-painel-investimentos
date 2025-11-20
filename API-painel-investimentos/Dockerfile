# Estágio base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Define variáveis de ambiente padrão para o container
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_HTTPS_PORTS=8081

# Estágio de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["API-painel-investimentos.csproj", "."]
RUN dotnet restore "./API-painel-investimentos.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./API-painel-investimentos.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Estágio de publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./API-painel-investimentos.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Estágio final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Adicione esta linha para garantir que o appsettings.json esteja presente
RUN mkdir -p /app && chmod -R 755 /app

#Diretório do volume
VOLUME /app/data

ENTRYPOINT ["dotnet", "API-painel-investimentos.dll"]