#!/usr/bin/env bash
# Fails when build/mono/build.sh compiles a different set of files than the SDK-style projects.
#
# The Mono build lists its sources by hand, so a new folder, a linked file or a new exclusion in a
# .csproj can drift from it. This compares the script's list with each project's Compile items.
#
# Usage: build/mono/check-sources.sh
# Needs: a .NET SDK that knows the projects' frameworks, and jq.
set -euo pipefail

root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
cd "$root"

declare -A projects=(
  [PlaneDisaster]=PlaneDisaster.csproj
  [PlaneDisaster.Dba]=PlaneDisaster.Dba/PlaneDisaster.Dba.csproj
  [Jet2SQLite]=Jet2SQLite/Jet2SQLite.csproj
  [jsql_admin]=jsql_admin/jsql_admin.csproj
  [UnitTests]=UnitTests/UnitTests.csproj
)

expected=$(mktemp)
actual=$(mktemp)
trap 'rm -f "$expected" "$actual"' EXIT

for name in "${!projects[@]}"; do
  # Evaluation only, so no restore and no reference assemblies are needed.
  dotnet msbuild "${projects[$name]}" -nologo -getItem:Compile \
      -p:TargetFramework=net20 -p:EnableWindowsTargeting=true \
    | jq -r --arg name "$name" --arg root "$root/" \
        '.Items.Compile[].FullPath | gsub("\\\\"; "/") | ltrimstr($root) | "\($name) \(.)"'
done | sort > "$expected"

bash build/mono/build.sh --list-sources | sort > "$actual"

if ! diff -u --label projects --label build/mono/build.sh "$expected" "$actual"; then
  echo "::error file=build/mono/build.sh::build.sh compiles different files than the projects; update its source lists." >&2
  exit 1
fi
echo "build/mono/build.sh compiles the same $(wc -l < "$actual") files as the projects."
