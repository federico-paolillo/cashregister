# Town fair cash register

> The most simplestest cash register application. It _does not_ track money, just orders.

## Release Management

Releases are manual. Run the `CD` GitHub Actions workflow with the version you want to publish and the git ref you want to release. The workflow validates the source, builds Docker images for `linux/amd64` and `linux/arm64`, pushes them to GitHub Container Registry, creates the git tag, and creates a GitHub draft release.

Version names are operator-owned. Use SemVer-like names if they are useful, but the project does not enforce strict SemVer. The only hard rule is that the version must be valid as both a git tag and a Docker image tag.

Published images are:

- `ghcr.io/<owner>/cashregister-api:<version>`
- `ghcr.io/<owner>/cashregister-fe:<version>`

Do not reuse a version after any public artifact exists. Do not rerun a successful release workflow for the same version. Git tags and GitHub releases fail naturally when duplicated; GHCR image tag reuse is treated as a discipline issue, not as something hidden behind complex preflight scripts.

If a release fails before publishing images, tags, or releases, rerunning with the same version is acceptable. If images, a git tag, or a release already exist, treat that version as burned and use a new one, unless the only failed step was draft release creation and the already-pushed images and tag are correct.

Draft releases are expected. Edit the generated release notes before publishing the release.
