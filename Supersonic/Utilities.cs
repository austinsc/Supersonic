// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  Utilities.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:09 PM

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Supersonic
{
    /// <summary>
    /// General utility class for EF code generation
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Regex helper for scrubbing variable names.
        /// </summary>
        private static readonly Regex NAME_SCRUB_REGEX = new Regex(@"[^\w\d_]", RegexOptions.Compiled);

        /// <summary>
        /// Regex helper for scrubbing the password from a connection string.
        /// </summary>
        private static readonly Regex PASSWORD_SCRUB_REGEX = new Regex("([Pp]assword|[Pp]wd=).*;", RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Replace punctuation and symbols in variable names as these are not allowed.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        internal static readonly Func<string, string> ScrubName = str =>
        {
            // Remove 'vw_'
            if (str.StartsWith("vw_"))
                str = str.Replace("vw_", string.Empty);

            var len = str.Length;
            if (len == 0)
                return str;

            var sb = new StringBuilder();
            var replacedCharacter = false;
            for (var n = 0; n < len; ++n)
            {
                var c = str[n];
                if (c != '_' && (Char.IsSymbol(c) || Char.IsPunctuation(c)))
                {
                    int ascii = c;
                    sb.AppendFormat("{0}", ascii);
                    replacedCharacter = true;
                    continue;
                }
                sb.Append(c);
            }
            if (replacedCharacter)
                str = sb.ToString();

            // Remove non alphanumerics
            str = NAME_SCRUB_REGEX.Replace(str, string.Empty);
            if (Char.IsDigit(str[0]))
                str = "C" + str;

            return str;
        };

        /// <summary>
        /// List of reserved keywords from the C# language spec.
        /// </summary>
        internal static string[] ReservedKeywords =
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char",
            "checked", "class", "const", "continue", "decimal", "default", "delegate", "do",
            "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed",
            "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
            "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator",
            "out", "override", "params", "private", "protected", "public", "readonly", "ref",
            "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string",
            "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
            "unchecked", "unsafe", "ushort", "using", "virtual", "volatile", "void", "while"
        };

        /// <summary>
        /// Calculate the relationship value between tables/keys
        /// </summary>
        /// <param name="pkTable">The table containing the primary key.</param>
        /// <param name="fkTable">The table containing the foreign key.</param>
        /// <param name="fkCol">The column specified as the foreign key.</param>
        /// <param name="pkCol">The column specified as the primary key.</param>
        /// <returns></returns>
        internal static Relationship CalculateRelationship(Table pkTable, Table fkTable, Column fkCol, Column pkCol)
        {
            var fkTableSinglePrimaryKey = fkTable.PrimaryKeys.Count() == 1;
            var pkTableSinglePrimaryKey = pkTable.PrimaryKeys.Count() == 1;

            // 1:1
            if (fkCol.IsPrimaryKey && pkCol.IsPrimaryKey && fkTableSinglePrimaryKey && pkTableSinglePrimaryKey)
                return Relationship.OneToOne;

            // 1:n
            if (fkCol.IsPrimaryKey && !pkCol.IsPrimaryKey && fkTableSinglePrimaryKey)
                return Relationship.OneToMany;

            // n:1
            if (!fkCol.IsPrimaryKey && pkCol.IsPrimaryKey && pkTableSinglePrimaryKey)
                return Relationship.ManyToOne;

            // n:n
            return Relationship.ManyToMany;
        }

        /// <summary>
        /// Remove the password from visible connection strings.
        /// </summary>
        /// <param name="connectionString">The connection string to scrub.</param>
        /// <returns>A clean, password free connection string.</returns>
        internal static string ScrubPassword(string connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
                return PASSWORD_SCRUB_REGEX.Replace(connectionString, "password=*******;");
            return connectionString;
        }

        internal static Table GetTable(this ICollection<Table> self, string tableName, string schema)
        {
            return self.SingleOrDefault(x => string.Equals(x.SqlName, tableName, StringComparison.OrdinalIgnoreCase)
                                             && string.Equals(x.Schema, schema, StringComparison.OrdinalIgnoreCase));
        }

        internal static void SetPrimaryKeys(this ICollection<Table> self)
        {
            foreach (var tbl in self)
                tbl.SetPrimaryKeys();
        }

        internal static void IdentifyMappingTables(this ICollection<Table> self, List<ForeignKey> fkList, string collectionType)
        {
            foreach (var tbl in self.Where(x => x.HasForeignKey))
                tbl.IdentifyMappingTable(fkList, self, collectionType);
        }

        internal static void ProcessForeignKeys(ICollection<ForeignKey> fkList, ICollection<Table> tables, string collectionType)
        {
            var constraints = fkList.Select(x => x.ConstraintName).Distinct();
            foreach (var constraint in constraints)
            {
                var localConstraint = constraint;
                var foreignKeys = fkList.Where(x => x.ConstraintName == localConstraint).ToList();
                var foreignKey = foreignKeys.First();

                var fkTable = tables.GetTable(foreignKey.FkTableSqlName, foreignKey.FkSchema);
                if (fkTable == null || fkTable.IsMapping || !fkTable.HasForeignKey)
                    continue;

                var pkTable = tables.GetTable(foreignKey.PkTableSqlName, foreignKey.PkSchema);
                if (pkTable == null || pkTable.IsMapping)
                    continue;

                var pkTableHumanCase = foreignKey.PkTableClassName;
                var pkPropName = fkTable.GetUniqueColumnPropertyName(pkTableHumanCase);
                var fkPropName = pkTable.GetUniqueColumnPropertyName(fkTable.ClassName);

                var fkCols = foreignKeys.Select(x => fkTable.Columns.FirstOrDefault(n => n.PropertyName == x.FkColumnSqlName)).Where(x => x != null).ToList();
                var pkCols = foreignKeys.Select(x => pkTable.Columns.FirstOrDefault(n => n.PropertyName == x.PkColumnSqlName)).Where(x => x != null).ToList();

                var fkCol = fkCols.First();
                var pkCol = pkCols.First();

                if (!pkTable.IsEnum)
                {
                    fkCol.EntityFk = string.Format("public virtual {0} {1} {2} {3}", pkTableHumanCase, pkPropName, "{ get; set; } // ", foreignKey.ConstraintName);

                    var relationship = CalculateRelationship(pkTable, fkTable, fkCol, pkCol);

                    var manyToManyMapping = foreignKeys.Count > 1
                                                ? string.Format("c => new {{ {0} }}", string.Join(", ", fkCols.Select(x => "c." + x.PropertyNameHumanCase).ToArray()))
                                                : string.Format("c => c.{0}", fkCol.PropertyNameHumanCase);

                    fkCol.ForeignKeyConfiguration = string.Format("{0}; // {1}", GetRelationshipConfiguration(relationship, fkCol, pkCol, pkPropName, fkPropName, manyToManyMapping), foreignKey.ConstraintName);
                    pkTable.AddReverseNavigation(relationship, pkTableHumanCase, fkTable, fkPropName, string.Format("{0}.{1}", fkTable.SqlName, foreignKey.ConstraintName), collectionType);
                }
                else
                {
                    fkCol.EntityFk = string.Format("public virtual {0}{1} {2} {3} {4}", pkTable.EnumName, fkCol.IsNullable ? "?" : string.Empty, pkPropName, "{ get; set; } // ", foreignKey.ConstraintName);
                    fkCol.ForeignKeyConfiguration = fkCol.PropertyConfiguration;
                }
            }
        }

        internal static void IdentifyForeignKeys(ICollection<ForeignKey> fkList, ICollection<Table> tables)
        {
            foreach (var foreignKey in fkList)
            {
                foreignKey.FkTable = tables.GetTable(foreignKey.FkTableSqlName, foreignKey.FkSchema);
                if (foreignKey.FkTable == null)
                    continue; // Could be filtered out

                foreignKey.PkTable = tables.GetTable(foreignKey.PkTableSqlName, foreignKey.PkSchema);
                if (foreignKey.PkTable == null)
                    continue; // Could be filtered out

                foreignKey.FkColumn = foreignKey.FkTable.Columns.Find(n => n.PropertyName == foreignKey.FkColumnSqlName);
                if (foreignKey.FkColumn == null)
                    continue; // Could not find fk column
                foreignKey.FkColumn.IsForeignKey = true;
                if (foreignKey.PkTable.IsEnum)
                    foreignKey.FkColumn.IsForeignKeyEnum = foreignKey.PkTable.IsEnum;
                foreignKey.FkColumn.ForeignKey = foreignKey;

                foreignKey.PkColumn = foreignKey.PkTable.Columns.Find(n => n.PropertyName == foreignKey.PkColumnSqlName);
                //if(foreignKey.PkColumn == null)
                //    continue; // Could not find pk column
            }
        }

        // HasOptional
        // HasRequired
        // HasMany
        private static string GetHasMethod(Column fkCol, Column pkCol)
        {
            if (pkCol.IsPrimaryKey)
                return fkCol.IsNullable ? "Optional" : "Required";

            return "Many";
        }

        // WithOptional
        // WithRequired
        // WithMany
        // WithRequiredPrincipal
        // WithRequiredDependent
        private static string GetWithMethod(Relationship relationship, Column fkCol, string fkPropName, string manyToManyMapping)
        {
            switch (relationship)
            {
                case Relationship.OneToOne:
                    return string.Format(".WithOptional(b => b.{0})", fkPropName);

                case Relationship.OneToMany:
                    return string.Format(".WithRequiredDependent(b => b.{0})", fkPropName);

                case Relationship.ManyToOne:
                    return string.Format(".WithMany(b => b.{0}).HasForeignKey(c => c.{1})", SqlNameToClassNamePlural(fkPropName), fkCol.PropertyNameHumanCase);

                case Relationship.ManyToMany:
                    return string.Format(".WithMany(b => b.{0}).HasForeignKey({1})", SqlNameToClassNamePlural(fkPropName), manyToManyMapping);

                default:
                    throw new ArgumentOutOfRangeException("relationship");
            }
        }

        internal static string GetRelationshipConfiguration(Relationship relationship, Column fkCol, Column pkCol, string pkPropName, string fkPropName, string manyToManyMapping)
        {
            var hasMethod = GetHasMethod(fkCol, pkCol);
            var propName = hasMethod == "Many" ? SqlNameToClassNamePlural(pkPropName) : pkPropName;
            return string.Format("Has{0}(a => a.{1}){2}", hasMethod, propName, GetWithMethod(relationship, fkCol, fkPropName, manyToManyMapping));
        }

        /// <summary>
        /// Converts the SQL identifier to a C# code friendly PascalCase name.
        /// </summary>
        /// <param name="sqlName">The name to convert.</param>
        /// <returns>A C# code friendly PascalCase name.</returns>
        public static string SqlNameToClassName(string sqlName)
        {
            var scrubbedName = ScrubName(sqlName);
            var titlep = Inflector.ToTitleCase(scrubbedName);
            var lastp = titlep.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last();
            var lasts = Inflector.IsPlural(lastp) ? Inflector.MakeSingular(lastp) : lastp;
            var titles = titlep.Replace(lastp, lasts);
            return titles.Replace(" ", string.Empty);
        }

        internal static string SqlNameToClassNamePlural(string sqlName)
        {
            var scrubbedName = ScrubName(sqlName);
            var titlep = Inflector.ToTitleCase(scrubbedName);
            var lastp = titlep.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last();
            var lasts = Inflector.IsSingular(lastp) ? Inflector.MakePlural(lastp) : lastp;
            var titles = titlep.Replace(lastp, lasts);
            return titles.Replace(" ", string.Empty);
        }

        internal static string SqlNameToParameterName(string sqlName)
        {
            var nameNoAt = sqlName.Substring(1);
            return Inflector.MakeInitialLowerCase(nameNoAt);
        }

        internal static string SqlTypeToCodeType(string sqlType)
        {
            var sysType = "string";
            switch (sqlType)
            {
                case "bigint":
                    sysType = "long";
                    break;
                case "smallint":
                    sysType = "short";
                    break;
                case "int":
                    sysType = "int";
                    break;
                case "uniqueidentifier":
                    sysType = "Guid";
                    break;
                case "smalldatetime":
                case "datetime":
                case "datetime2":
                case "date":
                    sysType = "DateTime";
                    break;
                case "datetimeoffset":
                    sysType = "DateTimeOffset";
                    break;
                case "time":
                    sysType = "TimeSpan";
                    break;
                case "float":
                    sysType = "double";
                    break;
                case "real":
                    sysType = "float";
                    break;
                case "numeric":
                case "smallmoney":
                case "decimal":
                case "money":
                    sysType = "decimal";
                    break;
                case "tinyint":
                    sysType = "byte";
                    break;
                case "bit":
                    sysType = "bool";
                    break;
                case "image":
                case "binary":
                case "varbinary":
                case "varbinary(max)":
                case "timestamp":
                    sysType = "byte[]";
                    break;
                case "geography":
                    sysType = "System.Data.Spatial.DbGeography";
                    break;
                case "geometry":
                    sysType = "System.Data.Spatial.DbGeometry";
                    break;
            }
            return sysType;
        }

        internal static SqlDbType SqlTypeToDbType(string sqlType)
        {
            switch (sqlType)
            {
                default:
                    throw new ArgumentOutOfRangeException("sqlType: " + sqlType + " cannot be mapped to the SqlDbType enum.");
                case "bigint":
                    return SqlDbType.BigInt;
                case "smallint":
                    return SqlDbType.SmallInt;
                case "int":
                    return SqlDbType.Int;
                case "uniqueidentifier":
                    return SqlDbType.UniqueIdentifier;
                case "smalldatetime":
                    return SqlDbType.SmallDateTime;
                case "datetime":
                    return SqlDbType.DateTime;
                case "datetime2":
                    return SqlDbType.DateTime2;
                case "date":
                    return SqlDbType.Date;
                case "datetimeoffset":
                    return SqlDbType.DateTimeOffset;
                case "time":
                    return SqlDbType.Time;
                case "float":
                    return SqlDbType.Float;
                case "real":
                    return SqlDbType.Real;
                case "smallmoney":
                    return SqlDbType.SmallMoney;
                case "decimal":
                    return SqlDbType.Decimal;
                case "money":
                    return SqlDbType.Money;
                case "tinyint":
                    return SqlDbType.TinyInt;
                case "bit":
                    return SqlDbType.Bit;
                case "image":
                    return SqlDbType.Image;
                case "binary":
                    return SqlDbType.Binary;
                case "varbinary":
                    return SqlDbType.VarBinary;
                case "timestamp":
                    return SqlDbType.Timestamp;
                case "varchar":
                    return SqlDbType.VarChar;
                case "variant":
                    return SqlDbType.Variant;
                case "nvarchar":
                    return SqlDbType.NVarChar;
                case "nchar":
                    return SqlDbType.NChar;
                case "char":
                    return SqlDbType.Char;
            }
        }

        internal static string SqlTypeToDbTypeString(string sqlType)
        {
            switch (sqlType)
            {
                case "bigint":
                    return "SqlDbType.BigInt";
                case "smallint":
                    return "SqlDbType.SmallInt";
                case "int":
                    return "SqlDbType.Int";
                case "uniqueidentifier":
                    return "SqlDbType.UniqueIdentifier";
                case "smalldatetime":
                    return "SqlDbType.SmallDateTime";
                case "datetime":
                    return "SqlDbType.DateTime";
                case "datetime2":
                    return "SqlDbType.DateTime2";
                case "date":
                    return "SqlDbType.Date";
                case "datetimeoffset":
                    return "SqlDbType.DateTimeOffset";
                case "time":
                    return "SqlDbType.Time";
                case "float":
                    return "SqlDbType.Float";
                case "real":
                    return "SqlDbType.Real";
                case "smallmoney":
                    return "SqlDbType.SmallMoney";
                case "decimal":
                    return "SqlDbType.Decimal";
                case "money":
                    return "SqlDbType.Money";
                case "tinyint":
                    return "SqlDbType.TinyInt";
                case "bit":
                    return "SqlDbType.Bit";
                case "image":
                    return "SqlDbType.Image";
                case "binary":
                    return "SqlDbType.Binary";
                case "varbinary":
                    return "SqlDbType.VarBinary";
                case "timestamp":
                    return "SqlDbType.Timestamp";

                default:
                case "varchar":
                    return "SqlDbType.VarChar";
                case "variant":
                    return "SqlDbType.Variant";
                case "nvarchar":
                    return "SqlDbType.NVarChar";
                case "nchar":
                    return "SqlDbType.NChar";
                case "char":
                    return "SqlDbType.Char";
            }
        }
    }
}