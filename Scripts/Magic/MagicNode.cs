using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public abstract partial class MagicNode : Node3D
{
    protected static List<MagicNode> _allNodes = new List<MagicNode>();
    protected List<List<MagicNode>> _powerPaths = new List<List<MagicNode>>();
    protected List<MagicNode> _connectedNodes = new List<MagicNode>();
    protected List<MagicNode> _powerSources = new List<MagicNode>();
    protected float _powerLevel = 0;
    protected Building _parent;
    public float PowerLevel { get => _powerLevel; }
    [Export] protected float _maxRange = 20;
    [Export]protected Magic.PowerTypes _powerType;
    public Magic.PowerTypes PowerType { get => _powerType; }

    public float PowerState { get; }

    public override void _Ready()
    {
        _allNodes.Add(this);
        var parent = GetParent();
        if (parent is BuildingNode)
        {
            _parent = (parent as BuildingNode).Building;
        }
    }


    protected int DrawPower(float request)
    {
        return 0;
    }
    
    protected async void CheckNodeConnection()
    {
        
        while (this.IsInsideTree())
        {
            await Task.Delay(1000);

            bool changed = false;
            // check nodes
            foreach (MagicNode mNode in _allNodes)
            {
                if (mNode == this)
                {
                    continue;
                }
                // check in range
                if (mNode.GlobalPosition.DistanceTo(this.GlobalPosition) < mNode._maxRange * mNode._powerLevel * Mathf.Abs(Magic.MagicMultiplier[(int)_powerType, (int)mNode._powerType]))
                {
                    if (mNode is MagicSource)
                    {
                        if (!_connectedNodes.Contains(mNode))
                        {
                            _connectedNodes.Add(mNode);
                            changed = true;
                        }
                        continue;
                    }

                    // need to ensure we are "down the chain" from the node in question 
                    foreach (List<MagicNode> mNodePath in mNode._powerPaths)
                    {
                        if (mNodePath.Contains(this))
                        {
                            continue;
                        }
                        if (_powerPaths.Count > 0)
                        {
                            if (!(mNodePath.Intersect(_connectedNodes).Count() > 0))
                            {
                                if (!_connectedNodes.Contains(mNode))
                                {
                                    _connectedNodes.Add(mNode);
                                    changed = true;
                                }
                                goto NEXTNODE;
                            }
                        }
                        else
                        {
                            _connectedNodes.Add(mNode);
                            changed = true;
                            goto NEXTNODE;
                        }
                    }
                }
                if (_connectedNodes.Contains(mNode))
                {
                    _connectedNodes.Remove(mNode); // 
                    changed = true;
                }
            NEXTNODE:;
            }
            if (changed)
            {
                _powerPaths.Clear();
                _powerSources.Clear();
                //_maxRange = 0;
                foreach (MagicNode mNode in _connectedNodes)
                {
                    if (mNode is MagicSource)
                    {
                        _powerPaths.Add(new List<MagicNode> { mNode });
                    }
                    else
                    {
                        foreach (List<MagicNode> path in mNode._powerPaths)
                        {
                            List<MagicNode> newPath = new List<MagicNode>();
                            newPath.Add(mNode);
                            newPath.AddRange(path); // this will create some duplicate entries, don't think its a problem
                            _powerPaths.Add(newPath);
                        }
                    }
                    _powerSources = _powerSources.Union(mNode._powerSources).ToList();
                }
            }
            
        }
    }
}
