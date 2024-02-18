using Godot;
using System.Collections.Generic;


public static class MeshGenerator
{

	public static Mesh GenerateSphereMesh(){

		var surfaceArray = new Godot.Collections.Array();
        surfaceArray.Resize((int)Mesh.ArrayType.Max);

        // C# arrays cannot be resized or expanded, so use Lists to create geometry.
        var verts = new List<Vector3>();
        var uvs = new List<Vector2>();
        var normals = new List<Vector3>();
        var indices = new List<int>();

        /***********************************
        * Insert code here to generate mesh.
        * *********************************/

		GenerateSphereData(ref verts, ref uvs, ref normals, ref indices);

        // Convert Lists to arrays and assign to surface array
        surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
        surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
        surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

        var arrMesh = new ArrayMesh();

            // Create mesh surface from mesh array
            // No blendshapes, lods, or compression used.
		arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

		return arrMesh;
	}

	private static void GenerateSphereData(ref List<Vector3> verts, ref List<Vector2> uvs, ref List<Vector3> normals, ref List<int> indices){

		int _rings = 50;
    	int _radialSegments = 50;
     	float _radius = 100;
		// Vertex indices.
        var thisRow = 0;
        var prevRow = 0;
        var point = 0;

        // Loop over rings.
        for (var i = 0; i < _rings + 1; i++)
        {
            var v = ((float)i) / _rings;
            var w = Mathf.Sin(Mathf.Pi * v);
            var y = Mathf.Cos(Mathf.Pi * v);

            // Loop over segments in ring.
            for (var j = 0; j < _radialSegments + 1; j++)
            {
                var u = ((float)j) / _radialSegments;
                var x = Mathf.Sin(u * Mathf.Pi * 2);
                var z = Mathf.Cos(u * Mathf.Pi * 2);
                var vert = new Vector3(x * _radius * w, y * _radius, z * _radius * w);
                verts.Add(vert);
                normals.Add(vert.Normalized());
                uvs.Add(new Vector2(u, v));
                point += 1;

                // Create triangles in ring using indices.
                if (i > 0 && j > 0)
                {
                    indices.Add(prevRow + j - 1);
                    indices.Add(prevRow + j);
                    indices.Add(thisRow + j - 1);

                    indices.Add(prevRow + j);
                    indices.Add(thisRow + j);
                    indices.Add(thisRow + j - 1);
                }
            }

            prevRow = thisRow;
            thisRow = point;
        }
	}

	public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, Curve heightCurve, int levelOfDetail){

		int width = heightMap.GetLength(0);
		int height = heightMap.GetLength(1);

		float topLeftX = (width-1)/-2f;
		float topLeftZ = (height-1)/2f;

		int meshSimplificationIncrement = (int)Mathf.Pow(2, levelOfDetail);
		int verticesPerLine = ((width-1)/meshSimplificationIncrement) + 1;

		MeshData meshData = new(verticesPerLine, verticesPerLine);
		int vertexIndex = 0;

		for(int y = 0; y < height;y+=meshSimplificationIncrement){
			for(int x = 0; x < width; x+=meshSimplificationIncrement){

				meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Sample(heightMap[x,y]) * heightMultiplier, topLeftZ - y);
				meshData.uvs[vertexIndex] = new Vector2(x/(float)width, y/(float)height);

				if(x < width - 1 && y < height - 1){
					meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine, vertexIndex + verticesPerLine + 1);
					meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex + 1 ,vertexIndex );
				}

				vertexIndex++;
			}
		}

		return meshData;
	}

}

public class MeshData{
	public Vector3[] vertices;

	//Array of Index, containing 3 index per triangle
	public int[] triangles;
	public Vector2[] uvs;
	int triangleIndex;
	public MeshData(int meshWidth, int meshHeight){
		vertices = new Vector3[meshWidth * meshHeight];
		triangles = new int[(meshWidth-1) * (meshHeight-1) * 6];
		uvs = new Vector2[meshWidth* meshHeight];

	}

	public void AddTriangle(int a, int b, int c){
		triangles[triangleIndex] = a;
		triangles[triangleIndex+1] = b;
		triangles[triangleIndex+2] = c;
		triangleIndex+=3;
	}

	public ArrayMesh CreateMesh(){

		var arrays = new Godot.Collections.Array();
		var arrMesh = new ArrayMesh();
		arrays.Resize((int)Mesh.ArrayType.Max);

		arrays[(int)Mesh.ArrayType.Vertex] = vertices;
		arrays[(int)Mesh.ArrayType.TexUV] = uvs;
		arrays[(int)Mesh.ArrayType.Index] = triangles;

		arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

		return arrMesh;
	}
}
