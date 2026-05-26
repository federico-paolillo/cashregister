#!/usr/bin/env sh

set -e
set -u

API_BASE_URL="${API_BASE_URL:-http://localhost:60000}"
LOW_QUANTITY_WARNING_THRESHOLD="${LOW_QUANTITY_WARNING_THRESHOLD:-5}"

docker buildx build \
  --platform linux/arm64 \
  -f ui.Dockerfile \ 
  --build-arg API_BASE_URL="${API_BASE_URL}" \
  --build-arg LOW_QUANTITY_WARNING_THRESHOLD="${LOW_QUANTITY_WARNING_THRESHOLD}" \
  --load \
  -t cashregister-ui:local \
  .
