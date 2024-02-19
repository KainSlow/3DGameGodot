using System;
using Godot;

namespace Utility{
	public static class NoiseGenerator 
	{
		public static float[,] GenerateNoiseMap(NoiseMapParams noiseParams, int chunkSize)
		{	
			int octaves = noiseParams.Octaves;
			float scale = noiseParams.Scale;

			Random prng = new(noiseParams.Seed);
			
            FastNoiseLite noise = new()
            {
                NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin
				
            };
			Vector2[] octavesOffsets = new Vector2[octaves];
			for(int i = 0; i < octaves; i++){
				float offsetX = prng.Next(-100000, 100000) + noiseParams.Offset.X;
				float offsetY = prng.Next(-100000, 100000) + noiseParams.Offset.Y;
				octavesOffsets[i] = new Vector2(offsetX, offsetY);
			}

			float maxNoisechunckSize = float.MinValue;
			float minNoisechunckSize = float.MaxValue;

			float halfchunckSize = chunkSize/2f;

            float[,] noiseMap = new float[chunkSize, chunkSize];

			for(int y = 0; y < chunkSize; y++){
				for(int x = 0; x < chunkSize; x++){

					float amplitude = 1;
					float frequency = 1;
					float noisechunckSize = 0;


					for(int i = 0; i < octaves;i++){

						float sampleX = (x-halfchunckSize) / scale * frequency + octavesOffsets[i].X * frequency;
						float sampleY = (y-halfchunckSize) / scale * frequency + octavesOffsets[i].Y * frequency;

						float perlinValue = noise.GetNoise2D(sampleX, sampleY) * 2 - 1;

						noisechunckSize += perlinValue * amplitude;

						amplitude *= noiseParams.Persistance;
						frequency *= noiseParams.Lacunarity;
					}

					if(noisechunckSize > maxNoisechunckSize){
						maxNoisechunckSize = noisechunckSize;
					}
					else if( noisechunckSize < minNoisechunckSize){
						minNoisechunckSize = noisechunckSize;
					}

					noiseMap[x,y] = noisechunckSize;
				}
			}

			for(int y = 0; y < chunkSize; y++){
				for(int x = 0; x < chunkSize; x++){
					noiseMap[x,y] = Mathf.InverseLerp(minNoisechunckSize, maxNoisechunckSize, noiseMap[x,y]);
				}
			}

			return noiseMap;
		}
	}
}
