// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  Column.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:02 PM

using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace Supersonic
{
    /// <summary>
    /// Represents a SQL table or view's column
    /// </summary>
    public class Column : IDatabaseMember
    {
        private static readonly Regex RX_CLEAN = new Regex("^(event|Equals|GetHashCode|GetType|ToString|repo|Save|IsNew|Insert|Update|Delete|Exists|SingleOrDefault|Single|First|FirstOrDefault|Fetch|Page|Query)$");

        private readonly Table _table;

        internal Column(IDataRecord rdr, Table table)
        {
            if(rdr == null)
                throw new ArgumentNullException("rdr");

            this._table = table;
            var typename = rdr["TypeName"].ToString().Trim();
            this.Name = rdr["ColumnName"].ToString().Trim();
            this.PropertyType = Utilities.SqlTypeToCodeType(typename);
            this.ColumnType = Regex.Replace(typename, @"\([0-9].\)", string.Empty);
            this.MaxLength = this.PropertyType == "string" ? (int)rdr["MaxLength"] : 0;
            this.Scale = (int)rdr["Scale"];
            this.Precision = (int)rdr["Precision"];
            this.IsRowVersion = !this.IsNullable && typename == "timestamp";
            if(this.IsRowVersion)
                this.MaxLength = 8;

            this.Ordinal = (int)rdr["Ordinal"];
            this.IsIdentity = (bool)rdr["IsIdentity"];
            this.IsNullable = (bool)rdr["IsNullable"];
            this.IsStoreGenerated = (bool)rdr["IsStoreGenerated"];
            this.IsPrimaryKey = (bool)rdr["IsPrimaryKey"];
            this.IsEnumValue = string.Equals(rdr["EnumValueColumn"].ToString().Trim(), this.Name, StringComparison.CurrentCultureIgnoreCase);
            this.IsEnumName = string.Equals(rdr["EnumNameColumn"].ToString().Trim(), this.Name, StringComparison.CurrentCultureIgnoreCase);
            this.IsForeignKey = false;

            this.Default = rdr["Default"].ToString().Trim();
            this.Default = ConvertValueToString(this.Default, this.PropertyType);
            this.PropertyName = Utilities.ScrubName(this.Name);
            this.PropertyName = RX_CLEAN.Replace(this.PropertyName, "_$1");
            this.PropertyNameHumanCase = Utilities.SqlNameToClassName(this.PropertyName);

            // Make sure property name doesn't clash with class name
            if(this.PropertyName == table.ClassName)
                this.PropertyName = this.PropertyName + "_";

            if(this.PropertyNameHumanCase == string.Empty)
                this.PropertyNameHumanCase = this.PropertyName;

            // Make sure property name doesn't clash with class name
            if(this.PropertyNameHumanCase == table.ClassName)
                this.PropertyNameHumanCase = this.PropertyNameHumanCase + "_";

            if(char.IsDigit(this.PropertyNameHumanCase[0]))
                this.PropertyNameHumanCase = "_" + this.PropertyNameHumanCase;
        }

        /// <summary>
        /// The SQL name of the column.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// A string representing the default value of the column.
        /// </summary>
        public string Default { get; private set; }

        /// <summary>
        /// The maximum length (in bytes) of the column data.
        /// </summary>
        public int MaxLength { get; private set; }

        /// <summary>
        /// The SQL precision of the column.
        /// </summary>
        public int Precision { get; private set; }

        /// <summary>
        /// The c# .net PropertyName for the associated entity framework class property.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// The c# .net PropertyName for the associated entity framework class property.
        /// </summary>
        public string PropertyNameHumanCase { get; private set; }

        /// <summary>
        /// A string representing the .net type of the property.
        /// </summary>
        public string PropertyType { get; private set; }

        /// <summary>
        /// A string representing the SQL datatype of the column.
        /// </summary>
        public string ColumnType { get; private set; }

        /// <summary>
        /// The SQL scale of the datatype (if available).
        /// </summary>
        public int Scale { get; private set; }

        /// <summary>
        /// The SQL ordinal position of the column relative to the other columns in the table/view.
        /// </summary>
        public int Ordinal { get; private set; }

        /// <summary>
        /// Returns true if the column is a SQL key.
        /// </summary>
        public bool IsIdentity { get; private set; }

        /// <summary>
        /// Returns true if the column's data can be null.
        /// </summary>
        public bool IsNullable { get; private set; }

        /// <summary>
        /// Returns true if the column is the associated table's primary key (or a component thereof).
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Returns true if the column holds generated data like a row version or an incrementing primary key.
        /// </summary>
        public bool IsStoreGenerated { get; private set; }

        /// <summary>
        /// Returns true if this instance represents a SQL timestamp/generated column.
        /// </summary>
        public bool IsRowVersion { get; private set; }

        /// <summary>
        /// Returns true if the column represents an enum description column
        /// </summary>
        public bool IsEnumName { get; private set; }

        /// <summary>
        /// Returns true if this column is configured as a foreign key to an enum.
        /// </summary>
        public bool IsEnumValue { get; private set; }

        /// <summary>
        /// Returns true if this column is a foreign key.
        /// </summary>
        public bool IsForeignKey { get; set; }

        /// <summary>
        /// Returns true if the associated foreign key's table is configured as an enum.
        /// </summary>
        public bool IsForeignKeyEnum { get; set; }

        /// <summary>
        /// The instance of the foreign key data from SQL (if available).
        /// </summary>
        internal ForeignKey ForeignKey { get; set; }

        /// <summary>
        /// This property holds the foreign key configuration class c# code for this column.
        /// </summary>
        public string ForeignKeyConfiguration { get; set; }

        /// <summary>
        /// Gets the configuration of the property in c#.
        /// </summary>
        public string PropertyConfiguration
        {
            get
            {
                var hasDatabaseGeneratedOption = false;
                var propertyType = this.PropertyType.ToLower();
                switch(propertyType)
                {
                    case "long":
                    case "short":
                    case "int":
                    case "double":
                    case "float":
                    case "decimal":
                        hasDatabaseGeneratedOption = true;
                        break;
                }
                var databaseGeneratedOption = string.Empty;
                if(hasDatabaseGeneratedOption)
                {
                    if(this.IsIdentity)
                        databaseGeneratedOption = ".HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)";
                    if(this.IsStoreGenerated)
                        databaseGeneratedOption = ".HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed)";
                    if(this.IsPrimaryKey && !this.IsIdentity && !this.IsStoreGenerated)
                        databaseGeneratedOption = ".HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)";
                }
                return string.Format("Property(x => x.{0}).HasColumnName(\"{1}\"){2}{3}{4}{5}{6}{7};",
                                     (this.IsForeignKey) ? this.ForeignKey.PkTableClassName : this.PropertyNameHumanCase,
                                     this.Name,
                                     (this.IsNullable) ? ".IsOptional()" : ".IsRequired()",
                                     (this.MaxLength > 0) ? ".HasMaxLength(" + this.MaxLength + ")" : string.Empty,
                                     //(this.PropertyType != "DateTime" && this.Scale > 0) ? ".HasPrecision(" + this.Precision + "," + this.Scale + ")" : string.Empty,
                                     (this.IsStoreGenerated && propertyType == "timestamp") ? ".IsFixedLength().IsRowVersion()" : string.Empty,
                                     databaseGeneratedOption,
                                     (this.IsRowVersion) ? ".IsRowVersion()" : string.Empty,
                                     ".HasColumnType(\"" + this.ColumnType + "\")");
            }
        }

        /// <summary>
        /// Returns the entity foreign key configuration in c#.
        /// </summary>
        public string EntityFk { get; set; }

        /// <summary>
        /// Returns the entity (POCO) property code in c#.
        /// </summary>
        public string Entity
        {
            get
            {
                return string.Format("public {0}{1} {2} {3} // {4}{5}",
                                     this.PropertyType,
                                     this.CheckNullable(),
                                     this.PropertyNameHumanCase,
                                     "{ get; set; }",
                                     this.Name,
                                     this.IsPrimaryKey ? " (Primary key)" : string.Empty);
            }
        }

        /// <summary>
        /// The generated C# object name (identifier)
        /// </summary>
        public string CodeName
        {
            get { return this.PropertyName; }
        }

        /// <summary>
        /// The C# Type of the member
        /// </summary>
        public string CodeType
        {
            get { return this.PropertyType; }
        }

        /// <summary>
        /// The original SQL name of the member.
        /// </summary>
        public string SqlName
        {
            get { return this.Name; }
        }

        /// <summary>
        /// The SQL datatype of the member.
        /// </summary>
        public string SqlType
        {
            get { return this.ColumnType; }
        }

        private string CheckNullable()
        {
            if(this.IsNullable
               && this.PropertyType != "byte[]"
               && this.PropertyType != "string"
               && this.PropertyType != "Microsoft.SqlServer.Types.SqlGeography"
               && this.PropertyType != "Microsoft.SqlServer.Types.SqlGeometry"
               && this.PropertyType != "System.Data.Spatial.DbGeography"
               && this.PropertyType != "System.Data.Spatial.DbGeometry")
                return "?";
            return string.Empty;
        }

        internal string ConvertToLiteral(object o)
        {
            if(o == null || o == DBNull.Value)
                return "null";
            return ConvertValueToString(o.ToString(), this.PropertyType);
        }

        private static string ConvertValueToString(string @default, string propertyType)
        {
            if(string.IsNullOrEmpty(@default))
                return @default;

            while(@default.First() == '(' && @default.Last() == ')' && @default.Length > 2)
                @default = @default.Substring(1, @default.Length - 2);

            if(@default.First() == '\'' && @default.Last() == '\'' && @default.Length > 2)
                @default = string.Format("\"{0}\"", @default.Substring(1, @default.Length - 2));

            switch(propertyType.ToLower())
            {
                case "bool":
                    @default = (@default == "0") ? "false" : "true";
                    break;

                case "string":
                case "datetime":
                case "timespan":
                case "datetimeoffset":
                    if(@default.First() != '"')
                        @default = string.Format("\"{0}\"", @default);
                    if(@default.Contains('\\'))
                        @default = "@" + @default;
                    break;

                case "long":
                case "short":
                case "int":
                case "double":
                case "float":
                case "decimal":
                case "byte":
                case "guid":
                    if(@default.First() == '\"' && @default.Last() == '\"' && @default.Length > 2)
                        @default = @default.Substring(1, @default.Length - 2);
                    break;

                case "byte[]":
                case "System.Data.Spatial.DbGeography":
                case "System.Data.Spatial.DbGeometry":
                    @default = string.Empty;
                    break;
            }

            if(string.IsNullOrWhiteSpace(@default))
                return @default;

            // Validate default
            switch(propertyType.ToLower())
            {
                case "long":
                    long l;
                    if(!Int64.TryParse(@default, out l))
                        @default = string.Empty;
                    break;

                case "short":
                    short s;
                    if(!Int16.TryParse(@default, out s))
                        @default = string.Empty;
                    break;

                case "int":
                    int i;
                    if(!Int32.TryParse(@default, out i))
                        @default = string.Empty;
                    break;

                case "datetime":
                    DateTime dt;
                    if(!DateTime.TryParse(@default, out dt))
                        @default = @default.ToLower().Contains("getdate()") ? "System.DateTime.Now" : string.Empty;
                    else
                        @default = string.Format("System.DateTime.Parse({0})", @default);
                    break;

                case "datetimeoffset":
                    DateTimeOffset dto;
                    if(!DateTimeOffset.TryParse(@default, out dto))
                        @default = @default.ToLower().Contains("sysdatetimeoffset()") ? "System.DateTimeOffset.Now" : string.Empty;
                    else
                        @default = string.Format("System.DateTimeOffset.Parse({0})", @default);
                    break;

                case "timespan":
                    TimeSpan ts;
                    @default = TimeSpan.TryParse(@default, out ts)
                                   ? string.Format("System.TimeSpan.Parse({0})", @default)
                                   : string.Empty;
                    break;

                case "double":
                    double d;
                    if(!Double.TryParse(@default, out d))
                        @default = string.Empty;
                    break;

                case "float":
                    float f;
                    if(!Single.TryParse(@default, out f))
                        @default = string.Empty;
                    break;

                case "decimal":
                    decimal dec;
                    if(!Decimal.TryParse(@default, out dec))
                        @default = string.Empty;
                    break;

                case "byte":
                    byte b;
                    if(!Byte.TryParse(@default, out b))
                        @default = string.Empty;
                    break;

                case "bool":
                    bool x;
                    if(!Boolean.TryParse(@default, out x))
                        @default = string.Empty;
                    break;

                case "guid":
                    if(@default.ToLower() == "newid()" || @default.ToLower() == "newsequentialid()")
                        @default = "System.Guid.NewGuid()";
                    break;
            }

            // Append type letters
            switch(propertyType.ToLower())
            {
                case "decimal":
                    @default = @default + "m";
                    break;
            }
            return @default;
        }
    }
}