// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  IDatabaseMember.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:08 PM

namespace Supersonic
{
    /// <summary>
    /// Defines a common interface for dealing with child objects from SQL.
    /// </summary>
    public interface IDatabaseMember
    {
        /// <summary>
        /// The generated C# object name (identifier)
        /// </summary>
        string CodeName { get; }

        /// <summary>
        /// The C# Type of the member
        /// </summary>
        string CodeType { get; }

        /// <summary>
        /// The original SQL name of the member.
        /// </summary>
        string SqlName { get; }

        /// <summary>
        /// The SQL datatype of the member.
        /// </summary>
        string SqlType { get; }
    }
}