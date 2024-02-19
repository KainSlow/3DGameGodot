using Godot;

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

	public static Vector3 Min(Vector3 vec, float value){

		return new Vector3(
			(vec.X < value)? vec.X: value,
			(vec.Y < value)? vec.Y: value,
			(vec.Z < value)? vec.Z: value
		);
	}

	public static Vector3 Min(float value, Vector3 vec){
		return Min(vec, value);
	}

	public static Vector3 Max(Vector3 vec, float value){
		return new Vector3(
			(vec.X > value)? vec.X: value,
			(vec.Y > value)? vec.Y: value,
			(vec.Z > value)? vec.Z: value
		);
	}

	public static Vector3 Max(float value, Vector3 vec){
		return Max(vec, value);
	}

}

	public static class Extensions{
		
		public static float Distance(this Aabb box, Vector3 point){

			Vector3 pointOnBounds = new Vector3(
				Mathf.Clamp(point.X, box.Position.X, box.End.X),
				Mathf.Clamp(point.Y, box.Position.Y, box.End.Y),
				Mathf.Clamp(point.Z, box.Position.Z, box.End.Z)
			);

			return (point - pointOnBounds).Length();

		}

		public static float SqrDistance(this Aabb box, Vector3 point){

			Vector3 pointOnBounds = new Vector3(
				Mathf.Clamp(point.X, box.Position.X, box.End.X),
				Mathf.Clamp(point.Y, box.Position.Y, box.End.Y),
				Mathf.Clamp(point.Z, box.Position.Z, box.End.Z)
			);

			return (point - pointOnBounds).LengthSquared();
		}
	}
