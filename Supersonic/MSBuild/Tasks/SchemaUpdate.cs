// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  SchemaUpdate.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:20 PM

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Supersonic.MSBuild.Tasks
{
    /// <summary>
    /// Reruns the EF context generator
    /// </summary>
    public class SchemaUpdate : Task
    {
        private Encoding _encoding = System.Text.Encoding.UTF8;

        /// Maintain the behaviour of the original implementation for compatibility
        /// (i.e. initialize _useDefaultEncoding with false) and use utf-8-without-bom,  
        /// which is Microsoft's default encoding, only when Encoding property is set 
        /// to "utf-8-without-bom".
        private bool _useDefaultEncoding;

        /// <summary>
        /// Gets or sets the files to update.
        /// </summary>
        /// <value>The files.</value>
        [Required]
        public ITaskItem[] Files { get; set; }

        /// <summary>
        /// </summary>
        [Required]
        public string Solution { get; set; }

        /// <summary>
        /// </summary>
        [Required]
        public string Configuration { get; set; }

        /// <summary>
        /// The character encoding used to read and write the file.
        /// </summary>
        /// <remarks>
        /// Any value returned by <see cref="System.Text.Encoding.WebName" /> is valid input.
        /// <para>The default is <c>utf-8</c></para>
        /// <para>Additionally, <c>utf-8-without-bom</c>can be used.</para>
        /// </remarks>
        public string Encoding
        {
            get
            {
                if (this._useDefaultEncoding) return "utf-8-without-bom";
                return this._encoding.WebName;
            }
            set
            {
                if (value.ToLower().CompareTo("utf-8-without-bom") == 0) this._useDefaultEncoding = true;
                else this._encoding = System.Text.Encoding.GetEncoding(value);
            }
        }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            try
            {
                foreach (var item in this.Files)
                {
                    var fileName = item.ItemSpec;
                    this.Log.LogMessage("Updating \"{0}\".", fileName);

                    var buffer = this._useDefaultEncoding
                                     ? File.ReadAllText(fileName)
                                     : File.ReadAllText(fileName, this._encoding);

                    var contents = string.Empty;

                    foreach (var line in buffer.Split(Environment.NewLine.ToCharArray()))
                        if (line.StartsWith("<#="))
                        {
                            var r = new Regex("Generator.GenerateCode((.*))");
                            var m = r.Match(line);
                            if (m.Success)
                            {
                                var arguments = m.Groups[0].Value.Split(',').Select(CleanupArgument).ToArray();
                                //var j = 0;
                                //foreach (var argument in arguments)
                                //    this.Log.LogMessage("Argument " + ++j + " = " + argument);

                                var elements = Elements.None;

                                if (arguments[1].Contains("Elements.PocoConfiguration"))
                                    elements |= Elements.PocoConfiguration;
                                else if (arguments[1].Contains("Elements.Poco"))
                                    elements |= Elements.Poco;
                                if (arguments[1].Contains("Elements.Context"))
                                    elements |= Elements.Context;
                                if (arguments[1].Contains("Elements.UnitOfWork"))
                                    elements |= Elements.UnitOfWork;

                                //this.Log.LogMessage(elements.ToString());
                                //this.Log.LogMessage(arguments[1]);
                                //this.Log.LogMessage(this.Solution);
                                //this.Log.LogMessage(this.Configuration);
                                contents = Generator.GenerateCode(arguments[0], elements, arguments[2], this.Solution, this.Configuration, arguments.Length > 4 ? arguments[4] : null);
                            }
                        }
                    var outputname = fileName + ".cs";
                    var output = new FileInfo(outputname);
                    output.IsReadOnly = false;
                    output.Refresh();
                    if (this._useDefaultEncoding)
                        File.WriteAllText(outputname, contents);
                    else
                        File.WriteAllText(outputname, contents, this._encoding);
                }
            }
            catch (Exception ex)
            {
                this.Log.LogErrorFromException(ex);
                return false;
            }
            return true;
        }

        private static string CleanupArgument(string arg)
        {
            return arg.Replace("Generator.GenerateCode(", string.Empty).Replace("\"", string.Empty).Replace(") #>", string.Empty).Replace(" | ", ",").Trim();
        }
    }
}