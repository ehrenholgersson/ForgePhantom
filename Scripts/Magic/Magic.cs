using Godot;
using System.Collections.Generic;


public partial class Magic : Node
{
    public enum PowerTypes { General, Light, Dark, Fire, Water }

    public static Color[] PowerColors = new Color[] { new Color(1, 1, 1, 1), new Color(0.79f, 0.79f, 0.35f, 1), new Color(0.39f,0.39f,1,1), new Color(1,0.27f,0.13f,1), new Color(0.13f,0.27f,1,1) };

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
