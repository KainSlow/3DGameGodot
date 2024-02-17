using System;
using Godot;

[Tool]
public partial class TerrainGenerator : MeshInstance3D
{
    #region Exports
    private bool update = false;

    private bool isSubscribed;
    [Export] public DrawMode drawMode;

    [Export] bool AutoGenerate{get => isSubscribed; set{

        isSubscribed = value;

        if(value){
            noiseParams.OnValidated += OnVariableChanged;
        }
        else{
            noiseParams.OnValidated -= OnVariableChanged;
        }
    }}
    [Export] bool Generate {get => update; set{
        update = false;
        OnVariableChanged();
    }}
    [Export] bool SaveToPath{get => update; set{
        update = false;
        SaveToDataFile();
    }}

    [Export] bool LoadFromPath{get=> update; set{
        update = false;
        LoadFromDataFile();
    }}

    private string imagePath;

    [Export(PropertyHint.File, "*.jpg")] string ImagePath{get=> imagePath; set{
        imagePath = value;
    }}
    
    private NoiseMapParams noiseParams;
    [Export] public NoiseMapParams NoiseParams {get => noiseParams; set{

        noiseParams = value;
    }}

    [Export] public TerrainType[] regions;

    #endregion
    
    Texture2D currentTexture;
	public override void _Ready()
	{
        currentTexture = GD.Load<Texture2D>(imagePath);
	}

    private void LoadFromDataFile(){
        currentTexture = GD.Load<Texture2D>(imagePath);
        noiseParams.Width = currentTexture.GetWidth();
        noiseParams.Height = currentTexture.GetHeight();
        SetImageTexture();
    }
    public override void _Process(double delta){

    }

    public void OnVariableChanged(){

        GenerateImageMap();
        SetImageTexture();
    }

    public void OnVariableChanged(object sender, EventArgs e){
        GenerateImageMap();
        SetImageTexture();
    }

    private void SetImageTexture(){

        var mesh = Mesh as PlaneMesh;
        mesh.Size = new(noiseParams.Width, noiseParams.Height);
        var material = GetSurfaceOverrideMaterial(0);
        material.Set("albedo_texture", currentTexture);
    }


    private void GenerateImageMap(){
        
        int width = noiseParams.Width, height = noiseParams.Height;
        float[,] noiseMap = Utility.NoiseGenerator.GenerateNoiseMap(noiseParams);
        Color[] colorMap = new Color[width * height];

        Image image = Image.Create(width, height, false, Image.Format.Rgba8);


        for(int y = 0; y < height; y++){

            for(int x = 0; x < width; x++){
                
                float currentHeight = noiseMap[x,y];

                for(int i = 0; i < regions.Length; i++){
                    
                    if(currentHeight <= regions[i].height){
                        colorMap[y* width + x] = regions[i].color;
                        break;
                    }

                }
            }
        }
        
        if(drawMode == DrawMode.NoiseMap){
            currentTexture = ImageTexture.CreateFromImage(TextureGenerator.ImageFromHeightMap(noiseMap));
        }
        else if(drawMode == DrawMode.ColorMap){
            currentTexture = ImageTexture.CreateFromImage(TextureGenerator.ImageFromColorMap(colorMap, width, height));
        }

    }

    private void SaveToDataFile(){

        currentTexture.GetImage().SavePng(imagePath);
    }
	
    public enum DrawMode{
        NoiseMap, ColorMap
    };
}

