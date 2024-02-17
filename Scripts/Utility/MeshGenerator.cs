using Godot;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;


public static class MeshGenerator
{

	private static Mesh GenerateTerrainMesh(float[,] heightMap){

		int width = heightMap.GetLength(0);
		int height = heightMap.GetLength(1);

		var surfaceArray = new Godot.Collections.Array();
		var arrMesh = new ArrayMesh();

		var verts = new List<Vector3>();
		var uvs = new List<Vector2>();
		var normals = new List<Vector3>();
		var indices = new List<int>();

		surfaceArray.Resize((int)Mesh.ArrayType.Max);

		///Insert generate logic here
		//GenerateSphere(ref verts, ref uvs, ref normals, ref indices);
		///

		surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
		surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
		surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
		surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();


		arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.TriangleStrip, surfaceArray);

		return arrMesh;
	}

}

public class MeshData{
	public Vector3[] vertices;
	public int[] triangles;

	public MeshData(int meshWidth, int meshHeight){
		vertices = new Vector3[meshWidth * meshHeight];
	}
}
