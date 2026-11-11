#!/usr/bin/env bash
# Builds and runs the harness image for each driver and collects the results in results/<driver>.md.
#
# Usage: experiments/linux-odbc/run.sh [driver...]     (default: every directory under drivers/)
# Needs: docker reaching a Linux engine. On a Windows host, run it inside WSL, or see the host-capabilities skill.
set -uo pipefail

here=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
root=$(cd "$here/../.." && pwd)
tag=${CLAUDE_SESSION_TAG:-manual}
labels=(--label com.zippy1981.created-by=claude-code --label "com.zippy1981.session=$tag")
mkdir -p "$here/results"

if [ $# -eq 0 ]; then
  set -- $(ls "$here/drivers")
fi

for driver in "$@"; do
  image="plane-disaster-odbc:$driver"
  echo "== $driver"
  if ! docker build -q "${labels[@]}" -f "$here/Dockerfile" --build-arg "DRIVER=$driver" -t "$image" "$root" >/dev/null; then
    echo "### $driver" > "$here/results/$driver.md"
    echo >> "$here/results/$driver.md"
    echo "Image build failed." >> "$here/results/$driver.md"
    continue
  fi
  docker run --rm "${labels[@]}" --name "claude-odbc-$driver-$tag" "$image" | tee "$here/results/$driver.md"
  echo "exit: ${PIPESTATUS[0]}"
done
