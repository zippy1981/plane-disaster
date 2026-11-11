#!/usr/bin/env bash
# MDB Tools' ODBC driver (read-only Access .mdb/.accdb), from the image's Ubuntu archive, plus the mdbtools test databases.
set -euo pipefail
apt-get update -qq
apt-get install -y -qq --no-install-recommends odbc-mdbtools mdbtools >/dev/null
rm -rf /var/lib/apt/lists/*

# MDB Tools ships a wide-character build (System.Data.Odbc calls the W entry points) and an ANSI one,
# which unixODBC converts to. Register both, to compare.
wide=$(dpkg -L odbc-mdbtools | grep -E '/libmdbodbcW\.so$' || true)
ansi=$(dpkg -L odbc-mdbtools | grep -E '/libmdbodbc\.so$')
{
  [ -n "$wide" ] && printf '[MDBTools]\nDescription=MDB Tools ODBC (wide)\nDriver=%s\n\n' "$wide"
  printf '[MDBTools ANSI]\nDescription=MDB Tools ODBC (ANSI)\nDriver=%s\n' "$ansi"
} > /etc/odbcinst.ini
echo "registered ${wide:-no wide build} and $ansi; mdbtools $(dpkg-query -W -f='${Version}' odbc-mdbtools)"

mkdir -p /data
for f in nwind.mdb ASampleDatabase.accdb; do
  curl -fsSL -o "/data/$f" "https://raw.githubusercontent.com/mdbtools/mdbtestdata/master/data/$f"
done
