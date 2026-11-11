#!/usr/bin/env bash
# Read/write: create tblTest in a new SQLite file through the driver.
set -uo pipefail
rm -f /tmp/test.sqlite
dotnet /harness/OdbcHarness.dll --name "sqliteodbc: new SQLite file" --mode rw \
  --connection "Driver={SQLite3};Database=/tmp/test.sqlite;"
