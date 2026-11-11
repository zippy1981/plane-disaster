#!/usr/bin/env bash
# Christian Werner's SQLite ODBC driver (read/write SQLite 3), from the image's Ubuntu archive.
set -euo pipefail
apt-get update -qq
apt-get install -y -qq --no-install-recommends libsqliteodbc >/dev/null
rm -rf /var/lib/apt/lists/*

lib=$(dpkg -L libsqliteodbc | grep -E '/libsqlite3odbc\.so$')
printf '[SQLite3]\nDescription=SQLite3 ODBC (Christian Werner)\nDriver=%s\n' "$lib" > /etc/odbcinst.ini
echo "registered $lib; sqliteodbc $(dpkg-query -W -f='${Version}' libsqliteodbc)"
