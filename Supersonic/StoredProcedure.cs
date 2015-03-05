// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  StoredProcedure.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:09 PM

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Web.Script.Serialization;

namespace Supersonic
{
    /// <summary>
    /// This class represents a stored procedure object in the database.
    /// </summary>
    public class StoredProcedure : IDatabaseObject
    {
        internal const string DEFAULT_CONTAINER_NAME = "StoredProcedures";

        private StoredProcedure()
        {
            this.Parameters = new List<Parameter>();
            this.ContainerName = string.Empty;
            this.IsOmitted = true;
        }

        internal StoredProcedure(DbDataReader rdr)
            : this()
        {
            this.Schema = rdr["Schema"].ToString().Trim();
            this.SqlName = rdr["ObjectName"].ToString().Trim();
            this.CodeName = rdr["MethodName"] == DBNull.Value
                                ? Utilities.SqlNameToClassName(this.SqlName)
                                : rdr["MethodName"].ToString().Trim();
            this.ContainerName = rdr["ContainerName"] == DBNull.Value
                                     ? DEFAULT_CONTAINER_NAME
                                     : rdr["ContainerName"].ToString().Trim();
            this.IsOmitted = !(bool)rdr["IsIncluded"];
            this.ExecutionMode = rdr["ExecutionMode"] == DBNull.Value
                                     ? ExecutionMode.VoidResult
                                     : (ExecutionMode)Enum.Parse(typeof(ExecutionMode), (rdr["ExecutionMode"].ToString().Trim()));
        }

        /// <summary>
        /// The parameters required by this stored procedure.
        /// </summary>
        public List<Parameter> Parameters { get; private set; }

        /// <summary>
        /// The c# class name that contains the stored procedure.
        /// </summary>
        public string ContainerName { get; private set; }

        /// <summary>
        /// The type of function to generate (based on the hint stored in the SQL extended prooperty for the stored procedure).
        /// </summary>
        public ExecutionMode ExecutionMode { get; private set; }

        /// <summary>
        /// C# code return type.
        /// </summary>
        [ScriptIgnore]
        public string CodeReturnType
        {
            get
            {
                switch (this.ExecutionMode)
                {
                    default:
                    case ExecutionMode.VoidResult:
                        return "int";
                    case ExecutionMode.ScalarResult:
                        return "dynamic";
                    case ExecutionMode.TableResult:
                        return "IEnumerable<dynamic>";
                }
            }
        }

        /// <summary>
        /// The SQL name of the stored procedure.
        /// </summary>
        public string SqlName { get; private set; }

        /// <summary>
        /// The SQL Schema name of the stored procedure.
        /// </summary>
        public string Schema { get; private set; }

        /// <summary>
        /// Returns the partially qualified ([schema].[objectname]) SQL identifier for this object.
        /// </summary>
        public string QualifiedSqlName
        {
            get { return string.Format("[{0}].[{1}]", this.Schema, this.SqlName); }
        }

        /// <summary>
        /// The c# name of the stored procedure.
        /// </summary>
        public string CodeName { get; private set; }

        /// <summary>
        /// True if this object is a table.
        /// </summary>
        public bool IsTable
        {
            get { return false; }
        }

        /// <summary>
        /// True if this object is a view.
        /// </summary>
        public bool IsView
        {
            get { return false; }
        }

        /// <summary>
        /// True if this object is configured as an enum.
        /// </summary>
        public bool IsEnum
        {
            get { return false; }
        }

        /// <summary>
        /// True if this object is a stored procedure.
        /// </summary>
        public bool IsStoredProcedure
        {
            get { return true; }
        }

        /// <summary>
        /// The child members of this object.
        /// </summary>
        [ScriptIgnore]
        public IEnumerable<IDatabaseMember> Members
        {
            get { return this.Parameters; }
        }

        /// <summary>
        /// True if the stored procedure is ommitted from the generator.
        /// </summary>
        public bool IsOmitted { get; internal set; }

        /// <summary>
        /// If non-null, contains the reason why this object was rejected (not included) in the output
        /// </summary>
        public string RejectionReason { get; internal set; }

        /// <summary>
        /// True if this entity is marked with a rejection reason
        /// </summary>
        public bool IsRejected
        {
            get { return !string.IsNullOrEmpty(this.RejectionReason); }
        }

        /// <summary>
        /// Creates a string that represents a c# method signature for the SQL parameters.
        /// </summary>
        /// <returns>A string representing the c# method signature.</returns>
        public string GetParameterMethodSignature()
        {
            var results = string.Empty;
            foreach (var parameter in this.Parameters)
            {
                results += parameter.IsOutput ? "ref " : string.Empty;
                results += parameter.CodeType + " ";
                results += parameter.CodeName + ", ";
            }
            return results.TrimEnd(", ".ToCharArray());
        }

        /// <summary>
        /// Builds the return value assignment expression based on the stored procedures' execution mode.
        /// </summary>
        /// <returns>The correct return value assignment expression.</returns>
        public string GetReturnValueCode()
        {
            switch (this.ExecutionMode)
            {
                default:
                case ExecutionMode.VoidResult:
                    return @"var __returnValue = command.ExecuteNonQuery();";

                case ExecutionMode.ScalarResult:
                    return @"var __returnValue = command.ExecuteScalar();";

                case ExecutionMode.TableResult:
                    return @"var __returnValue = new List<dynamic>();
                    var reader = command.ExecuteReader();
                    while(reader.Read())
                    {
                        dynamic result = new ExpandoObject();
                        for(int i = 0; i < reader.FieldCount; i++)
                            ((IDictionary<String, object>)result)[reader.GetName(i)] = /*reader[i] == DbNull.Value ? null : */reader[i];
                        __returnValue.Add(result);
                    }";
            }
        }
    }
}