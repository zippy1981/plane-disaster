# One image per ODBC driver: the harness, unixODBC, and the driver that drivers/<DRIVER>/install.sh sets up.
# Build from the repository root:
#   docker build -f experiments/linux-odbc/Dockerfile --build-arg DRIVER=sqliteodbc -t plane-disaster-odbc:sqliteodbc .
ARG DOTNET_VERSION=11.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src
COPY Directory.Build.props ./
COPY PlaneDisaster.Dba/ PlaneDisaster.Dba/
COPY experiments/linux-odbc/harness/ experiments/linux-odbc/harness/
RUN dotnet publish experiments/linux-odbc/harness -f net11.0 -c Release -o /harness -nologo

FROM mcr.microsoft.com/dotnet/runtime:${DOTNET_VERSION}
ARG DRIVER
# unixODBC's driver manager; System.Data.Odbc loads libodbc.so.2.
RUN apt-get update -qq \
 && apt-get install -y -qq --no-install-recommends unixodbc ca-certificates curl unzip >/dev/null \
 && rm -rf /var/lib/apt/lists/*
COPY experiments/linux-odbc/drivers/${DRIVER}/ /driver/
RUN bash /driver/install.sh
COPY --from=build /harness /harness
ENTRYPOINT ["bash", "/driver/run.sh"]
