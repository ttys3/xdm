#!/usr/bin/env bash

set -eou pipefail

# https://developer.chrome.com/docs/extensions/mv3/manifest/key/#keep-consistent-id
# ref https://stackoverflow.com/a/46739698

openssl genrsa 2048 | openssl pkcs8 -topk8 -nocrypt -out key.pem


openssl rsa -in key.pem -pubout -outform DER | openssl base64 -A > pubkey.pem

echo "Extension ID:"

openssl rsa -in key.pem -pubout -outform DER | sha256sum | head -c32 | tr 0-9a-f a-p
