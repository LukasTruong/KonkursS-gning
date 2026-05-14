FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["KonkursCheck.Worker/KonkursCheck.Worker.csproj", "KonkursCheck.Worker/"]
COPY ["KonkursCheck.Application/KonkursCheck.Application.csproj", "KonkursCheck.Application/"]
COPY ["KonkursCheck.Infrastructure/KonkursCheck.Infrastructure.csproj", "KonkursCheck.Infrastructure/"]
COPY ["KonkursCheck.Domain/KonkursCheck.Domain.csproj", "KonkursCheck.Domain/"]
RUN dotnet restore "KonkursCheck.Worker/KonkursCheck.Worker.csproj"
COPY . .
RUN dotnet publish "KonkursCheck.Worker/KonkursCheck.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KonkursCheck.Worker.dll"]
