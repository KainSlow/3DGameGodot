using Godot;
using System;

[GlobalClass]
[Tool]
public partial class TerrainType : Resource
{
    [Export] public string name;
    [Export] public float height;
    [Export] public Color color;
}
