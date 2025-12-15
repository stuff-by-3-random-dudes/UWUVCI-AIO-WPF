#!/bin/bash
APP="/Applications/UWUVCI-V3.app"
if [ -d "$APP" ]; then
    xattr -dr com.apple.quarantine "$APP"
    echo "Quarantine cleared for $APP"
else
    echo "UWUVCI-v3.app not found in /Applications"
fi
read -n 1 -s -r -p "Press any key to close"