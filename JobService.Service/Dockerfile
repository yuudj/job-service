#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 81

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["JobService.Service/JobService.Service.csproj", "JobService.Service/"]
COPY ["JobService.Components/JobService.Components.csproj", "JobService.Components/"]
RUN dotnet restore "JobService.Service/JobService.Service.csproj"
COPY . .
WORKDIR "/src/JobService.Service"
RUN dotnet build "JobService.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JobService.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JobService.Service.dll"]