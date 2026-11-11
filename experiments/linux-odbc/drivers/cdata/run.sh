#!/usr/bin/env bash
# Read/write against a copy of Northwind. Unverified: the attribute names follow CData's docs.
set -uo pipefail
if ! odbcinst -q -d | grep -q 'CData ODBC Driver for Access'; then
  echo "### cdata"
  echo
  echo "Not installed: put the CData Access ODBC .deb (and license.key) in drivers/cdata/vendor/ and rebuild."
  exit 3
fi
cp /data/nwind.mdb /tmp/nwind.mdb
status=0
dotnet /harness/OdbcHarness.dll --name "cdata: nwind.mdb" --mode ro --table Customers \
  --connection "Driver={CData ODBC Driver for Access};DataSource=/tmp/nwind.mdb;" || status=1
echo
dotnet /harness/OdbcHarness.dll --name "cdata: write to nwind.mdb" --mode rw \
  --connection "Driver={CData ODBC Driver for Access};DataSource=/tmp/nwind.mdb;" || status=1
exit $status
