using UnityEngine;
using System.Collections;

[System.Serializable]
public class Recipe 
{
    public int ID;
    public string name;
    public RecipeComponent[] components;

    public int componentCount;

    public int resultID;
    public int resultAmount;

    public Recipe(int _id, string _name, int _componentCount, RecipeComponent[] _components, int _resultID, int _resultAmount)
    {
        ID = _id;
        name = _name;
        componentCount = _componentCount;
        components = _components;
        resultID = _resultID;
        resultAmount = _resultID;
    }
}

[System.Serializable]
public struct RecipeComponent
{
    public ComponentType componentType;
    public int Amount;

    public RecipeComponent(ComponentType _componentType, int _Amount)
    {
        componentType = _componentType;
        Amount = _Amount;
    }
}
