FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
LABEL stage=intermediate

RUN curl -sL https://deb.nodesource.com/setup_16.x |  bash - && \
    apt-get install -y nodejs

WORKDIR /src
COPY ["QA.Engine.Administration.WebApp.Core/QA.Engine.Administration.WebApp.Core.csproj", "QA.Engine.Administration.WebApp.Core/"]
COPY ["QA.Engine.Administration.Data.Core/QA.Engine.Administration.Data.Core.csproj", "QA.Engine.Administration.Data.Core/"]
COPY ["QA.Engine.Administration.Data.Interfaces.Core/QA.Engine.Administration.Data.Interfaces.Core.csproj", "QA.Engine.Administration.Data.Interfaces.Core/"]
COPY ["QA.Engine.Administration.Common.Core/QA.Engine.Administration.Common.Core.csproj", "QA.Engine.Administration.Common.Core/"]
COPY ["QA.Engine.Administration.Services.Core/QA.Engine.Administration.Services.Core.csproj", "QA.Engine.Administration.Services.Core/"]
COPY .nuget .
RUN dotnet restore "QA.Engine.Administration.WebApp.Core/QA.Engine.Administration.WebApp.Core.csproj"

WORKDIR /src/QA.Engine.Administration.WebApp.Core
COPY QA.Engine.Administration.WebApp.Core/package.json .
COPY QA.Engine.Administration.WebApp.Core/package-lock.json .
RUN npm ci

WORKDIR /src
COPY . .

WORKDIR /src/QA.Engine.Administration.WebApp.Core
RUN npm run build
RUN dotnet publish "QA.Engine.Administration.WebApp.Core.csproj" -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
ARG SERVICE_VERSION=0.0.0
ENV ServiceVersion=${SERVICE_VERSION}
WORKDIR /app
COPY --from=build-env /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "QA.Engine.Administration.WebApp.Core.dll"]
