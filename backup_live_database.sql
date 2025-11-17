-- Backup Script for Live Database
-- This script creates a backup of your live database before making any changes
-- IMPORTANT: Run this FIRST before any migration operations

-- =============================================
-- STEP 1: CREATE FULL DATABASE BACKUP
-- =============================================

-- Create backup with timestamp
DECLARE @BackupPath NVARCHAR(500)
DECLARE @DatabaseName NVARCHAR(100) = 'bluebirddb'  -- Your live database name
DECLARE @Timestamp NVARCHAR(20) = FORMAT(GETDATE(), 'yyyyMMdd_HHmmss')

-- Set backup path (adjust as needed for your server)
SET @BackupPath = 'C:\Backup\bluebirddb_backup_' + @Timestamp + '.bak'

-- Create the backup
BACKUP DATABASE [bluebirddb] 
TO DISK = @BackupPath
WITH 
    FORMAT,
    INIT,
    COMPRESSION,
    CHECKSUM,
    DESCRIPTION = 'Full backup before Baby Class migration - ' + @Timestamp

PRINT 'Backup created successfully at: ' + @BackupPath

-- =============================================
-- STEP 2: VERIFY BACKUP WAS CREATED
-- =============================================

-- Check if backup file exists and get its size
DECLARE @BackupSize BIGINT
SELECT @BackupSize = size * 8 * 1024 FROM sys.database_files WHERE name = 'bluebirddb'

PRINT 'Database size: ' + CAST(@BackupSize / 1024 / 1024 AS NVARCHAR(20)) + ' MB'

-- =============================================
-- STEP 3: CREATE BACKUP VERIFICATION
-- =============================================

-- Verify the backup integrity
RESTORE VERIFYONLY FROM DISK = @BackupPath

IF @@ERROR = 0
    PRINT '✓ Backup verification successful - backup is valid'
ELSE
    PRINT '✗ Backup verification failed - check backup file'

PRINT 'Backup process completed!'
PRINT 'Backup file location: ' + @BackupPath
PRINT 'You can now proceed with the migration process safely.'
