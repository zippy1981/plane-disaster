#!/usr/bin/env bash
# Inside the image: run the probe, then read the files it wrote with MDB Tools.
set -uo pipefail
dotnet /probe/JackcessProbe.dll /work /data/nwind.mdb
status=$?
echo
echo "MDB Tools reading the files Jackcess and UCanAccess wrote:"
echo
echo '```text'
for f in /work/new.mdb /work/new.accdb; do
  [ -f "$f" ] || continue
  echo "$f: version $(mdb-ver "$f"), tables: $(mdb-tables -1 "$f" | tr '\n' ' ')"
  mdb-export "$f" tblSql 2>&1 | head -4
done
echo '```'
exit $status
