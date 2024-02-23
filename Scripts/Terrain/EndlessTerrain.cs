using Godot;
using System;
using System.Collections.Generic;

public partial class EndlessTerrain : Node3D {

	#region Exports & Variables

	const float viewerMoveThresholdForChunkUpdate = 25f;
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
	int chunkSize;
	int chunkVisibleInViewDst;
	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new();
	static List<TerrainChunk> terrainChunksVisibleLastUpdate = new();
	[Export] LODInfoGroup detailLevels;
	[Export] static float maxViewDst = 1000;

	[Export] Node3D viewer;
	[Export] Material mat;
	public static Vector2 viewerPosition;
	Vector2 oldViewerPosition;
	[Export] static TerrainGenerator terrainGenerator;

	#endregion
    
	#region Godot Main Thread
	public override void _Ready()
    {
		maxViewDst = detailLevels.LODInfos[detailLevels.LODInfos.Length-1].visibleDstThreshold;
		chunkSize = TerrainGenerator.mapChunkSize - 1;

		chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDst/chunkSize);

		terrainGenerator = GetParent().GetNode<TerrainGenerator>("./TerrainGenerator");

		UpdateVisibleChunks();
    }

    public override void _Process(double delta)
    {
		viewerPosition = new Vector2(viewer.Position.X, viewer.Position.Z);

		if((oldViewerPosition - viewerPosition).LengthSquared() > sqrViewerMoveThresholdForChunkUpdate){
			oldViewerPosition = viewerPosition;
			UpdateVisibleChunks();
		}
    }

	#endregion
    void UpdateVisibleChunks(){

		foreach(var chunk in terrainChunksVisibleLastUpdate){
			chunk.SetVisible(false);
		}

		terrainChunksVisibleLastUpdate.Clear();

		int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.X / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.Y / chunkSize);

		for(int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; yOffset++){
			for(int xOffset = -chunkVisibleInViewDst; xOffset <= chunkVisibleInViewDst; xOffset++){
				
				Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if(terrainChunkDictionary.ContainsKey(viewedChunkCoord)){

					terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();

					/*
					if(terrainChunkDictionary[viewedChunkCoord].IsVisible()){
						terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
					}
					*/

				}
				else{

					terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels.LODInfos, mat));
					AddChild(terrainChunkDictionary[viewedChunkCoord].meshObject);
				
				}
			}
		}
	}
	public class TerrainChunk {

		public MeshInstance3D meshObject;
		Vector2 position;	
		Aabb bounds;
		LODInfo[] detailLevels;
		LODMesh[] lodMeshes;
		MapData mapData;
		bool mapDataReceived;
		int previousLODIndex = -1;

		public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Material material){

			this.detailLevels = detailLevels;
			position = coord * size;	
			Vector3 positionV3 = new Vector3(position.X, 0, position.Y);
			bounds = new Aabb(positionV3, new Vector3(size, 0, size));

            meshObject = new()
            {
				//Name = "TerrainChunk",

                Mesh = new PlaneMesh
                {
                    Size = new Vector2(size, size)
                },

                Position = positionV3,

            };

			Material localMat = (Material)material.Duplicate();
			
			meshObject.SetSurfaceOverrideMaterial(0, localMat);

			lodMeshes = new LODMesh[detailLevels.Length];

			for(int i = 0; i < detailLevels.Length; i++){
				lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
			}

			terrainGenerator.RequestMapData(OnMapDataReceived, position);
			SetVisible(false);
		}

		void OnMapDataReceived(MapData mapData){

			this.mapData = mapData;

			mapDataReceived = true; 

			var texture = ImageTexture.CreateFromImage(TextureGenerator.ImageFromColorMap(mapData.colorMap, TerrainGenerator.mapChunkSize, TerrainGenerator.mapChunkSize));
			//var texture = ImageTexture.CreateFromImage(TextureGenerator.ImageFromHeightMap(mapData.heightMap));

			this.meshObject.GetSurfaceOverrideMaterial(0).Set("albedo_texture", texture);

			UpdateTerrainChunk();
		}

		public void UpdateTerrainChunk(){

			if(!mapDataReceived) return;

			float viewerDstFromNearestEdge = bounds.Distance(new Vector3(viewerPosition.X, 0f, viewerPosition.Y));
			
			bool visible = viewerDstFromNearestEdge <= maxViewDst;

			SetVisible(visible);

			if(!visible)return;

			int lodIndex = 0;

			for(int i = 0; i < detailLevels.Length - 1; i++){

				if(viewerDstFromNearestEdge <= detailLevels[i].visibleDstThreshold){
					break;
				}

				lodIndex = i+1;
				
			}

			if(lodIndex != previousLODIndex){

				LODMesh lodMesh = lodMeshes[lodIndex];
				if(lodMesh.hasMesh){

					previousLODIndex = lodIndex;
					meshObject.Mesh = lodMesh.mesh;
				
				}
				else if(!lodMesh.hasRequestedMesh){
					lodMesh.RequestMesh(mapData);
				}
			}

			terrainChunksVisibleLastUpdate.Add(this);

		}

		public void SetVisible(bool visible){
			meshObject.Visible = visible;
			meshObject.ProcessMode = visible? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
		}

		public bool IsVisible(){
			return meshObject.IsVisibleInTree();
		}

	}

	class LODMesh{

		public Mesh mesh;
		public bool hasRequestedMesh;
		public bool hasMesh;
		int lod;

		Action updateCallback;

		public LODMesh(int lod, Action updateCallback){
			this.lod = lod;
			this.updateCallback = updateCallback;
		}
		void OnMeshDataReceived(MeshData meshData){

			mesh = meshData.CreateMesh();
			hasMesh = true;

			updateCallback();
		}
		public void RequestMesh(MapData mapData){
			terrainGenerator.RequestMeshData(OnMeshDataReceived, mapData, lod);
			hasRequestedMesh = true;
		}
	}
}

