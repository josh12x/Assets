using UnityEngine;
using System.Collections;
using LitJson;
using System.Collections.Generic;
using System.IO;

public class RecipeDatabase : MonoBehaviour 
{
    public Recipe[] recipeDatabase;
    private JsonData recipeData;

    void Awake()
    {
        recipeData = JsonMapper.ToObject (File.ReadAllText (Application.dataPath + "/StreamingAssets/Recipes.json"));
        recipeDatabase = new Recipe[recipeData.Count];
        ConstructRecipeDatabase();
    }

    public void PrintRecipes()
    {
        for (int x = 0; x < recipeDatabase.Length; x++)
        {
            if (recipeDatabase[x].componentCount > 1)
            {
            }
        }
    }

    public Recipe FetchByID(int id)
    {
        if (id > recipeDatabase.Length) 
        {
            return null;
        } 

        else 
        {
            return recipeDatabase[id];
        }
    }

    void ConstructRecipeDatabase()
    {
        for (int i = 0; i < recipeData.Count; i++) 
        {
            RecipeComponent[] componentsToAdd;
            componentsToAdd = figureOutComponentCount(i);
            recipeDatabase[i] = new Recipe
                (
                    (int)recipeData[i]["ID"],
                    (string)recipeData[i]["Name"],
                    (int)recipeData[i]["ComponentCount"],
                    componentsToAdd,
                    (int)recipeData[i]["Result"],
                    (int)recipeData[i]["ResultAmount"]
                );
        }
    }

    public RecipeComponent[] figureOutComponentCount(int recipeNumber)
    {
        RecipeComponent[] thingToReturn;
        thingToReturn = new RecipeComponent[(int)recipeData[recipeNumber]["ComponentCount"]];

        //lets say the list has 2 components

        for (int x = 0; x < (int)recipeData[recipeNumber]["ComponentCount"]; x++)
        {
            thingToReturn[x].componentType = (ComponentType)System.Enum.Parse(typeof(ComponentType), (string)recipeData[recipeNumber][x + "ComponentType"]);
            thingToReturn[x].Amount = (int)recipeData[recipeNumber][x + "ComponentAmount"];
        }
        return thingToReturn;
    }
}
