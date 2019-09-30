FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build-env
LABEL stage=intermediate

RUN apt-get update && \
    apt-get install unzip && \
    apt-get install -y wget && \
    apt-get install -y gnupg2 && \
    wget -qO- https://deb.nodesource.com/setup_8.x | bash - && \
    apt-get install -y build-essential nodejs

WORKDIR /src
COPY ["QA.Engine.Administration.WebApp.Core/QA.Engine.Administration.WebApp.Core.csproj", "QA.Engine.Administration.WebApp.Core/"]
COPY ["QA.Engine.Administration.Data.Core/QA.Engine.Administration.Data.Core.csproj", "QA.Engine.Administration.Data.Core/"]
COPY ["QA.Engine.Administration.Data.Interfaces.Core/QA.Engine.Administration.Data.Interfaces.Core.csproj", "QA.Engine.Administration.Data.Interfaces.Core/"]
COPY ["QA.Engine.Administration.Common.Core/QA.Engine.Administration.Common.Core.csproj", "QA.Engine.Administration.Common.Core/"]
COPY ["QA.Engine.Administration.Services.Core/QA.Engine.Administration.Services.Core.csproj", "QA.Engine.Administration.Services.Core/"]
COPY ["QA.Engine.Administration.Logger.Core/QA.Engine.Administration.Logger.Core.csproj", "QA.Engine.Administration.Logger.Core/"]
COPY .nuget .
RUN dotnet restore "QA.Engine.Administration.WebApp.Core/QA.Engine.Administration.WebApp.Core.csproj"

WORKDIR /src/QA.Engine.Administration.WebApp.Core
COPY QA.Engine.Administration.WebApp.Core/package.json .
RUN npm install

WORKDIR /src
COPY . .

WORKDIR /src/QA.Engine.Administration.WebApp.Core
RUN npm run build
RUN dotnet publish "QA.Engine.Administration.WebApp.Core.csproj" -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim
ARG SERVICE_VERSION=0.0.0
ENV ServiceVersion=${SERVICE_VERSION}
WORKDIR /app
COPY --from=build-env /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "QA.Engine.Administration.WebApp.Core.dll"]
