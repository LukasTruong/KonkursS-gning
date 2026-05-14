FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["KonkursCheck.Api/KonkursCheck.Api.csproj", "KonkursCheck.Api/"]
COPY ["KonkursCheck.Application/KonkursCheck.Application.csproj", "KonkursCheck.Application/"]
COPY ["KonkursCheck.Infrastructure/KonkursCheck.Infrastructure.csproj", "KonkursCheck.Infrastructure/"]
COPY ["KonkursCheck.Domain/KonkursCheck.Domain.csproj", "KonkursCheck.Domain/"]
RUN dotnet restore "KonkursCheck.Api/KonkursCheck.Api.csproj"
COPY . .
RUN dotnet publish "KonkursCheck.Api/KonkursCheck.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KonkursCheck.Api.dll"]
