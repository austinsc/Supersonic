// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  ExecutionMode.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:06 PM

namespace Supersonic
{
    /// <summary>
    /// Specifies the options for stored procedure code generation
    /// </summary>
    public enum ExecutionMode
    {
        /// <summary>
        /// Indicates that the stored procedure does not return any results.
        /// </summary>
        VoidResult,

        /// <summary>
        /// Indicates that the stored procedure returns a single scalar result.
        /// </summary>
        ScalarResult,

        /// <summary>
        /// Indicates that the stored procedure returns many complex results.
        /// </summary>
        TableResult,
    };
}