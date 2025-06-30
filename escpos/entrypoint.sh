#!/bin/bash

set -e
set -u
set -o pipefail
set -x

echo "[entrypoint.sh] Starting Custom IPP Printer Emulator"

mkdir -p /app/jobs
mkdir -p /var/log/cups
mkdir -p /var/run/dbus
mkdir -p /var/run/avahi-daemon

CONTAINER_IP=$(hostname -i | awk '{print $1}')

echo "[entrypoint.sh] Container IP is $CONTAINER_IP"

echo "[entrypoint.sh] What the fuck ?!"

socat TCP-LISTEN:8631,bind=$CONTAINER_IP,fork,reuseaddr TCP:localhost:8631 &

echo "[entrypoint.sh] Starting D-Bus"
dbus-daemon --system --fork

echo "[entrypoint.sh] Starting Avahi daemon"
avahi-daemon --daemonize --no-chroot

# Wait for D-BUS and Avahi to come online

sleep 2

echo "[entrypoint.sh] Launching ippeveprinter"

exec ippeveprinter \
    -p 8631 \
    -vvv \
    -d /app/jobs \
    -r off \
    -M "federico-paolillo" \
    -n "localhost" \
    -c /app/print-command.sh \
    "escposemu"