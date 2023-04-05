FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Poly6502.API/Poly6502.API.csproj", "Poly6502.API/"]
RUN dotnet restore "Poly6502.API/Poly6502.API.csproj"
COPY . .
WORKDIR "/src/Poly6502.API"
RUN dotnet build "Poly6502.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Poly6502.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Poly6502.API.dll"]
