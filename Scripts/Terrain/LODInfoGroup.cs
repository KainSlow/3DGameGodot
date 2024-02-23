using Godot;
using System;

[Tool]
[GlobalClass]
public partial class LODInfoGroup : Resource
{
    [Export] public LODInfo[] LODInfos;
}
