FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 53 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["src/Service/Service.csproj", "src/Service/"]
RUN dotnet restore "src/Service/Service.csproj"
COPY . .
WORKDIR "/src/src/Service"
RUN dotnet build "Service.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Service.csproj" -c $configuration -o /app/publish /p:UseAppHost=false
COPY ./src/Service/management/ /app/publish/management/

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Service.dll"]
