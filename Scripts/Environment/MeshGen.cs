using Godot;
using System;
using System.Collections.Generic;
//using System.Numerics;

public partial class MeshGen : StaticBody3D
{
	struct VertsnNormals
	{
		public Vector3[] Verts;
		public Vector3[] Normals;

		public VertsnNormals (List<Vector3> verticies, List<Vector3> normals)
		{
			Verts = verticies.ToArray();
			Normals = normals.ToArray();
		}
	}

	[ExportGroup("Noise Parameters")]
	[Export] int _seed;
	[Export] float _incrementSize;
    [Export] int _planeSize = 10;
	[Export] int _mapheight;

    [ExportGroup("Other")]

    
	[Export] int _solidThreshold = 1;
	[Export] float _cubeValueLimit = 2;
    [Export] Material _material;
	
	int[,,] _voxels;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        var startTime  = Time.GetTicksMsec();
		_voxels = GenerateHeightMap(300, 20, 300);
			
		//	new int[,,]
		//{
		//	{
		//		{ 0, 0, 0, 0 },
		//		{ 0, 0, 0, 0 },
		//		{ 0, 0, 0, 0 },
		//		{ 0, 0, 0, 0 }
		//	},
		//	{
		//		{ 0, 0, 0, 0 },
		//		{ 0, 2, 2, 0 },
		//		{ 0, 2, 2, 0 },
		//		{ 0, 0, 0, 0 }
		//	},

  //          {
  //              { 0, 0, 0, 0 },
  //              { 0, 2, 2, 0 },
  //              { 0, 2, 2, 0 },
  //              { 0, 0, 0, 0 }
  //          },

  //          {
		//		{ 0, 0, 0, 0 },
		//		{ 0, 0, 0, 0 },
		//		{ 0, 0, 0, 0 },
		//		{ 0, 0, 0, 0 }
		//	}
		//};

        // (almost completyely) copy pasta from godot documentation @ https://docs.godotengine.org/en/stable/classes/class_arraymesh.html
        var vertices = buildmesh();
		for ( var i = 0;i<vertices.Verts.Length; i++)
		{
			//GD.Print(vertices.Verts[i]);
		}
		// mesh as arraymesh, worked but coillisions were wrong
		//      // Initialize the ArrayMesh.
		//      var arrMesh = new ArrayMesh();
		//      var arrays = new Godot.Collections.Array();
		//      arrays.Resize((int)Mesh.ArrayType.Max);
		//      arrays[(int)Mesh.ArrayType.Vertex] = vertices.Verts;
		//      arrays[(int)Mesh.ArrayType.Normal] = vertices.Normals;

		//      // Create the Mesh.
		//      arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
		//var mI3d = new MeshInstance3D();
		//mI3d.Mesh = arrMesh;
		//arrMesh.RegenNormalMaps();

		var st = new SurfaceTool();

		st.Begin(Mesh.PrimitiveType.Triangles);
		for (int i = 0; i < vertices.Verts.Length;i++)
		{
			st.SetNormal(vertices.Normals[i]);
			st.SetUV(new Vector2(0, 0)); // FIXME
			st.AddVertex(vertices.Verts[i]);
		}


        var mI3d = new MeshInstance3D();
        mI3d.Mesh = st.Commit();
        mI3d.Mesh.SurfaceSetMaterial(0, _material);
		mI3d.CreateTrimeshCollision();
		AddChild(mI3d);
		// collision
		//var collider = new CollisionShape3D();
  //      AddChild(collider);
  //      collider.MakeConvexFromSiblings();


        GD.Print("completed in " + (Time.GetTicksMsec() - startTime));
    }

    int[,,] GenerateHeightMap(int width, int height, int depth)
	{
		int[,,] map = new int[width, height, depth];
		var rng = new RandomNumberGenerator();
		var noise = new FastNoiseLite();

		noise.Seed = (int)rng.Randi();
		noise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
        noise.FractalType = FastNoiseLite.FractalTypeEnum.None;
        noise.Frequency = (0.2f);
        noise.FractalLacunarity = (2f);
        noise.FractalGain = (5f);

        for (int x = 0; x < width; x++)
            for (int z = 0; z < depth; z++)
			{
				float level = noise.GetNoise2D(x/10f, z/10f);
				level = Mathf.Clamp((level +1)/2,0.3f, 0.6f);
				for (int y = 0;y< height; y++)
				{
					//GD.Print("writing " + x + ',' + y + ',' + z);
					if (level * (height-1) < y )  
					{
						map[x,y,z] = (int)_cubeValueLimit;
						
					}
					else
					{
                        map[x, y, z] = 0;
                    }
                    //GD.Print("cube at "+ x + ',' + y + ',' + z + "value of " + level + " "+ map[x, y, z]);
                }

            }

                return map;
	}

	float GetCornerIndex(Vector3I voxelLocation)
	{
		return _voxels[voxelLocation.X, voxelLocation.Y, voxelLocation.Z];
	}
    float GetCornerIndex(Vector3 voxelLocation)
    {
        return _voxels[(int)(voxelLocation.X), (int)(voxelLocation.Y), (int)(voxelLocation.Z)];
    }

    VertsnNormals buildmesh()
	{
        List<Vector3> verts = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		//List<int> tris = new List<int>();
		
        //Loop xyz
        for (int x = 0; x < _voxels.GetLength(0) -1; x++)
			for (int y = 0; y < _voxels.GetLength(1) -1; y++)
				for (int z = 0; z < _voxels.GetLength(2) - 1; z++)
				{
					Vector3I cubePos = new Vector3I(x, y, z);
                    int cubeIndex = 0;
                    // the lookup tables I got seem a bit backwards in the order they do this? I would have started bottom front left
                    if (GetCornerIndex(cubePos + MarchingCubesTables.cubeCorners[0]) > _solidThreshold) cubeIndex |= 1;
                    if (GetCornerIndex(cubePos + MarchingCubesTables.cubeCorners[1]) > _solidThreshold) cubeIndex |= 2;
                    if (GetCornerIndex(cubePos + MarchingCubesTables.cubeCorners[2]) > _solidThreshold) cubeIndex |= 4;
                    if (GetCornerIndex(cubePos + MarchingCubesTables.cubeCorners[3]) > _solidThreshold) cubeIndex |= 8;
                    if (GetCornerIndex(cubePos + MarchingCubesTables.cubeCorners[4]) > _solidThreshold) cubeIndex |= 16;
                    if (GetCornerIndex(cubePos + MarchingCubesTables.cubeCorners[5]) > _solidThreshold) cubeIndex |= 32;
                    if (GetCornerIndex(cubePos + MarchingCubesTables.cubeCorners[6]) > _solidThreshold) cubeIndex |= 64;
                    if (GetCornerIndex(cubePos + MarchingCubesTables.cubeCorners[7]) > _solidThreshold) cubeIndex |= 128;

                    int[] intersectedEdges = MarchingCubesTables.triTable[cubeIndex];

					//GD.Print("cube at " + cubePos + "index of " + cubeIndex);

					int counter = 0;
					for (int i = 0; intersectedEdges[i] >=0; i++)
					{
						//GD.Print("Edge: " + intersectedEdges[i] +" corners "+ MarchingCubesTables.edgeConnections[intersectedEdges[i]][0] +" and " + MarchingCubesTables.edgeConnections[intersectedEdges[i]][1]);
						//Vector3 localPosition = new Vector3 (x, y, z);
						Vector3 start = MarchingCubesTables.cubeCorners[MarchingCubesTables.edgeConnections[intersectedEdges[i]][0]];
                        Vector3 end = MarchingCubesTables.cubeCorners[MarchingCubesTables.edgeConnections[intersectedEdges[i]][1]];
						//GD.Print("Start: " + start);
						//GD.Print("End: " + end);

						float difference = Mathf.Abs(GetCornerIndex(cubePos + MarchingCubesTables.cubeCorners[MarchingCubesTables.edgeConnections[intersectedEdges[i]][0]]) - GetCornerIndex(cubePos + MarchingCubesTables.cubeCorners[MarchingCubesTables.edgeConnections[intersectedEdges[i]][1]]));
						Vector3 vertPos = start + ((end - start) / 2) * (difference / _cubeValueLimit);
                        //GD.Print(start.Y + " + (("+end.Y+" - "+start.Y+") / 2) * ("+ difference+" / "+_cubeValueLimit+") = "+vertPos.Y);
                        //GD.Print("local vertex at " + vertPos);
                        vertPos += cubePos;
						verts.Add(vertPos);
						//GD.Print("new vertex at " + vertPos);

						// lazy way to generate normals, using same for each vert in the triangle
						if (counter == 2)
						{
							var v1 = verts[verts.Count - 2] - verts[verts.Count -3];
							var v2 = verts[verts.Count - 1] - verts[verts.Count - 3];
							Vector3  normal = - v1.Cross(v2);

							normals.Add(normal);
                            normals.Add(normal);
                            normals.Add(normal);
                        }
						counter = (counter + 1) % 3;
                    }
                }
		
		return new VertsnNormals(verts,normals);
    }
}
