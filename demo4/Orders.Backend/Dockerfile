FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Orders.Backend/Orders.Backend.csproj", "Orders.Backend/"]
RUN dotnet restore "Orders.Backend/Orders.Backend.csproj"
COPY . .
WORKDIR "/src/Orders.Backend"
RUN dotnet build "Orders.Backend.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Orders.Backend.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Orders.Backend.dll"]
