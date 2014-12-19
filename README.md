Entity Framework: Debug Visualizations
-----------------

A visualization of the entities and their relations as tracked by Db- and ObjectContext.
- Displays all entities tracked by the ObjectStateManager (Db- or ObjectContext) as 'entity cards'
- Each entitiy card shows 
    - The type name of the entity and its key (if one has been assigned)
    - The (color coded) state of the entity (added, modified, deleted or unchanged)
    - The properties with their current and original values (changed properties are highlighted)
    - The relations of the entity (current value only)
- Also displays the edges connecting the entity cards represent the relations
    - The edge color encodes the state of the relation (added, deleted or unchanged)
    - The tooltip of each edge contains the name of the navigation property and the state and multiplicity of the relation
- The shown entities can be filtered by
    - their state
    - their type 
- Thanks to GraphSharp you are free to rearrange, zoom, pane and choose a layout algorithm you like

![](https://raw.github.com/andypelzer/EntityFrameworkDebugVisualizations/master/Documentation/Screenshots/DebugVisualizer.png)

The project provides two extension methods to IObjectContextAdapter (DbContext / ObjectContext)

    IObjectContextAdapter context = [..];
    context.DumpTrackedEntities();

and

    context.ShowVisualizer();

The first extension method just produces a string with a textual representation of the entities tracked by the context, while the second visualizes the entities and their relations graphically as presented in the screenshot above.

The graphical version is also implemented as a DebuggerVisualizer. At the moment there is no easy, packaged way to install it. You'll either have to download the source, compile it and copy the assemblies manually or just copy the assemblies after NuGet has installed the package.

There also is a [NuGet package](https://www.nuget.org/packages/EntityFrameworkDebugVisualization/) to install the project assemblies.

This project uses my [fork](https://github.com/andypelzer/GraphSharp) of [GraphSharp](https://graphsharp.codeplex.com/) to display the entity graph and [Json.Net](http://james.newtonking.com/json) to serialize it. The UI is implemented using [MahApps.Metro](http://mahapps.com/MahApps.Metro/).
