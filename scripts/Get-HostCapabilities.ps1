<#
.SYNOPSIS
Reports which container engines this host can use for this repo's Linux work (the Mono build, the .NET SDK, the
Linux ODBC and Jackcess experiments, shellcheck), and caches the answer for 24 hours.

.DESCRIPTION
The host-capabilities skill runs this before any container work, and routes each task to a local engine or to
GitHub Actions.

On Windows it checks:
- the engine behind the default docker context. A Windows engine is reported, but nothing in this repo needs one.
- a Linux engine, in this order: the default WSL distro's own Docker, any other WSL 2 distro's Docker (the
  docker-desktop distros are skipped), the default docker context if it is a Linux engine, and Docker Desktop's
  Linux engine pipe if it already exists. It stops at the first that answers. Probing a stopped WSL distro boots it.
- gh, and whether it is logged in (the exit code only; nothing it prints is kept)

It never switches Docker Desktop between Windows and Linux containers, never starts a daemon, and installs nothing.

The report is cached in .claude/host-capabilities.windows.local.json, which is gitignored. A cached report is reused
until it is older than maxAgeHours, the overrides file is newer than it, or -Refresh is given. Overrides are read
from .claude/host-capabilities.overrides.local.json, a flat JSON object:
  linuxEngine  'auto' (default), 'none', 'wsl:<distro>', 'docker' (the default docker context) or 'docker-desktop-linux'
  sharedHost   true (default). Set false only on a host that nothing else uses.
  maxAgeHours  24 (default)

On Linux and macOS it runs scripts/host-capabilities.sh instead, which writes the same report.

.PARAMETER Refresh
Probe again even if the cached report is still fresh, for example after a docker command failed to reach its engine.

.PARAMETER AsJson
Print the report as JSON text instead of returning an object.

.EXAMPLE
./scripts/Get-HostCapabilities.ps1 -AsJson

.EXAMPLE
(./scripts/Get-HostCapabilities.ps1 -Refresh).linuxEngine.invoke
#>
#Requires -Version 7.0
[CmdletBinding()]
param(
    [switch]$Refresh,
    [switch]$AsJson
)
$ErrorActionPreference = 'Stop'

if (-not $IsWindows) {
    $shArgs = @("$PSScriptRoot/host-capabilities.sh")
    if ($Refresh) { $shArgs += '--refresh' }
    $json = (& sh @shArgs) -join "`n"
    if ($LASTEXITCODE) { throw "host-capabilities.sh failed with exit code $LASTEXITCODE" }
    if ($AsJson) { $json } else { $json | ConvertFrom-Json }
    return
}

$schemaVersion = 1
$repoRoot = Split-Path -Parent $PSScriptRoot
$cachePath = Join-Path $repoRoot '.claude/host-capabilities.windows.local.json'
$overridesPath = Join-Path $repoRoot '.claude/host-capabilities.overrides.local.json'
$claudeLabel = 'com.zippy1981.created-by=claude-code'

$overrides = [ordered]@{ linuxEngine = 'auto'; sharedHost = $true; maxAgeHours = 24 }
if (Test-Path $overridesPath) {
    $userOverrides = Get-Content -Raw $overridesPath | ConvertFrom-Json -AsHashtable
    foreach ($key in @($userOverrides.Keys)) {
        if ($overrides.Contains($key)) { $overrides[$key] = $userOverrides[$key] } else { Write-Warning "Ignoring unknown override '$key'" }
    }
    if ($overrides.linuxEngine -notmatch '^(auto|none|docker|docker-desktop-linux|wsl:.+)$') {
        throw "Override linuxEngine '$($overrides.linuxEngine)' must be auto, none, docker, docker-desktop-linux or wsl:<distro>"
    }
}

if (-not $Refresh -and (Test-Path $cachePath)) {
    $cachedJson = Get-Content -Raw $cachePath
    $cached = $cachedJson | ConvertFrom-Json
    $ageHours = ([DateTimeOffset]::UtcNow.ToUnixTimeSeconds() - $cached.probedAtEpoch) / 3600
    $overridesChanged = (Test-Path $overridesPath) -and
        (Get-Item $overridesPath).LastWriteTimeUtc -gt (Get-Item $cachePath).LastWriteTimeUtc
    if ($cached.schemaVersion -eq $schemaVersion -and $ageHours -lt $overrides.maxAgeHours -and -not $overridesChanged) {
        if ($AsJson) { $cachedJson.TrimEnd() } else { $cached }
        return
    }
}

# Runs a native command and returns its exit code and output. A missing command is exit code -1.
function Invoke-Native {
    param([Parameter(Mandatory)][string[]]$Command)
    $ErrorActionPreference = 'Continue'
    try {
        $output = & $Command[0] @($Command | Select-Object -Skip 1) 2>&1
        [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = (@($output | ForEach-Object { "$_" }) -join "`n").Trim() }
    } catch {
        [pscustomobject]@{ ExitCode = -1; Output = "$_" }
    }
}

# Asks a docker CLI (an array such as 'wsl.exe','-d','Ubuntu','--exec','docker') about its engine. Returns $null
# and sets $script:probeError when the engine doesn't answer.
function Get-EngineFact {
    param([Parameter(Mandatory)][string[]]$Docker)
    $version = Invoke-Native ($Docker + @('version', '--format', '{{.Server.Os}}|{{.Server.Version}}'))
    if ($version.ExitCode -ne 0 -or $version.Output -notmatch '^(\w+)\|(\S+)$') {
        $script:probeError = ($version.Output -split "`n" | Where-Object { $_ } | Select-Object -First 1) ?? 'no output'
        return $null
    }
    $os, $serverVersion = $Matches[1], $Matches[2]
    $info = Invoke-Native ($Docker + @('info', '--format', '{{.OperatingSystem}}|{{.Isolation}}'))
    $operatingSystem, $isolation = if ($info.ExitCode -eq 0) { $info.Output -split '\|', 2 } else { $null, $null }
    $endpoint = Invoke-Native ($Docker + @('context', 'inspect', '--format', '{{.Endpoints.docker.Host}}'))
    $names = Invoke-Native ($Docker + @('ps', '-a', '--format', '{{.Names}}'))
    $mine = Invoke-Native ($Docker + @('ps', '-aq', '--filter', "label=$claudeLabel"))
    [pscustomobject]@{
        Os              = $os
        ServerVersion   = $serverVersion
        OperatingSystem = $operatingSystem
        Isolation       = if ($isolation) { $isolation } else { $null }
        Endpoint        = if ($endpoint.ExitCode -eq 0) { $endpoint.Output } else { $null }
        Names           = @($names.Output -split "`n" | Where-Object { $_ })
        ClaudeCount     = @($mine.Output -split "`n" | Where-Object { $_ }).Count
    }
}

# Host
$os = Get-CimInstance Win32_OperatingSystem
$hostBuild = [Environment]::OSVersion.Version.Build
$hypervisorPresent = [bool](Get-CimInstance Win32_ComputerSystem).HypervisorPresent
$dockerDesktopInstalled = Test-Path (Join-Path $env:ProgramFiles 'Docker/Docker/Docker Desktop.exe')
$dockerDesktopLinuxPipe = Test-Path '\\.\pipe\dockerDesktopLinuxEngine'

# The default docker context: usually the Windows engine, or a Linux engine if Docker Desktop is in Linux mode
$defaultEngine = Get-EngineFact @('docker')
$defaultError = $script:probeError

# Windows engine: reported for completeness. Nothing in this repo builds Windows containers.
$windowsEngine = [ordered]@{
    available = $false; invoke = $null; endpoint = $null; serverVersion = $null; defaultIsolation = $null
    reason = $null
}
if ($defaultEngine -and $defaultEngine.Os -eq 'windows') {
    $windowsEngine.available = $true
    $windowsEngine.invoke = @('docker')
    $windowsEngine.endpoint = $defaultEngine.Endpoint
    $windowsEngine.serverVersion = $defaultEngine.ServerVersion
    $windowsEngine.defaultIsolation = $defaultEngine.Isolation
} elseif ($defaultEngine) {
    $windowsEngine.reason = "the default docker context is a $($defaultEngine.Os) engine; it is not switched"
} else {
    $windowsEngine.reason = "docker: $defaultError"
}

# WSL distros. WSL_UTF8 makes wsl.exe print UTF-8 instead of UTF-16.
$previousWslUtf8 = $env:WSL_UTF8
$env:WSL_UTF8 = '1'
try {
    $wslList = Invoke-Native @('wsl.exe', '--list', '--verbose')
    $distros = @()
    if ($wslList.ExitCode -eq 0) {
        foreach ($line in $wslList.Output -split "`n") {
            if ($line -match '^\s*(\*)?\s*(\S+)\s+(\S+)\s+([12])\s*$' -and $Matches[2] -ne 'NAME') {
                $distros += [pscustomobject][ordered]@{
                    name = $Matches[2]; state = $Matches[3]; version = [int]$Matches[4]; default = [bool]$Matches[1]
                }
            }
        }
    }
    $wsl = [ordered]@{
        installed     = $wslList.ExitCode -eq 0
        defaultDistro = ($distros | Where-Object default | Select-Object -First 1).name
        distros       = $distros
    }

    # Linux engine candidates, in the order the skill prefers them
    $candidates = @()
    $wslCandidates = @($distros | Where-Object { $_.version -eq 2 -and $_.name -notlike 'docker-desktop*' } |
        Sort-Object { -not $_.default })
    switch -Regex ($overrides.linuxEngine) {
        '^auto$' {
            $candidates += $wslCandidates | ForEach-Object { [pscustomobject]@{ Kind = 'wsl'; Distro = $_.name } }
            $candidates += [pscustomobject]@{ Kind = 'docker'; Distro = $null }
            if ($dockerDesktopLinuxPipe) { $candidates += [pscustomobject]@{ Kind = 'docker-desktop-linux'; Distro = $null } }
        }
        '^wsl:(.+)$' { $candidates += [pscustomobject]@{ Kind = 'wsl'; Distro = $Matches[1] } }
        '^(docker|docker-desktop-linux)$' { $candidates += [pscustomobject]@{ Kind = $Matches[1]; Distro = $null } }
    }

    $linuxEngine = [ordered]@{
        available = $false; kind = $null; distro = $null; invoke = $null; endpoint = $null; serverVersion = $null
        operatingSystem = $null; containers = $null; claudeContainers = $null; repoPath = $null; reason = $null; tried = @()
    }
    foreach ($candidate in $candidates) {
        $docker = switch ($candidate.Kind) {
            'wsl' { @('wsl.exe', '--distribution', $candidate.Distro, '--exec', 'docker') }
            'docker' { @('docker') }
            'docker-desktop-linux' { @('docker', '--host', 'npipe:////./pipe/dockerDesktopLinuxEngine') }
        }
        $fact = if ($candidate.Kind -eq 'docker') { $defaultEngine } else { Get-EngineFact $docker }
        $failure = if (-not $fact) { if ($candidate.Kind -eq 'docker') { $defaultError } else { $script:probeError } }
            elseif ($fact.Os -ne 'linux') { "it is a $($fact.Os) engine" }
        if ($failure) {
            $linuxEngine.tried += [ordered]@{ kind = $candidate.Kind; distro = $candidate.Distro; error = $failure }
            continue
        }
        $repoPath = if ($candidate.Kind -eq 'wsl') {
            (Invoke-Native @('wsl.exe', '--distribution', $candidate.Distro, '--exec', 'wslpath', '-a', $repoRoot)).Output
        } else { $null }
        $linuxEngine.available = $true
        $linuxEngine.kind = $candidate.Kind
        $linuxEngine.distro = $candidate.Distro
        $linuxEngine.invoke = $docker
        $linuxEngine.endpoint = $fact.Endpoint
        $linuxEngine.serverVersion = $fact.ServerVersion
        $linuxEngine.operatingSystem = $fact.OperatingSystem
        $linuxEngine.containers = $fact.Names.Count
        $linuxEngine.claudeContainers = $fact.ClaudeCount
        $linuxEngine.repoPath = $repoPath
        break
    }
    if (-not $linuxEngine.available) {
        $linuxEngine.reason = if ($overrides.linuxEngine -eq 'none') { 'disabled by the linuxEngine override' }
            elseif (-not $wsl.installed) { 'WSL is not installed and no other Linux engine answered' }
            else { 'no candidate answered; see tried' }
    }
} finally {
    $env:WSL_UTF8 = $previousWslUtf8
}

$gh = [bool](Get-Command gh -ErrorAction SilentlyContinue)
$report = [ordered]@{
    schemaVersion = $schemaVersion
    probedAt      = [DateTimeOffset]::UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
    probedAtEpoch = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    platform      = 'windows'
    overrides     = $overrides
    policy        = [ordered]@{ sharedHost = [bool]$overrides.sharedHost; dockerDesktopSwitch = 'never' }
    host          = [ordered]@{
        os                = 'windows'
        caption           = $os.Caption
        build             = $hostBuild
        productType       = if ($os.ProductType -eq 1) { 'workstation' } else { 'server' }
        hypervisorPresent = $hypervisorPresent
        dockerDesktop     = [ordered]@{ installed = $dockerDesktopInstalled; linuxEnginePipe = $dockerDesktopLinuxPipe }
    }
    windowsEngine = $windowsEngine
    linuxEngine   = $linuxEngine
    wsl           = $wsl
    github        = [ordered]@{ gh = $gh; authenticated = $gh -and (Invoke-Native @('gh', 'auth', 'status')).ExitCode -eq 0 }
}
$json = $report | ConvertTo-Json -Depth 8
Set-Content -Path $cachePath -Value $json -Encoding utf8NoBOM
if ($AsJson) { $json } else { $json | ConvertFrom-Json }
