FROM alpine:3.22

ARG ACT_VERSION=0.2.88
ARG TARGETARCH

RUN apk add --no-cache ca-certificates curl docker-cli git tar \
    && case "${TARGETARCH}" in \
        amd64) act_arch=x86_64 ;; \
        arm64) act_arch=arm64 ;; \
        *) echo "Unsupported architecture: ${TARGETARCH}" >&2; exit 1 ;; \
    esac \
    && curl -fsSL \
        "https://github.com/nektos/act/releases/download/v${ACT_VERSION}/act_Linux_${act_arch}.tar.gz" \
        -o /tmp/act.tar.gz \
    && tar -xzf /tmp/act.tar.gz -C /usr/local/bin act \
    && rm /tmp/act.tar.gz
