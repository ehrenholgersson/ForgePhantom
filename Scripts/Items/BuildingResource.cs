using Godot;

[GlobalClass]
public partial class BuildingResource : Resource, IDraggable
{
    [Export] public Texture2D Icon;
    [Export] public Mesh ObjectMesh;
    [Export] public PackedScene BuildingObject;
    [Export] public string BuildingName;
    [Export] public float MinHeight;
    public float Health = 10;
    Vector3 _size = Vector3.Zero;

    public Vector3 Size { get => (_size != Vector3.Zero) ? _size : CalculateSize(); }

    public Mesh Model { get => ObjectMesh; } // why?
    

    public BuildingResource()
    {
        BuildingName = string.Empty;
        Icon = null;
        ObjectMesh = null;
        Health = 10;
        BuildingObject = null;
    }
    public BuildingResource(BuildingResource original)
    {
        Icon = original.Icon;
        Health = original.Health;
        ObjectMesh = original.Model;
        BuildingObject = original.BuildingObject;
        BuildingName = original.BuildingName;
        if (original._size == Vector3.Zero) 
        { 
            CalculateSize();
        }
        else
        {
            _size = original._size;  
        }
        
    }

    public Vector3 CalculateSize() // Iterate through each mesh instance to determine the maximum size along each axis
    {
        if (BuildingObject != null)
        {
            var instance = BuildingObject.Instantiate();
            foreach (Node child in instance.GetChildren())
                if (child is MeshInstance3D)
                {
                    var aabb = ((MeshInstance3D)child).GetAabb();
                    _size.X = Mathf.Max(_size.X, aabb.Size.X);
                    _size.Y = Mathf.Max(_size.Y, aabb.Size.Y);
                    _size.Z = Mathf.Max(_size.Z, aabb.Size.Z);
                }
        }
        else if (ObjectMesh!= null)
        {
            _size = ObjectMesh.GetAabb().Size;
        }
        GD.Print("Building " + BuildingName + " size is " + _size);
        return _size;
    }

    public void ScaleMesh(float size)
    {
        if (ObjectMesh == null)
        {
            return;
        }

        size = Mathf.Max(size, MinHeight);
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