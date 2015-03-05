SELECT		[Schema].[Name] [Schema], 
			[Objects].[Name] [ObjectName], 
			[Parameters].[name] [ParameterName],
			[Parameters].[parameter_id] [ParameterId],
			[Types].[Name] [TypeName],
			[Parameters].[max_length] [MaxLength],
			[Parameters].[default_value] [Default],
			[Parameters].[is_output] [IsOutput],
			(SELECT TOP(1) ep.value
				FROM sys.fn_listextendedproperty(NULL, 'SCHEMA', [Schema].[Name], 'PROCEDURE', [Objects].[Name], NULL, NULL) AS ep 
				WHERE ep.name = 'EF_MethodName') AS [MethodName],
			(SELECT TOP(1) ep.value
				FROM sys.fn_listextendedproperty(NULL, 'SCHEMA', [Schema].[Name], 'PROCEDURE', [Objects].[Name], NULL, NULL) AS ep 
				WHERE ep.name = 'EF_ContainerName') AS [ContainerName],
			(SELECT TOP(1) ep.value
				FROM sys.fn_listextendedproperty(NULL, 'SCHEMA', [Schema].[Name], 'PROCEDURE', [Objects].[Name], NULL, NULL) AS ep 
				WHERE ep.name = 'EF_ExecutionMode') AS [ExecutionMode],
			(CASE WHEN ((SELECT TOP(1) ep.value 
				FROM sys.fn_listextendedproperty(NULL, 'SCHEMA', [Schema].[Name], 'PROCEDURE', [Objects].[Name], NULL, NULL) AS ep 
				WHERE ep.name = 'EF_Include') IS NULL) 
				THEN CAST(0 AS BIT) 
				ELSE CAST(1 AS BIT)
				END) AS [IsIncluded]
FROM		sys.objects [Objects]
JOIN		sys.schemas [Schema] ON [Schema].[schema_id] = [Objects].[schema_id]
LEFT JOIN	sys.parameters [Parameters] ON [Parameters].[object_id] = [Objects].[object_id]
JOIN		sys.types [Types] ON [Parameters].[user_type_id] = [Types].[user_type_id]
WHERE		[Objects].[is_ms_shipped] = 0 AND [Objects].[type] = 'P'
ORDER BY	[Schema].[schema_id], [Objects].[Name], [Parameters].[parameter_id]