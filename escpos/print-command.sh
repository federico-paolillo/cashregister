#!/bin/bash

set -e
set -u
set -o pipefail
set -x

echo "[Print Command] Invoked with file $1"

TEMP_FILENAME="$1_noheader"

echo "[Print Command] Removing hacked preamble and emitting temp. file at $TEMP_FILENAME"

tail --bytes +8 $1 > $TEMP_FILENAME # START from Byte n. We skip the first 7 bytes with the header hack

OUT_FILENAME=$(date +%S%N).html

php /app/esc2html.php $TEMP_FILENAME > $OUT_FILENAME 

echo "[Print Command] Should have emitted the HTML file $OUT_FILENAME"

echo "[Print Command] Cleaning up $TEMP_FILENAME"

# rm --force $TEMP_FILENAME