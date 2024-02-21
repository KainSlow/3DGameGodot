using Godot;

[GlobalClass]
[Tool]
public partial class LODInfo : Resource
{
    [Export] public int lod;
    [Export] public float visibleDstThreshold;
}
