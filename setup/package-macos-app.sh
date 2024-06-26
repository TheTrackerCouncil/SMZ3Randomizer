# Define variables
APP_NAME="SMZ3Randomizer.app"
ZIP_FILE="SMZ3Randomizer.zip"
PUBLISH_OUTPUT_DIRECTORY="src/Randomizer.CrossPlatform/bin/Release/net8.0/osx-arm64/publish"
INFO_PLIST="Info.plist"
ICON_FILE="AppIcon.icns"

# Remove old .app bundle if it exists
if [ -d "$APP_NAME" ]; then
    rm -rf "$APP_NAME"
fi

echo $GITHUB_OUTPUT

# Create the .app bundle structure
mkdir -p "$APP_NAME/Contents/MacOS"
mkdir -p "$APP_NAME/Contents/Resources"

# Copy the Info.plist file and the icon
cp "setup/$INFO_PLIST" "$APP_NAME/Contents/Info.plist"
cp "setup/$ICON_FILE" "$APP_NAME/Contents/Resources/AppIcon.icns"

# Copy the published output to the MacOS directory
cp -a "$PUBLISH_OUTPUT_DIRECTORY/." "$APP_NAME/Contents/MacOS"

echo "Packaged $APP_NAME successfully."

mkdir -p "setup/output"

# Zip the .app bundle
zip -r "setup/output/$ZIP_FILE" "$APP_NAME"