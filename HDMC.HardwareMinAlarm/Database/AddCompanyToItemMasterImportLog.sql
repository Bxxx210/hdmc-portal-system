IF COL_LENGTH('dbo.Item_Master_Import_Log', 'company') IS NULL
BEGIN
    ALTER TABLE dbo.Item_Master_Import_Log
        ADD company NVARCHAR(50) NULL;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Item_Master_Import_Log_Company_UploadedDate'
      AND object_id = OBJECT_ID('dbo.Item_Master_Import_Log')
)
BEGIN
    CREATE INDEX IX_Item_Master_Import_Log_Company_UploadedDate
        ON dbo.Item_Master_Import_Log(company, uploaded_date DESC);
END;
