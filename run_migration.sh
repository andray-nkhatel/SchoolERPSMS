#!/bin/bash
# Script to run the migration
# Make sure SQL Server is running before executing this script

cd "$(dirname "$0")"

echo "Running migration script..."
echo "Connecting to SQL Server..."

sqlcmd -S localhost,1433 \
       -d SchoolDB \
       -U sa \
       -P 'scherp@2025' \
       -C \
       -i migration_add_subject_inheritance.sql

if [ $? -eq 0 ]; then
    echo "Migration completed successfully!"
else
    echo "Migration failed. Please check:"
    echo "1. SQL Server is running"
    echo "2. Connection details are correct"
    echo "3. Database 'SchoolDB' exists"
fi


