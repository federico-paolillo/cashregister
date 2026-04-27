# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY be/global.json be/Directory.Build.props be/Cashregister.slnx ./be/
COPY be/Cashregister.Activities/Cashregister.Activities.csproj ./be/Cashregister.Activities/
COPY be/Cashregister.Api/Cashregister.Api.csproj ./be/Cashregister.Api/
COPY be/Cashregister.Application/Cashregister.Application.csproj ./be/Cashregister.Application/
COPY be/Cashregister.Commons/Cashregister.Commons.csproj ./be/Cashregister.Commons/
COPY be/Cashregister.Database/Cashregister.Database.csproj ./be/Cashregister.Database/
COPY be/Cashregister.Domain/Cashregister.Domain.csproj ./be/Cashregister.Domain/
COPY be/Cashregister.Printmon/Cashregister.Printmon.csproj ./be/Cashregister.Printmon/
COPY be/Cashregister.Printmon.Emulator/Cashregister.Printmon.Emulator.csproj ./be/Cashregister.Printmon.Emulator/

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore be/Cashregister.Api/Cashregister.Api.csproj --runtime linux-x64

COPY be/ ./be/

RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish be/Cashregister.Api/Cashregister.Api.csproj \
        --configuration Release \
        --runtime linux-x64 \
        --self-contained true \
        --no-restore \
        -p:PublishSingleFile=true \
        -p:PublishAot=false \
        -p:IncludeNativeLibrariesForSelfExtract=false \
        --output /app/publish

RUN mkdir -p /runtime/var/lib/cashregister \
    && find /app/publish -type d -exec chmod 0555 {} + \
    && find /app/publish -type f -exec chmod 0444 {} + \
    && chmod 0555 /app/publish/Cashregister.Api

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled AS runtime

ENV ASPNETCORE_URLS=http://0.0.0.0:65000
ENV DOTNET_URLS=http://0.0.0.0:65000

ENV CASHREGISTER_DATASOURCE=/var/lib/cashregister/cashregister.db
ENV CASHREGISTER_FILEDEVICE__TARGET=/dev/null
ENV CASHREGISTER_MARKDOWNDEVICE__ROOTFOLDER=/tmp

WORKDIR /app

COPY --from=build --chown=0:0 /app/publish/ /app/
COPY --from=build --chown=1654:1654 /runtime/var/lib/cashregister/ /var/lib/cashregister/

USER $APP_UID

EXPOSE 65000

ENTRYPOINT ["./Cashregister.Api"]
