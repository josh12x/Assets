using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.IO;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using System;


public class LevelDataSavingLoading : MonoBehaviour
{

    public static LevelDataSavingLoading thisInstance;

    public Transform parentListContainers;

    public Transform parentListDroppedItems;

    public Transform parentListHarvestable;

    public Transform parentWorkbench;

    public string currentSaveLoadedPath;

    public string currentSaveLoadedEndName;

    public string currentScene;

    public Transform parentSaveUISlots;

    public GameObject saveSlotPrefab;

    public Text currentSaveUIShowing;

    string[] allSaves;

    public Transform[] uiWindows;

    StringBuilder sbSaveTransforms;
    JsonWriter writerSaveTransforms;

    void Start ()
    {
        thisInstance = this;

        UpdateListOfSaves();

        parentListDroppedItems = GameObject.FindGameObjectWithTag("LevelDrops").transform;
	}

    void Update()
    {
        //if we are hovering, hover type is save, and we left click, then we should load that save.
        if (Inventory._InvRef.isOpen && Inventory._InvRef.isHovering && Inventory._InvRef.currentHoverType == HoverType.Saves)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ChangeHoveringSaveSelection(Inventory._InvRef.hoverIndex);
            }
        }
    }

    public void CreateNewSaveWithName(string saveName)
    {
        //create a new save/folder using a string from this method.
        //if it doesnt exist, then create it.
        if (!Directory.Exists(Application.dataPath + "/StreamingAssets/Saves/" + saveName))
        {
            Directory.CreateDirectory(Application.dataPath + "/StreamingAssets/Saves/" + saveName);

            //set our current save that is loaded, to be the one we created with the data path to the folder.
            currentSaveLoadedPath = Application.dataPath + "/StreamingAssets/Saves/" + saveName;

            //get just the name of the folder
            currentSaveLoadedEndName = Path.GetFileName(Application.dataPath + "/StreamingAssets/Saves/" + saveName);

            currentSaveUIShowing.text = currentSaveLoadedEndName;
        }

        UpdateListOfSaves();
    }

    public void UpdateListOfSaves()
    {
        //this saves an array of strings that hold the data paths for every single folder inside the saves folder, it does not inculde any extensions.
        allSaves = Directory.GetDirectories(Application.dataPath + "/StreamingAssets/Saves/");

        Debug.Log(allSaves[0]);


        //we clear the user interface list of saves.
        for (int i = 0; i < parentSaveUISlots.childCount; i++)
        {
            Destroy(parentSaveUISlots.GetChild(i).gameObject);
        }

        //we create the user interface block of saves.
        for (int i = 0; i < allSaves.Length; i++)
        {
            GameObject clone;
            clone = Instantiate(saveSlotPrefab, parentSaveUISlots);
            clone.GetComponent<Slot>().index = i;
            clone.GetComponent<Slot>().thisHoverType = HoverType.Saves;
            clone.GetComponent<Slot>().stackText.text = Path.GetFileName(allSaves[i]);
        }
    }

    public void ChangeHoveringSaveSelection(int savePathIndex)
    {
        //set our current loaded save, to whatever index we were hovering over on the inventory slots.
        currentSaveLoadedPath = allSaves[savePathIndex];

        currentSaveLoadedEndName = Path.GetFileName(allSaves[savePathIndex]);

        currentSaveUIShowing.text = currentSaveLoadedEndName;
    }

    public void Save()
    {
        if (currentSaveLoadedPath != null)
        {
            SaveChildrenTransforms(parentListContainers, Application.dataPath + "/StreamingAssets/Saves/" + currentSaveLoadedEndName + "/" + currentScene + "/LevelContainersSave.json");

            SaveChildrenTransforms(parentListDroppedItems, Application.dataPath + "/StreamingAssets/Saves/" + currentSaveLoadedEndName + "/" + currentScene + "/LevelItemDrops.json");

            SaveChildrenTransforms(parentListHarvestable, Application.dataPath + "/StreamingAssets/Saves/" + currentSaveLoadedEndName + "/" + currentScene + "/LevelHarvestables.json");

            Inventory._InvRef.SaveInventory(Application.dataPath + "/StreamingAssets/Saves/" + currentSaveLoadedEndName + "/Inventory.json");

            SavePlayerData();
        }
    }

    public void Load()
    {
        //load the containers.

        LoadDroppedItems(currentSaveLoadedPath);

        LoadHarvestables(currentSaveLoadedPath);

        LoadContainers(currentSaveLoadedPath);

        //then whatever our inventory was.
        Inventory._InvRef.LoadInventory(Application.dataPath + "/StreamingAssets/Saves/" + currentSaveLoadedEndName + "/Inventory.json");

        LoadPlayerData();

        Inventory._InvRef.UpdateHotBarEquipped();

        Inventory._InvRef.ToggleAllWindows(false);

        for (int i = 0; i < parentWorkbench.childCount; i++)
        {
            if (parentWorkbench.GetChild(i).GetComponentInChildren<Workbench>() != null)
            {
                parentWorkbench.GetChild(i).GetComponentInChildren<Workbench>().OnLoad();
            }
        }
    }

    public void SaveChildrenTransforms(Transform parent, string SaveName)
    {
        sbSaveTransforms = new StringBuilder();
        writerSaveTransforms = new JsonWriter(sbSaveTransforms);
        SaveableItem currentCycledSavableItem;
        Transform currentCycledChild;
        writerSaveTransforms.WriteArrayStart();

        for (int i = 0; i < parent.childCount; i++)
        {
            currentCycledSavableItem = parent.GetChild(i).GetComponent<SaveableItem>();
            currentCycledChild = parent.GetChild(i);

            writerSaveTransforms.WriteObjectStart();

            writerSaveTransforms.WritePropertyName("posX");
            writerSaveTransforms.Write(currentCycledChild.transform.position.x);

            writerSaveTransforms.WritePropertyName("posY");
            writerSaveTransforms.Write(currentCycledChild.transform.position.y);

            writerSaveTransforms.WritePropertyName("posZ");
            writerSaveTransforms.Write(currentCycledChild.transform.position.z);

            writerSaveTransforms.WritePropertyName("rotX");
            writerSaveTransforms.Write(currentCycledChild.transform.rotation.x);

            writerSaveTransforms.WritePropertyName("rotY");
            writerSaveTransforms.Write(currentCycledChild.transform.rotation.y);

            writerSaveTransforms.WritePropertyName("rotZ");
            writerSaveTransforms.Write(currentCycledChild.transform.rotation.z);

            writerSaveTransforms.WritePropertyName("rotW");
            writerSaveTransforms.Write(currentCycledChild.transform.rotation.w);

            if (currentCycledSavableItem.thisSaveType == SaveableItemType.DroppedItem)
            {

                AddOnClick reference;
                reference = currentCycledChild.GetComponent<AddOnClick>();

                writerSaveTransforms.WritePropertyName("prefabName");
                writerSaveTransforms.Write(reference.thisItem.DropModel);


                SaveItemAsJsonData(reference.thisItem, "");

                writerSaveTransforms.WriteObjectEnd();
            }

            else if (currentCycledSavableItem.thisSaveType == SaveableItemType.Container)
            {

                writerSaveTransforms.WritePropertyName("prefabName");
                writerSaveTransforms.Write(currentCycledSavableItem.prefabName);

                Container conversion;
                conversion = currentCycledSavableItem as Container;

                writerSaveTransforms.WritePropertyName("containerSpace");
                writerSaveTransforms.Write(conversion.containerSpace);

                for (int x = 0; x < conversion.containerSpace; x++)
                {
                    SaveItemAsJsonData(conversion.containerContents[x], x.ToString());
                }

                for (int c = 0; c < conversion.containerColors.Length; c++)
                {
                    writerSaveTransforms.WritePropertyName(c + "ColorR");
                    writerSaveTransforms.Write(conversion.containerColors[c].r);

                    writerSaveTransforms.WritePropertyName(c + "ColorB");
                    writerSaveTransforms.Write(conversion.containerColors[c].b);

                    writerSaveTransforms.WritePropertyName(c + "ColorG");
                    writerSaveTransforms.Write(conversion.containerColors[c].g);


                    writerSaveTransforms.WritePropertyName(c + "ColorA");
                    writerSaveTransforms.Write(conversion.containerColors[c].a);
                }

                writerSaveTransforms.WritePropertyName("Container Name");
                writerSaveTransforms.Write(conversion.containerName);

                writerSaveTransforms.WriteObjectEnd();
            }

            else if (currentCycledSavableItem.thisSaveType == SaveableItemType.Harvestable)
            {
                writerSaveTransforms.WritePropertyName("prefabName");
                writerSaveTransforms.Write(currentCycledSavableItem.prefabName);

                writerSaveTransforms.WriteObjectEnd();
            }
            else
            {
                Debug.Log("There wasn't a proper save type given");
            }
        }

        writerSaveTransforms.WriteArrayEnd();

        ///write all the data down as a custom text file to the asset datapath.
        File.WriteAllText(SaveName, sbSaveTransforms.ToString());
    }

    public void SavePlayerData()
    {
        StringBuilder sb = new StringBuilder();
        JsonWriter writer = new JsonWriter(sb);

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        

        writer.WriteArrayStart();

        writer.WriteObjectStart();

        writer.WritePropertyName("PlayerPosX");
        writer.Write(player.transform.position.x);

        writer.WritePropertyName("PlayerPosY");
        writer.Write(player.transform.position.y);

        writer.WritePropertyName("PlayerPosZ");
        writer.Write(player.transform.position.z);

        writer.WritePropertyName("PlayerRotX");
        writer.Write(player.transform.rotation.x);

        writer.WritePropertyName("PlayerRotY");
        writer.Write(player.transform.rotation.y);

        writer.WritePropertyName("PlayerRotZ");
        writer.Write(player.transform.rotation.z);

        writer.WritePropertyName("PlayerRotW");
        writer.Write(player.transform.rotation.w);


        writer.WritePropertyName("CameraPosX");
        writer.Write(Camera.main.transform.position.x);

        writer.WritePropertyName("CameraPosY");
        writer.Write(Camera.main.transform.position.y);

        writer.WritePropertyName("CameraPosZ");
        writer.Write(Camera.main.transform.position.z);

        writer.WritePropertyName("CameraRotX");
        writer.Write(Camera.main.transform.rotation.x);

        writer.WritePropertyName("CameraRotY");
        writer.Write(Camera.main.transform.rotation.y);

        writer.WritePropertyName("CameraRotZ");
        writer.Write(Camera.main.transform.rotation.z);

        writer.WritePropertyName("CameraRotW");
        writer.Write(Camera.main.transform.rotation.w);

        writer.WriteObjectEnd();

        for (int i = 0; i < uiWindows.Length; i++)
        {

            writer.WriteObjectStart();

            writer.WritePropertyName("UIX");
            writer.Write(uiWindows[i].transform.position.x);

            writer.WritePropertyName("UIY");
            writer.Write(uiWindows[i].transform.position.y);

            writer.WriteObjectEnd();

        }

        writer.WriteArrayEnd();

        ///write all the data down as a custom text file to the asset datapath.
        File.WriteAllText(Application.dataPath + "/StreamingAssets/Saves/" + currentSaveLoadedEndName + "/PlayerInformation.json", sb.ToString());
    }

    public void SaveItemAsJsonData(Item itemToSave, string namePrefix)
    {
        writerSaveTransforms.WritePropertyName(String.Format("{0}Type", namePrefix));
        writerSaveTransforms.Write(itemToSave.Type);

        writerSaveTransforms.WritePropertyName(String.Format("{0}ID", namePrefix));
        writerSaveTransforms.Write(itemToSave.ID);

        //if its an empty spot, just cut out now, break out
        if (itemToSave.ID == -1)
        {
            return;
        }

        writerSaveTransforms.WritePropertyName(String.Format("{0}Title", namePrefix));
        writerSaveTransforms.Write(itemToSave.Title);

        writerSaveTransforms.WritePropertyName(String.Format("{0}SpriteName", namePrefix));
        writerSaveTransforms.Write(itemToSave.SpriteName);

        writerSaveTransforms.WritePropertyName(String.Format("{0}DropModel", namePrefix));
        writerSaveTransforms.Write(itemToSave.DropModel);

        writerSaveTransforms.WritePropertyName(String.Format("{0}Value", namePrefix));
        writerSaveTransforms.Write(itemToSave.Value);

        writerSaveTransforms.WritePropertyName(String.Format("{0}StackSize", namePrefix));
        writerSaveTransforms.Write(itemToSave.StackSize);

        writerSaveTransforms.WritePropertyName(String.Format("{0}MaxStackSize", namePrefix));
        writerSaveTransforms.Write(itemToSave.MaxStackSize);

        writerSaveTransforms.WritePropertyName(String.Format("{0}Description", namePrefix));
        writerSaveTransforms.Write(itemToSave.Description);

        //if its a equipable weapons, add on the associated variables
        if (itemToSave.Type == 1)
        {
            Melee item;
            item = itemToSave as Melee;

            writerSaveTransforms.WritePropertyName(String.Format("{0}EquipPrefab", namePrefix));
            writerSaveTransforms.Write(item.equipPrefab);

            writerSaveTransforms.WritePropertyName(String.Format("{0}Range", namePrefix));
            writerSaveTransforms.Write(item.range);

            writerSaveTransforms.WritePropertyName(String.Format("{0}AttackSpeed", namePrefix));
            writerSaveTransforms.Write(item.attackSpeed);

            writerSaveTransforms.WritePropertyName(String.Format("{0}MainDamageCount", namePrefix));
            writerSaveTransforms.Write(item.dmgProperties.primaryDamageCount);

            writerSaveTransforms.WritePropertyName(String.Format("{0}AdditonalDamageCount", namePrefix));
            writerSaveTransforms.Write(item.dmgProperties.additonalDamageCount);


            for (int x = 0; x < item.dmgProperties.primaryDamageTypes.Length; x++)
            {
                if (item.dmgProperties.primaryDamageTypes[x] == PrimaryDamageType.Melee)
                {
                    writerSaveTransforms.WritePropertyName(String.Format("{0}Melee", namePrefix));
                    writerSaveTransforms.Write(item.dmgProperties.primaryDamageAmounts[x]);
                }

                if (item.dmgProperties.primaryDamageTypes[x] == PrimaryDamageType.Magic)
                {
                    writerSaveTransforms.WritePropertyName(String.Format("{0}Magic", namePrefix));
                    writerSaveTransforms.Write(item.dmgProperties.primaryDamageAmounts[x]);
                }

                if (item.dmgProperties.primaryDamageTypes[x] == PrimaryDamageType.Ranged)
                {
                    writerSaveTransforms.WritePropertyName(String.Format("{0}Ranged", namePrefix));
                    writerSaveTransforms.Write(item.dmgProperties.primaryDamageAmounts[x]);
                }
            }
        }


        if (itemToSave.ComponentCount > 0)
        {
            writerSaveTransforms.WritePropertyName(String.Format("{0}ComponentCount", namePrefix));
            writerSaveTransforms.Write(itemToSave.ComponentCount);

            for (int c = 0; c < itemToSave.ComponentCount; c++)
            {
                writerSaveTransforms.WritePropertyName(String.Format("{0}{1}ComponentType", namePrefix, c));
                writerSaveTransforms.Write(itemToSave.componentsHave[c].thisComponent.ToString());

                writerSaveTransforms.WritePropertyName(String.Format("{0}{1}ComponentAmount", namePrefix, c));
                writerSaveTransforms.Write(itemToSave.componentsHave[c].amountOfThisComponent);
            }
        }

    }

    public Item LoadItemFromJsonData(JsonData dataToLoadFrom, string namePrefix)
    {
        Item baseItem;

        ComponentTypeAndAmount[] tempList = new ComponentTypeAndAmount[1];
        int componentCount = 0;

        if (dataToLoadFrom.Keys.Contains(String.Format("{0}ComponentCount", namePrefix)))
        {
            componentCount = (int)dataToLoadFrom[(String.Format("{0}ComponentCount", namePrefix))];
            tempList = new ComponentTypeAndAmount[componentCount];
            for (int c = 0; c < componentCount; c++)
            {
                tempList[c].thisComponent = (ComponentType)System.Enum.Parse(typeof(ComponentType), (string)dataToLoadFrom[(String.Format("{0}{1}ComponentType", namePrefix, c))]);
                tempList[c].amountOfThisComponent = (int)dataToLoadFrom[(String.Format("{0}{1}ComponentAmount", namePrefix, c))];
            }
        }

        else
        {
            componentCount = 0;
            tempList[0].thisComponent = ComponentType.Nothing;
            tempList[0].amountOfThisComponent = 0;
        }

        if ((int)dataToLoadFrom[String.Format("{0}ID", namePrefix)] == -1)
        {
            baseItem = new Item();
        }

        else
        {
            baseItem = new Item
            (
                (int)dataToLoadFrom[String.Format("{0}Type", namePrefix)],
                (int)dataToLoadFrom[String.Format("{0}ID", namePrefix)],
                (string)dataToLoadFrom[String.Format("{0}Title", namePrefix)],
                (string)dataToLoadFrom[String.Format("{0}SpriteName", namePrefix)],
                (string)dataToLoadFrom[String.Format("{0}DropModel", namePrefix)],
                (int)dataToLoadFrom[String.Format("{0}Value", namePrefix)],
                (int)dataToLoadFrom[String.Format("{0}StackSize", namePrefix)],
                (int)dataToLoadFrom[String.Format("{0}MaxStackSize", namePrefix)],
                (string)dataToLoadFrom[String.Format("{0}Description", namePrefix)],
                componentCount,
                tempList
            );
        }

        if ((int)dataToLoadFrom[String.Format("{0}Type", namePrefix)] == 0)
        {
            return baseItem;
        }

        else if ((int)dataToLoadFrom[String.Format("{0}Type", namePrefix)] == 1)
        {
            Melee conversion = new Melee();
            conversion.ID = baseItem.ID;
            conversion.Title = baseItem.Title;
            conversion.SpriteName = baseItem.SpriteName;
            conversion.DropModel = baseItem.DropModel;
            conversion.Type = baseItem.Type;
            conversion.StackSize = baseItem.StackSize;
            conversion.MaxStackSize = baseItem.MaxStackSize;
            conversion.Description = baseItem.Description;
            conversion.Value = baseItem.Value;

            conversion.ComponentCount = baseItem.ComponentCount;

            conversion.componentsHave = baseItem.componentsHave;

            conversion.equipPrefab = (string)dataToLoadFrom[String.Format("{0}EquipPrefab", namePrefix)];

            conversion.range = (double)dataToLoadFrom[String.Format("{0}Range", namePrefix)];

            conversion.attackSpeed = (double)dataToLoadFrom[String.Format("{0}AttackSpeed", namePrefix)];

            conversion.dmgProperties = new DamageType();

            conversion.dmgProperties.primaryDamageCount = (int)dataToLoadFrom[String.Format("{0}MainDamageCount", namePrefix)];

            conversion.dmgProperties.primaryDamageAmounts = new double[conversion.dmgProperties.primaryDamageCount];

            conversion.dmgProperties.primaryDamageTypes = new PrimaryDamageType[conversion.dmgProperties.primaryDamageCount];


            conversion.dmgProperties.additonalDamageCount = (int)dataToLoadFrom[String.Format("{0}AdditonalDamageCount", namePrefix)];

            conversion.dmgProperties.additonalDamageAmounts = new double[conversion.dmgProperties.primaryDamageCount];

            conversion.dmgProperties.thisAdditonalDamageTypes = new AdditionalDamageType[conversion.dmgProperties.primaryDamageCount];

            int index = 0;
            if (dataToLoadFrom.Keys.Contains(String.Format("{0}Melee", namePrefix)))
            {
                conversion.dmgProperties.primaryDamageAmounts[index] = (double)dataToLoadFrom[String.Format("{0}Melee", namePrefix)];
                conversion.dmgProperties.primaryDamageTypes[index] = PrimaryDamageType.Melee;
                index += 1;
            }

            if (dataToLoadFrom.Keys.Contains(String.Format("{0}Magic", namePrefix)))
            {
                conversion.dmgProperties.primaryDamageAmounts[index] = (double)dataToLoadFrom[String.Format("{0}Magic", namePrefix)];
                conversion.dmgProperties.primaryDamageTypes[index] = PrimaryDamageType.Magic;
                index += 1;
            }

            if (dataToLoadFrom.Keys.Contains(String.Format("{0}Ranged", namePrefix)))
            {
                conversion.dmgProperties.primaryDamageAmounts[index] = (double)dataToLoadFrom[String.Format("{0}Ranged", namePrefix)];
                conversion.dmgProperties.primaryDamageTypes[index] = PrimaryDamageType.Ranged;
                index += 1;
            }
            return conversion;
        }

        return null;
    }

    public void LoadContainers(string SaveName)
    {
        for (int i = 0; i < parentListContainers.childCount; i++)
        {
            Destroy(parentListContainers.GetChild(i).gameObject);
        }

        JsonData JsonToLoad = JsonMapper.ToObject(File.ReadAllText(Application.dataPath + "/StreamingAssets/Saves/" + currentSaveLoadedEndName + "/" + currentScene + "/LevelContainersSave.json"));

        for (int i = 0; i < JsonToLoad.Count; i++)
        {
            GameObject clone;
            clone = Instantiate(Resources.Load("DropModels/Containers/" + (string)JsonToLoad[i]["prefabName"], typeof(GameObject)) as GameObject, parentListContainers);

            double floatX = (double)JsonToLoad[i]["posX"];

            double floatY = (double)JsonToLoad[i]["posY"];

            double floatZ = (double)JsonToLoad[i]["posZ"];

            double floatRotX = (double)JsonToLoad[i]["rotX"];

            double floatRotY = (double)JsonToLoad[i]["rotY"];

            double floatRotZ = (double)JsonToLoad[i]["rotZ"];

            double floatRotW = (double)JsonToLoad[i]["rotW"];

            Vector3 savedPos = new Vector3((float)floatX, (float)floatY, (float)floatZ);

            Quaternion savedRot = new Quaternion((float)floatRotX, (float)floatRotY, (float)floatRotZ, (float)floatRotW);

            clone.transform.position = savedPos;

            clone.transform.rotation = savedRot;

            Container reference;
            reference = clone.GetComponent<Container>();
            reference.containerSpace = (int)JsonToLoad[i]["containerSpace"];

            reference.containerContents = new Item[reference.containerSpace];

            for (int x = 0; x < reference.containerSpace; x++)
            {
                reference.containerContents[x] = LoadItemFromJsonData(JsonToLoad[i], x.ToString());
            }

            for (int c = 0; c < reference.containerColors.Length; c++)
            {

                double colR = (double)JsonToLoad[i][c + "ColorR"];

                double colG = (double)JsonToLoad[i][c + "ColorG"];

                double colB = (double)JsonToLoad[i][c + "ColorB"];

                double colA = (double)JsonToLoad[i][c + "ColorA"];

                reference.containerColors[c].r = (float)colR;

                reference.containerColors[c].g = (float)colG;

                reference.containerColors[c].b = (float)colB;

                reference.containerColors[c].a = (float)colA;

            }

           reference.containerName = (string)JsonToLoad[i]["Container Name"];
        }
    }

    public void LoadHarvestables(string SaveName)
    {
        for (int i = 0; i < parentListHarvestable.childCount; i++)
        {
            Destroy(parentListHarvestable.GetChild(i).gameObject);
        }

        JsonData JsonToLoad = JsonMapper.ToObject(File.ReadAllText(Application.dataPath + "/StreamingAssets/Saves/" + currentSaveLoadedEndName + "/" + currentScene + "/LevelHarvestables.json"));

        for (int i = 0; i < JsonToLoad.Count; i++)
        {
            GameObject clone;
            clone = Instantiate(Resources.Load("Harvestable/" + (string)JsonToLoad[i]["prefabName"], typeof(GameObject)) as GameObject, parentListHarvestable);

            double floatX = (double)JsonToLoad[i]["posX"];

            double floatY = (double)JsonToLoad[i]["posY"];

            double floatZ = (double)JsonToLoad[i]["posZ"];

            double floatRotX = (double)JsonToLoad[i]["rotX"];

            double floatRotY = (double)JsonToLoad[i]["rotY"];

            double floatRotZ = (double)JsonToLoad[i]["rotZ"];

            double floatRotW = (double)JsonToLoad[i]["rotW"];

            Vector3 savedPos = new Vector3((float)floatX, (float)floatY, (float)floatZ);

            Quaternion savedRot = new Quaternion((float)floatRotX, (float)floatRotY, (float)floatRotZ, (float)floatRotW);

            clone.transform.position = savedPos;

            clone.transform.rotation = savedRot;
        }
    }

    public void LoadDroppedItems(string SaveName)
    {
        for (int i = 0; i < parentListDroppedItems.childCount; i++)
        {
            Destroy(parentListDroppedItems.GetChild(i).gameObject);
        }

        JsonData JsonToLoad = JsonMapper.ToObject(File.ReadAllText(Application.dataPath + "/StreamingAssets/Saves/" + currentSaveLoadedEndName + "/" + currentScene + "/LevelItemDrops.json"));

        for (int i = 0; i < JsonToLoad.Count; i++)
        {
            GameObject clone;
            clone = Instantiate(Resources.Load("DropModels/" + (string)JsonToLoad[i]["prefabName"], typeof(GameObject)) as GameObject, parentListDroppedItems);

            double floatX = (double)JsonToLoad[i]["posX"];

            double floatY = (double)JsonToLoad[i]["posY"];

            double floatZ = (double)JsonToLoad[i]["posZ"];

            double floatRotX = (double)JsonToLoad[i]["rotX"];

            double floatRotY = (double)JsonToLoad[i]["rotY"];

            double floatRotZ = (double)JsonToLoad[i]["rotZ"];

            double floatRotW = (double)JsonToLoad[i]["rotW"];

            Vector3 savedPos = new Vector3((float)floatX, (float)floatY, (float)floatZ);

            Quaternion savedRot = new Quaternion((float)floatRotX, (float)floatRotY, (float)floatRotZ, (float)floatRotW);

            clone.transform.position = savedPos;

            clone.transform.rotation = savedRot;

            AddOnClick reference = clone.GetComponent<AddOnClick>();

            reference.createItemAtStartFromDatabase = false;

            reference.DroppedItem = true;

            reference.thisItem = LoadItemFromJsonData(JsonToLoad[i], "");

            reference.Amnt = reference.thisItem.StackSize;
        }
    }

    public void LoadPlayerData()
    {
        JsonData JsonToLoad = JsonMapper.ToObject(File.ReadAllText(Application.dataPath + "/StreamingAssets/Saves/" + currentSaveLoadedEndName + "/PlayerInformation.json"));

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        double floatX = (double)JsonToLoad[0]["PlayerPosX"];

        double floatY = (double)JsonToLoad[0]["PlayerPosY"];

        double floatZ = (double)JsonToLoad[0]["PlayerPosZ"];

        double floatRotX = (double)JsonToLoad[0]["PlayerRotX"];

        double floatRotY = (double)JsonToLoad[0]["PlayerRotY"];

        double floatRotZ = (double)JsonToLoad[0]["PlayerRotZ"];

        double floatRotW = (double)JsonToLoad[0]["PlayerRotW"];

        Vector3 updatedPos = new Vector3((float)floatX, (float)floatY, (float)floatZ);

        Quaternion updatedPlayerRot = new Quaternion((float)floatRotX, (float)floatRotY, (float)floatRotZ, (float)floatRotW);

        player.transform.position = updatedPos;

        player.transform.rotation = updatedPlayerRot;



        double camfloatX = (double)JsonToLoad[0]["CameraPosX"];

        double camfloatY = (double)JsonToLoad[0]["CameraPosY"];

        double camfloatZ = (double)JsonToLoad[0]["CameraPosZ"];

        double camfloatRotX = (double)JsonToLoad[0]["CameraRotX"];

        double camfloatRotY = (double)JsonToLoad[0]["CameraRotY"];

        double camfloatRotZ = (double)JsonToLoad[0]["CameraRotZ"];

        double camfloatRotW = (double)JsonToLoad[0]["CameraRotW"];


        Vector3 updatedCamPos = new Vector3((float)camfloatX, (float)camfloatY, (float)camfloatZ);

        Quaternion updatedCamRot = new Quaternion((float)camfloatRotX, (float)camfloatRotY, (float)camfloatRotZ, (float)camfloatRotW);

        Camera.main.transform.position = updatedCamPos;
        Camera.main.transform.rotation = updatedCamRot;

        for (int i = 0; i < uiWindows.Length; i++)
        {
            //plus one is because of where we started to add the ui elements at, json data 0, is the player and camera pos/rot
            double UIFloatX = (double)JsonToLoad[i + 1]["UIX"];

            double UIFloatY = (double)JsonToLoad[i + 1]["UIY"];

            uiWindows[i].transform.position = new Vector3((float)UIFloatX, (float)UIFloatY, 0);

        }

    }
}
