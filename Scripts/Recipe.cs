using Godot;
using System;

[GlobalClass]
public partial class Recipe : Resource
{
    [Export]CollectableResource[] _ingredients;
    string[] _ingredientStrings;
    [Export]CollectableResource _result;
    [Export] float _time;

    public float Time { get => _time; }
    public CollectableResource Result { get => _result; }

    public String[] Ingredients { get => GetIngredientStrings(); }

    string[] GetIngredientStrings()
    {
        var result = new string[_ingredients.Length];
        int i = 0;
        foreach (CollectableResource res in _ingredients)
        {
            result[i] = res.Type;
            i++;
        }
        return result;
    }
}
