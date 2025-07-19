#!/bin/bash

set -e
set -u
set -o pipefail
set -x

WRAPPED_FILENAME="$2_wrapped"

printf "UNIRAST" > $WRAPPED_FILENAME

cat $2 >> $WRAPPED_FILENAME

lp -d $1 -o "document-format=image/urf" $WRAPPED_FILENAME