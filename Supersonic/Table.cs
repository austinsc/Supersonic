// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  Table.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:09 PM

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;

namespace Supersonic
{
    /// <summary>
    /// Represents a SQL table or view.
    /// </summary>
    public class Table : IDatabaseObject
    {
        private Table()
        {
            this.Columns = new List<Column>();
            this.ReverseNavigationProperty = new List<string>();
            this.MappingConfiguration = new List<string>();
            this.ReverseNavigationCtor = new List<string>();
            this.ReverseNavigationUniquePropName = new List<string>();
            this.EnumMembers = new Dictionary<string, string>();
        }

        internal Table(IDataRecord rdr)
            : this()
        {
            this.SqlName = rdr["TableName"].ToString().Trim();
            this.Schema = rdr["SchemaName"].ToString().Trim();
            this.IsView = string.Equals(rdr["TableType"].ToString().Trim(), "View", StringComparison.CurrentCultureIgnoreCase);
            this.ClassName = Utilities.SqlNameToClassName(this.SqlName);
            this.ClassCollectionName = Utilities.SqlNameToClassNamePlural(this.SqlName);
            this.EnumNameColumn = rdr["EnumNameColumn"] == DBNull.Value ? null : rdr["EnumNameColumn"].ToString();
            this.EnumValueColumn = rdr["EnumValueColumn"] == DBNull.Value ? null : rdr["EnumValueColumn"].ToString();
            this.IsOmitted = !(bool)rdr["IsIncluded"];
        }

        /// <summary>
        /// The columns in this table or view.
        /// </summary>
        public List<Column> Columns { get; private set; }

        /// <summary>
        /// Returns true if this table has foreign keys.
        /// </summary>
        public bool HasForeignKey
        {
            get { return this.Columns.Any(x => x.IsForeignKey); }
        }

        /// <summary>
        /// Returns true if any of the columns are nullable.
        /// </summary>
        public bool HasNullableColumns
        {
            get { return this.Columns.Any(x => x.IsNullable); }
        }

        /// <summary>
        /// Returns true if this table is used as a mapping table.
        /// </summary>
        public bool IsMapping { get; private set; }

        /// <summary>
        /// The c# class name of the entity.
        /// </summary>
        public string ClassName { get; private set; }

        /// <summary>
        /// The c# name of a collection of instances of this table.
        /// </summary>
        public string ClassCollectionName { get; private set; }

        /// <summary>
        /// The c# code for building the mapping configuration.
        /// </summary>
        [ScriptIgnore]
        public List<string> MappingConfiguration { get; private set; }

        /// <summary>
        /// The c# code describing reverse navigation property initiailization in the constructor block.
        /// </summary>
        [ScriptIgnore]
        public List<string> ReverseNavigationCtor { get; private set; }

        /// <summary>
        /// The c# code describing reverse navigation property binding in the configuration class.
        /// </summary>
        [ScriptIgnore]
        public List<string> ReverseNavigationProperty { get; private set; }

        [ScriptIgnore]
        internal List<string> ReverseNavigationUniquePropName { get; private set; }

        /// <summary>
        /// Returns the c# code name of the enum
        /// </summary>
        public string EnumName
        {
            get { return Utilities.SqlNameToClassNamePlural(this.SqlName); }
        }

        /// <summary>
        /// The name of the enum 'Name' column
        /// </summary>
        public string EnumNameColumn { get; internal set; }

        /// <summary>
        /// The name of the enum 'Value' column
        /// </summary>
        public string EnumValueColumn { get; internal set; }

        /// <summary>
        /// The data retrieved from the database used to populate the enum values.
        /// </summary>
        public Dictionary<string, string> EnumMembers { get; private set; }

        /// <summary>
        /// A list of primary keys from the associated columns.
        /// </summary>
        [ScriptIgnore]
        public IEnumerable<Column> PrimaryKeys
        {
            get { return this.Columns.Where(x => x.IsPrimaryKey); }
        }

        /// <summary>
        /// An indexer to traverse the collection of columns
        /// </summary>
        /// <param name="columnName">The name of the column to retrieve.</param>
        [ScriptIgnore]
        public Column this[string columnName]
        {
            get { return this.GetColumn(columnName); }
        }

        /// <summary>
        /// True if this object is a table.
        /// </summary>
        public bool IsTable
        {
            get { return !this.IsView; }
        }

        /// <summary>
        /// Returns true is this instance is a view in SQL
        /// </summary>
        public bool IsView { get; private set; }

        /// <summary>
        /// True if this object is a stored procedure.
        /// </summary>
        public bool IsStoredProcedure
        {
            get { return false; }
        }

        /// <summary>
        /// True if this entity is marked with a rejection reason
        /// </summary>
        public bool IsRejected
        {
            get { return !string.IsNullOrEmpty(this.RejectionReason); }
        }

        /// <summary>
        /// Returns true if this table should be omitted from the generator.
        /// </summary>
        public bool IsOmitted { get; internal set; }

        /// <summary>
        /// The child members of this object.
        /// </summary>
        [ScriptIgnore]
        public IEnumerable<IDatabaseMember> Members
        {
            get { return this.Columns; }
        }

        /// <summary>
        /// The SQL name of the table or view.
        /// </summary>
        public string SqlName { get; private set; }

        /// <summary>
        /// Returns the partially qualified ([schema].[objectname]) SQL identifier for this object.
        /// </summary>
        public string QualifiedSqlName
        {
            get { return string.Format("[{0}].[{1}]", this.Schema, this.SqlName); }
        }

        /// <summary>
        /// The generated C# name for this object.
        /// </summary>
        public string CodeName
        {
            get { return this.ClassName; }
        }

        /// <summary>
        /// Gets the SQL schema of the table
        /// </summary>
        public string Schema { get; private set; }

        /// <summary>
        /// Returns true if this table has been configured as an enum
        /// </summary>
        public bool IsEnum
        {
            get { return !string.IsNullOrEmpty(this.EnumNameColumn) && !string.IsNullOrEmpty(this.EnumValueColumn); }
        }

        /// <summary>
        /// If non-null, contains the reason why this object was rejected (not included) in the output
        /// </summary>
        public string RejectionReason { get; internal set; }

        /// <summary>
        /// Calculates the c# lambda expression string for the primary key of this table.
        /// </summary>
        /// <returns>The c# lambda expression string for the primary key of this table.</returns>
        public string PrimaryKeyNameHumanCase()
        {
            var data = this.PrimaryKeys.Select(x => "x." + x.PropertyNameHumanCase).ToList();
            var n = data.Count();
            if (n == 0)
                return string.Empty;
            if (n == 1)
                return "x => " + data.First();
            // More than one primary key
            return string.Format("x => new {{ {0} }}", string.Join(", ", data));
        }

        /// <summary>
        /// Create an object initiailzer code snippet from an actual database entry
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public string CreateObjectInitializer(IDataRecord record)
        {
            var result = string.Empty;
            for (var i = 0; i < record.FieldCount; i++)
            {
                var col = this.GetColumn(record.GetName(i));
                if (col == null)
                    continue;
                var val = col.ConvertToLiteral(record.GetValue(i));
                if (!string.IsNullOrEmpty(val))
                    result += string.Format("{0} = {1}, ", col.PropertyNameHumanCase, val);
            }
            return result;
        }

        internal Column GetColumn(string columnName)
        {
            return this.Columns.SingleOrDefault(x => string.Equals(x.Name, columnName, StringComparison.OrdinalIgnoreCase));
        }

        internal string GetUniqueColumnPropertyName(string columnName)
        {
            if (this.ReverseNavigationUniquePropName.Count == 0)
            {
                this.ReverseNavigationUniquePropName.Add(this.ClassName);
                this.ReverseNavigationUniquePropName.AddRange(this.Columns.Select(c => c.PropertyNameHumanCase));
            }

            if (!this.ReverseNavigationUniquePropName.Contains(columnName))
            {
                this.ReverseNavigationUniquePropName.Add(columnName);
                return columnName;
            }

            for (var n = 1; n < 100; ++n)
            {
                var col = columnName + n;
                if (this.ReverseNavigationUniquePropName.Contains(col))
                    continue;
                this.ReverseNavigationUniquePropName.Add(col);
                return col;
            }

            // Give up
            return columnName;
        }

        internal void AddReverseNavigation(Relationship relationship, string fkName, Table fkTable, string propName, string constraint, string collectionType)
        {
            switch (relationship)
            {
                case Relationship.OneToOne:
                    this.ReverseNavigationProperty.Add(string.Format("public virtual {0} {1} {{ get; set; }} // {2}", fkTable.ClassName, propName, constraint));
                    break;

                case Relationship.OneToMany:
                    this.ReverseNavigationProperty.Add(string.Format("public virtual {0} {1} {{ get; set; }} // {2}", fkTable.ClassName, propName, constraint));
                    break;

                case Relationship.ManyToOne:
                    this.ReverseNavigationProperty.Add(string.Format("public virtual ICollection<{0}> {1} {{ get; set; }} // {2}", fkTable.ClassName, Utilities.SqlNameToClassNamePlural(propName), constraint));
                    this.ReverseNavigationCtor.Add(string.Format("{0} = new {1}<{2}>();", Utilities.SqlNameToClassNamePlural(propName), collectionType, fkTable.ClassName));
                    break;

                case Relationship.ManyToMany:
                    this.ReverseNavigationProperty.Add(string.Format("public virtual ICollection<{0}> {1} {{ get; set; }} // Many to many mapping", fkTable.ClassName, Utilities.SqlNameToClassNamePlural(propName)));
                    this.ReverseNavigationCtor.Add(string.Format("{0} = new {1}<{2}>();", Utilities.SqlNameToClassNamePlural(propName), collectionType, fkTable.ClassName));
                    break;
            }
        }

        internal void AddMappingConfiguration(ForeignKey left, ForeignKey right)
        {
            var leftTableHumanCase = left.PkTableClassNamePlural;
            var rightTableHumanCase = right.PkTableClassNamePlural;

            this.MappingConfiguration.Add(string.Format(@"HasMany(t => t.{0}).WithMany(t => t.{1}).Map(m => 
            {{
                m.ToTable(""{2}"");
                m.MapLeftKey(""{3}"");
                m.MapRightKey(""{4}"");
            }});", rightTableHumanCase, leftTableHumanCase, left.FkTableSqlName, left.FkColumnSqlName, right.FkColumnSqlName));
        }

        internal void SetPrimaryKeys()
        {
            if (this.PrimaryKeys.Any())
                return; // Table has at least one primary key

            // This table is not allowed in EntityFramework as it does not have a primary key.
            // Therefore generate a composite key from all non-null fields.
            foreach (var col in this.Columns.Where(x => !x.IsNullable))
                col.IsPrimaryKey = true;
        }

        internal void IdentifyMappingTable(List<ForeignKey> fkList, ICollection<Table> tables, string collectionType)
        {
            this.IsMapping = false;

            // Must have only 2 columns to be a mapping table
            if (this.Columns.Count != 2)
                return;

            // All columns must be primary keys
            if (this.PrimaryKeys.Count() != 2)
                return;

            // No columns should be nullable
            if (this.Columns.Any(x => x.IsNullable))
                return;

            // Find the foreign keys for this table
            var foreignKeys = fkList.Where(x => string.Equals(x.FkTableSqlName, this.SqlName, StringComparison.OrdinalIgnoreCase)
                                                && string.Equals(x.FkSchema, this.Schema, StringComparison.OrdinalIgnoreCase)).ToList();

            // Each column must have a foreign key, therefore check column and foreign key counts match
            if (foreignKeys.Select(x => x.FkColumnSqlName).Distinct().Count() != 2)
                return;

            var left = foreignKeys[0];
            var right = foreignKeys[1];

            var leftTable = tables.GetTable(left.PkTableSqlName, left.PkSchema);
            if (leftTable == null)
                return;

            var rightTable = tables.GetTable(right.PkTableSqlName, right.PkSchema);
            if (rightTable == null)
                return;

            leftTable.AddMappingConfiguration(left, right);

            this.IsMapping = true;
            rightTable.AddReverseNavigation(Relationship.ManyToMany, rightTable.ClassName, leftTable,
                                            rightTable.GetUniqueColumnPropertyName(leftTable.ClassCollectionName), null, collectionType);

            leftTable.AddReverseNavigation(Relationship.ManyToMany, leftTable.ClassName, rightTable,
                                           leftTable.GetUniqueColumnPropertyName(rightTable.ClassCollectionName), null, collectionType);
        }
    }
}