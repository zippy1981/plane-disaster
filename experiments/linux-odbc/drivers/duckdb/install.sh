#!/usr/bin/env bash
# DuckDB's ODBC driver. DuckDB reads and writes SQLite files through its sqlite extension, so it is a
# second, independent way to reach SQLite over ODBC. The sqlite3 CLI makes the fixture.
set -euo pipefail
version=${DUCKDB_ODBC_VERSION:-v2.0.0.1}
apt-get update -qq
apt-get install -y -qq --no-install-recommends sqlite3 >/dev/null
rm -rf /var/lib/apt/lists/*

mkdir -p /opt/duckdb-odbc
curl -fsSL -o /tmp/duckdb_odbc.zip \
  "https://github.com/duckdb/duckdb-odbc/releases/download/$version/duckdb_odbc-linux-amd64.zip"
unzip -q -o /tmp/duckdb_odbc.zip -d /opt/duckdb-odbc
lib=$(find /opt/duckdb-odbc -name 'libduckdb_odbc.so' | head -1)
printf '[DuckDB]\nDescription=DuckDB ODBC\nDriver=%s\n' "$lib" > /etc/odbcinst.ini
echo "registered $lib; duckdb-odbc $version"

mkdir -p /data
sqlite3 /data/fixture.sqlite \
  "CREATE TABLE tblTest (id INTEGER PRIMARY KEY, description VARCHAR(255));
   INSERT INTO tblTest VALUES (1, 'Number one.'), (2, 'Number two.'), (3, 'Number three.');"
