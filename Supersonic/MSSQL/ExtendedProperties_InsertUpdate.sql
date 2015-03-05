select p.name as Name
, p.value as Value
, t.name as ObjectName
, cast(case
	when t.type_desc = 'USER_TABLE' then 'TABLE'
	when t.type_desc = 'SQL_STORED_PROCEDURE' then 'PROCEDURE'
	else 'VIEW'
  end as varchar(20)) as ObjectType
, s.name as ObjectSchema
from sys.extended_properties p
inner join sys.objects t on p.major_id = t.object_id
inner join sys.schemas s on t.schema_id = s.schema_id
where p.name like 'EF_%'
order by t.name
