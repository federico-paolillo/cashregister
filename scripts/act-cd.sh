#!/usr/bin/env sh

set -e
set -u

cd "$(dirname "$0")/.."

REPOSITORY_ROOT="$PWD" docker compose -f docker-compose.act.yaml run --rm act \
    workflow_dispatch \
    --bind \
    -W .github/workflows/cd.yml \
    -e .act/cd-event.json \
    --env ACT=true \
    -P ubuntu-latest=ghcr.io/catthehacker/ubuntu:act-latest
