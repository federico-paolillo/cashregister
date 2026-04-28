#!/usr/bin/env sh

set -e
set -u

API_BASE_URL="${API_BASE_URL:-http://localhost:60000}"

docker buildx build --platform linux/amd64 -f Ui.Dockerfile --build-arg API_BASE_URL="${API_BASE_URL}" --load -t cashregister-ui:local .
