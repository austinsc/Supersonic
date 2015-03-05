// Copyright (c) 2009-2015 Pwnt & Co. All Right Reserved.
// 
// Filename:  GeneratorOutput.cs
// Author:    Stephen C. Austin (stephen.austin)
// Modified:  03/04/2015 11:08 PM

namespace Supersonic
{
    /// <summary>
    /// Delegate describing the interface for attaching a listner to the output of the generator.
    /// </summary>
    /// <param name="output">The data being produced by the generator.</param>
    public delegate void GeneratorOutput(string output);
}