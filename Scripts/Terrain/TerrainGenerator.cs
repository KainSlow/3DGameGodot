using Godot;
using System;
using System.Threading;
using System.Collections.Generic;


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

    [Export(PropertyHint.Range, "0, 8, 1, min, max")] 
    public int EditorLevelOfDetal{get => editorLevelOfDetal; set{
        editorLevelOfDetal = value;
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
    #region Godot Main Thread
	public override void _Ready()
	{
        Mesh?.Dispose();
        Mesh = null;
        Visible = false;
        //OnVariableChanged();
	}

    public override void _Process(double delta)
    {
        UpdateMapDataInfoQueue();
    }

    #endregion
    #region Custom Threading
    public void RequestMapData(Action<MapData> callback){

        ThreadStart threadStart = delegate{
            MapDataThread(callback);
        };

        new Thread(threadStart).Start();
    }
    private void MapDataThread(Action<MapData> callback){

        MapData mapData = GenerateMapData();

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

        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);

        lock (meshDataThreadInfoQueue){
            
            if(meshData == null){
                GD.Print("enqueue null mesh data");
            }
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

                if(threadInfo.callBack == null){
                    GD.Print("dequeue null mesh data");
                }
                else{
                    threadInfo.callBack(threadInfo.parameter);
                }
            }
        }    
    }

    #endregion

    #region Helpers
    private void LoadFromDataFile(){
        currentTexture = GD.Load<Texture2D>(ImagePath);
        SetImageTexture();
    }
    private void SaveToDataFile(){

        currentTexture?.GetImage()?.SavePng(ImagePath);
    }
    private void SetImageTexture(){

        if(Mesh == null){ GD.Print("No Mesh Found"); return; }

        var material = GetSurfaceOverrideMaterial(0);
        material?.Set("albedo_texture", currentTexture);
        //material.Set("texture_filter", "Nearest");
    }
    public void OnVariableChanged(){

        if(Engine.IsEditorHint())
            DrawMapInEditor();
        SetImageTexture();
    }
    public void OnVariableChanged(object sender, EventArgs e){
        if(Engine.IsEditorHint())
            DrawMapInEditor();
        SetImageTexture();
    }
    private float[,] GenerateNoiseMap(){

        float[,] noiseMap = Utility.NoiseGenerator.GenerateNoiseMap(NoiseParams, mapChunkSize);

        if(Engine.IsEditorHint()){

            PlaneMesh planeMesh = new PlaneMesh(){
                Size = new(mapChunkSize, mapChunkSize)
            };

            Mesh = planeMesh;
            
        }

        return noiseMap;
    }
    private MapData GenerateMapData(){

        float[,] noiseMap = GenerateNoiseMap();

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
        return new MapData(noiseMap, colorMap);
    }
    private void DrawMapInEditor(){
        
        MapData mapData = GenerateMapData();

        if(drawMode == DrawMode.NoiseMap){
            currentTexture = ImageTexture.CreateFromImage(TextureGenerator.ImageFromHeightMap(mapData.heightMap));
        }
        else if(drawMode == DrawMode.ColorMap){
            currentTexture = ImageTexture.CreateFromImage(TextureGenerator.ImageFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if(drawMode == DrawMode.Mesh){

            Mesh = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorLevelOfDetal).CreateMesh();
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