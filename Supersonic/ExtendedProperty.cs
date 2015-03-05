// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  ExtendedProperty.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:06 PM

using System.Data.SqlClient;

namespace Supersonic
{
    /// <summary>
    /// Extended Property identifier information
    /// </summary>
    public class ExtendedProperty
    {
        /// <summary>
        /// Default constructor builds an extended property instance from the database.
        /// </summary>
        /// <param name="rdr">The database reader to pull the values from.</param>
        public ExtendedProperty(SqlDataReader rdr)
        {
            this.Name = rdr["Name"].ToString();
            this.Value = rdr["Value"].ToString();
            this.ObjectName = rdr["ObjectName"].ToString();
            this.ObjectType = rdr["ObjectType"].ToString();
            this.ObjectSchema = rdr["ObjectSchema"].ToString();
        }

        /// <summary>
        /// Extended property name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Extended property value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Extended property target object SQL name
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// Extended property object type (table, view, procedure)
        /// </summary>
        public string ObjectType { get; set; }

        /// <summary>
        /// Extended property target object schema
        /// </summary>
        public string ObjectSchema { get; set; }
    }
}