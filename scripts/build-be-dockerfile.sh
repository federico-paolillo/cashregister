#!/usr/bin/env sh

set -e
set -u

docker buildx build --platform linux/amd64 -f be.Dockerfile --load -t cashregister-api:local .
