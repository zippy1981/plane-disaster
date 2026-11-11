#!/bin/sh
# host-capabilities.sh: report, as JSON, which container engines this host can use for this repo's work, and
# cache the answer (the host-capabilities skill).
#
#   scripts/host-capabilities.sh            print the cached report, probing first if it is missing or stale
#   scripts/host-capabilities.sh --refresh  probe again
#
# For Linux, macOS and WSL. On Windows (Git Bash, MSYS, Cygwin) it runs scripts/Get-HostCapabilities.ps1 instead.
# The report is cached in .claude/host-capabilities.<platform>.local.json. Overrides are read from
# .claude/host-capabilities.overrides.local.json, a flat JSON object with linuxEngine (auto, none or docker),
# sharedHost (true) and maxAgeHours (24). It never starts a daemon and installs nothing.
set -eu

here=$(cd "$(dirname "$0")" && pwd)
root=$(dirname "$here")
refresh=0
case "${1:-}" in
    --refresh) refresh=1 ;;
    '') ;;
    -h | --help) sed -n '2,11p' "$0"; exit 0 ;;
    *) echo "host-capabilities.sh: unknown argument '$1'" >&2; exit 2 ;;
esac

case "$(uname -s)" in
    MINGW* | MSYS* | CYGWIN*)
        if [ "$refresh" = 1 ]; then
            exec pwsh -NoProfile -File "$here/Get-HostCapabilities.ps1" -AsJson -Refresh
        fi
        exec pwsh -NoProfile -File "$here/Get-HostCapabilities.ps1" -AsJson
        ;;
    Darwin) platform=macos ;;
    *) if grep -qi microsoft /proc/sys/kernel/osrelease 2>/dev/null; then platform=wsl; else platform=linux; fi ;;
esac

schema=1
cache="$root/.claude/host-capabilities.$platform.local.json"
overrides="$root/.claude/host-capabilities.overrides.local.json"
claude_label=com.zippy1981.created-by=claude-code

# json_value <file> <key>: a top-level string, number or boolean from a flat JSON file (empty if absent)
json_value() {
    [ -f "$1" ] || return 0
    tr -d '\n\r' <"$1" | sed -n "s/.*\"$2\"[[:space:]]*:[[:space:]]*\"\{0,1\}\([^\",}[:space:]]*\).*/\1/p"
}
# js <value>: a JSON string, or null when empty
js() {
    if [ -z "$1" ]; then printf null; return; fi
    printf '"%s"' "$(printf '%s' "$1" | tr -d '\n\r\t' | sed 's/\\/\\\\/g; s/"/\\"/g')"
}
# jn <value>: a JSON number, or null when empty
jn() { if [ -z "$1" ]; then printf null; else printf '%s' "$1"; fi; }

ov_engine=$(json_value "$overrides" linuxEngine)
ov_shared=$(json_value "$overrides" sharedHost)
ov_age=$(json_value "$overrides" maxAgeHours)
: "${ov_engine:=auto}" "${ov_shared:=true}" "${ov_age:=24}"
case "$ov_engine" in auto | none | docker) ;; *)
    echo "host-capabilities.sh: override linuxEngine '$ov_engine' must be auto, none or docker here" >&2; exit 2 ;;
esac
case "$ov_shared" in true | false) ;; *) echo "host-capabilities.sh: override sharedHost must be true or false" >&2; exit 2 ;; esac
case "$ov_age" in '' | *[!0-9]*) echo "host-capabilities.sh: override maxAgeHours must be a whole number" >&2; exit 2 ;; esac

now=$(date +%s)
if [ "$refresh" = 0 ] && [ -f "$cache" ] && [ -z "$(find "$overrides" -newer "$cache" 2>/dev/null)" ]; then
    cached_schema=$(json_value "$cache" schemaVersion)
    cached_epoch=$(json_value "$cache" probedAtEpoch)
    if [ "$cached_schema" = "$schema" ] && [ -n "$cached_epoch" ] && [ $((now - cached_epoch)) -lt $((ov_age * 3600)) ]; then
        cat "$cache"
        exit 0
    fi
fi

# probe <docker command...>: asks a docker CLI about its engine. Sets p_os, p_version, p_osname, p_isolation,
# p_endpoint, p_total and p_mine, or p_error when the engine doesn't answer.
probe() {
    p_os='' p_version='' p_osname='' p_isolation='' p_endpoint='' p_total='' p_mine='' p_error=''
    if ! out=$("$@" version --format '{{.Server.Os}}|{{.Server.Version}}' 2>&1) || [ "${out#*|}" = "$out" ]; then
        p_error=$(printf '%s\n' "$out" | sed -n '/./{p;q;}')
        : "${p_error:=no output}"
        return 1
    fi
    p_os=${out%%|*} p_version=${out#*|}
    info=$("$@" info --format '{{.OperatingSystem}}|{{.Isolation}}' 2>/dev/null || true)
    p_osname=${info%%|*} p_isolation=${info#*|}
    p_endpoint=$("$@" context inspect --format '{{.Endpoints.docker.Host}}' 2>/dev/null || true)
    p_total=$("$@" ps -aq 2>/dev/null | wc -l | tr -d ' ')
    p_mine=$("$@" ps -aq --filter "label=$claude_label" 2>/dev/null | wc -l | tr -d ' ')
}

# Linux engine: the docker CLI's current context (DOCKER_HOST, Docker Desktop, colima, a distro's own Docker)
l_available=false l_kind='' l_invoke=null l_version='' l_osname='' l_endpoint='' l_total='' l_mine='' l_reason='' l_tried=''
if [ "$ov_engine" = none ]; then
    l_reason='disabled by the linuxEngine override'
elif ! command -v docker >/dev/null 2>&1; then
    l_reason='docker CLI not installed'
    l_tried="{\"kind\": \"docker\", \"distro\": null, \"error\": $(js "$l_reason")}"
elif probe docker && [ "$p_os" = linux ]; then
    l_available=true l_kind=docker l_invoke='["docker"]' l_version=$p_version l_osname=$p_osname
    l_endpoint=$p_endpoint l_total=$p_total l_mine=$p_mine
else
    if [ -n "$p_os" ]; then p_error="it is a $p_os engine"; fi
    l_reason='no candidate answered; see tried'
    l_tried="{\"kind\": \"docker\", \"distro\": null, \"error\": $(js "$p_error")}"
fi

# Windows engine: only reachable from WSL, through Windows' docker.exe
w_available=false w_invoke=null w_endpoint='' w_version='' w_isolation='' w_reason=''
if [ "$platform" != wsl ]; then
    w_reason="a $platform host has no Windows engine"
elif ! command -v docker.exe >/dev/null 2>&1; then
    w_reason='docker.exe is not on the PATH (WSL interop off, or no Windows Docker)'
elif probe docker.exe && [ "$p_os" = windows ]; then
    w_available=true w_invoke='["docker.exe"]' w_endpoint=$p_endpoint w_version=$p_version w_isolation=$p_isolation
else
    if [ -n "$p_os" ]; then p_error="docker.exe reaches a $p_os engine"; fi
    w_reason="docker.exe: $p_error"
fi

gh=false gh_auth=false
if command -v gh >/dev/null 2>&1; then
    gh=true
    if gh auth status >/dev/null 2>&1; then gh_auth=true; fi
fi
if [ "$platform" = macos ]; then os=macos; else os=linux; fi

mkdir -p "$(dirname "$cache")"
cat >"$cache" <<EOF
{
  "schemaVersion": $schema,
  "probedAt": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
  "probedAtEpoch": $now,
  "platform": "$platform",
  "overrides": {
    "linuxEngine": $(js "$ov_engine"),
    "sharedHost": $ov_shared,
    "maxAgeHours": $ov_age
  },
  "policy": {
    "sharedHost": $ov_shared,
    "dockerDesktopSwitch": "never"
  },
  "host": {
    "os": "$os",
    "caption": $(js "$(uname -sr)"),
    "build": null,
    "productType": null,
    "hypervisorPresent": null,
    "dockerDesktop": null
  },
  "windowsEngine": {
    "available": $w_available,
    "invoke": $w_invoke,
    "endpoint": $(js "$w_endpoint"),
    "serverVersion": $(js "$w_version"),
    "defaultIsolation": $(js "$w_isolation"),
    "reason": $(js "$w_reason")
  },
  "linuxEngine": {
    "available": $l_available,
    "kind": $(js "$l_kind"),
    "distro": $(js "$(if [ "$platform" = wsl ]; then printf '%s' "${WSL_DISTRO_NAME:-}"; fi)"),
    "invoke": $l_invoke,
    "endpoint": $(js "$l_endpoint"),
    "serverVersion": $(js "$l_version"),
    "operatingSystem": $(js "$l_osname"),
    "containers": $(jn "$l_total"),
    "claudeContainers": $(jn "$l_mine"),
    "repoPath": $(js "$root"),
    "reason": $(js "$l_reason"),
    "tried": [$l_tried]
  },
  "wsl": null,
  "github": {
    "gh": $gh,
    "authenticated": $gh_auth
  }
}
EOF
cat "$cache"
