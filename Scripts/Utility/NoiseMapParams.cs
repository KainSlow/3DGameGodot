using Godot;
using System;
using System.Reflection.Metadata;

[GlobalClass, Tool, Serializable]
public partial class NoiseMapParams : Resource
{
    public EventHandler OnValidated;
    private int width;
    private int height;
    private int octaves;
    private float scale;
    private float persistance;
    private float lacunarity;
    private int seed;
    private Vector2 offset;

    [Export(PropertyHint.Range, "1, 1, 1, or_greater, or_greater, hide_slider")] 
    public int Width{get => width; set{
        width = value;
        OnValidate(this, EventArgs.Empty);
    }}
    
    [Export(PropertyHint.Range, "1, 1, 1, or_greater, or_greater, hide_slider")] 
    public int Height{get => height; set{
        height = value;
        OnValidate(this, EventArgs.Empty);
    }}

    [Export(PropertyHint.Range, "0,001f, 1f, 0.01f, or_greater, or_greater, hide_slider")]
    public float Scale{get => scale; set{
        scale = value;
        OnValidate(this, EventArgs.Empty);
    }}
    
    [Export(PropertyHint.Range, "1, 10, 1, min, max")] 
    public int Octaves{get => octaves; set{
        octaves = value;
        OnValidate(this, EventArgs.Empty);
    }}
    
    [Export(PropertyHint.Range, "0,001f, 1f")] 
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
}
