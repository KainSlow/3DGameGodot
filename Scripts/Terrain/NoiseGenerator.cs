using System;
using Godot;

namespace Utility{
	public static class NoiseGenerator 
	{
		public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale)
		{
            FastNoiseLite noise = new()
            {
                NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin
            };

            float[,] noiseMap = new float[mapWidth,mapHeight];

			for(int y = 0; y < mapHeight; y++){
				for(int x = 0; x < mapWidth; x++){

					float sampleX = x / scale;
					float sampleY = y / scale;

					float perlinValue = noise.GetNoise2D(sampleX, sampleY);

					noiseMap[x,y] = perlinValue;
				}
			}

			return noiseMap;
		}

		public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, int octaves, float persistance, float lacunarity)
		{
            FastNoiseLite noise = new()
            {
                NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin
            };

			float maxNoiseHeight = float.MinValue;
			float minNoiseHeight = float.MaxValue;

            float[,] noiseMap = new float[mapWidth,mapHeight];

			for(int y = 0; y < mapHeight; y++){
				for(int x = 0; x < mapWidth; x++){

					float amplitude = 1;
					float frequency = 1;
					float noiseHeight = 0;


					for(int i = 0; i < octaves;i++){

						float sampleX = x / scale * frequency;
						float sampleY = y / scale * frequency;

						float perlinValue = noise.GetNoise2D(sampleX, sampleY) * 2 - 1;

						noiseHeight += perlinValue * amplitude;

						amplitude *= persistance;
						frequency *= lacunarity;
					}

					if(noiseHeight > maxNoiseHeight){
						maxNoiseHeight = noiseHeight;
					}
					else if( noiseHeight < minNoiseHeight){
						minNoiseHeight = noiseHeight;
					}

					noiseMap[x,y] = noiseHeight;
				}
			}

			for(int y = 0; y < mapHeight; y++){
				for(int x = 0; x < mapWidth; x++){
					noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);
				}
			}

			return noiseMap;
		}

		public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
		{	
			Random prng = new(seed);
			
            FastNoiseLite noise = new()
            {
                NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin,
				Seed = seed
            };
			Vector2[] octavesOffsets = new Vector2[octaves];
			for(int i = 0; i < octaves; i++){
				float offsetX = prng.Next(-100000, 100000) + offset.X;
				float offsetY = prng.Next(-100000, 100000) + offset.Y;
				octavesOffsets[i] = new Vector2(offsetX, offsetY);
			}

			float maxNoiseHeight = float.MinValue;
			float minNoiseHeight = float.MaxValue;

			float halfWidth = mapWidth/2f;
			float halfHeight = mapHeight/2f;

            float[,] noiseMap = new float[mapWidth,mapHeight];

			for(int y = 0; y < mapHeight; y++){
				for(int x = 0; x < mapWidth; x++){

					float amplitude = 1;
					float frequency = 1;
					float noiseHeight = 0;


					for(int i = 0; i < octaves;i++){

						float sampleX = (x-halfWidth) / scale * frequency + octavesOffsets[i].X;
						float sampleY = (y-halfHeight) / scale * frequency + octavesOffsets[i].Y;

						float perlinValue = noise.GetNoise2D(sampleX, sampleY) * 2 - 1;

						noiseHeight += perlinValue * amplitude;

						amplitude *= persistance;
						frequency *= lacunarity;
					}

					if(noiseHeight > maxNoiseHeight){
						maxNoiseHeight = noiseHeight;
					}
					else if( noiseHeight < minNoiseHeight){
						minNoiseHeight = noiseHeight;
					}

					noiseMap[x,y] = noiseHeight;
				}
			}

			for(int y = 0; y < mapHeight; y++){
				for(int x = 0; x < mapWidth; x++){
					noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);
				}
			}

			return noiseMap;
		}

		public static float[,] GenerateNoiseMap(NoiseMapParams noiseParams)
		{	
			int width = noiseParams.Width, height = noiseParams.Height, octaves = noiseParams.Octaves;
			float scale = noiseParams.Scale;

			Random prng = new(noiseParams.Seed);
			
            FastNoiseLite noise = new()
            {
                NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin
				//Seed = noiseParams.Seed
            };
			Vector2[] octavesOffsets = new Vector2[octaves];
			for(int i = 0; i < octaves; i++){
				float offsetX = prng.Next(-100000, 100000) + noiseParams.Offset.X;
				float offsetY = prng.Next(-100000, 100000) + noiseParams.Offset.Y;
				octavesOffsets[i] = new Vector2(offsetX, offsetY);
			}

			float maxNoiseHeight = float.MinValue;
			float minNoiseHeight = float.MaxValue;

			float halfWidth = width/2f;
			float halfHeight = height/2f;

            float[,] noiseMap = new float[width, height];

			for(int y = 0; y < height; y++){
				for(int x = 0; x < width; x++){

					float amplitude = 1;
					float frequency = 1;
					float noiseHeight = 0;


					for(int i = 0; i < octaves;i++){

						float sampleX = (x-halfWidth) / scale * frequency + octavesOffsets[i].X;
						float sampleY = (y-halfHeight) / scale * frequency + octavesOffsets[i].Y;

						float perlinValue = noise.GetNoise2D(sampleX, sampleY) * 2 - 1;

						noiseHeight += perlinValue * amplitude;

						amplitude *= noiseParams.Persistance;
						frequency *= noiseParams.Lacunarity;
					}

					if(noiseHeight > maxNoiseHeight){
						maxNoiseHeight = noiseHeight;
					}
					else if( noiseHeight < minNoiseHeight){
						minNoiseHeight = noiseHeight;
					}

					noiseMap[x,y] = noiseHeight;
				}
			}

			for(int y = 0; y < height; y++){
				for(int x = 0; x < width; x++){
					noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);
				}
			}

			return noiseMap;
		}
	}
}
