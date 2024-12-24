#!/usr/bin/env bash

set -eou pipefail

rm -f app/XDM/chrome-extension.pem

env CRX_KEY_FILE=app/XDM/chrome-extension.pem /usr/bin/google-chrome-stable --pack-extension=./app/XDM/chrome-extension --pack-extension-key=$CRX_KEY_FILE

echo "the erx file is at: app/XDM/chrome-extension.crx"

