# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. ก๊อปปี้เฉพาะ .csproj ของตัวหลักเข้ามา restore เพื่อให้ได้ cache ที่เร็วที่สุด
COPY ["MyFirstApi/MyFirstApi.csproj", "MyFirstApi/"]
RUN dotnet restore "MyFirstApi/MyFirstApi.csproj"

# 2. ก๊อปปี้เฉพาะโฟลเดอร์โปรเจกต์หลักเข้ามา (ไม่เอา .Tests เข้ามา)
COPY MyFirstApi/ MyFirstApi/
WORKDIR "/src/MyFirstApi"

# 3. Publish โปรเจกต์
RUN dotnet publish "MyFirstApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# แนะนำ: ใช้ non-root user เพื่อความปลอดภัย (ค่าเริ่มต้นใน .NET 8)
USER $APP_UID

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MyFirstApi.dll"]