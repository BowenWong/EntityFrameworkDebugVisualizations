EntityFrameworkDebugVisualizations
=================

A visualization of the entities and their relations as tracked by Db- and ObjectContext.

![](https://raw.github.com/andypelzer/EntityFrameworkDebugVisualizations/master/Documentation/Screenshots/DebugVisualizer.png)

The project provides two extension methods to IObjectContextAdapter (DbContext / ObjectContext)

    IObjectContextAdapter context = [..];
    context.DumpTrackedEntities();

and

    context.ShowVisualizer();

The first extension method just produces a string with a textual representation of the entities tracked by the context, while the second visualizes the entities and their relations graphically (as presented in the screenshot above).

The graphical version is implemented as a DebuggerVisualizer. At the moment there is no easy (packaged) way to install it. You'll either have to download the source, compile it and copy the assemblies manually or just copy the assemblies after NuGet has installed the package.

There also is a [NuGet package](https://www.nuget.org/packages/EntityFrameworkDebugVisualization/) to install the project assemblies.

This project uses [GraphSharp](https://graphsharp.codeplex.com/) to display the entity graph and [Json.Net](http://james.newtonking.com/json) to serialize it.
