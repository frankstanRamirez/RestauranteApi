FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RestaurantFlow.Api/RestaurantFlow.Api.csproj", "RestaurantFlow.Api/"]
COPY ["RestaurantFlow.Application/RestaurantFlow.Application.csproj", "RestaurantFlow.Application/"]
COPY ["RestaurantFlow.Domain/RestaurantFlow.Domain.csproj", "RestaurantFlow.Domain/"]
COPY ["RestaurantFlow.Infrastructure/RestaurantFlow.Infrastructure.csproj", "RestaurantFlow.Infrastructure/"]
RUN dotnet restore "RestaurantFlow.Api/RestaurantFlow.Api.csproj"
COPY . .
WORKDIR "/src/RestaurantFlow.Api"
RUN dotnet build "RestaurantFlow.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RestaurantFlow.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RestaurantFlow.Api.dll"]
