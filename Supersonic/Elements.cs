// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  Elements.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:05 PM

using System;

namespace Supersonic
{
    /// <summary>
    /// Settings to allow selective code generation
    /// </summary>
    [Flags]
    public enum Elements
    {
        /// <summary>
        /// Empty
        /// </summary>
        None = 0,

        /// <summary>
        /// The POCO classes
        /// </summary>
        Poco = 1,

        /// <summary>
        /// The Entity Framework DbContext class
        /// </summary>
        Context = 2,

        /// <summary>
        /// The Entity Framework DbContext interface
        /// </summary>
        UnitOfWork = 4,

        /// <summary>
        /// The fluent API configuration for binding the POCO objects to the context.
        /// </summary>
        PocoConfiguration = 8
    };
}