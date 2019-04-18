FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Orders.Frontend/Orders.Frontend.csproj", "Orders.Frontend/"]
RUN dotnet restore "Orders.Frontend/Orders.Frontend.csproj"
COPY . .
WORKDIR "/src/Orders.Frontend"
RUN dotnet build "Orders.Frontend.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Orders.Frontend.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Orders.Frontend.dll"]