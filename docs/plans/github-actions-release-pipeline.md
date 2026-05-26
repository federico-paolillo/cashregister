# GitHub Actions release pipeline

This ExecPlan is a living document. The sections `Progress`, `Surprises & Discoveries`, `Decision Log`, and `Outcomes & Retrospective` must be kept up to date as work proceeds.

This plan follows `docs/PLANS.md`.

## Purpose / Big Picture

Cash Register needs a repeatable release path that validates the source, publishes deployable Docker images for x64 and arm64 Linux hosts, and leaves a draft GitHub release for a human to publish after adding notes. After this change, a maintainer can run the `CD` workflow manually with a version such as `v1.0.0`, then expect GHCR images named `cashregister-api:<version>` and `cashregister-fe:<version>`, a matching git tag, and a draft release.

The work also adds a small local `act` harness. `act` runs GitHub Actions workflows locally through Docker, which is useful for catching YAML and workflow-shape errors before pushing, but it is not treated as proof that GHCR publishing or GitHub release creation will succeed.

## Progress

- [x] (2026-05-25 18:42Z) Confirmed the repository has no existing `.github`, `.act`, or `scripts` workflow support and that the current Dockerfiles are `be.Dockerfile` and `ui.Dockerfile`.
- [x] (2026-05-25 18:42Z) Added CI and CD workflow definitions using GitHub setup actions and official Docker actions.
- [x] (2026-05-25 18:42Z) Added minimal workflow helper scripts for release version validation and draft release creation.
- [x] (2026-05-25 18:42Z) Added local `act` support through Mise tasks, shell entrypoints, a Compose-built act CLI image, and a sample dispatch event.
- [x] (2026-05-25 18:42Z) Documented release management in `README.md` and appended the diary entry.
- [x] (2026-05-25 19:06Z) Ran direct backend/frontend verification, `git diff --check`, `act:ci`, and `act:cd`; all completed successfully.

## Surprises & Discoveries

- Observation: The repository still tracks `build-be-dockerfile.sh` and `build-ui-dockerfile.sh`, but those scripts reference stale Dockerfile names.
  Evidence: the tracked Dockerfiles are `be.Dockerfile` and `ui.Dockerfile`, while the helper scripts mention `Api.Dockerfile` and `Ui.Dockerfile`.

- Observation: `docs/DIARY.md` and `docs/v2.MD` already had uncommitted user changes before this task.
  Evidence: `git status --short --branch` reported `M docs/DIARY.md` and `?? docs/v2.MD` before edits.

- Observation: `ghcr.io/nektos/act:latest` is not a usable public act CLI image.
  Evidence: Docker returned `Head "https://ghcr.io/v2/nektos/act/manifests/latest": denied`, so the local Compose harness now builds a tiny Alpine image from the official nektos/act release archive.

- Observation: act's default copy mode is a bad fit after local builds have produced dependency and build-output directories.
  Evidence: the first `act:ci` run stalled at `docker cp src=/work/. dst=/work`; the local scripts now pass `--bind` so act mounts the working tree instead of copying it.

- Observation: act bind mounts must use a path the host Docker daemon can see.
  Evidence: binding from `/work` failed with `The path /work is not shared from the host`; the Compose service now mounts the repository at the same absolute path inside the act CLI container.

- Observation: the CD workflow's explicit checkout ref requires a token under act, and a fake token breaks public action cloning.
  Evidence: `act:cd` first failed with `Input required and not supplied: token`; passing a fake token then caused `Invalid username or token` while cloning public actions. The CD workflow now skips `actions/checkout` only under `ACT=true` and uses the bind-mounted local repository.

## Decision Log

- Decision: CI and CD workflows do not call Mise.
  Rationale: The requested workflow shape is GitHub Actions native. Mise is limited to local convenience tasks for running `act`.
  Date/Author: 2026-05-25 / Codex

- Decision: The CD workflow repeats CI validation before publishing.
  Rationale: Manual releases must prove the exact checked-out ref before images, tags, or releases are written.
  Date/Author: 2026-05-25 / Codex

- Decision: Duplicate GHCR image tags are managed by release discipline instead of registry preflight code.
  Rationale: Git tags and GitHub releases fail naturally when duplicated, but GHCR tag existence checks would add brittle custom scripting that the user explicitly rejected.
  Date/Author: 2026-05-25 / Codex

- Decision: Local `act` CD runs skip GHCR pushes, git tag creation, and release creation.
  Rationale: Local workflow testing should validate workflow shape without mutating public release artifacts.
  Date/Author: 2026-05-25 / Codex

## Outcomes & Retrospective

The release pipeline is implemented. CI validates pushes to `main`; CD is manually dispatched, repeats validation, publishes the planned image names when running on GitHub, and skips public mutation when running under local `act`. The local act harness needed two adjustments discovered during verification: building an act CLI image locally and using host-visible bind paths.

Direct repository verification and both local workflow checks completed successfully on 2026-05-25.

## Context and Orientation

The backend is a .NET solution under `be/` and pins SDK `10.0.202` in `be/global.json`. The frontend is a React/Vite application under `ui/` and builds with Node 22 in `ui.Dockerfile`. The backend image is built by `be.Dockerfile`; the frontend image is built by `ui.Dockerfile`.

GitHub Actions workflow files live under `.github/workflows/`. CI means continuous integration: it checks formatting, compilation, linting, and tests. CD means continuous delivery: it performs the same checks, builds deployable Docker images, pushes them to GitHub Container Registry, then creates release metadata. GHCR means GitHub Container Registry, available at `ghcr.io`.

## Plan of Work

Add `.github/workflows/ci.yml` so pushes to `main` run the documented backend and frontend verification commands. The workflow installs .NET from `be/global.json`, installs Node 22 with npm caching, restores frontend packages with `npm ci`, then runs the backend and frontend checks.

Add `.github/workflows/cd.yml` so a maintainer can manually dispatch a release with a `version` and `target_ref`. The workflow checks out the requested ref, validates the version as a valid git tag and Docker tag, reruns source validation, builds and pushes the backend and frontend images as multi-platform images for `linux/amd64` and `linux/arm64`, then creates an annotated git tag and draft GitHub release. The published image names are `ghcr.io/<owner>/cashregister-api:<version>` and `ghcr.io/<owner>/cashregister-fe:<version>`.

Add `.github/scripts/validate-release-version.sh` and `.github/scripts/create-draft-release.sh` for the workflow logic that GitHub Actions does not provide as a simple native action. Keep these scripts minimal and fail-fast.

Add `mise.toml`, `scripts/act-ci.sh`, `scripts/act-cd.sh`, `docker-compose.act.yaml`, `.act/Dockerfile`, and `.act/cd-event.json` to test workflows locally with `act`. Mise is only a local task launcher. It is not used by GitHub-hosted CI or CD.

Update `README.md` with release management rules. The README should state that version naming is operator-owned, that version reuse is forbidden once public artifacts exist, and that draft releases are edited and published manually.

Append `docs/DIARY.md` with the implementation decision. Do not rewrite existing diary entries.

## Concrete Steps

From the repository root, add the workflow, script, local act, README, and diary files described above.

Make shell scripts executable:

    chmod +x .github/scripts/validate-release-version.sh .github/scripts/create-draft-release.sh scripts/act-ci.sh scripts/act-cd.sh

Run direct repository verification:

    cd be && dotnet format --verify-no-changes && dotnet build && dotnet test
    cd ui && npm ci && npm run lint && npm run build && npm run test

Run local workflow validation when Docker and act images are available:

    mise run act:ci
    mise run act:cd

The `act:cd` task sets `ACT=true`, so the CD workflow must skip GHCR push, tag creation, and GitHub release creation.

## Validation and Acceptance

Acceptance is met when `git diff --check` passes, backend verification passes, frontend verification passes, and the local `act` tasks either pass or report an environmental limitation that does not indicate invalid workflow YAML.

The first real CD run should be performed with a low-stakes version. After it completes, GHCR should contain both `cashregister-api:<version>` and `cashregister-fe:<version>` multi-platform manifests with `linux/amd64` and `linux/arm64` entries, and GitHub should show a draft release for the same tag.

## Idempotence and Recovery

CI is safe to rerun. CD is safe to rerun only before public release artifacts have been created. Once any image, git tag, or release exists for a version, that version is considered burned unless the only failure was draft release creation and the already-pushed images and tag are correct.

If local `act` runs fail because Docker is not running, start Docker and rerun the same Mise task. If they fail because the local act runner image lacks a hosted-runner tool, treat that as local harness drift and verify on GitHub-hosted Actions.

## Artifacts and Notes

The workflow intentionally does not publish `latest`. Version tags are immutable by project discipline, not by custom GHCR preflight scripting.

## Interfaces and Dependencies

The CI workflow depends on `actions/checkout@v4`, `actions/setup-dotnet@v4`, and `actions/setup-node@v4`. The CD workflow also depends on `docker/setup-buildx-action@v4`, `docker/login-action@v4`, and `docker/build-push-action@v7`.

Local workflow tests depend on Docker, Docker Compose, Mise, and the official nektos/act release archive downloaded while building `.act/Dockerfile`. The local scripts pass `--bind` to avoid copying generated dependency and build-output folders into the runner container, and the Compose service uses the host repository path as its container working directory so Docker bind mounts resolve correctly.
