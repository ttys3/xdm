#!/usr/bin/env bash

export BUILD_VER=8.0.29

OUT_DIR=./binary-source

set -eou pipefail

echo "removing ./${OUT_DIR}"
rm -rf ./${OUT_DIR}

mkdir -p ${OUT_DIR}/chrome-extension

dotnet publish -c Release -f net6.0 -r linux-x64 --self-contained ../XDM.Gtk.UI/XDM.Gtk.UI.csproj -o ${OUT_DIR}
