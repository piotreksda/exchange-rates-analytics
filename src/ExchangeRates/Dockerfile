FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_HTTP_PORTS=80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ExchangeRates.csproj", "ExchangeRates/"]
RUN dotnet restore "ExchangeRates/ExchangeRates.csproj"
COPY . ExchangeRates/
WORKDIR "/src/ExchangeRates"
RUN dotnet build "ExchangeRates.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ExchangeRates.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExchangeRates.dll"]
