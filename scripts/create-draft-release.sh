#!/usr/bin/env sh

set -e
set -u

if [ "$#" -ne 6 ]; then
    echo "Usage: $0 <version> <target-ref> <api-image> <fe-image> <api-digest> <fe-digest>" >&2
    exit 2
fi

version="$1"
target_ref="$2"
api_image="$3"
fe_image="$4"
api_digest="$5"
fe_digest="$6"

git config user.name "github-actions[bot]"
git config user.email "41898282+github-actions[bot]@users.noreply.github.com"

git tag -a "${version}" -m "Release ${version}"
git push origin "refs/tags/${version}"

notes_file="$(mktemp)"
trap 'rm -f "${notes_file}"' EXIT

cat > "${notes_file}" <<EOF
Draft release for ${version}.

Target ref: ${target_ref}

Images:

- [${api_image}@${api_digest}](${api_image}@${api_digest})
- [${fe_image}@${fe_digest}](${fe_image}@${fe_digest})

Note: unknown/unknown OS/Arch entries are [Attestation Manifest Descriptors](https://docs.docker.com/build/metadata/attestations/attestation-storage/#attestation-manifest-descriptor).

Edit these notes before publishing the release.
EOF

gh release create "${version}" \
    --draft \
    --title "${version}" \
    --notes-file "${notes_file}"
