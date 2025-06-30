#!/bin/bash

set -e
set -u
set -o pipefail
set -x

echo "[Print Command] Print command invoked with file $1"

FILENAME=$(date +%S%N).html

php /app/esc2html.php $1 > $FILENAME 

echo "[Print Command] Print command should have emitted the HTML file $FILENAME"