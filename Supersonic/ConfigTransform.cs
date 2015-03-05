// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  ConfigTransform.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:06 PM

using System;
using System.Diagnostics;
using System.IO;

namespace Supersonic
{
    internal class TempFile : IDisposable
    {
        public TempFile()
        {
            this.Filename = Path.GetTempFileName();
        }

        public string Filename { get; private set; }

        public void Dispose()
        {
            if (File.Exists(this.Filename))
                File.Delete(this.Filename);
        }
    }

    /// <summary>
    /// Provides a mechanism to evaluate config file transforms pre-deploy.
    /// </summary>
    internal static class ConfigTransform
    {
        private const string TRANSFORM_PROJECT = @" <Project ToolsVersion=""4.0"" DefaultTargets=""Demo"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
                                                        <UsingTask TaskName=""TransformXml"" AssemblyFile=""$(MSBuildExtensionsPath)\Microsoft\VisualStudio\v10.0\Web\Microsoft.Web.Publishing.Tasks.dll""/>
                                                        <Target Name=""Transform"">
                                                            <TransformXml Source=""{source}"" Transform=""{transform}"" Destination=""{destination}""/>
                                                        </Target>
                                                    </Project>";

        public static TempFile ApplyTransformation(string configFile, string transformFile)
        {
            return ApplyTransformation(@"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe", configFile, transformFile);
        }

        public static TempFile ApplyTransformation(string msBuildPath, string configFile, string transformFile)
        {
            using (var proj = new TempFile())
            {
                var output = new TempFile();
                //Create transformation project
                var project = TRANSFORM_PROJECT.Replace("{source}", configFile)
                                               .Replace("{transform}", transformFile)
                                               .Replace("{destination}", output.Filename);
                File.WriteAllText(proj.Filename, project);

                //Run the project
                var cmd = new ProcessStartInfo(msBuildPath, proj.Filename + " /t:Transform")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                var proc = Process.Start(cmd);
                if (proc != null)
                    proc.WaitForExit();

                //Return output
                return output;
            }
        }
    }
}