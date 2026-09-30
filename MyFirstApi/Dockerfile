FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MyFirstApi/MyFirstApi.csproj", "MyFirstApi/"]
RUN dotnet restore "MyFirstApi/MyFirstApi.csproj"

COPY MyFirstApi/ MyFirstApi/
WORKDIR "/src/MyFirstApi"

RUN dotnet publish "MyFirstApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

USER $APP_UID

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MyFirstApi.dll"]