// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  Parameter.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:08 PM

using System;
using System.Data;
using System.Data.Common;

namespace Supersonic
{
    /// <summary>
    /// This class represents a SQL parameter in a stored procedure.
    /// </summary>
    public class Parameter : IDatabaseMember
    {
        private Parameter()
        {
        }

        internal Parameter(DbDataReader rdr)
            : this()
        {
            this.SqlName = rdr["ParameterName"].ToString().Trim();
            this.SqlType = rdr["TypeName"].ToString().Trim();
            this.MaxLength = int.Parse(rdr["MaxLength"].ToString().Trim());
            this.IsOutput = (bool)rdr["IsOutput"];
            this.DefaultExpression = rdr["Default"] == DBNull.Value
                                         ? null
                                         : rdr["MethodName"].ToString().Trim();
            this.CodeName = Utilities.SqlNameToParameterName(this.SqlName);
            this.CodeType = Utilities.SqlTypeToCodeType(this.SqlType);
            this.Index = (int)rdr["ParameterId"];
        }

        /// <summary>
        /// The maximum length of the parameter's datatype in bytes.
        /// </summary>
        public int MaxLength { get; private set; }

        /// <summary>
        /// The default value of the parameter as a c# code expression.
        /// </summary>
        public string DefaultExpression { get; private set; }

        /// <summary>
        /// Returns true if this parameter has a default expression.
        /// </summary>
        public bool HasDefaultExpression
        {
            get { return !string.IsNullOrEmpty(this.DefaultExpression); }
        }

        /// <summary>
        /// Returns true if this parameter is an output parameter.
        /// </summary>
        public bool IsOutput { get; private set; }

        /// <summary>
        /// The ordinal index of the parameters in the stored procedure.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Returns a string representing the SqlDbType.
        /// </summary>
        public string SqlDbTypeString
        {
            get { return Utilities.SqlTypeToDbTypeString(this.SqlType); }
        }

        /// <summary>
        /// The SqlDbType of the parameter.
        /// </summary>
        public SqlDbType SqlDbType
        {
            get { return Utilities.SqlTypeToDbType(this.SqlType); }
        }

        /// <summary>
        /// The SQL name of the parameter.
        /// </summary>
        public string SqlName { get; private set; }

        /// <summary>
        /// The SQL type of the parameter
        /// </summary>
        public string SqlType { get; private set; }

        /// <summary>
        /// The name of the parameter in valid c# syntax.
        /// </summary>
        public string CodeName { get; private set; }

        /// <summary>
        /// The c# .net type of the parameter as a string.
        /// </summary>
        public string CodeType { get; private set; }
    }
}