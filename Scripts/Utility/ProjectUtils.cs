using Godot;
using System;

public static class ProjectUtils {

	/// <summary>
	/// Assumes RGBA8 Format
	/// </summary>
	/// <param name="colors"></param>
	/// <returns></returns>
	public static byte[] FromColorToByteArray(Color[] colors){

		int size = colors.Length * 4;
		byte[] bytes = new byte[size];	

		for(int i = 0; i < size; i+=4){
			
			int colorIt = i/4;
			bytes[i] = (byte)(colors[colorIt].R * 255);
			bytes[i+1] = (byte)(colors[colorIt].G * 255);
			bytes[i+2] = (byte)(colors[colorIt].B * 255);
			bytes[i+3] = (byte)(colors[colorIt].A * 255);
		}

		return bytes;
	}

}
