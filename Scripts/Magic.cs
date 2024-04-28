using Godot;
using System.Collections.Generic;

public partial class Magic : Node
{
    public enum PowerTypes { General, Light, Dark, Fire, Water }

    public static List<IMagicNode> magicNodes = new List<IMagicNode>();

    public static readonly float[,] MagicMultiplier =
    {
         {1,0.1f,0.1f,0.1f,0.1f },
         {0.1f,1,-1,0,0},
         {0.1f,-1,1,0,0},
         {0.1f,0,0,1,-1},
         {0.1f,0,0,-1,1}
    };

}
