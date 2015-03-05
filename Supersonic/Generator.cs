// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  Generator.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:08 PM

using System;
using System.Collections.Generic;
using System.Linq;
using Supersonic.Templates;

namespace Supersonic
{
    /// <summary>
    /// Primary code generation class that wraps together all of the functionality needed to generate entity framework
    /// data access contexts and classes from the given GeneratorOptions
    /// </summary>
    public class Generator
    {
        private readonly GeneratorOptions _options;

        /// <summary>
        /// Initializes a new generator from the specified GeneratorOptions, including two delegates for handling output.
        /// </summary>
        /// <param name="options">The options for creating and executing the generator.</param>
        public Generator(GeneratorOptions options)
        {
            this._options = options;
        }

        /// <summary>
        /// Executes the generator and creates a collection of tables found in the database.
        /// </summary>
        /// <returns>A collection of tables from the specified database.</returns>
        public IEnumerable<Table> LoadTables(bool keepRejectedObjects)
        {
            var tableReader = new SqlServerSchemaReader(this._options.ConnectionString);
            var tables = tableReader.RetrieveSchema().ToList();

            // Remove unrequired tables/views
            foreach (var table in tables)
                //if(table.IsOmitted)
                //    table.RejectionReason = table.IsView
                //                                ? "This object is not marked with an 'EF_Include' extended property."
                //                                : "This object was deliberately marked with an 'EF_Omit' extended property.";
                if (this._options.SchemaName != null && !string.Equals(table.Schema, this._options.SchemaName, StringComparison.OrdinalIgnoreCase))
                    table.RejectionReason += "<p><strong>Schema Mismatch</strong> - The schema name of this object does not match the schema name filter configured in the generation options (" + this._options.SchemaName + ").</p>";
                //else if(this._options.TableFilterExclude != null && this._options.TableFilterExclude.IsMatch(table.SqlName))
                //    table.RejectionReason += "<p><strong>Table Name Filter</strong> - The name of this object matches the table exclusion regular expression configured in the generation options.</p>";
                //else if(!this._options.IncludeViews && table.IsView)
                //    table.RejectionReason = "The generation options are set to exclude all SQL views.";
                //else if(table.IsView && !table.SqlName.StartsWith("vw_"))
                //    table.RejectionReason = "SQL views must be named according to naming standards- this view does not start with 'vw_' and is therefore invalid.";
                else if (!table.IsView && string.IsNullOrEmpty(table.PrimaryKeyNameHumanCase()))
                    table.RejectionReason += "<p><strong>No Primary Key</strong> - Entity Framework requires entities to have a primary key.</p>";

            // Must be done in this order
            var fkList = tableReader.RetrieveForeignKeys();
            var fkTables = tables.Where(x => !x.IsRejected && !x.IsOmitted).ToList();
            Utilities.IdentifyForeignKeys(fkList, fkTables);
            tables.SetPrimaryKeys();
            tables.IdentifyMappingTables(fkList, this._options.CollectionType);
            Utilities.ProcessForeignKeys(fkList, fkTables, this._options.CollectionType);

            // Remove unrequired tables/views
            foreach (var table in tables)
            {
                if (string.IsNullOrEmpty(table.PrimaryKeyNameHumanCase()))
                    table.RejectionReason += "<p><strong>No Primary Key</strong> - Entity Framework requires entities to have a primary key.</p>";
                if (keepRejectedObjects || (!table.IsRejected && !table.IsOmitted))
                    yield return table;
            }
        }

        /// <summary>
        /// Creates a collection of stored procedures marked for import in the SQL database.
        /// </summary>
        /// <returns>A collection of stored procedures from the database.</returns>
        public IEnumerable<StoredProcedure> LoadStoredProcedures(bool keepRejectedObjects)
        {
            var reader = new SqlServerSchemaReader(this._options.ConnectionString);
            var storedProcedures = reader.RetrieveStoredProcedures();
            foreach (var storedProcedure in storedProcedures)
                //if(storedProcedure.IsOmitted)
                //    storedProcedure.RejectionReason = "This object is not marked with an 'EF_Include' extended property.";

                if (keepRejectedObjects || (!storedProcedure.IsRejected && !storedProcedure.IsOmitted))
                    yield return storedProcedure;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExtendedProperty> LoadExtendedProperties()
        {
            var reader = new SqlServerSchemaReader(this._options.ConnectionString);
            return reader.RetrieveExtendedProperties();
        }

        /// <summary>
        /// Runs the full text template source code generator and outputs the content string
        /// </summary>
        /// <param name="namespace"></param>
        /// <param name="elements"></param>
        /// <param name="connectionStringName"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="pocoNamespace"></param>
        /// <returns></returns>
        public static string GenerateCode(string @namespace, Elements elements, string connectionStringName, IServiceProvider serviceProvider, string pocoNamespace = null)
        {
            var template = new CompleteTemplate(new GeneratorOptions(@namespace, connectionStringName, serviceProvider) { ElementsToGenerate = elements, PocoNamespace = pocoNamespace });
            return template.TransformText();
        }

        /// <summary>
        /// Runs the full text template source code generator and outputs the content string
        /// </summary>
        /// <param name="namespace"></param>
        /// <param name="elements"></param>
        /// <param name="connectionString"></param>
        /// <param name="pocoNamespace"></param>
        /// <returns></returns>
        public static string GenerateCode(string @namespace, Elements elements, string connectionString, string pocoNamespace = null)
        {
            var template = new CompleteTemplate(new GeneratorOptions(@namespace, connectionString) { ElementsToGenerate = elements, PocoNamespace = pocoNamespace });
            return template.TransformText();
        }

        /// <summary>
        /// Runs the full text template source code generator and outputs the content string
        /// </summary>
        /// <param name="namespace"></param>
        /// <param name="elements"></param>
        /// <param name="connectionStringName"></param>
        /// <param name="configuration"></param>
        /// <param name="pocoNamespace"></param>
        /// <param name="solutionDir"></param>
        /// <returns></returns>
        public static string GenerateCode(string @namespace, Elements elements, string connectionStringName, string solutionDir, string configuration, string pocoNamespace = null)
        {
            var template = new CompleteTemplate(new GeneratorOptions(@namespace, connectionStringName, solutionDir, configuration + ".config") { ElementsToGenerate = elements, PocoNamespace = pocoNamespace });
            return template.TransformText();
        }
    }
}