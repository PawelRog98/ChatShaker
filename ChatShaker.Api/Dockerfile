FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 5005 
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ChatShaker.Api/ChatShaker.Api.csproj", "ChatShaker.Api/"]
COPY ["ChatShaker.Application/ChatShaker.Application.csproj", "ChatShaker.Application/"]
COPY ["ChatShaker.Infrastructure/ChatShaker.Infrastructure.csproj", "ChatShaker.Infrastructure/"]
COPY ["ChatShaker.Domain/ChatShaker.Domain.csproj", "ChatShaker.Domain/"]

RUN dotnet restore "ChatShaker.Api/ChatShaker.Api.csproj"

COPY ["ChatShaker.Api/", "ChatShaker.Api/"]
COPY ["ChatShaker.Application/", "ChatShaker.Application/"]
COPY ["ChatShaker.Infrastructure/", "ChatShaker.Infrastructure/"]
COPY ["ChatShaker.Domain/", "ChatShaker.Domain/"]
WORKDIR "/src/ChatShaker.Api"
RUN dotnet build "ChatShaker.Api.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "ChatShaker.Api.csproj" -c Debug -o /app/publish /p:UseAppHost=false  

FROM base AS final
WORKDIR /app
RUN curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /vsdbg
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatShaker.Api.dll"]  

RUN apt-get update && apt-get install -y curl bash \
    && curl -o /wait-for-it.sh https://raw.githubusercontent.com/vishnubob/wait-for-it/master/wait-for-it.sh \
    && chmod +x /wait-for-it.sh

ENTRYPOINT ["/bin/bash", "-c", "/wait-for-it.sh mssql:1433 -t 60 -- dotnet ChatShaker.Api.dll"]