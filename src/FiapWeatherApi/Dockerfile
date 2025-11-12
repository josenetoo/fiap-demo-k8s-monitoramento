# 🚀 FIAP Weather API - Dockerfile Otimizado
# Multi-stage build para aplicação .NET 8

# ============================================
# STAGE 1: Build da aplicação
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivo de projeto primeiro (para cache de layers)
COPY ["FiapWeatherApi.csproj", "."]

# Restaurar dependências (layer cacheável)
RUN dotnet restore "FiapWeatherApi.csproj"

# Copiar código fonte
COPY . .

# Build da aplicação
RUN dotnet build "FiapWeatherApi.csproj" -c Release -o /app/build

# ============================================
# STAGE 2: Publish da aplicação
# ============================================
FROM build AS publish
RUN dotnet publish "FiapWeatherApi.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# ============================================
# STAGE 3: Runtime final
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Configurar usuário não-root para segurança
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Criar diretório da aplicação
WORKDIR /app

# Copiar arquivos publicados
COPY --from=publish /app/publish .

# Configurar permissões
RUN chown -R appuser:appuser /app
USER appuser

# Configurar variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Expor porta
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Labels para identificação
LABEL maintainer="FIAP POS Tech - Arquitetura .NET" \
      version="1.0" \
      description="Weather API com OpenTelemetry para demo de observabilidade"

# Comando de inicialização
ENTRYPOINT ["dotnet", "FiapWeatherApi.dll"]
