﻿using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.Reflection;
using EntityFramework.Debug;
using Microsoft.VisualStudio.DebuggerVisualizers;

[assembly: AssemblyTitle("EFDebugExtensions")]
[assembly: AssemblyDescription("Useful debug extensions for Object- and DbContext")]
[assembly: AssemblyProduct("EFDebugExtensions")]
[assembly: AssemblyCopyright("Copyright © Andy Pelzer 2014")]

[assembly: AssemblyVersion("0.0.1.0")]
[assembly: AssemblyFileVersion("0.0.1.0")]

[assembly: DebuggerVisualizer(typeof(DbContextDebuggerVisualizer), typeof(VisualizerObjectSource), Target = typeof(DbContext), Description = "Context Visualizer")]
[assembly: DebuggerVisualizer(typeof(DbContextDebuggerVisualizer), typeof(VisualizerObjectSource), Target = typeof(ObjectContext), Description = "Context Visualizer")]
