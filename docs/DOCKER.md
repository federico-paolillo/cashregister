# Cash Register DevOps Conventions

> Use this file for Docker, container-image, and shell-script conventions. Application architecture belongs in `ARCH.md`; backend and frontend development conventions belong in `CONVENTIONS.md`.

## Docker

Docker images should default to a hardened runtime shape:

- Use official distroless or chiseled base images for runtime stages when the application can run on them.
- Run runtime containers as a non-root user.
- Keep application files read-only at runtime.
- Put writable application state, caches, and logs in explicit writable directories separate from application binaries.
- Use multi-stage builds so SDKs, compilers, package managers, and build-only tools do not ship in runtime images.
- Prefer BuildKit cache mounts for dependency restoration and other repeatable expensive build steps.

## Bash Scripting

Shell scripts should be minimal and fail early:

- Use `#!/usr/bin/env sh` unless Bash-specific behavior is required.
- Use `set -e` by default so scripts stop after failed commands.
- Use `set -u` by default so unset variables are treated as errors.
- Keep scripts focused on one operational task.
