using Godot;
using Godot.Collections;

public static class TextureGenerator 
{
	public static Image ImageFromColorMap(Color[] colorMap, int resolution){

		byte[] colorData = ProjectUtils.FromColorToByteArray(colorMap);

		Image image = Image.CreateFromData(resolution, resolution, false, Image.Format.Rgba8, colorData);

		return image;
	}

	public static Image ImageFromHeightMap(float[,] heightMap){

		int resolution = heightMap.GetLength(0);

		Color[] colorMap = new Color[resolution * resolution];

		for(int y = 0; y < resolution; y++){
			for(int x = 0; x < resolution; x++){

				float colorValue = Mathf.Lerp(0f,1f, heightMap[x,y]);
				colorMap[y*resolution + x] = new Color(colorValue, colorValue, colorValue);
			}
		}

		return ImageFromColorMap(colorMap, resolution);
	}
}
