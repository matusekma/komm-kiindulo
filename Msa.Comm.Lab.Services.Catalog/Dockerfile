#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Msa.Comm.Lab.Services.Catalog/Msa.Comm.Lab.Services.Catalog.csproj", "Msa.Comm.Lab.Services.Catalog/"]
RUN dotnet restore "Msa.Comm.Lab.Services.Catalog/Msa.Comm.Lab.Services.Catalog.csproj"
COPY . .
WORKDIR "/src/Msa.Comm.Lab.Services.Catalog"
RUN dotnet build "Msa.Comm.Lab.Services.Catalog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Msa.Comm.Lab.Services.Catalog.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Msa.Comm.Lab.Services.Catalog.dll"]