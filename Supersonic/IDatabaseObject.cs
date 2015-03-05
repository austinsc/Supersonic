// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  IDatabaseObject.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:08 PM

using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Supersonic
{
    /// <summary>
    /// Defines a common interface for dealing with objects from SQL
    /// </summary>
    public interface IDatabaseObject
    {
        /// <summary>
        /// The SQL Schema name of the stored procedure.
        /// </summary>
        string Schema { get; }

        /// <summary>
        /// The original SQL identifier (name) of this object.
        /// </summary>
        string SqlName { get; }

        /// <summary>
        /// Returns the partially qualified ([schema].[objectname]) SQL identifier for this object.
        /// </summary>
        string QualifiedSqlName { get; }

        /// <summary>
        /// The generated C# name for this object.
        /// </summary>
        string CodeName { get; }

        /// <summary>
        /// True if this object is a table.
        /// </summary>
        bool IsTable { get; }

        /// <summary>
        /// True if this object is a view.
        /// </summary>
        bool IsView { get; }

        /// <summary>
        /// True if this object is configured as an enum.
        /// </summary>
        bool IsEnum { get; }

        /// <summary>
        /// True if this object is a stored procedure.
        /// </summary>
        bool IsStoredProcedure { get; }

        /// <summary>
        /// True if this object is omitted from code generation.
        /// </summary>
        bool IsOmitted { get; }

        /// <summary>
        /// True if this entity is marked with a rejection reason
        /// </summary>
        bool IsRejected { get; }

        /// <summary>
        /// If non-null, contains the reason why this object was rejected (not included) in the output
        /// </summary>
        string RejectionReason { get; }

        /// <summary>
        /// The child members of this object.
        /// </summary>
        [ScriptIgnore]
        IEnumerable<IDatabaseMember> Members { get; }
    }
}