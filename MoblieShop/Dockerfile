FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MoblieShop.csproj", "."]
RUN dotnet restore "MoblieShop.csproj"
COPY . .
RUN dotnet publish "MoblieShop.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_URLS=http://+:5122
EXPOSE 5122

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MoblieShop.dll"]
