#!/usr/bin/env bash

export BUILD_VER=8.0.29

OUT_DIR=./binary-source

set -eou pipefail

# Input value for private key must be a valid format (PKCS#8-format PEM-encoded RSA key).
CRX_KEY_FILE=${CRX_KEY_FILE:-}

if [ -z "$CRX_KEY_FILE" ]; then
    echo "you must set CRX_KEY_FILE point to chrome private key file path"
    exit 1
fi

echo "removing ./${OUT_DIR}"
rm -rf ./${OUT_DIR}

mkdir -p ${OUT_DIR}/chrome-extension

dotnet publish -c Release -f net6.0 -r linux-x64 --self-contained ../XDM.Gtk.UI/XDM.Gtk.UI.csproj -o ${OUT_DIR}

echo "packing chrome extension ..."

# https://developer.chrome.com/docs/extensions/mv3/linux_hosting/#package-through-command-line
# https://developer.chrome.com/docs/extensions/mv3/manifest/key/#keep-consistent-id
#/usr/bin/microsoft-edge-stable --pack-extension=$(pwd)/${OUT_DIR}/chrome-extension --pack-extension-key=$CRX_KEY_FILE
/usr/bin/google-chrome-stable --pack-extension=$(pwd)/${OUT_DIR}/chrome-extension --pack-extension-key=$CRX_KEY_FILE

echo "packing chrome extension done"

echo "$(pwd)/${OUT_DIR}/chrome-extension.crx"

command -v crx3-info || npm -g i crx3-utils

crx3-info < "$(pwd)/${OUT_DIR}/chrome-extension.crx" > "$(pwd)/${OUT_DIR}/chrome-extension.json"

echo "you may need add the extension id to /etc/opt/edge/policies/managed/xxx.json whitelist"
