using Godot;

[Tool]
[GlobalClass]
public partial class TerrainGroup : Resource
{
    [Export] public TerrainType[] regions;

    public TerrainGroup(){
        regions ??= new TerrainType[1]{new(){}};
    }
}
