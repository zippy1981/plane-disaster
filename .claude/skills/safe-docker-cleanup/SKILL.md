---
name: safe-docker-cleanup
description: Remove the containers and images your own experiments left on this repo's shared Docker hosts (the WSL distro's Linux engine, where the user's devcontainers also run) without touching anyone else's. Use after any local container experiment, or when asked to clean up images.
---

# Safe Docker cleanup

The Linux engine this repo uses (the WSL distro's own Docker, found by the `host-capabilities` skill) also runs the user's devcontainers and their images. Broad cleanup commands destroy that work, so **never run `docker system prune`, `docker image prune`, `docker container prune`, `docker builder prune` or `docker rmi` on images you didn't create.** Adapted from [zippy1981/windows-nano-baseimages](https://github.com/zippy1981/windows-nano-baseimages).

## While experimenting

Name and label everything you create, as the `host-capabilities` skill says: containers `claude-<task>-<tag>`, images under a repository you own for the task (`plane-disaster-odbc:<driver>`), and `--label com.zippy1981.created-by=claude-code` on `docker run` and `docker build`. That makes cleanup exact.

## Cleaning up

```pwsh
./scripts/Remove-DockerArtifacts.ps1 -Prefix claude-odbc- -WhatIf          # list what would go
./scripts/Remove-DockerArtifacts.ps1 -Prefix claude-odbc-                  # remove containers, then untag images
./scripts/Remove-DockerArtifacts.ps1 -Prefix plane-disaster-odbc -IncludeParents   # also free their layers
```

- **Linux engine by default.** A container or image is removed only if it has both the prefix and the label, so a matching name alone never removes someone else's work.
- **Default:** images are untagged with `docker rmi --no-prune`, which keeps their parent layers for the next build. `-IncludeParents` frees them, for images nothing else builds on.
- **Refusals:** prefixes shorter than 4 characters, and any prefix that overlaps `vsc-` (devcontainer images), `zippy1981/`, `mcr.microsoft.com` or the common base images.
- **Pulled images** carry no label, so the script won't remove them. Remove one you pulled by its exact reference (`docker rmi debian:sid`), and only if it wasn't there before you pulled it.
- Without pwsh: `docker ps -aq --filter label=com.zippy1981.session=<tag> | xargs -r docker rm -f`.

## Things to leave alone

- Containers and images whose names start with `vsc-`, and any container you didn't start: they are the user's devcontainers.
- The Windows engine (`docker` on the Windows host): it can run other repos' CI runners. Nothing in this repo needs it.
- Never run `wsl --shutdown` or `wsl --terminate`.
