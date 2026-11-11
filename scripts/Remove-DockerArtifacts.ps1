<#
.SYNOPSIS
Removes only the Docker containers and images whose names start with a given prefix, such as the leftovers of an
experiment. It never prunes, and it refuses prefixes that would touch shared infrastructure.

.DESCRIPTION
Experiments should name everything they create with a unique prefix (for example `cgo-exp-`). This script removes:
- containers whose name starts with the prefix (stopped or running)
- image tags whose repository starts with the prefix

By default an image is untagged with `docker rmi --no-prune`, which keeps its untagged parent layers, because those
may be the build cache other images share. Pass -IncludeParents to free that space too, for images that only the
experiment used.

With -Engine Linux it cleans the Linux engine that scripts/Get-HostCapabilities.ps1 finds (on this host, the WSL
distro's own Docker, which also runs the user's devcontainers). Linux is the default, since this repo's
container work is all Linux. There a container or image must also carry the label
com.zippy1981.created-by=claude-code, so a matching name alone never removes someone else's work.

Use -WhatIf to see what would be removed.

.EXAMPLE
./scripts/Remove-DockerArtifacts.ps1 -Prefix claude-odbc- -WhatIf
./scripts/Remove-DockerArtifacts.ps1 -Prefix plane-disaster-odbc -IncludeParents
./scripts/Remove-DockerArtifacts.ps1 -Prefix claude-mono- -Engine Windows -WhatIf
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory)]
    [ValidateLength(4, 128)]
    [string]$Prefix,
    [switch]$IncludeParents,
    [ValidateSet('Windows', 'Linux')]
    [string]$Engine = 'Linux'
)
$ErrorActionPreference = 'Stop'

# The docker command for the chosen engine, and the label filter that the Linux engine also requires
$docker = @('docker')
$labelFilter = @()
if ($Engine -eq 'Linux') {
    $linux = (& "$PSScriptRoot/Get-HostCapabilities.ps1").linuxEngine
    if (-not $linux.available) { throw "No Linux engine on this host: $($linux.reason)" }
    $docker = @($linux.invoke)
    $labelFilter = @('--filter', 'label=com.zippy1981.created-by=claude-code')
}
function Invoke-Docker { & $docker[0] @($docker | Select-Object -Skip 1) @args }

# Shared infrastructure that this script must never touch, whatever prefix it is given
# vsc- is VS Code's devcontainer images; the user's devcontainers run on the same Linux engine.
$protected = @('vsc-', 'zippy1981/', 'mcr.microsoft.com', 'debian', 'ubuntu', 'postgres', 'mariadb')
foreach ($name in $protected) {
    if ($name.StartsWith($Prefix, [StringComparison]::OrdinalIgnoreCase) -or $Prefix.StartsWith($name, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing prefix '$Prefix': it overlaps protected '$name'"
    }
}

$containers = Invoke-Docker ps -a @labelFilter --format '{{.Names}}' |
    Where-Object { $_.StartsWith($Prefix, [StringComparison]::OrdinalIgnoreCase) }
$images = Invoke-Docker images @labelFilter --format '{{.Repository}}:{{.Tag}}' |
    Where-Object { $_ -notlike '*<none>*' -and $_.StartsWith($Prefix, [StringComparison]::OrdinalIgnoreCase) }

foreach ($container in $containers) {
    if ($PSCmdlet.ShouldProcess($container, 'docker rm -f')) {
        Invoke-Docker rm -f $container | Out-Null
        Write-Information -InformationAction Continue "removed container $container"
    }
}
foreach ($image in $images) {
    $rmiArgs = if ($IncludeParents) { @('rmi', $image) } else { @('rmi', '--no-prune', $image) }
    if ($PSCmdlet.ShouldProcess($image, "docker $($rmiArgs -join ' ')")) {
        Invoke-Docker @rmiArgs | Out-Null
        Write-Information -InformationAction Continue "removed image $image"
    }
}
if (-not $containers -and -not $images) { Write-Information -InformationAction Continue "Nothing matches '$Prefix'." }
