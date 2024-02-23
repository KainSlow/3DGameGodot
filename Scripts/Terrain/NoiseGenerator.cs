using System;
using Godot;

namespace Utility{
	public static class NoiseGenerator 
	{

		public enum NormalizeMode {Local, Global};
		public static float[,] GenerateNoiseMap(float scale, int octaves, float persistance, float lacunarity, int seed, int chunkSize, Vector2 offset, NormalizeMode mode){

			Random prng = new(seed);
			
            FastNoiseLite noise = new()
            {
                NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin,
            };

			float maxPossibleHeight = 0f;
			float amplitude = 1;
			float frequency = 1;

			Vector2[] octavesOffsets = new Vector2[octaves];
			for(int i = 0; i < octaves; i++){
				float offsetX = prng.Next(-100000, 100000) + offset.X;
				float offsetY = prng.Next(-100000, 100000) - offset.Y;
				octavesOffsets[i] = new Vector2(offsetX, offsetY);

				maxPossibleHeight += amplitude;
				amplitude *= persistance;
			}

			float maxLocalNoisechunkSize = float.MinValue;
			float minLocalNoisechunkSize = float.MaxValue;

			float halfchunckSize = chunkSize/2f;

            float[,] noiseMap = new float[chunkSize, chunkSize];

			for(int y = 0; y < chunkSize; y++){
				for(int x = 0; x < chunkSize; x++){
					
					amplitude = 1;
			 		frequency = 1;
					float noisechunkSize = 0;
					
					for(int i = 0; i < octaves;i++){

						float sampleX = (x-halfchunckSize) / scale * frequency + octavesOffsets[i].X * frequency;
						float sampleY = (y-halfchunckSize) / scale * frequency + octavesOffsets[i].Y * frequency;

						float perlinValue = noise.GetNoise2D(sampleX, sampleY) * 2 - 1;

						noisechunkSize += perlinValue * amplitude;

						amplitude *= persistance;
						frequency *= lacunarity;
					}

					if(noisechunkSize > maxLocalNoisechunkSize){
						maxLocalNoisechunkSize = noisechunkSize;
					}
					else if( noisechunkSize < minLocalNoisechunkSize){
						minLocalNoisechunkSize = noisechunkSize;
					}

					noiseMap[x,y] = noisechunkSize;
				}
			}

			for(int y = 0; y < chunkSize; y++){
				for(int x = 0; x < chunkSize; x++){

					if(mode == NormalizeMode.Local){
						noiseMap[x,y] = Mathf.InverseLerp(minLocalNoisechunkSize, maxLocalNoisechunkSize, noiseMap[x,y]);
						continue;
					}

					//NormalizeMode.Global

					float normalizeHeight = (noiseMap[x,y] + 1) / (2f * maxPossibleHeight);
					noiseMap[x,y] = Mathf.Clamp(normalizeHeight, -1, int.MaxValue);

				}
			}

			return noiseMap;
		}
		
	}
}
