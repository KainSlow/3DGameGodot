using Godot;
using System;

[GlobalClass, Tool]
public partial class TerrainGroup : Resource
{
    [Export] public TerrainType[] regions;

}
