using Godot;

public static class TextureGenerator 
{
	public static Image ImageFromColorMap(Color[] colorMap, int width, int height){

		byte[] colorData = ProjectUtils.FromColorToByteArray(colorMap);

		Image image = Image.CreateFromData(width, height, false, Image.Format.Rgba8, colorData);

		return image;
	}

	public static Image ImageFromHeightMap(float[,] heightMap){

		int width = heightMap.GetLength(0);
		int height = heightMap.GetLength(0);

		Color[] colorMap = new Color[width * height];

		for(int y = 0; y < height; y++){
			for(int x = 0; x < width; x++){

				float colorValue = Mathf.Lerp(0f,1f, heightMap[x,y]);
				colorMap[y * width + x] = new Color(colorValue, colorValue, colorValue);
			}
		}

		return ImageFromColorMap(colorMap, width, height);
	}
}
