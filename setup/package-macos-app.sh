# Define variables
APP_NAME="SMZ3CasRandomizer.app"
ZIP_FILE="SMZ3CasRandomizerMacOS_$1.zip"
PUBLISH_OUTPUT_DIRECTORY="src/TrackerCouncil.Smz3.UI/bin/Release/net8.0/osx-arm64/publish"
INFO_PLIST="Info.plist"
ICON_FILE="SMZ3.icns"

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

# Bundle sprites and configs
cp -r "sprites/Sprites" "$APP_NAME/Contents/MacOS/Sprites"
cp -r "configs/Profiles" "$APP_NAME/Contents/MacOS/Configs"

echo "Packaged $APP_NAME successfully."

mkdir -p "setup/output"

# Move the app to the output folder
mv "$APP_NAME" "setup/output/$APP_NAME"