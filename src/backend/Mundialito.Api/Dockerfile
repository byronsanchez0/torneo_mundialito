FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["src/backend/Mundialito.Domain/Mundialito.Domain.csproj", "src/backend/Mundialito.Domain/"]
COPY ["src/backend/Mundialito.Application/Mundialito.Application.csproj", "src/backend/Mundialito.Application/"]
COPY ["src/backend/Mundialito.Infrastructure/Mundialito.Infrastructure.csproj", "src/backend/Mundialito.Infrastructure/"]
COPY ["src/backend/Mundialito.Api/Mundialito.Api.csproj", "src/backend/Mundialito.Api/"]
RUN dotnet restore "src/backend/Mundialito.Api/Mundialito.Api.csproj"

COPY . .
RUN dotnet publish "src/backend/Mundialito.Api/Mundialito.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8081
EXPOSE 8081
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Mundialito.Api.dll"]

