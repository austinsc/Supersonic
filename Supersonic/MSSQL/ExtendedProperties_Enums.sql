-- Add enum generation hints to a table
DECLARE @SchemaName nvarchar(120) = N'dbo'
DECLARE @TableName nvarchar(120) = N'SubjectNotificationTypes'
DECLARE @EnumNameColumn nvarchar(120) = N'Description'
DECLARE @EnumValueColumn nvarchar(120) = N'SubjectNotificationTypeId'

-- EF_EnumName
BEGIN
IF ((SELECT COUNT(*) FROM sys.fn_listextendedproperty(NULL, 'SCHEMA', @SchemaName, 'TABLE', @TableName, NULL, NULL) WHERE name = 'EF_EnumName') = 1)
	EXEC sys.sp_updateextendedproperty @name=N'EF_EnumName', @value=@EnumNameColumn , @level0type=N'SCHEMA',@level0name=@SchemaName, @level1type=N'TABLE',@level1name=@TableName 
ELSE 
	EXEC sys.sp_addextendedproperty @name=N'EF_EnumName', @value=@EnumNameColumn , @level0type=N'SCHEMA',@level0name=@SchemaName, @level1type=N'TABLE',@level1name=@TableName
END

-- EF_EnumValue
BEGIN
IF ((SELECT COUNT(*) FROM sys.fn_listextendedproperty(NULL, 'SCHEMA', @SchemaName, 'TABLE', @TableName, NULL, NULL) WHERE name = 'EF_EnumValue') = 1)
	EXEC sys.sp_updateextendedproperty @name=N'EF_EnumValue', @value=@EnumValueColumn , @level0type=N'SCHEMA',@level0name=@SchemaName, @level1type=N'TABLE',@level1name=@TableName 
ELSE 
	EXEC sys.sp_addextendedproperty @name=N'EF_EnumValue', @value=@EnumValueColumn , @level0type=N'SCHEMA',@level0name=@SchemaName, @level1type=N'TABLE',@level1name=@TableName
END
GO