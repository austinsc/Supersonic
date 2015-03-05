// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  GeneratorOptions.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:08 PM

using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using EnvDTE;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace Supersonic
{
    /// <summary>
    /// This class holds all of the data and options for creating an instance of the Generator class.
    /// </summary>
    public class GeneratorOptions
    {
        // Connection Information
        private GeneratorOptions(string @namespace)
        {
            this.IncludeViews = true;
            this.ConfigurationClassSuffix = "Configuration";
            this.CollectionType = "List";
            this.CollectionTypeNamespace = "";
            this.ElementsToGenerate = Elements.Poco | Elements.Context | Elements.UnitOfWork | Elements.PocoConfiguration;
            this.SchemaName = "dbo";
            this.Namespace = @namespace;
        }

        /// <summary>
        /// Default constructor for building a new Generator from the given database name and connection string.
        /// </summary>
        /// <param name="namespace">The namespace for the generated code</param>
        /// <param name="connectionString">The standard ADO.NET database connection string.</param>
        public GeneratorOptions(string @namespace, string connectionString)
            : this(@namespace)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            this.DatabaseName = builder.InitialCatalog;
            this.ConnectionStringName = builder.InitialCatalog;
            this.ConnectionString = connectionString;
        }

        /// <summary>
        /// Constructor overload for building the connection from project configuration files and transforms.
        /// </summary>
        /// <param name="namespace">The namespace for the generated code</param>
        /// <param name="connectionStringName">The name of the connection string.</param>
        /// <param name="serviceProvider">An instance of the Visual Studio Host from the text template.</param>
        public GeneratorOptions(string @namespace, string connectionStringName, IServiceProvider serviceProvider)
            : this(@namespace)
        {
            this.ConnectionStringName = connectionStringName;
            this.Host = serviceProvider;
            var dte = (DTE)this.Host.GetService(typeof(DTE));
            var currentConfigurationName = dte.Solution.SolutionBuild.ActiveConfiguration.Name;
            var transformName = currentConfigurationName + ".config";
            var root = Path.GetDirectoryName(dte.Solution.FullName);
            this.FindConnectionString(root, transformName);
        }

        /// <summary>
        /// Constructor overload for building the connection from project configuration files and transforms.
        /// </summary>
        /// <param name="namespace">The namespace for the generated code</param>
        /// <param name="connectionStringName">The name of the connection string.</param>
        /// <param name="root">The solution directory root</param>
        /// <param name="transformName">The config transform name suffix.</param>
        public GeneratorOptions(string @namespace, string connectionStringName, string root, string transformName)
            : this(@namespace)
        {
            this.ConnectionStringName = connectionStringName;
            this.FindConnectionString(root, transformName);
        }

        /// <summary>
        /// The namespace for the generated code
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// The name of the connection string.
        /// </summary>
        public string ConnectionStringName { get; private set; }

        /// <summary>
        /// The name of the database from the connection string.
        /// </summary>
        public string DatabaseName { get; private set; }

        /// <summary>
        /// The connection string used by the generator.
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// The Visual Studio instance containing the text templates being transformed.
        /// </summary>
        public IServiceProvider Host { get; private set; }

        // Code Generation Options
        /// <summary>
        /// The code elements to generate.
        /// </summary>
        public Elements ElementsToGenerate { get; set; }

        /// <summary>
        /// The type of collection to use to contain the entities on the many side of a navigational property.
        /// </summary>
        public string CollectionType { get; set; }

        /// <summary>
        /// The namespace of the collection type.
        /// </summary>
        public string CollectionTypeNamespace { get; set; }

        /// <summary>
        /// True (default) to include views in the generation strategy.
        /// </summary>
        public bool IncludeViews { get; set; }

        /// <summary>
        /// The SQL schema name to target.
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        /// Gets the name of the context class.
        /// </summary>
        public string ContextClassName
        {
            get { return this.ConnectionStringName + "Context"; }
        }

        /// <summary>
        /// Gets the name of the interface that defines the context (unit of work).
        /// </summary>
        public string ContextInterfaceName
        {
            get { return "I" + this.ContextClassName; }
        }

        /// <summary>
        /// The name of the configuration class
        /// </summary>
        public string ConfigurationClassSuffix { get; set; }

        /// <summary>
        /// Gets/sets the namespace of the context class.
        /// </summary>
        public string ContextNamespace { get; set; }

        /// <summary>
        /// The POCO namespace
        /// </summary>
        public string PocoNamespace { get; set; }

        /// <summary>
        /// The unit of work namespace.
        /// </summary>
        public string UnitOfWorkNamespace { get; set; }

        /// <summary>
        /// The configuration namespace.
        /// </summary>
        public string ConfigurationNamespace { get; set; }

        private void FindConnectionString(string root, string transformName)
        {
            if (root == null)
                return;
            var directories = Directory.EnumerateDirectories(root);

            foreach (var files in directories.Select(directory => Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).OrderBy(x => x.Length).ToList()))
                if (files.Any(x => x.EndsWith(transformName, StringComparison.InvariantCultureIgnoreCase)))
                    try
                    {
                        using (var transformed = ConfigTransform.ApplyTransformation(files.First(x => x.EndsWith(".config", StringComparison.InvariantCultureIgnoreCase) && !x.EndsWith("es.config", StringComparison.InvariantCultureIgnoreCase)),
                                                                                    files.First(x => x.EndsWith(transformName, StringComparison.InvariantCultureIgnoreCase))))
                            if (this.TrySetConnectionString(transformed.Filename))
                                return;
                    }
                    catch
                    {
                    }
                else if (files.Where(x => x.EndsWith(".config", StringComparison.InvariantCultureIgnoreCase) && !x.EndsWith("es.config", StringComparison.InvariantCultureIgnoreCase)).Any(this.TrySetConnectionString))
                    return;
        }

        private bool TrySetConnectionString(string path)
        {
            var configFile = new ExeConfigurationFileMap { ExeConfigFilename = path };
            var config = ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None);
            var connSection = config.ConnectionStrings;

            // Get the named connection string
            try
            {
                this.ConnectionString = connSection.ConnectionStrings[this.ConnectionStringName].ConnectionString;
                var builder = new SqlConnectionStringBuilder(this.ConnectionString);
                this.DatabaseName = builder.InitialCatalog;
                return true;
            }
            catch
            {
                this.ConnectionString = null;
            }
            return false;
        }
    }
}