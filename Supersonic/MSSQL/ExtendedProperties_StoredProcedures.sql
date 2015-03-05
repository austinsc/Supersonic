-- Add/Update a stored procedure to the context
DECLARE @SchemaName nvarchar(120) = N'dbo'
DECLARE @ProcedureName nvarchar(120) = N'usp_iSite_SaveConsumerNotificationPendingChanges'
DECLARE @ExecutionMode nvarchar(120) = N'VoidResult' --Valid options are VoidResult, ScalarResult, or TableResult
DECLARE @MethodName nvarchar(120) = N'SavePendingChanges'
DECLARE @ContainerName nvarchar(120) = N'ConsumerNotificationsOperations'

-- EF_Include
BEGIN 
IF ((SELECT COUNT(*) FROM sys.fn_listextendedproperty(NULL, 'SCHEMA', @SchemaName, 'PROCEDURE', @ProcedureName, NULL, NULL) WHERE name = 'EF_Include') = 1)
	EXEC sys.sp_updateextendedproperty @name=N'EF_Include', @value=N'' , @level0type=N'SCHEMA',@level0name=@SchemaName, @level1type=N'PROCEDURE',@level1name=@ProcedureName 
ELSE 
	EXEC sys.sp_addextendedproperty @name=N'EF_Include', @value=N'' , @level0type=N'SCHEMA',@level0name=@SchemaName, @level1type=N'PROCEDURE',@level1name=@ProcedureName
END

-- EF_ExecutionMode
BEGIN
IF ((SELECT COUNT(*) FROM sys.fn_listextendedproperty(NULL, 'SCHEMA', @SchemaName, 'PROCEDURE', @ProcedureName, NULL, NULL) WHERE name = 'EF_ExecutionMode') = 1)
	EXEC sys.sp_updateextendedproperty @name=N'EF_ExecutionMode', @value=@ExecutionMode , @level0type=N'SCHEMA',@level0name=@SchemaName, @level1type=N'PROCEDURE',@level1name=@ProcedureName 
ELSE 
	EXEC sys.sp_addextendedproperty @name=N'EF_ExecutionMode', @value=@ExecutionMode , @level0type=N'SCHEMA',@level0name=@SchemaName, @level1type=N'PROCEDURE',@level1name=@ProcedureName
END

-- EF_MethodName
BEGIN
IF ((SELECT COUNT(*) FROM sys.fn_listextendedproperty(NULL, 'SCHEMA', @SchemaName, 'PROCEDURE', @ProcedureName, NULL, NULL) WHERE name = 'EF_MethodName') = 1)
	EXEC sys.sp_updateextendedproperty @name=N'EF_MethodName', @value=@MethodName , @level0type=N'SCHEMA',@level0name=@SchemaName, @level1type=N'PROCEDURE',@level1name=@ProcedureName 
ELSE 
	EXEC sys.sp_addextendedproperty @name=N'EF_MethodName', @value=@MethodName , @level0type=N'SCHEMA',@level0name=@SchemaName, @level1type=N'PROCEDURE',@level1name=@ProcedureName
END

-- EF_ContainerName
BEGIN
IF ((SELECT COUNT(*) FROM sys.fn_listextendedproperty(NULL, 'SCHEMA', @SchemaName, 'PROCEDURE', @ProcedureName, NULL, NULL) WHERE name = 'EF_ContainerName') = 1)
	EXEC sys.sp_updateextendedproperty @name=N'EF_ContainerName', @value=@ContainerName , @level0type=N'SCHEMA',@level0name=@SchemaName, @level1type=N'PROCEDURE',@level1name=@ProcedureName 
ELSE 
	EXEC sys.sp_addextendedproperty @name=N'EF_ContainerName', @value=@ContainerName , @level0type=N'SCHEMA',@level0name=@SchemaName, @level1type=N'PROCEDURE',@level1name=@ProcedureName
END
GO
