#!/usr/bin/env bash
# Read-only: MDB Tools can't write, so read the Northwind tables from the mdbtools test data,
# through both the wide and the ANSI builds of the driver.
set -uo pipefail
status=0
for driver in "MDBTools" "MDBTools ANSI"; do
  dotnet /harness/OdbcHarness.dll --name "mdbtools ($driver): nwind.mdb (Jet 4)" --mode ro --table Customers \
    --connection "Driver={$driver};DBQ=/data/nwind.mdb;" || status=1
  echo
  dotnet /harness/OdbcHarness.dll --name "mdbtools ($driver): ASampleDatabase.accdb (ACE)" --mode ro --table "Asset Items" --select-name "[Asset Items]" \
    --connection "Driver={$driver};DBQ=/data/ASampleDatabase.accdb;" || status=1
  echo
done
exit $status
