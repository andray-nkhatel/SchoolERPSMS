#!/bin/bash

# Linux Script to Backup Live Database
# This script connects to your live database and creates a backup

# Database connection parameters
SERVER="bluebirddb.mssql.somee.com"
DATABASE="bluebirddb"
USERNAME="chsAdmin_SQLLogin_1"
PASSWORD="gapgxqurvb"

# Create backup directory if it doesn't exist
BACKUP_DIR="$HOME/backups"
mkdir -p "$BACKUP_DIR"

# Generate timestamp for backup file
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_PATH="$BACKUP_DIR/bluebirddb_backup_$TIMESTAMP.bak"

echo "Starting database backup..."
echo "Server: $SERVER"
echo "Database: $DATABASE"
echo "Backup Path: $BACKUP_PATH"

# Check if sqlcmd is available
if ! command -v sqlcmd &> /dev/null; then
    echo "❌ sqlcmd is not installed. Installing Microsoft SQL Server tools..."
    echo "Please run: sudo dnf install mssql-tools"
    echo "Or install from: https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-setup-tools"
    exit 1
fi

# Create backup using sqlcmd
echo "Executing backup command..."
sqlcmd -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -d "$DATABASE" -Q "
BACKUP DATABASE [$DATABASE] 
TO DISK = '$BACKUP_PATH'
WITH 
    FORMAT,
    INIT,
    COMPRESSION,
    CHECKSUM,
    DESCRIPTION = 'Full backup before Baby Class migration - $TIMESTAMP'
"

if [ $? -eq 0 ]; then
    echo "✅ Backup completed successfully!"
    echo "Backup file: $BACKUP_PATH"
    
    # Get backup file size
    if [ -f "$BACKUP_PATH" ]; then
        BACKUP_SIZE=$(du -h "$BACKUP_PATH" | cut -f1)
        echo "Backup size: $BACKUP_SIZE"
    fi
    
    echo ""
    echo "You can now proceed with the migration process safely."
else
    echo "❌ Backup failed!"
    echo "Please check your connection parameters and try again."
    exit 1
fi

echo ""
echo "Backup process completed."
