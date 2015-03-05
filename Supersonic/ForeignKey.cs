// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  ForeignKey.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:08 PM

using System.Data.Common;

namespace Supersonic
{
    internal class ForeignKey
    {
        internal ForeignKey(DbDataReader rdr)
        {
            this.ConstraintName = rdr["Constraint_Name"].ToString();
            this.PkColumnSqlName = rdr["PK_Column"].ToString();
            this.FkColumnSqlName = rdr["FK_Column"].ToString();
            this.PkSchema = rdr["pkSchema"].ToString();
            this.PkTableSqlName = rdr["PK_Table"].ToString();
            this.FkSchema = rdr["fkSchema"].ToString();
            this.FkTableSqlName = rdr["FK_Table"].ToString();
        }

        internal string FkTableSqlName { get; private set; }

        internal string FkTableClassName
        {
            get { return Utilities.SqlNameToClassName(this.FkTableSqlName); }
        }

        internal string FkTableClassNamePlural
        {
            get { return Utilities.SqlNameToClassNamePlural(this.FkTableSqlName); }
        }

        internal string FkSchema { get; private set; }
        internal string PkTableSqlName { get; private set; }

        internal string PkTableClassName
        {
            get { return Utilities.SqlNameToClassName(this.PkTableSqlName); }
        }

        internal string PkTableClassNamePlural
        {
            get { return Utilities.SqlNameToClassNamePlural(this.PkTableSqlName); }
        }

        internal string PkSchema { get; private set; }
        internal string FkColumnSqlName { get; private set; }
        internal string PkColumnSqlName { get; private set; }
        internal string ConstraintName { get; private set; }
        internal Table PkTable { get; set; }
        internal Table FkTable { get; set; }
        internal Column PkColumn { get; set; }
        internal Column FkColumn { get; set; }
    }
}