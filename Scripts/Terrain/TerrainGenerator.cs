using Godot;
using System;
using System.Threading;
using System.Collections.Generic;
using Utility;


[Tool]
public partial class TerrainGenerator : MeshInstance3D
{

    #region Exports && Variables
    private Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new();
    private Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new();
    private bool update = false;
    private bool isSubscribed;
    private DrawMode drawMode;
    private float meshHeightMultiplier = 60;
    private Texture2D currentTexture;
    private int editorLevelOfDetal;


    [Export] public const int mapChunkSize = 257;
    private Color[] colorMap;

    [Export] public DrawMode Draw_Mode{get=> drawMode; set{
        drawMode = value;
        OnVariableChanged();
    }}
    [Export] public NoiseGenerator.NormalizeMode normalizeMode;

    [Export(PropertyHint.Range, "0, 8, 1, min, max")] 
    public int EditorLevelOfDetal{get => editorLevelOfDetal; set{
        editorLevelOfDetal = value;
        if(AutoGenerate) OnVariableChanged();
    }}
    [Export] bool AutoGenerate{get => isSubscribed; set{

        isSubscribed = value;
        
        if(value && Engine.IsEditorHint()){
            NoiseParams.OnValidated += OnVariableChanged;
        }
        else if(!value && Engine.IsEditorHint()){
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
    [Export] Material DefaultMaterial {get; set;} = ResourceLoader.Load<Material>("res://Materials/DefaultTerrainMat.tres");

    [Export(PropertyHint.Range, "0.001f, 1f, min, or_greater, hide_slider")] 
    public float MeshHeightMultiplier {get => meshHeightMultiplier; set{
        meshHeightMultiplier = value;
        if(AutoGenerate) OnVariableChanged();
    }} 
    [Export] Curve MeshHeightCurve {get; set;} = ResourceLoader.Load<Curve>("res://Resources/Terrain/HeightCurve.tres");
    
    [Export] public NoiseMapParams NoiseParams;

    [Export] public TerrainGroup Terrains;

    

    #endregion
    #region Godot Main Thread
	public override void _Ready()
	{
        Mesh?.Dispose();
        Mesh = null;
        Visible = false;
        //OnVariableChanged();

        NoiseParams??= new NoiseMapParams();
        Terrains??= new TerrainGroup();
	}

    public override void _Process(double delta)
    {
        UpdateMapDataInfoQueue();
    }

    #endregion
    #region Custom Threading
    public void RequestMapData(Action<MapData> callback, Vector2 additionalOffset){

        ThreadStart threadStart = delegate{
            MapDataThread(callback, additionalOffset);
        };

        new Thread(threadStart).Start();
    }
    private void MapDataThread(Action<MapData> callback, Vector2 additionalOffset){

        MapData mapData = GenerateMapData(additionalOffset);

        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(Action<MeshData> callback, MapData mapData, int lod){
        
        ThreadStart threadStart = delegate{
            MeshDataThread(callback, mapData, lod);
        };

        new Thread(threadStart).Start();
    }

    private void MeshDataThread(Action<MeshData> callback, MapData mapData, int lod){

        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, MeshHeightCurve, lod);

        lock (meshDataThreadInfoQueue){
            
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void UpdateMapDataInfoQueue() {

        if(mapDataThreadInfoQueue.Count > 0){
            for(int i = 0; i < mapDataThreadInfoQueue.Count; i++){
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callBack(threadInfo.parameter);
            }
        }

        if(meshDataThreadInfoQueue.Count > 0){
            for(int i = 0; i < meshDataThreadInfoQueue.Count; i++){
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();

                threadInfo.callBack(threadInfo.parameter);
            }
        }    
    }

    #endregion

    #region Helpers
    private void LoadFromDataFile(){
        if(!Engine.IsEditorHint()) return;
        currentTexture = ResourceLoader.Load<Texture2D>(ImagePath);
        SetImageTexture();
    }
    private void SaveToDataFile(){

        currentTexture?.GetImage()?.SavePng(ImagePath);
    }
    private void SetImageTexture(){

        if(Mesh == null) return;

        var material = GetSurfaceOverrideMaterial(0);

        if(material == null){
            SetSurfaceOverrideMaterial(0, (Material)DefaultMaterial.Duplicate());
        }

        material?.Set("albedo_texture", currentTexture);
        //material.Set("texture_filter", "Nearest");
    }
    public void OnVariableChanged(){

        if(!Engine.IsEditorHint()) return;
        
        NoiseParams??= new NoiseMapParams();
        Terrains??= new TerrainGroup();

        DrawMapInEditor();
        SetImageTexture();
    }
    public void OnVariableChanged(object sender, EventArgs e){
        if(!Engine.IsEditorHint())return;

        DrawMapInEditor();
        SetImageTexture();
    }
    private float[,] GenerateNoiseMap(Vector2 additionalOffset){

        float[,] noiseMap = Utility.NoiseGenerator.GenerateNoiseMap(NoiseParams.Scale, NoiseParams.Octaves, NoiseParams.Persistance, NoiseParams.Lacunarity, NoiseParams.Seed, mapChunkSize, NoiseParams.Offset + additionalOffset, normalizeMode);

        if(Engine.IsEditorHint()){

            PlaneMesh planeMesh = new PlaneMesh(){
                Size = new(mapChunkSize, mapChunkSize)
            };

            Mesh = planeMesh;
            
        }

        return noiseMap;
    }
    private MapData GenerateMapData(Vector2 additionalOffset){

        float[,] noiseMap = GenerateNoiseMap(additionalOffset);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        for(int y = 0; y < mapChunkSize; y++){

            for(int x = 0; x < mapChunkSize; x++){
                
                float currentHeight = noiseMap[x,y];

                for(int i = 0; i < Terrains.regions.Length; i++){
                    
                    if(currentHeight >= Terrains.regions[i].height){
                        colorMap[y * mapChunkSize + x] = Terrains.regions[i].color;
                    }
                    else{
                        break;
                    }

                }
            }
        }
        return new MapData(noiseMap, colorMap);
    }
    private void DrawMapInEditor(){
        
        MapData mapData = GenerateMapData(Vector2.Zero);

        if(drawMode == DrawMode.NoiseMap){
            currentTexture = ImageTexture.CreateFromImage(TextureGenerator.ImageFromHeightMap(mapData.heightMap));
        }
        else if(drawMode == DrawMode.ColorMap){
            currentTexture = ImageTexture.CreateFromImage(TextureGenerator.ImageFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if(drawMode == DrawMode.Mesh){

            Mesh = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, MeshHeightCurve, editorLevelOfDetal).CreateMesh();
        }
    }
    #endregion
    #region Enums & Structs
    public enum DrawMode{

        NoiseMap, ColorMap, Mesh
    };
    struct MapThreadInfo<T>{
        public readonly Action<T> callBack;
        public readonly T parameter;
        public MapThreadInfo(Action<T> callBack, T parameter)
        {
            this.callBack = callBack;
            this.parameter = parameter;
        }
    }

    #endregion
}
public struct MapData{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }

}