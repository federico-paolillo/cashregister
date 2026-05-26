# Town fair cash register

> The most simplestest cash register application. It _does not_ track money, just orders.

## Getting Started

Use Docker Compose when you want the application running like the local deployment shape. It builds the backend API, frontend, and gateway, then exposes the app at `http://localhost:65000`.

```bash
mise run compose
```

Equivalent direct command:

```bash
docker compose --file=docker-compose.local.yaml up --build --detach
```

The Compose stack stores SQLite state in the named Docker volume `database`.

Use Mise tasks when you want to run the development servers directly. Install frontend dependencies once before the first frontend run:

```bash
cd ui
npm ci
```

Then run both services:

```bash
mise run run
```

Or run them separately:

```bash
mise run be
mise run ui
```

The backend listens on `http://localhost:5122`. The frontend is served by Vite, and its `/api` proxy forwards requests to the backend.

Local workflow checks also use Mise:

```bash
mise run ci
mise run cd
```

`mise run ci` runs the CI workflow locally through `act`. `mise run cd` runs the CD workflow locally through `act` without publishing images, creating git tags, or creating GitHub releases.

## Release Management

Releases are manual. Run the `CD` GitHub Actions workflow with the version you want to publish and the git ref you want to release. The workflow validates the source, builds Docker images for `linux/amd64` and `linux/arm64`, pushes them to GitHub Container Registry, creates the git tag, and creates a GitHub draft release.

Version names are operator-owned. Use SemVer-like names if they are useful, but the project does not enforce strict SemVer or preflight the version shape beyond what the release tools naturally accept.

Published images are:

- `ghcr.io/<owner>/cashregister-api:<version>`
- `ghcr.io/<owner>/cashregister-fe:<version>`

GitHub Container Registry may show an `unknown/unknown` OS/Arch entry for each image. That entry is BuildKit attestation metadata, not a third runnable image. The runnable image manifests are `linux/amd64` and `linux/arm64`.

Do not reuse a version after any public artifact exists. Do not rerun a successful release workflow for the same version. Git tags and GitHub releases fail naturally when duplicated; GHCR image tag reuse is treated as a discipline issue, not as something hidden behind complex preflight scripts.

If a release fails before publishing images, tags, or releases, rerunning with the same version is acceptable. If images, a git tag, or a release already exist, treat that version as burned and use a new one, unless the only failed step was draft release creation and the already-pushed images and tag are correct.

Draft releases are expected. Edit the generated release notes before publishing the release.
