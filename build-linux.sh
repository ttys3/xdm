#!/usr/bin/env bash

set -eou pipefail

cd ./app/XDM/XDM.Gtk.UI

dotnet restore

dotnet build --no-restore

