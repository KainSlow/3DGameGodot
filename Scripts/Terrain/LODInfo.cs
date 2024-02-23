using Godot;

[Tool]
[GlobalClass]
public partial class LODInfo : Resource
{
    [Export] public int lod;
    [Export] public float visibleDstThreshold;
}
