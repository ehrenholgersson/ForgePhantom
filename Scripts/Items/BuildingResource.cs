using Godot;

[GlobalClass]
public partial class BuildingResource : Resource, IDraggable
{
    [Export] public Texture2D Icon;
    [Export] public Mesh ObjectMesh;
    [Export] PackedScene _object;
    [Export] public string BuildingName;
    [Export] public float MinHeight;
    public float Health = 10;
    Vector3 _size = Vector3.Zero;
    
    public Vector3 Size { get => (_size != Vector3.Zero) ? _size : CalculateSize(); }
    public Mesh Model { get => ObjectMesh; } // for IDraggable Interface
    public PackedScene Object { get => _object; }
    

    public BuildingResource()
    {
        BuildingName = string.Empty;
        Icon = null;
        ObjectMesh = null;
        Health = 10;
        _object = null;
    }
    public BuildingResource(BuildingResource original)
    {
        Icon = original.Icon;
        Health = original.Health;
        ObjectMesh = original.Model;
        _object = original.Object;
        BuildingName = original.BuildingName;
        //if (original._size == Vector3.Zero) 
        //{ 
        //    CalculateSize();
        //}
        //else
        //{
        //    _size = original._size;  
        //}
        
    }

    public Node3D GetMeshObject(Material material)
    {
        var meshScene = Object.Instantiate();
        GameController.Singleton.AddChild(meshScene);
        if (meshScene is MeshContainer)
        {
            ((MeshContainer)meshScene).SetMaterialOverride(material);
            _size = ((MeshContainer)meshScene).Size;
        }
        GameController.Singleton.RemoveChild(meshScene);
        return meshScene as Node3D;
    }

    public Vector3 CalculateSize() // does nothing currently
    {
    //    if (_object != null)
    //    {
    //        var instance = _object.Instantiate();
    //        if (instance is Building)
    //            _size = ((Building)instance).Size;
    //    }
    //    else if (ObjectMesh!= null)
    //    {
    //        _size = ObjectMesh.GetAabb().Size;
    //    }
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