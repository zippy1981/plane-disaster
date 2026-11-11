# Builds JackcessProbe (Jackcess and UCanAccess compiled to .NET by IKVM) and runs it on Linux, then has
# MDB Tools, an unrelated Access reader, check the files Jackcess wrote. Build from the repository root:
#   docker build -f experiments/jackcess/Dockerfile -t plane-disaster-jackcess .
# .NET 10: IKVM.Maven.Sdk 1.12.2 fails under the .NET 11 SDK (see JackcessProbe.csproj).
ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src
COPY Directory.Build.props ./
COPY experiments/jackcess/JackcessProbe/ experiments/jackcess/JackcessProbe/
# IKVM.Maven.Sdk downloads the jars from Maven Central and compiles them with ikvmc during the build.
RUN dotnet publish experiments/jackcess/JackcessProbe -f net10.0 -c Release -o /probe -nologo

FROM mcr.microsoft.com/dotnet/runtime:${DOTNET_VERSION}
RUN apt-get update -qq \
 && apt-get install -y -qq --no-install-recommends mdbtools ca-certificates curl >/dev/null \
 && rm -rf /var/lib/apt/lists/* \
 && mkdir -p /data \
 && curl -fsSL -o /data/nwind.mdb https://raw.githubusercontent.com/mdbtools/mdbtestdata/master/data/nwind.mdb
COPY --from=build /probe /probe
COPY experiments/jackcess/run.sh /run.sh
ENTRYPOINT ["bash", "/run.sh"]
