using UnityEngine;
using System.Collections;
using LitJson;
using System.Collections.Generic;
using System.IO;

public class ItemDatabase : MonoBehaviour 
{
    //a list of all items that we specify in a json document and that is created at game start
    public List<Item> database = new List<Item>();

    //a variable/container for any read data from json
	private JsonData itemData;

    //singleton Principle to allow for universal access to this class
    static ItemDatabase instance;

    //need to get back to this
	public Item test;

	void Awake()
	{
        //set up our singleton instance/get the reference to our self
        instance = this;

        //we get what our inventory database actually is from reading a json file we create
		itemData = JsonMapper.ToObject (File.ReadAllText (Application.dataPath + "/StreamingAssets/Items.json"));

        //the method we use to create the database using that json data, convertion it between json, to a unity list
		ConstructItemDatabase ();
	}


    //get an item based on the id we request 
	public static Item FetchByID(int id)
	{
		if (id > instance.database.Count) 
        {
			return null;
		} 

		else 
		{
            return instance.database[id];
		}
	}

    //construct the database using the information/data we have from the json file
	void ConstructItemDatabase()
	{
        //for every item that is in the json data/every seperate thing
        for (int i = 0; i < itemData.Count; i++) 
		{
            //the item we start with, every item has basic information, we fill that in first, then add any extra.
            Item baseItem;

            ComponentTypeAndAmount[] tempList = new ComponentTypeAndAmount[1];
            int componentCount = 0;

            if (itemData[i].Keys.Contains("ComponentCount"))
            {
                componentCount = (int)itemData[i]["ComponentCount"];
                tempList = new ComponentTypeAndAmount[componentCount];
                for (int c = 0; c < componentCount; c++)
                {
                    tempList[c].thisComponent = (ComponentType)System.Enum.Parse(typeof(ComponentType), (string)itemData[i][c + "ComponentType"]);
                    tempList[c].amountOfThisComponent = (int)itemData[i][c + "ComponentAmount"];
                }
            }

            else
            {
                componentCount = 0;
                tempList[0].thisComponent = ComponentType.Nothing;
                tempList[0].amountOfThisComponent = (int)itemData[i][0];
            }

            //fill out basic information from the json data
            baseItem =
                new Item
                (
                    (int)itemData[i]["Type"],
                    (int)itemData[i]["ID"],
                    (string)itemData[i]["Title"],
                    (string)itemData[i]["SpriteName"],
                    (string)itemData[i]["DropModel"],
                    (int)itemData[i]["Value"],
                    (int)itemData[i]["StackSize"],
                    (int)itemData[i]["MaxStackSize"],
                    (string)itemData[i]["Description"],
                    componentCount,
                    tempList
                );
			
            //if the json data item type is 0, then its nothing special, the base info is all we really actually need
			if ((int)itemData[i]["Type"] == 0) 
			{
				database.Add (baseItem);
			}

			//if its a melee item, then we need more data, lets add that onto the base item, then add it to the database
			if((int)itemData[i]["Type"] == 1)
			{
				Melee conversion = new Melee ();
				conversion.ID = baseItem.ID;
				conversion.Title = baseItem.Title;
                conversion.SpriteName = baseItem.SpriteName;
                conversion.DropModel = baseItem.DropModel;
				conversion.Type = baseItem.Type;
				conversion.StackSize = baseItem.StackSize;
				conversion.MaxStackSize = baseItem.MaxStackSize;
				conversion.Description = baseItem.Description;

                conversion.ComponentCount = componentCount;
                conversion.componentsHave = tempList;

				conversion.Value = baseItem.Value;

                //the extra data we need for a melee weapons
                //equip prefab
                conversion.equipPrefab = (string)itemData[i]["EquipPrefab"];

                //how far away a weapon can attack
                conversion.range = (double)itemData[i]["Range"];

                //how fast can we attack
                conversion.attackSpeed = (double)itemData[i]["AttackSpeed"];

                conversion.dmgProperties = new DamageType();

                //what type and how much damage does this weapon do
                if (itemData[i].Keys.Contains("MainDamageCount"))
                {
                    conversion.dmgProperties.primaryDamageTypes = new PrimaryDamageType[(int)itemData[i]["MainDamageCount"]] as PrimaryDamageType[];

                    conversion.dmgProperties.primaryDamageAmounts = new double[(int)itemData[i]["MainDamageCount"]];

                    int currentIndex = 0;
                    //if it has the keyword blunt, add blunt damage to the list of damage we can do
                    if (itemData[i].Keys.Contains("Melee"))
                    {
                        conversion.dmgProperties.primaryDamageTypes[currentIndex] = PrimaryDamageType.Melee;
                        conversion.dmgProperties.primaryDamageAmounts[currentIndex] = (double)itemData[i]["Melee"];
                        currentIndex += 1;
                    }

                    if (itemData[i].Keys.Contains("Magic"))
                    {
                        conversion.dmgProperties.primaryDamageTypes[currentIndex] = PrimaryDamageType.Magic;
                        conversion.dmgProperties.primaryDamageAmounts[currentIndex] = (double)itemData[i]["Magic"];
                        currentIndex += 1;
                    }

                    if (itemData[i].Keys.Contains("Ranged"))
                    {
                        conversion.dmgProperties.primaryDamageTypes[currentIndex] = PrimaryDamageType.Ranged;
                        conversion.dmgProperties.primaryDamageAmounts[currentIndex] = (double)itemData[i]["Ranged"];
                        currentIndex += 1;
                    }
                }

                if (itemData[i].Keys.Contains("AdditonalDamageCount"))
                {
                    conversion.dmgProperties.thisAdditonalDamageTypes = new AdditionalDamageType[(int)itemData[i]["AdditonalDamageCount"]];

                    conversion.dmgProperties.additonalDamageAmounts = new double[(int)itemData[i]["AdditonalDamageCount"]];
                }


                //add the melee item to the database now that we fill out the important information from the json data we read from
                database.Add (conversion);
			}
		}
	}
}


//the base class every item starts with
[System.Serializable]
public class Item
{
    [SerializeField]
	public int Type;

    [SerializeField]
	public int ID;

    [SerializeField]
	public string Title;

    [SerializeField]
    public string SpriteName;

    [SerializeField]
    public string DropModel;

    [SerializeField]
	public int Value;

    [SerializeField]
	public int StackSize;

    [SerializeField]
	public int MaxStackSize;

    [SerializeField]
	public string Description;

    //used to determine how many component we need to look for later on when searching/utilzing reading and wrting formats such as json
    [SerializeField]
    public int ComponentCount;

    [SerializeField]
    public ComponentTypeAndAmount[] componentsHave;

    public Item(int _type, int _id, string _title, string _spriteName, string _dropModel, int _val, int _stackSize, int _maxStack, string _des, int _componentCount, ComponentTypeAndAmount[] _componentsHave)
	{
		this.Type = _type;
		this.ID = _id;
		this.Title = _title;
        this.SpriteName = _spriteName;
        this.DropModel = _dropModel;
		this.Value = _val;
		this.StackSize = _stackSize;
		this.MaxStackSize = _maxStack;
		this.Description = _des;
        this.ComponentCount = _componentCount;
        this.componentsHave = _componentsHave;
	}

    public Item(Item duplicate)
    {
        this.Type = duplicate.Type;
        this.ID = duplicate.ID;
        this.Title = duplicate.Title;
        this.SpriteName = duplicate.SpriteName;
        this.DropModel = duplicate.DropModel;
        this.Value = duplicate.Value;
        this.StackSize = duplicate.StackSize;
        this.MaxStackSize = duplicate.MaxStackSize;
        this.Description = duplicate.Description;
        this.ComponentCount = duplicate.ComponentCount;
        this.componentsHave = duplicate.componentsHave;
    }

	public Item()
	{
		this.ID = -1;
	}

    public virtual string[] constructToolTip()
    {
        string[] description = new string[2];

        description[0] = Title;

        description[1] = Description + "\n\f" + "Sell Value : " + Value.ToString();
        return description;
    }
}

public class Melee : Item
{
    [SerializeField]
    public DamageType dmgProperties;

    [SerializeField]
    public string equipPrefab;

    [SerializeField]
    public double range;

    [SerializeField]
    public double attackSpeed;

    [SerializeField]
    public double knockback;

    public Melee (int _type, int _id, string _title, string _spriteName, string _dropModel, int _val, int _stackSize, int _maxStack, string _des, int _componentCount, ComponentTypeAndAmount[] _componentsHave, DamageType _dmgProperties, string _equipPrefab, double _range, double _attackSpeed, double _knockback) : base (_type, _id, _title, _spriteName, _dropModel, _val, _stackSize, _maxStack, _des, _componentCount, _componentsHave)
	{
        dmgProperties = _dmgProperties;
        equipPrefab = _equipPrefab;
        range = _range;
        attackSpeed = _attackSpeed;
        knockback = _knockback;
	}

    public Melee(Melee duplicate)
    {
        this.Type = duplicate.Type;
        this.ID = duplicate.ID;
        this.Title = duplicate.Title;
        this.SpriteName = duplicate.SpriteName;
        this.DropModel = duplicate.DropModel;
        this.Value = duplicate.Value;
        this.StackSize = duplicate.StackSize;
        this.MaxStackSize = duplicate.MaxStackSize;
        this.Description = duplicate.Description;

        this.ComponentCount = duplicate.ComponentCount;
        this.componentsHave = duplicate.componentsHave;

        this.dmgProperties = duplicate.dmgProperties;
        this.equipPrefab = duplicate.equipPrefab;
        this.range = duplicate.range;
        this.attackSpeed = duplicate.attackSpeed;
        this.knockback = duplicate.knockback;
    }

	public Melee()
	{
		
	}

    public override string[] constructToolTip()
    {
        string[] desc = new string[2];

        desc[0] = Title;

        desc[1] = Description + "\n\f" + "Sell Value : " + Value.ToString();

        return desc;
    }

}

public enum ComponentType
{
    Nothing,
    Wood,
    Stone,
    String,
    NumberOfTypes
}

//a qucik struct holding 2 variables for both a component type and how much of it.
public struct ComponentTypeAndAmount
{
    public ComponentType thisComponent;
    public int amountOfThisComponent;

    public ComponentTypeAndAmount( ComponentType _thisComponent, int _amountOfThisComponent)
    {
        thisComponent = _thisComponent;
        amountOfThisComponent = _amountOfThisComponent;
    }

}

//ItemType
//0 = default
//1 = melee