#!/usr/bin/env bash

export BUILD_VER=8.0.29

OUT_DIR=./binary-source

set -eou pipefail

echo "removing ./${OUT_DIR}"
rm -rf ./${OUT_DIR}

mkdir -p ${OUT_DIR}/chrome-extension

dotnet publish -c Release -f net6.0 -r linux-x64 --self-contained ../XDM.Gtk.UI/XDM.Gtk.UI.csproj -o ${OUT_DIR}

echo "packing chrome extension ..."

/usr/bin/microsoft-edge-stable --pack-extension=$(pwd)/${OUT_DIR}/chrome-extension

echo "packing chrome extension done"

echo "$(pwd)/${OUT_DIR}/chrome-extension.crx"

command -v crx3-info || npm -g i crx3-utils

crx3-info < "$(pwd)/${OUT_DIR}/chrome-extension.crx" > "$(pwd)/${OUT_DIR}/chrome-extension.json"

echo "you may need add the extension id to /etc/opt/edge/policies/managed/xxx.json whitelist"
