#!/usr/bin/env bash
# Builds PlaneDisaster.NET with Mono's C# compiler (mcs) and runs the tests that work off Windows.
#
# Mono's xbuild can't read the SDK-style projects, so this compiles the same sources the projects
# glob, against Mono's .NET Framework 4.x profile. It checks that the code still builds and the
# SQLite parts still run on Mono; Access (Jet/ACE, ODBC) needs Windows, so those tests are skipped.
#
# Usage: build/mono/build.sh [output-dir]       (default: bin/mono)
# Needs: mono-devel (mcs, resgen), curl, unzip.
set -euo pipefail

root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
out=${1:-$root/bin/mono}
work=$out/obj
sqlite_version=1.0.119
nunit_version=2.6.4
mkdir -p "$out" "$work"

# fetch <nuget id> <version> <dir>: download and unpack a NuGet package once.
fetch() {
  if [ ! -d "$3" ]; then
    curl -fsSL -o "$3.nupkg" "https://api.nuget.org/v3-flatcontainer/$1/$2/$1.$2.nupkg"
    mkdir -p "$3"
    unzip -q -o "$3.nupkg" -d "$3"
  fi
}

# The repo's System.Data.SQLite 1.0.66 is a Windows mixed-mode DLL. Mono uses the managed build from
# NuGet, with the Linux native interop library from the .NET Standard package.
fetch stub.system.data.sqlite.core.netframework "$sqlite_version" "$work/sqlite-netfx"
fetch stub.system.data.sqlite.core.netstandard "$sqlite_version" "$work/sqlite-netstd"
fetch nunit.runners "$nunit_version" "$work/nunit"
cp "$work/sqlite-netfx/lib/net46/System.Data.SQLite.dll" "$out/"
cp "$work/sqlite-netstd/runtimes/linux-x64/native/SQLite.Interop.dll" "$out/"
cp "$root/packages/NUnit.2.6.2/lib/nunit.framework.dll" "$out/"

# sources <dir> [find args...]: the .cs files the SDK would glob, without bin/ and obj/.
sources() {
  local dir=$1
  shift
  find "$dir" \( -path '*/bin' -o -path '*/obj' \) -prune -o -name '*.cs' "$@" -print | sort
}

compile() {
  echo "mcs: $1" >&2
  mcs -nologo -sdk:4.5 -debug -optimize+ -warnaserror -warn:4 "${@:2}"
}

framework=(-r:System.dll -r:System.Data.dll -r:System.Xml.dll -r:System.Configuration.dll)
winforms=(-r:System.Drawing.dll -r:System.Windows.Forms.dll)

mapfile -t dba < <(sources "$root/PlaneDisaster.Dba" ! -name NpgsqlDba.cs)
compile PlaneDisaster.Dba -target:library -out:"$out/PlaneDisaster.Dba.dll" \
  "${framework[@]}" -r:"$out/System.Data.SQLite.dll" "${dba[@]}"

# Resource names must match the form's full type name, as ComponentResourceManager looks them up.
resgen "$root/MainForm.resx" "$work/PlaneDisaster.MainForm.resources" >/dev/null
resgen "$root/ConnectionStringDialog.resx" "$work/PlaneDisaster.ConnectionStringDialog.resources" >/dev/null
mapfile -t app < <(find "$root" -maxdepth 1 -name '*.cs' | sort; sources "$root/Configuration")
compile PlaneDisaster -target:winexe -out:"$out/PlaneDisaster.exe" -win32icon:"$root/PlaneDisaster.NET.ico" \
  "${framework[@]}" "${winforms[@]}" -r:"$out/PlaneDisaster.Dba.dll" -r:"$out/System.Data.SQLite.dll" \
  -resource:"$work/PlaneDisaster.MainForm.resources" \
  -resource:"$work/PlaneDisaster.ConnectionStringDialog.resources" \
  "${app[@]}"

resgen "$root/Jet2SQLite/MainForm.resx" "$work/PlaneDisaster.Jet2SQLite.MainForm.resources" >/dev/null
mapfile -t jet2sqlite < <(sources "$root/Jet2SQLite")
compile Jet2SQLite -target:winexe -out:"$out/Jet2SQLite.exe" -win32icon:"$root/Jet2SQLite/Jet2SQLite.ico" \
  "${framework[@]}" "${winforms[@]}" -r:"$out/PlaneDisaster.Dba.dll" \
  -resource:"$work/PlaneDisaster.Jet2SQLite.MainForm.resources" \
  "${jet2sqlite[@]}" "$root/InputDialog.cs"

mapfile -t jsql < <(sources "$root/jsql_admin")
compile jsql_admin -target:exe -out:"$out/jsql_admin.exe" "${framework[@]}" -r:"$out/PlaneDisaster.Dba.dll" "${jsql[@]}"

mapfile -t tests < <(sources "$root/UnitTests")
compile UnitTests -target:library -out:"$out/UnitTests.dll" \
  "${framework[@]}" -r:"$out/PlaneDisaster.Dba.dll" -r:"$out/System.Data.SQLite.dll" -r:"$out/nunit.framework.dll" \
  "${tests[@]}"

# Only the tests that don't need Windows' Access drivers. A name with no matching test runs nothing.
echo "nunit: SQLite and tokenizer tests on Mono $(mono --version | head -1 | awk '{print $5}')" >&2
cd "$out"
mono "$work/nunit/tools/nunit-console.exe" UnitTests.dll -nologo -noshadow -labels \
  -run:UnitTests.Tests.TestSQLite,UnitTests.TestSqlTokenizer -result:"$out/TestResult.xml"
