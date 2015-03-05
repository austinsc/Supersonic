// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  Extensions.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:14 PM

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Supersonic.Templates
{
    /// <summary>
    /// Extension methods for generating code elements from their respective object instances via text templating.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Default Stored Procedure Container Suffix
        /// </summary>
        public const string DEFAULT_CONTAINER_SUFFIX = "Container";

        /// <summary>
        /// Default Configuration Class Suffix
        /// </summary>
        public const string DEFAULT_CONFIGURATION_SUFFIX = "Configuration";

        /// <summary>
        /// Creates a POCO definition.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string ToPocoString(this Table self)
        {
            Contract.Requires<ArgumentNullException>(self != null, "self");
            Contract.Requires<ArgumentException>(!self.IsEnum, "self");
            Contract.Ensures(!String.IsNullOrWhiteSpace(Contract.Result<string>()));

            return new PocoTemplate(self).TransformText().Trim();
        }

        /// <summary>
        /// Creates an Enum definition.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string ToEnumString(this Table self)
        {
            Contract.Requires<ArgumentNullException>(self != null, "self");
            Contract.Requires<ArgumentException>(self.IsEnum, "self");
            Contract.Ensures(!String.IsNullOrWhiteSpace(Contract.Result<string>()));

            return new EnumTemplate(self).TransformText().Trim();
        }

        /// <summary>
        /// Creates a POCO configuration class declaration.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="configurationSuffix"></param>
        /// <returns></returns>
        public static string ToConfigurationString(this Table self, string configurationSuffix = DEFAULT_CONFIGURATION_SUFFIX)
        {
            Contract.Requires<ArgumentNullException>(self != null, "self");
            Contract.Requires<ArgumentException>(!self.IsEnum, "self");
            Contract.Ensures(!String.IsNullOrWhiteSpace(Contract.Result<string>()));

            return new ConfigurationTemplate(self, configurationSuffix).TransformText().Trim();
        }

        /// <summary>
        /// Creates a POCO configuration class declaration.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="databaseName"></param>
        /// <param name="commandText"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static string ToTestDataString(this Table self, string databaseName, string commandText, string connectionString)
        {
            Contract.Requires<ArgumentNullException>(self != null, "self");
            Contract.Requires<ArgumentException>(!self.IsEnum, "self");
            Contract.Ensures(!String.IsNullOrWhiteSpace(Contract.Result<string>()));

            return new TestDataTemplate(self, databaseName, commandText, connectionString).TransformText().Trim();
        }

        /// <summary>
        /// Creates a unit of work interface definition containing the given tables.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="interfaceName"></param>
        /// <returns></returns>
        public static string ToUnitOfWorkTemplateString(this IEnumerable<Table> self, string interfaceName)
        {
            Contract.Requires<ArgumentNullException>(self != null, "self");
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(interfaceName), "interfaceName");
            Contract.Ensures(!String.IsNullOrWhiteSpace(Contract.Result<string>()));

            return new UnitOfWorkTemplate(self, interfaceName).TransformText().Trim();
        }

        /// <summary>
        /// Creates an entity framework code first class declaration for the given tables.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="interfaceName"></param>
        /// <param name="className"></param>
        /// <param name="connectionStringName"></param>
        /// <param name="storedProcedureContainers"></param>
        /// <param name="configurationSuffix"></param>
        /// <returns></returns>
        public static string ToContextString(this IEnumerable<Table> self, string interfaceName, string className, string connectionStringName,
                                             IEnumerable<IGrouping<string, StoredProcedure>> storedProcedureContainers,
                                             string configurationSuffix = DEFAULT_CONFIGURATION_SUFFIX)
        {
            Contract.Requires<ArgumentNullException>(self != null, "self");
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(interfaceName), "interfaceName");
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(className), "className");
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(connectionStringName), "connectionStringName");
            Contract.Requires<ArgumentNullException>(storedProcedureContainers != null, "storedProcedureContainers");
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(configurationSuffix), "configurationSuffix");
            Contract.Ensures(!String.IsNullOrWhiteSpace(Contract.Result<string>()));

            return new ContextTemplate(self, interfaceName, className, connectionStringName, storedProcedureContainers, configurationSuffix).TransformText().Trim();
        }

        /// <summary>
        /// Creates a stored procedure binding method declaration.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string ToMethodString(this StoredProcedure self)
        {
            Contract.Requires<ArgumentNullException>(self != null, "self");
            Contract.Ensures(!String.IsNullOrWhiteSpace(Contract.Result<string>()));

            return new StoredProcedureTemplate(self).TransformText().Trim();
        }
    }
}