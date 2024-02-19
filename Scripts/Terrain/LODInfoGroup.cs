using Godot;
using System;

[GlobalClass, Tool]
public partial class LODInfoGroup : Resource
{
    [Export] public LODInfo[] LODInfos;
}
