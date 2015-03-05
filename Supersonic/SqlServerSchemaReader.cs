// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  SqlServerSchemaReader.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:09 PM

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using Supersonic.MSSQL;

namespace Supersonic
{
    internal class SqlServerSchemaReader
    {
        private readonly string _connectionString;

        public SqlServerSchemaReader(string connectionString)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(connectionString), "connectionString");

            this._connectionString = connectionString;
        }

        public List<Table> RetrieveSchema()
        {
            return this.Retrieve(this.ReadSchema, SqlScripts.TableSchema);
        }

        public List<ForeignKey> RetrieveForeignKeys()
        {
            return this.Retrieve(this.ReadForeignKeys, SqlScripts.ForeignKeySchema);
        }

        public List<StoredProcedure> RetrieveStoredProcedures()
        {
            return this.Retrieve(this.ReadStoredProcedures, SqlScripts.StoredProcedureSchema);
        }

        private List<T> Retrieve<T>(Func<SqlDataReader, IEnumerable<T>> work, string commandText)
        {
            var results = new List<T>();
            var command = new SqlCommand(commandText, new SqlConnection(this._connectionString));
            using (command)
            {
                command.Connection.Open();
                using (var rdr = command.ExecuteReader())
                    results.AddRange(work(rdr));
                command.Connection.Close();
            }
            return results;
        }

        private IEnumerable<StoredProcedure> ReadStoredProcedures(SqlDataReader rdr)
        {
            StoredProcedure storedProcedure = null;
            while (rdr.Read())
            {
                var objectName = rdr["ObjectName"].ToString().Trim();

                if (storedProcedure == null)
                    storedProcedure = new StoredProcedure(rdr);
                else if (objectName != storedProcedure.SqlName)
                {
                    yield return storedProcedure;
                    storedProcedure = new StoredProcedure(rdr);
                }
                storedProcedure.Parameters.Add(new Parameter(rdr));
            }
            yield return storedProcedure;
        }

        private IEnumerable<Table> ReadSchema(SqlDataReader rdr)
        {
            Table table = null;
            while (rdr.Read())
            {
                var objectName = rdr["TableName"].ToString().Trim();
                if (table == null)
                    table = new Table(rdr);
                else if (objectName != table.SqlName)
                {
                    yield return this.FillEnum(table);
                    table = new Table(rdr);
                }
                table.Columns.Add(new Column(rdr, table));
            }
            yield return this.FillEnum(table);
        }

        private IEnumerable<ForeignKey> ReadForeignKeys(SqlDataReader rdr)
        {
            while (rdr.Read())
                yield return new ForeignKey(rdr);
        }

        public Table FillEnum(Table table)
        {
            if (table.IsEnum)
            {
                var command = new SqlCommand(string.Empty, new SqlConnection(this._connectionString));
                using (command)
                {
                    command.Connection.Open();
                    command.CommandText = string.Format("SELECT [{0}], [{1}] FROM [{2}]", table.EnumValueColumn, table.EnumNameColumn, table.SqlName);
                    using (var rdr = command.ExecuteReader())
                        while (rdr.Read())
                            table.EnumMembers.Add(Utilities.SqlNameToClassName(rdr[table.EnumNameColumn].ToString()), rdr[table.EnumValueColumn].ToString());
                    command.Connection.Close();
                }
            }
            return table;
        }

        private IEnumerable<ExtendedProperty> ReadExtendedProperties(SqlDataReader rdr)
        {
            while (rdr.Read())
                yield return new ExtendedProperty(rdr);
        }

        public List<ExtendedProperty> RetrieveExtendedProperties()
        {
            return this.Retrieve(this.ReadExtendedProperties, SqlScripts.ExtendedProperties_InsertUpdate);
        }
    }
}