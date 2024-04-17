using Godot;
using System;

[GlobalClass]
public partial class BuildingResource : Resource, IDraggable
{
    [Export] public Texture2D Icon;
    [Export] public Mesh ObjectMesh;
    [Export] public string BuildingName;
    public float Health = 10;

    public Mesh Model { get => ObjectMesh; } // why?
    

    public BuildingResource()
    {
        BuildingName = string.Empty;
        Icon = null;
        ObjectMesh = null;
        Health = 10;
    }
    public BuildingResource(BuildingResource original)
    {
        Icon = original.Icon;
        Health = original.Health;
        ObjectMesh = original.Model;
        BuildingName = original.BuildingName;


    }

    public void ScaleMesh(float size)
    {
        Aabb bounds = ObjectMesh.GetAabb();
        Vector3 Offset = Vector3.Zero;
        float scale = 1;
        bool modify = false;
        
        if (bounds.GetCenter() != Vector3.Zero)
        {
            Offset = - bounds.GetCenter(); // seems weird but this is how it was for the generated meshes?
            modify = true;
        }
        GD.Print("for "+ BuildingName+"size is " + bounds.Size + " and center is at " + bounds.GetCenter().Y);
        if (bounds.Size.Y < size)
        {
            scale = size / ObjectMesh.GetAabb().Size.Y;
            modify = true;
        }
        if (modify)
        { 
            var scaled = new ArrayMesh();
            scaled.ClearSurfaces();
            for (int j = 0; j < ObjectMesh.GetSurfaceCount(); j++)
            {
                var mesh = new ArrayMesh();
                mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, ObjectMesh.SurfaceGetArrays(j));
                var mdt = new MeshDataTool();
                mdt.CreateFromSurface(mesh, 0);
                for (var i = 0; i < mdt.GetVertexCount(); i++)
                {
                    Vector3 vertex = mdt.GetVertex(i);
                    vertex *= scale;
                    vertex += Offset*scale;
                    mdt.SetVertex(i, vertex);
                }
                mesh.ClearSurfaces();
                mdt.CommitToSurface(scaled);
            }
            ObjectMesh = scaled;
            bounds = ObjectMesh.GetAabb();
            GD.Print("modified " + BuildingName);
            GD.Print("size is " + bounds.Size + " and base is at " + (bounds.GetCenter() - (bounds.Size / 2)).Y);
        }
    }

    public BuildingResource newInstance()
    {
        return new BuildingResource(this);
    }
}