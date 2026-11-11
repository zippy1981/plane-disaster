#!/usr/bin/env bash
# 1. DuckDB's own database file, read/write.
# 2. A SQLite file attached through the sqlite extension (downloaded from extensions.duckdb.org).
set -uo pipefail
status=0
rm -f /tmp/test.duckdb
dotnet /harness/OdbcHarness.dll --name "duckdb: native file" --mode rw \
  --connection "Driver={DuckDB};Database=/tmp/test.duckdb;" || status=1
echo
dotnet /harness/OdbcHarness.dll --name "duckdb: SQLite file via the sqlite extension" --mode ro --table tblTest \
  --connection "Driver={DuckDB};Database=:memory:;" \
  --setup "INSTALL sqlite" --setup "LOAD sqlite" \
  --setup "ATTACH '/data/fixture.sqlite' AS fixture (TYPE sqlite)" --setup "USE fixture" || status=1
exit $status
