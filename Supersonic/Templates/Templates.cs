// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  Templates.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:14 PM

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Supersonic.Templates
{
    internal partial class CompleteTemplate
    {
        private const string INDENT = "    ";
        private readonly GeneratorOptions _options;
        private readonly string _sourceCode;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="keepRejectedObjects"></param>
        public CompleteTemplate(GeneratorOptions options, bool keepRejectedObjects = false)
        {
            this._options = options;
            var generator = new Generator(options);
            var tables = generator.LoadTables(keepRejectedObjects).ToList();
            var storedProcedures = generator.LoadStoredProcedures(keepRejectedObjects).ToList();

            var parts = new List<string>();
            if (this._options.ElementsToGenerate.HasFlag(Elements.UnitOfWork))
                parts.Add(new UnitOfWorkTemplate(tables, this._options.ContextInterfaceName).TransformText());
            if (this._options.ElementsToGenerate.HasFlag(Elements.Context))
                parts.Add(new ContextTemplate(tables, this._options.ContextInterfaceName, this._options.ContextClassName, this._options.DatabaseName, storedProcedures.GroupBy(x => x.ContainerName)).TransformText());
            if (this._options.ElementsToGenerate.HasFlag(Elements.Poco))
                parts.Add(string.Join(Environment.NewLine, tables.Where(x => x.IsEnum).Select(y => new EnumTemplate(y).TransformText()).Concat(tables.Where(x => !x.IsEnum).Select(y => new PocoTemplate(y).TransformText()))));
            if (this._options.ElementsToGenerate.HasFlag(Elements.PocoConfiguration))
                parts.Add(string.Join(Environment.NewLine, tables.Where(x => !x.IsEnum).Select(x => new ConfigurationTemplate(x, this._options.ConfigurationClassSuffix).TransformText())));
            this._sourceCode = string.Join(Environment.NewLine, parts);
        }
    }

    internal partial class PocoTemplate
    {
        private readonly Table _table;

        public PocoTemplate(Table table)
        {
            this._table = table;
        }
    }

    internal partial class ConfigurationTemplate
    {
        private readonly string _configurationSuffix;
        private readonly Table _table;

        public ConfigurationTemplate(Table table, string configurationSuffix = Extensions.DEFAULT_CONFIGURATION_SUFFIX)
        {
            this._table = table;
            this._configurationSuffix = configurationSuffix;
        }
    }

    internal partial class EnumTemplate
    {
        private readonly Table _table;

        public EnumTemplate(Table table)
        {
            this._table = table;
        }
    }

    internal partial class UnitOfWorkTemplate
    {
        private readonly string _interfaceName;
        private readonly IEnumerable<Table> _tables;

        public UnitOfWorkTemplate(IEnumerable<Table> tables, string interfaceName)
        {
            this._tables = tables;
            this._interfaceName = interfaceName;
        }
    }

    internal partial class ContextTemplate
    {
        private readonly string _className;
        private readonly string _configurationSuffix;
        private readonly string _connectionStringName;
        private readonly string _interfaceName;
        private readonly IEnumerable<IGrouping<string, StoredProcedure>> _storedProcedureContainers;
        private readonly IEnumerable<Table> _tables;

        public ContextTemplate(IEnumerable<Table> tables, string interfaceName, string className, string connectionStringName, IEnumerable<IGrouping<string, StoredProcedure>> storedProcedureContainers, string configurationSuffix = Extensions.DEFAULT_CONFIGURATION_SUFFIX)
        {
            this._tables = tables;
            this._interfaceName = interfaceName;
            this._className = className;
            this._connectionStringName = connectionStringName;
            this._storedProcedureContainers = storedProcedureContainers;
            this._configurationSuffix = configurationSuffix;
        }

        public string RenderStoredProcedureContainers()
        {
            var builder = new StringBuilder();
            foreach (var group in this._storedProcedureContainers)
                builder.AppendLine(new StoredProcedureContainerTemplate(group).TransformText() + Environment.NewLine);
            return builder.ToString();
        }
    }

    internal partial class StoredProcedureContainerTemplate
    {
        private readonly string _containerSuffix;
        private readonly IGrouping<string, StoredProcedure> _storedProcedureContainer;

        public StoredProcedureContainerTemplate(IGrouping<string, StoredProcedure> storedProcedureContainer, string containerSuffix = Extensions.DEFAULT_CONTAINER_SUFFIX)
        {
            this._storedProcedureContainer = storedProcedureContainer;
            this._containerSuffix = containerSuffix;
        }

        public string RenderStoredProcedures()
        {
            var builder = new StringBuilder();
            foreach (var proc in this._storedProcedureContainer)
                builder.AppendLine(new StoredProcedureTemplate(proc).TransformText() + Environment.NewLine);
            return builder.ToString();
        }
    }

    internal partial class StoredProcedureTemplate
    {
        private readonly StoredProcedure _storedProcedure;

        public StoredProcedureTemplate(StoredProcedure storedProcedure)
        {
            this._storedProcedure = storedProcedure;
        }
    }

    internal partial class TestDataTemplate
    {
        private readonly string _commandText;
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly IEnumerable<IDataRecord> _records;
        private readonly Table _table;

        public TestDataTemplate(Table table, string databaseName, string commandText, string connectionString)
        {
            this._table = table;
            this._databaseName = databaseName;
            this._commandText = commandText;
            this._connectionString = connectionString;
            this._records = this.RetrieveData();
        }

        private IEnumerable<IDataRecord> RetrieveData()
        {
            using (var command = new SqlCommand(this._commandText, new SqlConnection(this._connectionString)))
            {
                command.Connection.Open();
                using (var rdr = command.ExecuteReader())
                    while (rdr.Read())
                        yield return rdr;
                command.Connection.Close();
            }
        }
    }

    /// <summary>
    /// Extended property generation template.
    /// </summary>
    public partial class ExtendedPropertyTemplate
    {
        private readonly IEnumerable<ExtendedProperty> _extps;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="ep"></param>
        public ExtendedPropertyTemplate(IEnumerable<ExtendedProperty> ep)
        {
            this._extps = ep;
        }
    }
}