using System;
using System.Runtime.CompilerServices;
using Godot;

[Tool]
public partial class TerrainGenerator : MeshInstance3D
{
    #region Exports && Variables
    private bool update = false;
    private bool isSubscribed;
    private DrawMode drawMode;

    [Export] public DrawMode Draw_Mode{get=> drawMode; set{
        drawMode = value;
        OnVariableChanged();
    }}

    [Export] bool AutoGenerate{get => isSubscribed; set{

        isSubscribed = value;

        if(value){
            NoiseParams.OnValidated += OnVariableChanged;
        }
        else{
            NoiseParams.OnValidated -= OnVariableChanged;
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
    [Export(PropertyHint.File, "*.png")] public string ImagePath{get; set;} = "res://NoiseImages/TerrainImage.png";

    private float meshHeightMultiplier;

    [Export(PropertyHint.Range, "0.001f, 1f, min, or_greater, hide_slider")] 
    public float MeshHeightMultiplier {get => meshHeightMultiplier; set{
        meshHeightMultiplier = value;
        if(AutoGenerate) OnVariableChanged();
    }} 

    [Export] Curve meshHeightCurve {get; set;} = GD.Load<Curve>("res://Resources/Terrain/HeightCurve.tres");
    
    [Export] public NoiseMapParams NoiseParams {get; set;} = GD.Load<NoiseMapParams>("res://Resources/Terrain/NoiseMap/NoiseMap.tres");

    [Export] public TerrainGroup terrainGroup{get; set;} = GD.Load<TerrainGroup>("res://Resources/Terrain/DefaultTerrainGroup.tres");

    #endregion
    
    Texture2D currentTexture;
	public override void _Ready()
	{
        currentTexture = GD.Load<Texture2D>(ImagePath);
	}

    private void LoadFromDataFile(){
        currentTexture = GD.Load<Texture2D>(ImagePath);
        NoiseParams.Width = currentTexture.GetWidth();
        NoiseParams.Height = currentTexture.GetHeight();
        SetImageTexture();
    }
    private void SaveToDataFile(){

        currentTexture.GetImage().SavePng(ImagePath);
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

        this.Mesh ??= new PlaneMesh();
        if(Mesh.IsClass("PlaneMesh")){
            (Mesh as PlaneMesh).Size = new Vector2(NoiseParams.Width, NoiseParams.Height);
        }

        var material = GetSurfaceOverrideMaterial(0);
        material.Set("albedo_texture", currentTexture);
        material.Set("texture_filter", "Nearest");
    }


    private void GenerateImageMap(){
        
        int width = NoiseParams.Width, height = NoiseParams.Height;
        float[,] noiseMap = Utility.NoiseGenerator.GenerateNoiseMap(NoiseParams);
        Color[] colorMap = new Color[width * height];

        for(int y = 0; y < height; y++){

            for(int x = 0; x < width; x++){
                
                float currentHeight = noiseMap[x,y];

                for(int i = 0; i < terrainGroup.regions.Length; i++){
                    
                    if(currentHeight <= terrainGroup.regions[i].height){
                        colorMap[y* width + x] = terrainGroup.regions[i].color;
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
        else if(drawMode == DrawMode.Mesh){

            currentTexture = ImageTexture.CreateFromImage(TextureGenerator.ImageFromColorMap(colorMap, width, height));
            Mesh = MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve).CreateMesh();
            //Mesh = MeshGenerator.GenerateSphereMesh();
        }
    }


	
    public enum DrawMode{
        NoiseMap, ColorMap, Mesh
    };
}

