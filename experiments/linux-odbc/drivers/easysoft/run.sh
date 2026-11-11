#!/usr/bin/env bash
# Read/write against a copy of Northwind. Unverified: the attribute names follow Easysoft's docs.
set -uo pipefail
if ! odbcinst -q -d | grep -q 'Easysoft ODBC-Access'; then
  echo "### easysoft"
  echo
  echo "Not installed: put the Easysoft ODBC-Access tarball in drivers/easysoft/vendor/ and rebuild."
  exit 3
fi
cp /data/nwind.mdb /tmp/nwind.mdb
status=0
dotnet /harness/OdbcHarness.dll --name "easysoft: nwind.mdb" --mode ro --table Customers \
  --connection "Driver={Easysoft ODBC-Access Driver};MDBFile=/tmp/nwind.mdb;" || status=1
echo
dotnet /harness/OdbcHarness.dll --name "easysoft: write to nwind.mdb" --mode rw \
  --connection "Driver={Easysoft ODBC-Access Driver};MDBFile=/tmp/nwind.mdb;" || status=1
exit $status
