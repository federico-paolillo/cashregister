#!/usr/bin/env sh

set -e
set -u

cd "$(dirname "$0")/.."

REPOSITORY_ROOT="$PWD" docker compose -f docker-compose.act.yaml run --rm act \
    push \
    --bind \
    -W .github/workflows/ci.yml \
    -P ubuntu-latest=ghcr.io/catthehacker/ubuntu:act-latest
