# Estágio 1: Build (Compilação)
# Usamos uma imagem completa do SDK para compilar o código
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia apenas o arquivo de projeto e restaura as dependências (Cache inteligente)
COPY ["Api/Api.csproj", "Api/"]
RUN dotnet restore "Api/Api.csproj"

# Copia todo o resto do código e compila
COPY . .
WORKDIR "/src/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

# Publica a versão final otimizada
FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 2: Runtime (Execução)
# Usamos uma imagem leve (só o necessário para rodar)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
