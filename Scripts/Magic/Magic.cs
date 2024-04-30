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

struct MagicPower
{
    float _fireWater;
    float _lightDark;
    float _general;

    public float Light { get => Mathf.Clamp(_lightDark, 0, 1) + General * Magic.MagicMultiplier[(int)Magic.PowerTypes.General, (int)Magic.PowerTypes.Light]; }
    public float Dark { get => Mathf.Clamp(-_lightDark, 0, 1) + General * Magic.MagicMultiplier[(int)Magic.PowerTypes.General, (int)Magic.PowerTypes.Dark]; }
    public float Fire { get => Mathf.Clamp(_fireWater, 0, 1) + General * Magic.MagicMultiplier[(int)Magic.PowerTypes.General, (int)Magic.PowerTypes.Fire]; }
    public float Water { get => Mathf.Clamp(-_fireWater, 0, 1) + General * Magic.MagicMultiplier[(int)Magic.PowerTypes.General, (int)Magic.PowerTypes.Fire]; }

    public float General
    {
        get
        {
            var value = _general;
            value += Mathf.Abs(_lightDark) * ((_lightDark > 0) ? Magic.MagicMultiplier[(int)Magic.PowerTypes.General, (int)Magic.PowerTypes.Light] : Magic.MagicMultiplier[(int)Magic.PowerTypes.General, (int)Magic.PowerTypes.Dark]);
            value += Mathf.Abs(_fireWater) * ((_fireWater > 0) ? Magic.MagicMultiplier[(int)Magic.PowerTypes.General, (int)Magic.PowerTypes.Fire] : Magic.MagicMultiplier[(int)Magic.PowerTypes.General, (int)Magic.PowerTypes.Water]);
            return value;
        }
    }

    public static MagicPower operator + (MagicPower a, MagicPower b)
    {
        MagicPower result = new MagicPower();
        result._general = a._general + b._general;
        result._lightDark = a._lightDark + b._lightDark;
        result._fireWater = a._fireWater + b._fireWater;
        return result;
    }

    public void Clear()
    {
        _fireWater = 0;
        _lightDark = 0;
        _general = 0;
    }

    public float GetByType(Magic.PowerTypes type)
    {
        switch (type)
        {
            case Magic.PowerTypes.General:
                return General;

            case Magic.PowerTypes.Light:
                return Light;

            case Magic.PowerTypes.Dark:
                return Dark;

            case Magic.PowerTypes.Fire:
                return Fire;

            case Magic.PowerTypes.Water:
                return Water;
        }
        return 0;
    }

    public void Add(float amount, Magic.PowerTypes type)
    {
        switch (type)
        {
            case Magic.PowerTypes.General:
                _general += amount;
                break;

            case Magic.PowerTypes.Light:
                _lightDark += amount;
                break;

            case Magic.PowerTypes.Dark:
                _lightDark -= amount;
                break;

            case Magic.PowerTypes.Fire:
                _fireWater += amount;
                break;

            case Magic.PowerTypes.Water:
                _fireWater -= amount;
                break;               
            }
    }
}
