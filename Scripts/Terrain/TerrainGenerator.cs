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
    private float meshHeightMultiplier = 60;
    private Texture2D currentTexture;
    private int levelOfDetail;
    [Export] public const int mapChunkSize = 257;
    private float[,] noiseMap;
    private Color[] colorMap;

    [Export] public DrawMode Draw_Mode{get=> drawMode; set{
        drawMode = value;
        OnVariableChanged();
    }}

    [Export(PropertyHint.Range, "0, 8, 1, min, max")] 
    public int LevelOfDetail{get => levelOfDetail; set{
        levelOfDetail = value;
        if(AutoGenerate) OnVariableChanged();
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


    [Export(PropertyHint.Range, "0.001f, 1f, min, or_greater, hide_slider")] 
    public float MeshHeightMultiplier {get => meshHeightMultiplier; set{
        meshHeightMultiplier = value;
        if(AutoGenerate) OnVariableChanged();
    }} 
    [Export] Curve meshHeightCurve {get; set;} = GD.Load<Curve>("res://Resources/Terrain/HeightCurve.tres");
    
    [Export] public NoiseMapParams NoiseParams {get; set;} = GD.Load<NoiseMapParams>("res://Resources/Terrain/NoiseMap/NoiseMap.tres");

    [Export] public TerrainGroup terrainGroup{get; set;} = GD.Load<TerrainGroup>("res://Resources/Terrain/DefaultTerrainGroup.tres");

    #endregion
	public override void _Ready()
	{
        OnVariableChanged();
	}

    private void LoadFromDataFile(){
        currentTexture = GD.Load<Texture2D>(ImagePath);
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

        if(Mesh == null){ GD.Print("No Mesh Found"); return; }

        var material = GetSurfaceOverrideMaterial(0);
        material.Set("albedo_texture", currentTexture);
        //material.Set("texture_filter", "Nearest");
    }


    private void GenerateNoiseMap(){

        noiseMap = Utility.NoiseGenerator.GenerateNoiseMap(NoiseParams, mapChunkSize);

        PlaneMesh planeMesh = new PlaneMesh(){
            Size = new(mapChunkSize, mapChunkSize)
        };
        Mesh = planeMesh;

        currentTexture = ImageTexture.CreateFromImage(TextureGenerator.ImageFromHeightMap(noiseMap));
    }

    private void GenerateColorMap(){

        GenerateNoiseMap();

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        for(int y = 0; y < mapChunkSize; y++){

            for(int x = 0; x < mapChunkSize; x++){
                
                float currentHeight = noiseMap[x,y];

                for(int i = 0; i < terrainGroup.regions.Length; i++){
                    
                    if(currentHeight <= terrainGroup.regions[i].height){
                        colorMap[y * mapChunkSize + x] = terrainGroup.regions[i].color;
                        break;
                    }

                }
            }
        }

        currentTexture = ImageTexture.CreateFromImage(TextureGenerator.ImageFromColorMap(colorMap, mapChunkSize));
    }
    private void GenerateMesh(){

        Mesh = MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail).CreateMesh();
    }

    private void GenerateImageMap(){
        
        if(drawMode == DrawMode.NoiseMap){
            GenerateNoiseMap();
        }
        else if(drawMode == DrawMode.ColorMap){
            GenerateColorMap();
        }
        else if(drawMode == DrawMode.Mesh){
            GenerateMesh();
        }
    }

    public enum DrawMode{
        NoiseMap, ColorMap, Mesh
    };
}

