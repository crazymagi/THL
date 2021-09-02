FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["THL.WebApi/THL.WebApi.csproj", "THL.WebApi/"]
RUN dotnet restore "THL.WebApi/THL.WebApi.csproj"
COPY . .
WORKDIR "/src/THL.WebApi"
RUN dotnet build "THL.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "THL.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "THL.WebApi.dll"]
