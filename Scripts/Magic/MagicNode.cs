using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public abstract partial class MagicNode : Node3D
{
    protected static List<MagicNode> _allNodes;
    protected List<MagicNode> _powerPath;
    protected List<MagicNode> _connectedNodes;
    protected List<MagicNode> _powerSources;
    protected float _powerLevel = 0;
    protected float _maxRange = 20;
    [Export]protected Magic.PowerTypes _powerType;
    [Export] Mesh _mesh;
    StandardMaterial3D _material;
    
    public float PowerState { get; }
    public Magic.PowerTypes PowerType { get; }

    public override void _Ready()
    {
        _allNodes.Add(this);
        _material = _mesh.SurfaceGetMaterial(0).Duplicate() as StandardMaterial3D;
    }

    public override void _PhysicsProcess(double delta)
    {
        _powerLevel = 0;
        foreach (MagicNode mNode in _allNodes)
        {
            _powerLevel += mNode._powerLevel * Magic.MagicMultiplier[(int)_powerType, (int)mNode._powerType];
        }
        Mathf.Clamp(_powerLevel, 0, 1);
        _material.AlbedoColor = Magic.PowerColors[(int)_powerType] * Mathf.Min(0.5f, _powerLevel);
    }

    protected int DrawPower(float request)
    {
        return 0;
    }
    
    async void CheckNodeConnection()
    {
        
        while (this.IsInsideTree())
        {
            bool changed = false;
            // check nodes
            foreach (MagicNode mNode in _allNodes)
            {
                // check in range
                if (mNode.GlobalPosition.DistanceTo(this.GlobalPosition) > mNode._maxRange * mNode._powerLevel * Magic.MagicMultiplier[(int)_powerType, (int)mNode._powerType])
                {
                    // check we are not in their _powerPath
                    if (!mNode._powerPath.Contains(this))
                    {
                        if (!_connectedNodes.Contains(mNode))
                        {
                            _connectedNodes.Add(mNode);
                            changed = true;
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (_connectedNodes.Contains(mNode))
                {
                    _connectedNodes.RemoveAll(t => t is MagicNode); // 
                    changed = true;
                }
            }
            if (changed)
            {
                _powerPath.Clear();
                _powerSources.Clear();
                foreach (MagicNode mNode in _connectedNodes)
                {
                    _powerPath.Add(mNode);
                    _powerPath.AddRange(mNode._powerPath); // this will create some duplicate entries, don't think its a problem
                    _powerSources = _powerSources.Union(mNode._powerSources).ToList();
                }
            }
            await Task.Delay(5000);
        }
    }
}
