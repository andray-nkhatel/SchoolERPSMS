#!/bin/bash

# === Configuration ===
PROJECT_NAME="BluebirdCore"    # <-- Replace with your project name
CONFIGURATION="Release"
OUTPUT_DIR="publish"
FTP_PASSWORD="zheAgile@1993"

# === FTP Settings ===

FTP_HOST="ftp://bluebird.somee.com/www.bluebird.somee.com"     # <-- Replace with actual FTP host
FTP_USER="chsAdmin"      # <-- Replace with FTP username
FTP_PASS="${FTP_PASSWORD}"  # <-- Replace with FTP password
REMOTE_DIR="/www.bluebird.somee.com"             # Usually for SmarterASP.NET

echo "=========================================="
echo "Publishing ASP.NET Core Web API for SmarterASP.NET"
echo "Project: $PROJECT_NAME"
echo "Configuration: $CONFIGURATION"
echo "Output Directory: $OUTPUT_DIR"
echo "=========================================="

# Remove previous publish folder
if [ -d "$OUTPUT_DIR" ]; then
    echo "Removing existing publish folder..."
    rm -rf "$OUTPUT_DIR"
fi

# Run dotnet publish
echo "Publishing project..."
#dotnet publish -c "$CONFIGURATION" -o "$OUTPUT_DIR"
dotnet publish "./BluebirdCore.csproj" -c "$CONFIGURATION" -o "$OUTPUT_DIR"

if [ $? -ne 0 ]; then
    echo "❌ Publish failed. Exiting."
    exit 1
fi

echo "✅ Publish complete!"

# Upload to FTP using lftp
echo "Uploading to SmarterASP.NET via FTP..."

lftp -u "$FTP_USER","$FTP_PASS" "$FTP_HOST" <<EOF
set ftp:ssl-allow no
mirror -R --delete --no-perms --verbose "$OUTPUT_DIR" "$REMOTE_DIR"

bye
EOF

if [ $? -ne 0 ]; then
    echo "❌ FTP upload failed. Please check credentials and network."
    exit 1
fi

echo "✅ Upload complete! Your site should now be live."

# Optionally open the folder in the file manager
xdg-open "$OUTPUT_DIR" 2>/dev/null
