using Godot;
using System;

[Tool]
[GlobalClass]
public partial class TerrainType : Resource
{
    [Export] public string name;
    [Export] public float height;
    [Export] public Color color;

    public TerrainType(){
        name = "DefaultTerrainType";
        height = 1f;
        color = Color.Color8(237, 70, 242, 255);
    }
}
