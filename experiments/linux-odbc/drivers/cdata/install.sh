#!/usr/bin/env bash
# CData ODBC Driver for Microsoft Access (commercial; Linux build in its Unix package). The download and
# the trial key need a CData account, so this installs only what you put in drivers/cdata/vendor/:
#   the Linux .deb from https://www.cdata.com/drivers/access/odbc/
#   license.key containing a trial or product key (optional; without it the driver runs unlicensed)
# Without the package the image still builds, and run.sh reports the driver as not installed.
set -euo pipefail
shopt -s nullglob
debs=(/driver/vendor/*.deb)
if [ ${#debs[@]} -eq 0 ]; then
  echo "cdata: no installer in drivers/cdata/vendor/; skipping"
  exit 0
fi
apt-get update -qq
apt-get install -y -qq --no-install-recommends "${debs[0]}" >/dev/null
rm -rf /var/lib/apt/lists/*
install_dir=$(dirname "$(dpkg -L "$(dpkg-deb -f "${debs[0]}" Package)" | grep -m1 '/lib/libaccessodbc')")/..
if [ -f /driver/vendor/license.key ]; then
  (cd "$install_dir/bin" && ./install-license.x64 "$(cat /driver/vendor/license.key)") || true
fi
odbcinst -q -d
mkdir -p /data
curl -fsSL -o /data/nwind.mdb "https://raw.githubusercontent.com/mdbtools/mdbtestdata/master/data/nwind.mdb"
