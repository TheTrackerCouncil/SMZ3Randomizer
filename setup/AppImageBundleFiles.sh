#!/bin/bash

echo "BUILD_ARCH: ${BUILD_ARCH}"
echo "BUILD_TARGET: ${BUILD_TARGET}"
echo "BUILD_SHARE: ${BUILD_SHARE}"
echo "BUILD_APP_BIN: ${BUILD_APP_BIN}"

mkdir ${BUILD_APP_BIN}/DefaultData
mv sprites/Sprites ${BUILD_APP_BIN}/DefaultData/Sprites
mv trackersprites ${BUILD_APP_BIN}/DefaultData/TrackerSprites
mv configs/Profiles ${BUILD_APP_BIN}/DefaultData/Configs
mv configs/Schemas ${BUILD_APP_BIN}/DefaultData/Schemas

UNIQUE_ID=$(uuidgen)
echo "$UNIQUE_ID" >> ${BUILD_APP_BIN}/DefaultData/id.txt

ls -al ${BUILD_APP_BIN}/DefaultData

