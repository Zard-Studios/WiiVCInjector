#!/bin/bash
APP_NAME="WiiVC Injector"
PUBLISH_DIR="./TeconMoon's WiiVC Injector/bin/Release/net6.0/osx-arm64/publish"
APP_BUNDLE="$PUBLISH_DIR/$APP_NAME.app"

# Crea la struttura del bundle
mkdir -p "$APP_BUNDLE/Contents/MacOS"
mkdir -p "$APP_BUNDLE/Contents/Resources"

# Copia i file compilati
cp -r "$PUBLISH_DIR"/* "$APP_BUNDLE/Contents/MacOS/"

# Crea il file Info.plist
cat > "$APP_BUNDLE/Contents/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>TeconMoon's WiiVC Injector</string>
    <key>CFBundleIdentifier</key>
    <string>com.teconmoon.wiivcinject</string>
    <key>CFBundleName</key>
    <string>WiiVC Injector</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.12</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0</string>
    <key>CFBundleVersion</key>
    <string>1</string>
</dict>
</plist>
EOF

# Rendi eseguibile il file principale
chmod +x "$APP_BUNDLE/Contents/MacOS/TeconMoon's WiiVC Injector" 