-- Mark the InitialCreate migration as applied since tables already exist
IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20251124003546_InitialCreate')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
    VALUES ('20251124003546_InitialCreate', '9.0.5')
    PRINT 'Migration marked as applied successfully'
END
ELSE
BEGIN
    PRINT 'Migration already exists in history'
END
