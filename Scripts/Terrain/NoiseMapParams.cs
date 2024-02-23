using Godot;
using System;

[Tool]
[GlobalClass]
public partial class NoiseMapParams : Resource
{
    public EventHandler OnValidated;
    private int octaves;
    private float scale;
    private float persistance;
    private float lacunarity;
    private int seed;
    private Vector2 offset;

    [Export(PropertyHint.Range, "0.001f, 1f, 0.01f, min, or_greater, hide_slider")]
    public float Scale{get => scale; set{
        scale = value;
        OnValidate(this, EventArgs.Empty);
    }}
    
    [Export(PropertyHint.Range, "1, 10, 1, min, max")] 
    public int Octaves{get => octaves; set{
        octaves = value;
        OnValidate(this, EventArgs.Empty);
    }}
    
    [Export(PropertyHint.Range, "0.001f, 1f, min, max")] 
    public float Persistance{get => persistance; set{
        persistance = value;
        OnValidate(this, EventArgs.Empty);
    }}
    
    [Export(PropertyHint.Range, "1f, 1f, 0.01, or_greater, or_greater, hide_slider")] 
    public float Lacunarity{get => lacunarity; set{
        lacunarity = value;
        OnValidate(this, EventArgs.Empty);
    }}
    
    [Export]
    public int Seed{get => seed; set{
        seed = value;
        OnValidate(this, EventArgs.Empty);
    }}
    
    [Export]
    public Vector2 Offset{get => offset; set{
        offset = value;
        OnValidate(this, EventArgs.Empty);
    }}

    public void OnValidate(object sender, EventArgs e){
        EventHandler handler = OnValidated;
        handler?.Invoke(sender, e);
    }

    public NoiseMapParams(){
        
        octaves = 1;
        scale = 1;
        persistance = .5f;
        lacunarity = 2f;
        seed = 0;
        offset = Vector2.Zero;

    }

}
