FROM node:16-bullseye as front-builder
LABEL stage=intermediate-front

WORKDIR /src/QA.Engine.Administration.WebApp.Core
COPY QA.Engine.Administration.WebApp.Core/package.json .
COPY QA.Engine.Administration.WebApp.Core/package-lock.json .
RUN npm ci

COPY QA.Engine.Administration.WebApp.Core/ClientApp ClientApp
COPY QA.Engine.Administration.WebApp.Core/webpack.common.js .
COPY QA.Engine.Administration.WebApp.Core/webpack.prod.js .
COPY QA.Engine.Administration.WebApp.Core/tsconfig.json .
COPY QA.Engine.Administration.WebApp.Core/.babelrc .
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
LABEL stage=intermediate-back

WORKDIR /src
COPY ["QA.Engine.Administration.WebApp.Core/QA.Engine.Administration.WebApp.Core.csproj", "QA.Engine.Administration.WebApp.Core/"]
COPY ["QA.Engine.Administration.Data.Core/QA.Engine.Administration.Data.Core.csproj", "QA.Engine.Administration.Data.Core/"]
COPY ["QA.Engine.Administration.Data.Interfaces.Core/QA.Engine.Administration.Data.Interfaces.Core.csproj", "QA.Engine.Administration.Data.Interfaces.Core/"]
COPY ["QA.Engine.Administration.Common.Core/QA.Engine.Administration.Common.Core.csproj", "QA.Engine.Administration.Common.Core/"]
COPY ["QA.Engine.Administration.Services.Core/QA.Engine.Administration.Services.Core.csproj", "QA.Engine.Administration.Services.Core/"]
COPY .nuget .
RUN dotnet restore "QA.Engine.Administration.WebApp.Core/QA.Engine.Administration.WebApp.Core.csproj"

COPY . .

WORKDIR /src
RUN dotnet publish "QA.Engine.Administration.WebApp.Core/QA.Engine.Administration.WebApp.Core.csproj" -c Release -o /app/out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0
ARG SERVICE_VERSION=0.0.0
ENV ServiceVersion=${SERVICE_VERSION}
WORKDIR /app
COPY --from=front-builder /src/QA.Engine.Administration.WebApp.Core/wwwroot ./wwwroot
COPY --from=build-env /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "QA.Engine.Administration.WebApp.Core.dll"]
