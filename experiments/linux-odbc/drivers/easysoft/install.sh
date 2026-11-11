#!/usr/bin/env bash
# Easysoft ODBC-Access Driver (commercial, read/write .mdb/.accdb on Linux). The download needs an
# Easysoft account and a trial licence, so this installs only what you put in drivers/easysoft/vendor/:
#   odbc-access-*-linux-x86-64.tar.gz    from https://www.easysoft.com/products/data_access/odbc-access-driver/
# Without it the image still builds, and run.sh reports the driver as not installed.
set -euo pipefail
shopt -s nullglob
tarballs=(/driver/vendor/odbc-access-*-linux-x86-64.tar.gz)
if [ ${#tarballs[@]} -eq 0 ]; then
  echo "easysoft: no installer in drivers/easysoft/vendor/; skipping"
  exit 0
fi
apt-get update -qq
apt-get install -y -qq --no-install-recommends procps >/dev/null
mkdir -p /tmp/easysoft
tar -xzf "${tarballs[0]}" -C /tmp/easysoft --strip-components=1
# The installer asks questions; accept the defaults (licence, /usr/local/easysoft, unixODBC already present).
(cd /tmp/easysoft && yes "" | ./install) || true
odbcinst -q -d
mkdir -p /data
curl -fsSL -o /data/nwind.mdb "https://raw.githubusercontent.com/mdbtools/mdbtestdata/master/data/nwind.mdb"
