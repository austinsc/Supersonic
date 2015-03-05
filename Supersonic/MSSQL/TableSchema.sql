select			ef_schemas.name as [SchemaName],
				ef_tables.name as [TableName],
				ef_tables.type_desc as [TableType],
				cast(ef_columns.column_id as int) as [Ordinal],
				ef_columns.name as [ColumnName],
				ef_columns.is_nullable as [IsNullable],
				ef_types.name as [TypeName],
				cast(isnull(ef_columns.max_length, 0) as int) as [MaxLength],
				cast(isnull(ef_columns.precision, 0) as int) as [Precision],
				object_definition(ef_columns.default_object_id) as [Default],
				cast(isnull(ef_columns.scale, 0) as int) as [Scale],
				cast(ef_columns.is_identity as bit) as [IsIdentity],
				cast(isnull(ef_index_columns.index_column_id, 0) as bit) as [IsPrimaryKey],
				cast(ef_columns.is_computed | case when ef_types.name = 'timestamp' then 1 else 0 end as bit) as [IsStoreGenerated],
				cast(case when ef_tables.type_desc = 'USER_TABLE' then case when ef_ep_omit.name is null then 1 else 0 end else case when ef_ep_include.name is null then 0 else 1 end end as bit) as [IsIncluded],
				ef_ep_enum_name.value AS [EnumNameColumn],
				ef_ep_enum_value.value AS [EnumValueColumn]
from			(select [name], [type_desc], [schema_id], [object_id] from sys.tables union all select [name], [type_desc], [schema_id], [object_id] from sys.views) as ef_tables
left outer join	sys.schemas as ef_schemas
					on ef_schemas.schema_id = ef_tables.schema_id
left outer join	sys.extended_properties as ef_ep_include
					on ef_ep_include.major_id = ef_tables.object_id
						and ef_ep_include.name = 'EF_Include'
left outer join	sys.extended_properties as ef_ep_omit
					on ef_ep_omit.major_id = ef_tables.object_id
						and ef_ep_omit.name = 'EF_Omit'
left outer join	sys.extended_properties as ef_ep_enum_name
					on ef_ep_enum_name.major_id = ef_tables.object_id
						and ef_ep_enum_name.name = 'EF_EnumName'
left outer join	sys.extended_properties as ef_ep_enum_value
					on ef_ep_enum_value.major_id = ef_tables.object_id
						and ef_ep_enum_value.name = 'EF_EnumValue'
inner join		sys.all_columns as ef_columns 
					on ef_columns.object_id = ef_tables.object_id
left outer join sys.indexes as ef_indexes 
					on ef_indexes.object_id = ef_columns.object_id 
						and 1 = ef_indexes.is_primary_key
left outer join sys.index_columns as ef_index_columns 
					on ef_index_columns.index_id = ef_indexes.index_id 
						and ef_index_columns.column_id = ef_columns.column_id
						and ef_index_columns.object_id = ef_columns.object_id
						and 0 = ef_index_columns.is_included_column
left outer join sys.types as ef_types 
					on ef_types.user_type_id = ef_columns.system_type_id