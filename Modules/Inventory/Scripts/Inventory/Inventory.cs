using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.IO;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using System;
using TMPro;

[RequireComponent(typeof(RecipeDatabase))]
public class Inventory : MonoBehaviour
{
    #region AllVariables
    //a reference to our database to use later
    RecipeDatabase recipeDataBaseRef;

    //the actual hard coded list of items we have in our inventory
    public Item[] inventory;

    //the list of slots/way we interact with the hard coded data
    public List<Slot> slots = new List<Slot>();


    [Header("Inventory References")]

    [Tooltip("The main transfrom that all dropped items/items in the world we be parented to, to then be saved and loaded on a per level basis")]
    public Transform itemLevelDataTransform;

    //the transform/parent that the prefab slots will be added to and then organised in a grid
    public Transform slotMaster;

    //the prefab for our slot/way to interact with the hard coded data
    public GameObject slotPrefab;

    //the canvas the inventory takes place on/global
    public Canvas invCanvas;

    //Used for containers, need more
    public ContainerManager containerMangerRef;

    //set up a reference to ourself so that any class can globally access us to add/remove items from inventory
    public static Inventory _InvRef;


    [Header("General Information")]
    public bool isOpen = true;

    //very important data i came up with to determine how things interact
    //what type of slot are we hovering over?
    //then get the item from the hard coded list and do stuff with it
    [Header("Hovering Information")]
    public bool isHovering = false;

    //the position in an array or list of the slot we are hovering over, 0 would be our first inventory slot
    public int hoverIndex;
    public Vector3 hovPosition;
    public HoverType currentHoverType;
    public UIWindow hoveringWindow;

    [Header("Description")]
    public Text descriptionText;
    public UIWindow descriptionWindow;

    //are we dragging an item, what are we dragging.
    [Header("Dragging Information")]
    public bool isDragging;

    public Item currentDraggedItem;

    //the transform that will move with the drag, follow the mouse, or highlight what we have selected with a controller.
    public Transform dragTransform;

    [Tooltip("Auto populate the inventory based on the number you set below? Grid layout recommended, otherwise simply set the slot transform master and place the predefined slot positions there")]
    [HideInInspector]
    public bool autoPopulate;

    [Tooltip("How big is our inventory going to be?")]
    [HideInInspector]
    public int inventorySize;

    [HideInInspector]
    public bool hasHotbar;

    [HideInInspector]
    public bool useRaycastInteractionMethod;

    [HideInInspector]
    public bool debugMode;


    #region Controller Variables

    [HideInInspector]
    public bool usingController;

    [HideInInspector]
    public Transform controllerItemSelectionMover;

    [HideInInspector]
    public int PixlesPerControllermovement;

    [HideInInspector]
    public GridLayoutGroup gridlayoutGroup;

    [HideInInspector]
    public UIinteractionCategory currentActiveWindowCategory;

    [HideInInspector]
    public KeyCode grabItemButton;
    #endregion Controller Variables

    [Header("Audio")]
    public AudioSource userInterfaceAudioSource;
    public AudioClip slotHoverNoise;
    public Transform pickUpSoundTransform;
    public AudioSource pickupSounds;

    [Header("HotbarStuff")]
    [HideInInspector]
    public Transform hotBarMaster;
    private int currentCycledHotBar;

    [Header("PlayerStuff")]
    //public Animator playerAnimator;
    //public Transform rightHand;
    public Transform playerTransform;

    [Header("Dropping Items")]
    [Tooltip("The shell object/blank empty gameobject that will be used to change the appearance to the dropped item using the mesh renderer")]
    public GameObject droppedObjectHighlight;

    public Transform droppedObjectHighlightRotationDirection;

    public Transform droppedObjectParent;
    public Material droppedObjectMaterialHighlight;
    public LayerMask maskForRay;
    public int droppedItemRotateState;


    [Header("Various Variables")]
    public float clickTimer;
    public float clickTimerActivate = .5f;

    public float objectInteractionRange;

    public Ray worldInteractionRay;
    public RaycastHit worldInteractionHit;


    #region RecipeVariables
    [Header("Recipe Stuff")]
    public Transform recipeSlotMaster;
    public List<Slot> RecipeSlots = new List<Slot>();
    public Recipe activeRecipe;
    public Recipe[] displayedListOfRecipes;
    public List<Recipe> ActiveListOfRecipes = new List<Recipe>();
    public InputField recipeSearchBar;
    public int recipesPerPage;
    public int currentNumberOfPages;
    public int currentPage;
    #endregion

    #region UIVariables
    [Header("Main UI Windows")]

    public Transform AllWindowsMaster;

    public Transform recipeWindowMaster;
    public Toggle recToggle;

    [Space(5)]
    public Transform inventoryWindowMaster;
    public Toggle invToggle;
    public bool isInvOpen;

    [Space(5)]
    public Transform saveWindowMaster;
    public Toggle saveToggle;

    [Space(5)]
    public Transform containerWindowMaster;
    public UIWindow UIWindowContainer;

    [Header("Interaction Wheel")]
    public GameObject UIInteractionWheelGameobject;
    public GameObject colorPickerWindow;
    public ColorPickerWindow colorPickerClass;
    public GameObject renamingWindow;
    public InputField renameWindowInput;
    #endregion

    #endregion AllVariables

    void Update()
    {

        if (clickTimer < clickTimerActivate)
        {
            clickTimer += 1 * Time.deltaTime;
        }

        DetectingPlayerInput();

        InspectHoveringItemPushingI();

        if (hasHotbar)
        {
            HotBarUpdate();
        }

        DraggingPhyscialItemUpdate();

        if (UIInteractionWheelGameobject != null)
        {
            UIInteractionWheelUpdate();
        }

        DescriptionUpdate();

        //is the inventory open right now?
        if (isOpen)
        {
            DroppingPhysicalItemUpdate();

            UIInteractionInputUpdate();

            HoveringRecipesInteractionsUpdate();
        }

        DraggedItemUpdate();
    }

    #region InitializationFunctions

    void Start()
    {
        //automagically get any attached referneces that we need.
        GetAttachedReferences();

        //do some of the inital setup of the inventory
        InitializeInventory();

        InitializeRecipes();
        if (hasHotbar)
        {
            UpdateHotBarEquipped();
        }

        if (usingController)
        {
            InitalizeController();
        }

        droppedObjectParent = GameObject.FindGameObjectWithTag("LevelDrops").transform;

        AddItem(ItemDatabase.FetchByID(0), 10);
        AddItem(ItemDatabase.FetchByID(0), 10);
        AddItem(ItemDatabase.FetchByID(1), 2);
    }

    void DraggedItemUpdate()
    {

        if (isDragging)
        {
            if (usingController)
            {
                dragTransform.position = slots[hoverIndex].transform.position;
            }
            else
            {
                dragTransform.position = Input.mousePosition;
            }
        }
    }

    void GetAttachedReferences()
    {
        _InvRef = this;
        //get any important classes attached to us automagicaly
        recipeDataBaseRef = GetComponent<RecipeDatabase>();

        containerMangerRef = GetComponent<ContainerManager>();
    }

    void InitializeInventory()
    {
        //if we do autopopulate
        if (autoPopulate)
        {
            inventory = new Item[inventorySize];
            if (debugMode)
            {
                Debug.Log("Inventory set to autopopulate, setting up now");
            }
            //create a new list of items with the size we define from the variable invsize

            //Do our hotbarstuff first, 10 items

            if (hasHotbar)
            {
                for (int i = 0; i < hotBarMaster.childCount; i++)
                {
                    slots.Add(hotBarMaster.GetChild(i).GetComponent<Slot>());
                    inventory[i] = new Item();
                    UpdateSlotUIAtIndex(i, slots.ToArray(), inventory);
                }

                //then add our inventory
                for (int x = 10; x < inventorySize; x++)
                {
                    GameObject clone;
                    clone = Instantiate(slotPrefab, slotMaster) as GameObject;
                    slots.Add(clone.GetComponent<Slot>());
                    slots[x].index = x;
                    inventory[x] = new Item();
                    UpdateSlotUIAtIndex(x, slots.ToArray(), inventory);
                }
            }

            else
            {
                //then add our inventory
                for (int x = 0; x < inventorySize; x++)
                {
                    GameObject clone;
                    clone = Instantiate(slotPrefab, slotMaster) as GameObject;
                    slots.Add(clone.GetComponent<Slot>());
                    slots[x].index = x;
                    inventory[x] = new Item();
                    UpdateSlotUIAtIndex(x, slots.ToArray(), inventory);
                }
            }
        }
        //need to get back to this, dont understand.
        //if we dont autopopulate
        else
        {
            if (debugMode)
            {
                Debug.Log("Inventory not set to autopopulate, setting up the slots already assigned");
            }

            inventory = new Item[getSlotCount()];

            //for each slot child we have, setup ourselves
            for (int x = 0; x < slotMaster.childCount; x++)
            {
                //GameObject clone;
                //clone = Instantiate(slotPrefab, slotMaster) as GameObject;
                if (slotMaster.GetChild(x).GetComponent<Slot>() != null)
                {
                    slots.Add(slotMaster.GetChild(x).GetComponent<Slot>());
                    slots[x].index = x;
                    inventory[x] = new Item();
                    UpdateSlotUIAtIndex(x, slots.ToArray(), inventory);
                }
            }

        }
    }

    void InitalizeController()
    {
        //controllerItemSelectionMover.SetParent(slotMaster);
        controllerItemSelectionMover.GetComponent<RectTransform>().position = slots[0].GetComponent<RectTransform>().position;
        currentActiveWindowCategory = UIinteractionCategory.Inventory;
    }

    public int getSlotCount()
    {
        int result = 0;
        for (int x = 0; x < slotMaster.childCount; x++)
        {
            if (slotMaster.GetChild(x).GetComponent<Slot>() != null)
            {
                result += 1;
            }
        }

        return result;
    }

    #endregion InitializationFunctions

    //the way we interact with the inventory.
    void DetectingPlayerInput()
    {
        if (useRaycastInteractionMethod)
        {
            worldInteractionRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(worldInteractionRay, out worldInteractionHit, 50, maskForRay))
            {
                if (clickTimer >= clickTimerActivate)
                {
                    if (Input.GetButton("PickupPhysicalItem"))
                    {
                        DeterminePhysicalObjectInteractedWith();
                    }

                    if (Input.GetButton("PickupUIItem"))
                    {
                        PickUpPhysicalDragItem();
                    }
                }
            }
        }

        if (Input.GetButtonDown("Cancel"))
        {
            ToggleAllWindows(false);
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button7))
        {
            Debug.Log("pressed start");

            isOpen = !isOpen;

            if (isOpen)
            {
                ToggleInventoryWindowDisplayed(true);
            }

            else
            {
                ToggleInventoryWindowDisplayed(false);
            }
        }

        if (usingController && clickTimer > clickTimerActivate && isOpen)
        {
            if (Input.GetAxis("DPadHorizontal") > .5f)
            {
                    controllerItemSelectionMover.transform.localPosition = GetclosestPosition(getPositionsOutOfSlots(slots.ToArray()), getBetweenPoint(controllerItemSelectionMover.localPosition, controllerItemSelectionMover.transform.localPosition + new Vector3(PixlesPerControllermovement, 0, 0), (PixlesPerControllermovement / 2 + 1)));
                clickTimer = 0;
            }

            if (Input.GetAxis("DPadHorizontal") < -.5f)
            {
                    controllerItemSelectionMover.transform.localPosition = GetclosestPosition(getPositionsOutOfSlots(slots.ToArray()), getBetweenPoint(controllerItemSelectionMover.localPosition, controllerItemSelectionMover.transform.localPosition - new Vector3(PixlesPerControllermovement, 0, 0), (PixlesPerControllermovement / 2 + 1)));
                clickTimer = 0;
            }

            if (Input.GetAxis("DPadVertical") > .5f)
            {
                    controllerItemSelectionMover.transform.localPosition = GetclosestPosition(getPositionsOutOfSlots(slots.ToArray()), getBetweenPoint(controllerItemSelectionMover.localPosition, controllerItemSelectionMover.transform.localPosition + new Vector3(0, PixlesPerControllermovement, 0), (PixlesPerControllermovement / 2 + 1)));
                clickTimer = 0;
            }

            if (Input.GetAxis("DPadVertical") < -.5f)
            {
                    controllerItemSelectionMover.transform.localPosition = GetclosestPosition(getPositionsOutOfSlots(slots.ToArray()), getBetweenPoint(controllerItemSelectionMover.localPosition, controllerItemSelectionMover.transform.localPosition - new Vector3(0, PixlesPerControllermovement, 0), (PixlesPerControllermovement / 2 + 1)));
                clickTimer = 0;
            }


            setClosestHoverFromControllerMovementPoint();

        }
    }

    #region ControllerMethods
    public void setClosestHoverFromControllerMovementPoint()
    {
        Slot tempSlot;
        tempSlot = GetclosestPosition(getTransformsOutOfSlots(slots.ToArray()), controllerItemSelectionMover).GetComponent<Slot>();
        //Debug.Log(tempSlot.transform.name);

        ToggleHover(tempSlot.thisHoverType, tempSlot.index, true);

    }

    public Vector3 getBetweenPoint(Vector3 from, Vector3 to, float multiplier)
    {
        Vector3 betweenOldAndNew = from - ((from - to).normalized) * multiplier;

        return betweenOldAndNew;
    }

    public bool CheckIfControllerUIInteractionCanMoveThere(Vector3[] positions, Vector3 currentPosition, Vector3 MovingTowardsPosition)
    {
        Vector3 betweenOldAndNew = currentPosition - ((currentPosition - MovingTowardsPosition).normalized) * (PixlesPerControllermovement / 2);
        Debug.Log(betweenOldAndNew);
        for (int i = 0; i < positions.Length; i++)
        {
            if (positions[i] == currentPosition)
            {
                continue;
            }
            if (Vector3.Distance(betweenOldAndNew, positions[i]) < PixlesPerControllermovement /1.98f)
            {
                return true;
            }
        }

        return false;
    }

    public Transform GetclosestPosition(Transform[] position, Transform posToCompareTo)
    {
        Transform tMin = null;
        float minDist = Mathf.Infinity;
        Transform currentPos = posToCompareTo;
        foreach (Transform t in position)
        {
            float dist = Vector3.Distance(t.position, posToCompareTo.position);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        return tMin;
    }

    public Vector3 GetclosestPosition(Vector3[] position, Vector3 posToCompareTo)
    {
        Vector3 tMin = Vector3.zero;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (Vector3 t in position)
        {
            float dist = Vector3.Distance(t, posToCompareTo);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        return tMin;
    }


    public Vector3[] getPositionsOutOfSlots(Slot[] slotlist)
    {
        Vector3[] result;

        result = new Vector3[slotlist.Length];
        for (int i = 0; i < slotlist.Length; i++)
        {
            result[i] = slotlist[i].transform.localPosition;
        }

        return result;
    }


    public Transform[] getTransformsOutOfSlots(Slot[] slotlist)
    {
        Transform[] result;

        result = new Transform[slotlist.Length];
        for (int i = 0; i < slotlist.Length; i++)
        {
            result[i] = slotlist[i].transform;
        }

        return result;
    }
    #endregion ControllerMehtods

    void DraggingPhyscialItemUpdate()
    {
        if (isDragging && droppedObjectHighlight != null && droppedObjectHighlightRotationDirection != null)
        {
            //then make sure that the dragged items user interface follows our mouse position, offset with vector 3
            dragTransform.position = Input.mousePosition + new Vector3(16, -16, 0);

            #region draggingPhyscialObjectRotating
            if (droppedItemRotateState == 0)
            {
                droppedObjectHighlight.transform.Rotate(new Vector3(0, Input.GetAxis("Mouse ScrollWheel") * 50, 0));

                droppedObjectHighlightRotationDirection.forward = droppedObjectHighlight.transform.forward;

                droppedObjectHighlightRotationDirection.right = droppedObjectHighlight.transform.right;

                droppedObjectHighlightRotationDirection.up = droppedObjectHighlight.transform.up;

                droppedObjectHighlightRotationDirection.GetComponent<Renderer>().material.color = Color.green;
            }

            else if (droppedItemRotateState == 1)
            {
                droppedObjectHighlight.transform.Rotate(new Vector3(Input.GetAxis("Mouse ScrollWheel") * 50, 0, 0));

                droppedObjectHighlightRotationDirection.up = droppedObjectHighlight.transform.right * -1;

                droppedObjectHighlightRotationDirection.GetComponent<Renderer>().material.color = Color.red;
            }

            else if (droppedItemRotateState == 2)
            {
                droppedObjectHighlight.transform.Rotate(new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel") * 50));

                droppedObjectHighlightRotationDirection.up = droppedObjectHighlight.transform.forward * -1;

                droppedObjectHighlightRotationDirection.GetComponent<Renderer>().material.color = Color.blue;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                droppedItemRotateState += 1;

                if (droppedItemRotateState > 2)
                {
                    droppedItemRotateState = 0;
                }
            }
            #endregion
        }
    }

    void DroppingPhysicalItemUpdate()
    {
        if (isDragging && !isHovering && clickTimer > clickTimerActivate)
        {
            //GameObject droppedItemObj;
            //droppedItemObj = Resources.Load("DropModels/" + currentDraggedItem.DropModel, typeof(GameObject)) as GameObject;
            //if (droppedObjectHighlight.activeInHierarchy == false)
            //{

            //    // droppedObjectHighlight.GetComponent<MeshRenderer>().material = droppedObjectMaterialHighlight;

            //    if (droppedItemObj.GetComponentInChildren<MeshFilter>() != null)
            //    {
            //        droppedObjectHighlight.GetComponent<MeshFilter>().sharedMesh = droppedItemObj.GetComponentInChildren<MeshFilter>().sharedMesh;
            //    }

            //    else if (droppedItemObj.GetComponentInChildren<SkinnedMeshRenderer>() != null)
            //    {
            //        //Debug.Log("reached RIght code");
            //        droppedObjectHighlight.GetComponent<MeshFilter>().sharedMesh = droppedItemObj.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
            //    }

            //    droppedObjectHighlight.SetActive(true);

            //    //just to update the texture of the green outline game object to match what its picking up
            //    //if it has a normal renderer
            //    if (droppedItemObj.GetComponentInChildren<Renderer>() != null)
            //    {
            //        droppedObjectHighlight.GetComponent<Renderer>().material.mainTexture = droppedItemObj.GetComponentInChildren<Renderer>().sharedMaterial.mainTexture;
            //    }
            //    //or a skinned mesh one, like the chest for example.
            //    else if (droppedItemObj.GetComponentInChildren<SkinnedMeshRenderer>() != null)
            //    {
            //        droppedObjectHighlight.GetComponent<Renderer>().material.mainTexture = droppedItemObj.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.mainTexture;
            //    }
            //}

#region unsure/dropping maybe
            //if (Vector3.Distance(playerTransform.position, CharacterMovement.thisInstance.hit.point) < 2)
            //{
            //    Vector3 objectMovementVector;

            //    MeshCollider thisCollider = null;

            //    if (thisCollider == null)
            //    {
            //        thisCollider = droppedObjectHighlight.GetComponent<MeshCollider>();
            //    }

            //    objectMovementVector = CharacterMovement.thisInstance.hit.point;

            //    objectMovementVector.y += .05f;

            //    droppedObjectHighlight.transform.position = objectMovementVector;

            //    droppedObjectHighlight.transform.localScale = droppedItemObj.transform.localScale;

            //    GameObject clone;
            //    if (Input.GetMouseButtonUp(0))
            //    {
            //        clone = Instantiate(Resources.Load("DropModels/" + currentDraggedItem.DropModel, typeof(GameObject)), droppedObjectParent) as GameObject;
            //        clone.GetComponent<AddOnClick>().DroppedItem = true;
            //        clone.GetComponent<AddOnClick>().thisItem = currentDraggedItem;
            //        clone.GetComponent<AddOnClick>().Amnt = currentDraggedItem.StackSize;

            //        if (clone.GetComponent<SaveableItem>().thisSaveType == SaveableItemType.Container)
            //        {
            //            clone.transform.SetParent(LevelDataSavingLoading.thisInstance.parentListContainers);
            //        }

            //        clone.transform.position = objectMovementVector;

            //        clone.transform.rotation = droppedObjectHighlight.transform.rotation;


            //        if (clone.transform.Find("RootBone") != null)
            //        {
            //            clone.transform.Rotate(90, 0, 0);
            //        }

            //        dragTransform.gameObject.SetActive(false);
            //        currentDraggedItem = new Item();
            //        isDragging = false;
            //        clickTimer = 0;
            //    }

            //    if (Input.GetKeyDown(KeyCode.T))
            //    {
            //        droppedItemRotateState = 0;
            //        droppedObjectHighlight.transform.rotation = new Quaternion(0, 0, 0, 0);
            //    }
            //}
            //else
            //{
            //    droppedObjectHighlight.SetActive(false);
            //}

#endregion

        }

        else
        {
            //droppedObjectHighlight.GetComponent<MeshFilter>().sharedMesh = null;
            //droppedObjectHighlight.GetComponent<SkinnedMeshRenderer>().sharedMesh = null;
            droppedObjectHighlight.SetActive(false);
        }

    }

    public void DeterminePhysicalObjectInteractedWith()
    {
        if (clickTimer >= clickTimerActivate)
        {
            //if the ray we shoot out does have a save type, is within our interaction range, we aren't dragging currently, or hovering over anything
            if (worldInteractionHit.transform.GetComponent<SaveableItem>() != null && Vector3.Distance(worldInteractionHit.point, playerTransform.position) < objectInteractionRange && !isDragging && !isHovering)
            {
                //if the savable item we interacted with is a container
                if (worldInteractionHit.transform.GetComponent<SaveableItem>().thisSaveType == SaveableItemType.Container)
                {
                    //if 
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        Debug.Log("reached Code");
                        PickUpItemInteraction();
                    }

                    if (!Input.GetKey(KeyCode.LeftShift))
                    {
                        Debug.Log("reached Code Open");
                        containerMangerRef.OpenContainerInteraction();
                    }
                }

                else if (worldInteractionHit.transform.GetComponent<SaveableItem>().thisSaveType == SaveableItemType.DroppedItem)
                {
                    PickUpItemInteraction();
                }
            }
        }
    }

    void PickUpPhysicalDragItem()
    {
        //it hits something with the pickupableitem class
        if (worldInteractionHit.collider.GetComponent<AddOnClick>() != null && Vector3.Distance(worldInteractionHit.point, playerTransform.position) < 2 && !isDragging)
        {
            if (worldInteractionHit.collider.GetComponent<SaveableItem>().thisSaveType != SaveableItemType.Container)
            {
                //we get the reference, and then add the item/destroy the physcial item.
                AddOnClick itemClicked = worldInteractionHit.collider.GetComponent<AddOnClick>();
                //Inventory.AddItemAnywhere(itemClicked.thisItem, itemClicked.Amnt);


                if (itemClicked.insideCollisionBoxList)
                {
                    itemClicked.collisionList.Remove(itemClicked.thisItem);
                }


                droppedObjectHighlight.transform.localScale = worldInteractionHit.transform.localScale;

                if (worldInteractionHit.transform.Find("RootBone") != null)
                {
                    print("Does have root bone");
                    Debug.Log("Has Root Bone");
                    droppedObjectHighlight.transform.rotation = worldInteractionHit.transform.rotation;
                    droppedObjectHighlight.transform.Rotate(-90, 0, 0);
                }

                else
                {
                    print("Does not have root bone");
                    Debug.Log("Does not have Root Bone");
                    droppedObjectHighlight.transform.rotation = worldInteractionHit.transform.rotation;
                }

                StartedDraggingSomething(itemClicked.thisItem, itemClicked.Amnt);
                Destroy(itemClicked.gameObject);
                clickTimer = 0;
            }

            //if it is a container we are trying to pickup/move, then make sure its empty first
            else if (worldInteractionHit.collider.GetComponent<SaveableItem>().thisSaveType == SaveableItemType.Container && worldInteractionHit.transform.GetComponent<Container>().IsEmpty())
            {
                //we get the reference, and then add the item/destroy the physcial item.
                AddOnClick itemClicked = worldInteractionHit.collider.GetComponent<AddOnClick>();
                //Inventory.AddItemAnywhere(itemClicked.thisItem, itemClicked.Amnt);


                if (worldInteractionHit.collider.GetComponent<Container>().activeBench != null)
                {
                    worldInteractionHit.collider.GetComponent<Container>().activeBench.collisionBoxReference.containersNearby.Remove(worldInteractionHit.collider.GetComponent<Container>());
                }


                droppedObjectHighlight.transform.localScale = worldInteractionHit.transform.localScale;

                if (worldInteractionHit.transform.Find("RootBone") != null)
                {
                    print("Does have root bone");
                    Debug.Log("Has Root Bone");
                    droppedObjectHighlight.transform.rotation = worldInteractionHit.transform.rotation;
                    droppedObjectHighlight.transform.Rotate(-90, 0, 0);
                }

                else
                {
                    print("Does not have root bone");
                    Debug.Log("Does not have Root Bone");
                    droppedObjectHighlight.transform.rotation = worldInteractionHit.transform.rotation;
                }

                containerMangerRef.CloseContainer();

                StartedDraggingSomething(itemClicked.thisItem, itemClicked.Amnt);
                Destroy(itemClicked.gameObject);
                clickTimer = 0;
            }
        }
    }

    void PickUpItemInteraction()
    {
        SaveableItem itemToPickUp;
        itemToPickUp = worldInteractionHit.collider.GetComponent<SaveableItem>();

        //if the item we pressed E on in the world is just a simple dropped item.
        if (itemToPickUp.thisSaveType == SaveableItemType.DroppedItem)
        {

            AddOnClick itemClicked = worldInteractionHit.collider.GetComponent<AddOnClick>();

            pickUpSoundTransform.position = itemClicked.transform.position;

            pickupSounds.clip = itemClicked.pickupSound;

            pickupSounds.pitch = UnityEngine.Random.Range(.8f, 1.2f);

            pickupSounds.Play();

            if (itemClicked.thisItem.StackSize > 0)
            {
                Inventory.AddItemAnywhere(itemClicked.thisItem, itemClicked.thisItem.StackSize);
            }

            if (itemClicked.insideCollisionBoxList)
            {
                itemClicked.collisionList.Remove(itemClicked.thisItem);
            }


            if (worldInteractionHit.collider.GetComponent<Container>() != null)
            {

                if (worldInteractionHit.collider.GetComponent<Container>().activeBench != null)
                {
                    worldInteractionHit.collider.GetComponent<Container>().activeBench.collisionBoxReference.containersNearby.Remove(worldInteractionHit.collider.GetComponent<Container>());
                }
            }

            Destroy(itemClicked.gameObject);
            // UpdateHotBarEquipped();
        }

        //if the item we pressed e on is a container
        if (itemToPickUp.thisSaveType == SaveableItemType.Container)
        {
            //make sure its empty, only then can you move it.
            if (worldInteractionHit.collider.GetComponent<Container>().IsEmpty())
            {
                AddOnClick itemClicked = worldInteractionHit.collider.GetComponent<AddOnClick>();


                if (itemClicked.thisItem.StackSize > 0)
                {
                    Inventory.AddItemAnywhere(itemClicked.thisItem, itemClicked.thisItem.StackSize);
                }

                if (itemClicked.insideCollisionBoxList)
                {
                    itemClicked.collisionList.Remove(itemClicked.thisItem);
                }


                if (worldInteractionHit.collider.GetComponent<Container>() != null)
                {

                    if (worldInteractionHit.collider.GetComponent<Container>().activeBench != null)
                    {
                        worldInteractionHit.collider.GetComponent<Container>().activeBench.collisionBoxReference.containersNearby.Remove(worldInteractionHit.collider.GetComponent<Container>());
                    }
                }

                Destroy(itemClicked.gameObject);

                containerMangerRef.CloseContainer();
                if (hasHotbar)
                {
                    UpdateHotBarEquipped();
                }
            }
        }
    }

    void UIInteractionInputUpdate()
    {
        //is our mouse hovering over something?
        if (isHovering && clickTimer >= clickTimerActivate)
        {
            #region InventoryHovering
            if (currentHoverType == HoverType.Inventory)
            {
                //if we arent dragging anything right now
                if (!isDragging)
                {
                    //if we left click and what we are hovering over isn't empty/has an item
                    if (Input.GetMouseButton(0) || Input.GetKeyDown(grabItemButton))
                    {
                        //if we aren't holding left shift
                        if (!Input.GetKey(KeyCode.LeftShift))
                        {
                            UIInteraction(UIInteractionType.GrabItemNotDragging, UIinteractionCategory.Inventory, hoverIndex, inventory, slots.ToArray());
                            return;
                        }

                        else
                        {
                            if (containerMangerRef.isContainerOpen)
                            {
                                TransferItem(inventory[hoverIndex], inventory[hoverIndex].StackSize, containerMangerRef.currentlyOpenedContainer.containerContents, inventory);
                            }
                        }
                    }

                    //if you right click while we aren't holding anything, then we should add just 1 of it as our current dragged
                    if (Input.GetMouseButtonDown(1) && inventory[hoverIndex].ID != -1)
                    {
                        UIInteraction(UIInteractionType.DropItemNotDragging, UIinteractionCategory.Inventory, hoverIndex, inventory, slots.ToArray());
                        return;
                    }
                }

                //if we are dragging.
                if (isDragging)
                {
                    #region LeftClickDrag
                    if (Input.GetMouseButton(0) || Input.GetKeyDown(grabItemButton))
                    {
                        UIInteraction(UIInteractionType.GrabItemDragging, UIinteractionCategory.Inventory, hoverIndex, inventory, slots.ToArray());
                        return;
                    }
                    #endregion

                    #region RightClickDrag
                    if (Input.GetMouseButtonDown(1))
                    {
                        UIInteraction(UIInteractionType.DropItemDragging, UIinteractionCategory.Inventory, hoverIndex, inventory, slots.ToArray());
                        return;
                    }
                    #endregion
                }
            }

            #endregion

            #region ContainerHovering
            if (currentHoverType == HoverType.Container && containerMangerRef.isContainerOpen)
            {
                if (isDragging)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        UIInteraction(UIInteractionType.GrabItemDragging, UIinteractionCategory.Container, (hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage), containerMangerRef.openedContainerContent, containerMangerRef.containerSlots);

                        return;
                    }
                }
                //if we arent dragging anything right now
                if (!isDragging)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (!Input.GetKey(KeyCode.LeftShift))
                        {
                            UIInteraction(UIInteractionType.GrabItemNotDragging, UIinteractionCategory.Container, (hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage), containerMangerRef.openedContainerContent, containerMangerRef.containerSlots);
                            return;
                        }

                        else
                        {

                            // TransferItem(containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage], containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage].StackSize, inventory, containerMangerRef.displayedContainerContent);

                            //activeOpenContainer.containerContents[hoverIndex + (containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage)] = new Item();

                            //pdateHotBarEquipped();
                            // UpdateSlotUIAtIndex(hoverIndex, slots.ToArray(), inventory);

                            //containerMangerRef.RefreshContainer();

                            //FinishUIInteraction();
                        }

                        return;

                    }

                    //if you right click while we aren't holding anything, then we should add just 1 of it as our current dragged
                    if (Input.GetMouseButton(1))
                    {
                        UIInteraction(UIInteractionType.DropItemNotDragging, UIinteractionCategory.Container, (hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage), containerMangerRef.openedContainerContent, containerMangerRef.containerSlots);

                        return;
                    }
                }

                ////if we are dragging.
                //if (isDragging)
                //{
                //    #region LeftClickDrag
                //    //if we left click
                //    if (Input.GetMouseButton(0))
                //    {
                //        //if we left click on an empty while we are dragging
                //        if (containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage].ID == -1)
                //        {
                //            dragTransform.gameObject.SetActive(false);
                //            containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage] = currentDraggedItem;
                //            UpdateSlotUIAtIndex(hoverIndex, containerMangerRef.containerSlots, containerMangerRef.displayedContainerContent);

                //            containerMangerRef.RefreshContainer();

                //            currentDraggedItem = new Item();
                //            isDragging = false;

                //            FinishUIInteraction();

                //            return;
                //        }

                //        //swap items
                //        if (containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage].ID != -1 && containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage].ID != currentDraggedItem.ID)
                //        {
                //            Item tempItem;
                //            tempItem = TypeChecker(containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage]);
                //            containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage] = currentDraggedItem;
                //            UpdateSlotUIAtIndex(hoverIndex, containerMangerRef.containerSlots, containerMangerRef.displayedContainerContent);

                //            containerMangerRef.RefreshContainer();


                //            isDragging = true;
                //            dragTransform.gameObject.SetActive(true);
                //            dragTransform.SetAsLastSibling();
                //            currentDraggedItem = tempItem;
                //            dragTransform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/" + currentDraggedItem.SpriteName) as Sprite;
                //            dragTransform.GetComponentInChildren<Text>().text = currentDraggedItem.StackSize.ToString();

                //            FinishUIInteraction();

                //            return;
                //        }

                //        if (containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage].ID == currentDraggedItem.ID)
                //        {
                //            int remainingNumberInStack;
                //            remainingNumberInStack = containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage].MaxStackSize - containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage].StackSize;

                //            //so we are dragging 64 to a 16 stack item, the remainingNumberInStack is 48

                //            //if the remaining number of the hovering stack is less that how many we are dragging
                //            //if 48 is < 64
                //            if (remainingNumberInStack < currentDraggedItem.StackSize)
                //            {
                //                //the hovering item gets added 48 brining it up to 64
                //                containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage].StackSize += remainingNumberInStack;
                //                //minus 48 from our stacksize
                //                currentDraggedItem.StackSize -= remainingNumberInStack;
                //            }

                //            else if (remainingNumberInStack >= currentDraggedItem.StackSize)
                //            {
                //                containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage].StackSize += currentDraggedItem.StackSize;
                //                dragTransform.gameObject.SetActive(false);
                //                currentDraggedItem = new Item();
                //                isDragging = false;
                //            }

                //            dragTransform.GetComponentInChildren<Text>().text = currentDraggedItem.StackSize.ToString();
                //            UpdateSlotUIAtIndex(hoverIndex, containerMangerRef.containerSlots, containerMangerRef.displayedContainerContent);

                //            containerMangerRef.RefreshContainer();

                //            FinishUIInteraction();
                //            return;
                //        }
                //    }
                //    #endregion

                //    #region RightClickDrag
                //    if (Input.GetMouseButton(1))
                //    {

                //        if (containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage].ID == currentDraggedItem.ID && containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage].StackSize < containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage].MaxStackSize)
                //        {
                //            containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage].StackSize += 1;
                //            currentDraggedItem.StackSize -= 1;
                //            dragTransform.GetComponentInChildren<Text>().text = currentDraggedItem.StackSize.ToString();

                //            if (containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage].StackSize <= 0)
                //            {
                //                containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage] = new Item();
                //            }

                //            if (currentDraggedItem.StackSize <= 0)
                //            {
                //                dragTransform.gameObject.SetActive(false);
                //                currentDraggedItem = new Item();
                //                isDragging = false;
                //            }

                //            UpdateSlotUIAtIndex(hoverIndex, containerMangerRef.containerSlots, containerMangerRef.displayedContainerContent);

                //            containerMangerRef.RefreshContainer();

                //            FinishUIInteraction();

                //            return;
                //        }

                //        if (containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage].ID == -1 && currentDraggedItem.StackSize <= currentDraggedItem.MaxStackSize)
                //        {
                //            currentDraggedItem.StackSize -= 1;
                //            dragTransform.GetComponentInChildren<Text>().text = currentDraggedItem.StackSize.ToString();
                //            containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage] = TypeChecker(currentDraggedItem);
                //            containerMangerRef.openedContainerContent[hoverIndex + containerMangerRef.containerItemsPerPage * containerMangerRef.currentContainerPage].StackSize = 1;

                //            if (currentDraggedItem.StackSize <= 0)
                //            {
                //                dragTransform.gameObject.SetActive(false);
                //                currentDraggedItem = new Item();
                //                isDragging = false;
                //            }

                //            UpdateSlotUIAtIndex(hoverIndex, containerMangerRef.containerSlots, containerMangerRef.displayedContainerContent);

                //            containerMangerRef.RefreshContainer();

                //            FinishUIInteraction();
                //            return;
                //        }
                //    }
                //    #endregion
                //}
            }
            #endregion ContainerHovering
        }
    }

    void UIInteraction(UIInteractionType thisUIInteractionType, UIinteractionCategory thisInteractionCategory, int hoverIndexPass,  Item[] arrayDataToChange, Slot[] slotDataToChange)
    {

        if (thisUIInteractionType == UIInteractionType.GrabItemNotDragging)
        {
            StartedDraggingSomething(arrayDataToChange[hoverIndexPass], arrayDataToChange[hoverIndexPass].StackSize);

            arrayDataToChange[hoverIndexPass] = new Item();

            FinishUIInteraction(thisInteractionCategory, arrayDataToChange, slotDataToChange, hoverIndexPass);
            return;
        }

        if (thisUIInteractionType == UIInteractionType.DropItemNotDragging)
        {

            StartedDraggingSomething(arrayDataToChange[hoverIndexPass], 1);

            arrayDataToChange[hoverIndexPass].StackSize -= 1;

            FinishUIInteraction(thisInteractionCategory, arrayDataToChange, slotDataToChange, hoverIndexPass);
            return;
        }

        if (thisUIInteractionType == UIInteractionType.GrabItemDragging)
        {

            //if we left click on an empty while we are dragging
            if (arrayDataToChange[hoverIndexPass].ID == -1)
            {
                dragTransform.gameObject.SetActive(false);
                arrayDataToChange[hoverIndexPass] = currentDraggedItem;
                currentDraggedItem = new Item();
                isDragging = false;
                FinishUIInteraction(thisInteractionCategory, arrayDataToChange, slotDataToChange, hoverIndexPass);
                return;
            }

            //swap items
            else if (arrayDataToChange[hoverIndexPass].ID != -1 && arrayDataToChange[hoverIndexPass].ID != currentDraggedItem.ID)
            {
                Item tempItem;
                tempItem = TypeChecker(inventory[hoverIndexPass]);
                arrayDataToChange[hoverIndexPass] = currentDraggedItem;


                isDragging = true;
                dragTransform.gameObject.SetActive(true);
                dragTransform.SetAsLastSibling();
                currentDraggedItem = tempItem;
                dragTransform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/" + currentDraggedItem.SpriteName) as Sprite;
                dragTransform.GetComponentInChildren<Text>().text = currentDraggedItem.StackSize.ToString();

                FinishUIInteraction(thisInteractionCategory, arrayDataToChange, slotDataToChange, hoverIndexPass);
                return;
            }

            else if (arrayDataToChange[hoverIndexPass].ID == currentDraggedItem.ID)
            {
                int remainingNumberInStack;
                remainingNumberInStack = arrayDataToChange[hoverIndexPass].MaxStackSize - arrayDataToChange[hoverIndexPass].StackSize;

                //so we are dragging 64 to a 16 stack item, the remainingNumberInStack is 48

                //if the remaining number of the hovering stack is less that how many we are dragging
                //if 48 is < 64
                if (remainingNumberInStack < currentDraggedItem.StackSize)
                {
                    //the hovering item gets added 48 brining it up to 64
                    arrayDataToChange[hoverIndexPass].StackSize += remainingNumberInStack;
                    //minus 48 from our stacksize
                    currentDraggedItem.StackSize -= remainingNumberInStack;
                }

                else if (remainingNumberInStack >= currentDraggedItem.StackSize)
                {
                    arrayDataToChange[hoverIndexPass].StackSize += currentDraggedItem.StackSize;
                    dragTransform.gameObject.SetActive(false);
                    currentDraggedItem = new Item();
                    isDragging = false;

                }

                dragTransform.GetComponentInChildren<TextMeshProUGUI>().text = currentDraggedItem.StackSize.ToString();

                FinishUIInteraction(thisInteractionCategory, arrayDataToChange, slotDataToChange, hoverIndexPass);
                return;
            }
        }

        if (thisUIInteractionType == UIInteractionType.DropItemDragging)
        {
            if (arrayDataToChange[hoverIndexPass].ID == currentDraggedItem.ID && arrayDataToChange[hoverIndexPass].StackSize < arrayDataToChange[hoverIndex].MaxStackSize)
            {
                arrayDataToChange[hoverIndexPass].StackSize += 1;
                currentDraggedItem.StackSize -= 1;
                dragTransform.GetComponentInChildren<TextMeshProUGUI>().text = currentDraggedItem.StackSize.ToString();


                FinishUIInteraction(thisInteractionCategory, arrayDataToChange, slotDataToChange, hoverIndexPass);
                return;
            }

            if (arrayDataToChange[hoverIndexPass].ID == -1 && currentDraggedItem.StackSize <= currentDraggedItem.MaxStackSize)
            {
                currentDraggedItem.StackSize -= 1;
                dragTransform.GetComponentInChildren<TextMeshProUGUI>().text = currentDraggedItem.StackSize.ToString();
                arrayDataToChange[hoverIndexPass] = TypeChecker(currentDraggedItem);
                arrayDataToChange[hoverIndexPass].StackSize = 1;


                FinishUIInteraction(thisInteractionCategory, arrayDataToChange, slotDataToChange, hoverIndexPass);
                return;
            }
        }
    }

    void FinishUIInteraction(UIinteractionCategory categoryToPass, Item[] arrayDataToPass, Slot[] slotDataToPass, int hoverIndexPass)
    {

        if (arrayDataToPass[hoverIndexPass].StackSize <= 0)
        {
            arrayDataToPass[hoverIndexPass] = new Item();
        }


        if (currentDraggedItem.StackSize <= 0)
        {
            dragTransform.gameObject.SetActive(false);
            currentDraggedItem = new Item();
            isDragging = false;
        }

        if (categoryToPass == UIinteractionCategory.Inventory)
        {
            UpdateSlotUIAtIndex(hoverIndexPass, slotDataToPass.ToArray(), arrayDataToPass);
        }

        if (categoryToPass == UIinteractionCategory.Container)
        {
            UpdateSlotUIAtIndex(hoverIndex, containerMangerRef.containerSlots, containerMangerRef.displayedContainerContent);

            if (containerMangerRef != null)
            {
                containerMangerRef.RefreshContainer();
            }
            else
            {
                if (debugMode)
                {
                    Debug.Log("ContainerMangerRef isn't assigned or set up in the inspect");
                }
            }
        }



        UpdateHotBarEquipped();

        clickTimer = 0;
    }

    void FinishUIInteraction()
    {
        clickTimer = 0;
        UpdateHotBarEquipped();
    }

    public void StartedDraggingSomething(Item itemWeStartedDragging, int howMany)
    {
        Debug.Log("Got to started dragging code");
        isDragging = true;
        dragTransform.gameObject.SetActive(true);
        dragTransform.SetAsLastSibling();
        currentDraggedItem = TypeChecker(itemWeStartedDragging);
        currentDraggedItem.StackSize = howMany;
        dragTransform.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>("Sprites/" + currentDraggedItem.SpriteName) as Sprite;
        dragTransform.GetComponentInChildren<TextMeshProUGUI>().text = currentDraggedItem.StackSize.ToString();
    }

    void UIInteractionWheelUpdate()
    {
        if (isHovering && currentHoverType == HoverType.InteractionWheel)
        {
            UIInteractionWheelGameobject.SetActive(true);
            UIInteractionWheelGameobject.transform.position = hovPosition;
            UIInteractionWheelGameobject.transform.SetAsLastSibling();
        }

        else
        {
            if (UIInteractionWheelGameobject.activeInHierarchy)
            {
                UIInteractionWheelGameobject.SetActive(false);
            }
        }
    }

    public void OpenColorPicker()
    {
        currentHoverType = HoverType.Nothing;
        UIInteractionWheelGameobject.SetActive(false);

        //colorPickerWindow.transform.SetParent(hoveringWindow.transform);
        colorPickerWindow.transform.SetAsLastSibling();

        colorPickerWindow.SetActive(true);

        if (Input.mousePosition.x < Screen.width / 2)
        {
            colorPickerWindow.gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);

            colorPickerWindow.gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);

            colorPickerWindow.gameObject.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        }

        else
        {
            colorPickerWindow.gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);

            colorPickerWindow.gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);

            colorPickerWindow.gameObject.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
        }

        colorPickerWindow.transform.position = hovPosition;

        colorPickerClass.state = 0;

        colorPickerClass.GetCurrentValues();

    }

    public void OpenRenamingWindow()
    {
        renamingWindow.SetActive(true);
        renamingWindow.transform.SetAsLastSibling();

        if (Input.mousePosition.x < Screen.width / 2)
        {
            renamingWindow.gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);

            renamingWindow.gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);

            renamingWindow.gameObject.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
        }

        else
        {
            renamingWindow.gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);

            renamingWindow.gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);

            renamingWindow.gameObject.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
        }

        renamingWindow.transform.position = hovPosition;
    }

    public void RenameActiveWindow(string newName)
    {
        hoveringWindow.windowName.text = newName;

        if (hoveringWindow.thisWindowType == WindowType.Container)
        {
            containerMangerRef.RenameContainer(newName);
        }

        ToggleRenamerWindow(false);
        renameWindowInput.text = "";
    }

    public void DescriptionUpdate()
    {
        if (descriptionWindow != null)
        {
            if (isHovering)
            {
                if (currentHoverType == HoverType.Inventory)
                {
                    //if the item we are hovering over isnt empty, then enable the tooltip information
                    if (inventory[hoverIndex].ID != -1)
                    {
                        if (Input.mousePosition.x > Screen.width / 2)
                        {
                            if (Input.mousePosition.y > Screen.height / 2)
                            {
                                descriptionWindow.mainWindowRectTransform.anchorMin = new Vector2(1, 1);
                                descriptionWindow.mainWindowRectTransform.anchorMax = new Vector2(1, 1);
                                descriptionWindow.mainWindowRectTransform.pivot = new Vector2(1f, 1f);
                            }
                            else
                            {
                                descriptionWindow.mainWindowRectTransform.anchorMin = new Vector2(1, 0);
                                descriptionWindow.mainWindowRectTransform.anchorMax = new Vector2(1, 0);
                                descriptionWindow.mainWindowRectTransform.pivot = new Vector2(1f, 0f);
                            }

                        }
                        else
                        {
                            if (Input.mousePosition.y > Screen.height / 2)
                            {
                                descriptionWindow.mainWindowRectTransform.anchorMin = new Vector2(0, 1);
                                descriptionWindow.mainWindowRectTransform.anchorMax = new Vector2(0, 1);
                                descriptionWindow.mainWindowRectTransform.pivot = new Vector2(0f, 1f);
                            }

                            else
                            {
                                descriptionWindow.mainWindowRectTransform.anchorMin = new Vector2(0, 0);
                                descriptionWindow.mainWindowRectTransform.anchorMax = new Vector2(0, 0);
                                descriptionWindow.mainWindowRectTransform.pivot = new Vector2(0f, 0f);
                            }
                        }
                        descriptionWindow.DescriptionName.text = inventory[hoverIndex].constructToolTip()[0];

                        descriptionWindow.DesciptionBase.text = inventory[hoverIndex].constructToolTip()[1];

                        descriptionWindow.transform.position = Input.mousePosition;
                        descriptionWindow.mainWindowRectTransform.SetAsLastSibling();
                        descriptionWindow.gameObject.SetActive(true);
                    }

                    //if we arent hovering over a valid item, disable the tooltip information
                    else
                    {
                        descriptionWindow.gameObject.SetActive(false);
                    }

                }
            }

            else
            {
                descriptionWindow.gameObject.SetActive(false);
            }
        }
        else
        {
            if (debugMode)
            {
                Debug.Log("Description Window isn't set up or assigned");
            }
        }
    }

    #region Hotbar
    void HotBarUpdate()
    {
        #region Hotbar
        //if we aren't holding down left control to interfere with zooming in and out, or dragging an item, or hovering over a drag bar to maybe increase the window we
        //would be hovering overs size
        if (!Input.GetKey(KeyCode.LeftControl) && !isDragging && currentHoverType != HoverType.PreventHotBarScrolling)
        {
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    slots[i].setNormal();
                }
                if (currentCycledHotBar < 9)
                {
                    currentCycledHotBar += 1;
                }
                else
                {
                    currentCycledHotBar = 0;
                }


                //UpdateHotBarEquipped();
            }

            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    slots[i].setNormal();
                }
                if (currentCycledHotBar > 0)
                {
                    currentCycledHotBar -= 1;
                }
                else
                {
                    currentCycledHotBar = 9;
                }

                //UpdateHotBarEquipped();
            }
        }
        #endregion
    }

    public void UpdateHotBarEquipped()
    {
        if (hasHotbar)
        {
            //if there is something in the right hand
            //if (rightHand.childCount > 0)
            //{
            //    //destroy everything inside the right hand
            //    for (int i = 0; i < rightHand.childCount; i++)
            //    {
            //        Destroy(rightHand.GetChild(i).gameObject);
            //    }
            //}

            //if what our hotbar has currently selected is of weapon type
            if (inventory[currentCycledHotBar].Type == 1)
            {
                //an empty shell game object to turn into the weapon later
                GameObject equipedItemClone;


                //a converison of the item on the hot bar, to that of the fitting type, such as melee
                Melee conversion;
                conversion = inventory[currentCycledHotBar] as Melee;

                //create the item from a name searching through our resource folder
                equipedItemClone = Instantiate(Resources.Load("EquipPrefabs/" + conversion.equipPrefab, typeof(GameObject))) as GameObject;

                //the postioning and roation of the prefab to use as a means to adjust each weapons held position
                GameObject transformRef;
                transformRef = Resources.Load("EquipPrefabs/" + conversion.equipPrefab, typeof(GameObject)) as GameObject;

                //set the parent of this item to that of our right hand/primary equip
                //equipedItemClone.transform.SetParent(rightHand);

                //update the postioning using that earlier reference.
                equipedItemClone.transform.localPosition = transformRef.transform.position;
                equipedItemClone.transform.localRotation = transformRef.transform.rotation;
                equipedItemClone.transform.localScale = transformRef.transform.localScale;

                //update the equiped weapon components/link the references it may need, such as the item itself from the hotbar
                //equipedItemClone.GetComponent<EquipableWeapon>().playerAnim = playerAnimator;

                //equipedItemClone.GetComponent<EquipableWeapon>().itemBasedOn = conversion;
            }

            //highlight the currently selected item from our hotbar
            slots[currentCycledHotBar].setHighlight();

            if (inventory[currentCycledHotBar].Type == 0)
            {
                //playerAnimator.SetBool("oneHandWeaponEquiped", false);
            }
            //if the item we have highlighted in our hotbar is a weapon
            else if (inventory[currentCycledHotBar].Type == 1)
            {
                //playerAnimator.SetBool("oneHandWeaponEquiped", true);
            }
        }
    }
    #endregion Hotbar

    #region WindowToggles

    public void CloseActiveWindow()
    {
        UIInteractionWheelGameobject.SetActive(false);

        isHovering = false;
        currentHoverType = HoverType.Nothing;

        ToggleColorPickerWindow(false);
        ToggleRenamerWindow(false);

        if (hoveringWindow.thisWindowType == WindowType.Container)
        {
            ToggleContainerMenu(false);
        }

        if (hoveringWindow.thisWindowType == WindowType.Inventory)
        {
            ToggleInventoryWindowDisplayed(false);
        }
    }

    public void ToggleRecipeWindowDisplayed(bool state)
    {
        if (state == false)
        {
            recipeWindowMaster.gameObject.SetActive(false);
            activeRecipe = null;
        }
        else
        {
            recipeWindowMaster.gameObject.SetActive(true);
        }


        if (recipeWindowMaster.gameObject.activeSelf)
        {
            recToggle.isOn = true;
        }
        else
        {
            recToggle.isOn = false;
        }
    }

    public void ToggleInventoryWindowDisplayed(bool state)
    {
        if (state == false)
        {
            inventoryWindowMaster.gameObject.SetActive(false);
        }

        else
        {
            inventoryWindowMaster.gameObject.SetActive(true);
        }

        if (inventoryWindowMaster.gameObject.activeSelf)
        {
            //invToggle.isOn = true;
            isInvOpen = true;
        }
        else
        {
           // invToggle.isOn = false;
            isInvOpen = false;
        }
    }

    public void ToggleSaveWindowDisplayed(bool state)
    {
        if (state == false)
        {
            saveWindowMaster.gameObject.SetActive(false);
        }

        else
        {
            saveWindowMaster.gameObject.SetActive(true);
        }


        if (saveWindowMaster.gameObject.activeSelf)
        {
            saveToggle.isOn = true;
        }
        else
        {
            saveToggle.isOn = false;
        }
    }

    public void ToggleContainerMenu(bool state)
    {
        if (state == false)
        {
            containerWindowMaster.gameObject.SetActive(false);

            if (containerMangerRef.isContainerOpen)
            {
                containerMangerRef.CloseContainer();
            }
        }

        else
        {
            containerWindowMaster.gameObject.SetActive(true);
        }

    }

    public void ToggleColorPickerWindow(bool state)
    {
        if (state == false)
        {
            colorPickerWindow.gameObject.SetActive(false);
        }

        else
        {
            colorPickerWindow.gameObject.SetActive(true);
        }
    }

    public void ToggleRenamerWindow(bool state)
    {
        if (state == false)
        {
            renamingWindow.gameObject.SetActive(false);
        }

        else
        {
            renamingWindow.gameObject.SetActive(true);
        }
    }

    public void ToggleAllWindows(bool state)
    {
        ToggleInventoryWindowDisplayed(state);
        ToggleSaveWindowDisplayed(state);
        ToggleRecipeWindowDisplayed(state);
        ToggleContainerMenu(state);
        ToggleColorPickerWindow(state);
        ToggleRenamerWindow(state);

        if (isHovering)
        {
            isHovering = false;
        }

    }
    #endregion WindowToggles

    #region Recipe
    void InitializeRecipes()
    {
        if (recipeSlotMaster != null)
        {
            recipesPerPage = recipeSlotMaster.childCount;
            displayedListOfRecipes = new Recipe[recipesPerPage];

            //get a reference to all the recipe slots
            for (int i = 0; i < recipesPerPage; i++)
            {
                RecipeSlots.Add(recipeSlotMaster.GetChild(i).GetComponent<Slot>());
            }

            //if the orgial inventory/contents is less than the number per page
            if (recipeDataBaseRef.recipeDatabase.Length > recipesPerPage)
            {
                for (int i = 0; i < recipesPerPage; i++)
                {
                    recipeSlotMaster.GetChild(i).transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/" + ItemDatabase.FetchByID(recipeDataBaseRef.recipeDatabase[i].resultID).SpriteName) as Sprite;
                    recipeSlotMaster.GetChild(i).transform.GetChild(0).GetComponent<Image>().enabled = true;
                    RecipeSlots[i].stackText.text = "1";
                }

                for (int i = 0; i < recipesPerPage; i++)
                {
                    displayedListOfRecipes[i] = recipeDataBaseRef.recipeDatabase[i];
                }
            }

            else
            {
                for (int i = 0; i < recipeDataBaseRef.recipeDatabase.Length; i++)
                {
                    recipeSlotMaster.GetChild(i).transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/" + ItemDatabase.FetchByID(recipeDataBaseRef.recipeDatabase[i].resultID).SpriteName) as Sprite;
                    recipeSlotMaster.GetChild(i).transform.GetChild(0).GetComponent<Image>().enabled = true;
                    RecipeSlots[i].stackText.text = "1";
                }

                for (int i = 0; i < recipeDataBaseRef.recipeDatabase.Length; i++)
                {
                    displayedListOfRecipes[i] = recipeDataBaseRef.recipeDatabase[i];
                }
            }


            ActiveListOfRecipes = recipeDataBaseRef.recipeDatabase.ToList<Recipe>();

            currentNumberOfPages = Mathf.CeilToInt((float)ActiveListOfRecipes.Count / (float)recipesPerPage);
            currentPage = 1;
        }
        else
        {
            Debug.Log("Recipes may not be set up properly");
        }
    }

    void HoveringRecipesInteractionsUpdate()
    {
        if (isHovering && currentHoverType == HoverType.Recipe)
        {
            if (Input.GetMouseButtonDown(0))
            {
                activeRecipe = displayedListOfRecipes[hoverIndex];

                for (int i = 0; i < RecipeSlots.Count; i++)
                {
                    RecipeSlots[i].setNormal();
                }

                RecipeSlots[hoverIndex].setHighlight();
            }
        }
    }

    public void SortRecipesBySearchbarBetterCG()
    {
        ActiveListOfRecipes.Clear();
        activeRecipe = null;

        for (int i = 0; i < recipeDataBaseRef.recipeDatabase.Length; i++)
        {
            if (HelperInventoryScript.Contains(recipeDataBaseRef.recipeDatabase[i].name, recipeSearchBar.text, StringComparison.OrdinalIgnoreCase))
            {
                ActiveListOfRecipes.Add(recipeDataBaseRef.recipeDatabase[i]);
            }
        }


        currentNumberOfPages = Mathf.CeilToInt((float)ActiveListOfRecipes.Count / (float)recipesPerPage);
        currentPage = 1;

        if ((currentPage * recipesPerPage) > ActiveListOfRecipes.Count)
        {
            //20 + 45 -0*20 = 15
            for (int i = 0; i < recipesPerPage + (ActiveListOfRecipes.Count - (currentPage * recipesPerPage)); i++)
            {
                if (!HelperInventoryScript.Contains(displayedListOfRecipes[i].name, recipeSearchBar.text, StringComparison.OrdinalIgnoreCase))
                {
                    RecipeSlots[i].thisSlotsItem.enabled = false;
                    RecipeSlots[i].stackText.text = "";
                }
            }
        }
        else
        {
            for (int i = 0; i < recipesPerPage; i++)
            {
                if (!HelperInventoryScript.Contains(displayedListOfRecipes[i].name, recipeSearchBar.text, StringComparison.OrdinalIgnoreCase))
                {
                    RecipeSlots[i].thisSlotsItem.enabled = false;
                    RecipeSlots[i].stackText.text = "";
                }
            }
        }

        for (int i = 0; i < recipesPerPage; i++)
        {
            RecipeSlots[i].thisSlotsItem.enabled = false;
            RecipeSlots[i].stackText.text = "";
        }

        if ((currentPage * recipesPerPage) > ActiveListOfRecipes.Count)
        {
            for (int i = 0; i < recipesPerPage + (ActiveListOfRecipes.Count - (currentPage * recipesPerPage)); i++)
            {
                if (ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)] != null)
                {
                    displayedListOfRecipes[i] = ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)];
                    RecipeSlots[i].thisSlotsItem.enabled = true;
                    RecipeSlots[i].index = i;
                    RecipeSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/" + ItemDatabase.FetchByID(ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)].resultID).SpriteName) as Sprite;
                    RecipeSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                    RecipeSlots[i].stackText.text = "1";
                }
            }
        }
        else
        {
            for (int i = 0; i < recipesPerPage; i++)
            {
                if (ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)] != null)
                {
                    displayedListOfRecipes[i] = ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)];
                    RecipeSlots[i].thisSlotsItem.enabled = true;
                    RecipeSlots[i].index = i;
                    RecipeSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/" + ItemDatabase.FetchByID(ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)].resultID).SpriteName) as Sprite;
                    RecipeSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                    RecipeSlots[i].GetComponent<Slot>().index = i;
                    RecipeSlots[i].stackText.text = "1";
                }
            }
        }
    }

    public void IncreaseRecipePage()
    {
        if (currentPage <= currentNumberOfPages)
        {
            currentPage += 1;
        }

        if (currentPage > currentNumberOfPages)
        {
            currentPage = 1;
        }

        for (int i = 0; i < recipesPerPage; i++)
        {
            RecipeSlots[i].thisSlotsItem.enabled = false;
            RecipeSlots[i].stackText.text = "";
        }

        // 1 * 20 is greater than 1
        if ((currentPage * recipesPerPage) > ActiveListOfRecipes.Count)
        {
            for (int i = 0; i < recipesPerPage + (ActiveListOfRecipes.Count - (currentPage * recipesPerPage)); i++)
            {
                //we would like this to be 15 i think
                if (ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)] != null)
                {
                    displayedListOfRecipes[i] = ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)];
                    RecipeSlots[i].thisSlotsItem.enabled = true;
                    RecipeSlots[i].index = i;
                    RecipeSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/" + ItemDatabase.FetchByID(ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)].resultID).SpriteName) as Sprite;
                    RecipeSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                    RecipeSlots[i].stackText.text = "1";
                }
            }
        }
        else
        {
            for (int i = 0; i < recipesPerPage; i++)
            {
                if (ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)] != null)
                {
                    displayedListOfRecipes[i] = ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)];
                    RecipeSlots[i].thisSlotsItem.enabled = true;
                    RecipeSlots[i].index = i;
                    RecipeSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/" + ItemDatabase.FetchByID(ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)].resultID).SpriteName) as Sprite;
                    RecipeSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                    RecipeSlots[i].GetComponent<Slot>().index = i;
                    RecipeSlots[i].stackText.text = "1";
                }
            }
        }

    }

    public void DecreaseRecipePage()
    {
        if (currentPage > 1)
        {
            currentPage -= 1;
        }

        if (currentPage < 0)
        {
            currentPage = 1;
        }

        for (int i = 0; i < recipesPerPage; i++)
        {
            RecipeSlots[i].thisSlotsItem.enabled = false;
            RecipeSlots[i].stackText.text = "";
        }

        // 1 * 20 is greater than 1
        if ((currentPage * recipesPerPage) > ActiveListOfRecipes.Count)
        {
            for (int i = 0; i < recipesPerPage + (ActiveListOfRecipes.Count - (currentPage * recipesPerPage)); i++)
            {
                //we would like this to be 15 i think
                if (ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)] != null)
                {
                    displayedListOfRecipes[i] = ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)];
                    RecipeSlots[i].thisSlotsItem.enabled = true;
                    RecipeSlots[i].index = i;
                    RecipeSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/" + ItemDatabase.FetchByID(ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)].resultID).SpriteName) as Sprite;
                    RecipeSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                    RecipeSlots[i].stackText.text = "1";
                }
            }
        }
        else
        {
            for (int i = 0; i < recipesPerPage; i++)
            {
                if (ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)] != null)
                {
                    displayedListOfRecipes[i] = ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)];
                    RecipeSlots[i].thisSlotsItem.enabled = true;
                    RecipeSlots[i].index = i;
                    RecipeSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/" + ItemDatabase.FetchByID(ActiveListOfRecipes[i + ((currentPage * recipesPerPage) - recipesPerPage)].resultID).SpriteName) as Sprite;
                    RecipeSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                    RecipeSlots[i].GetComponent<Slot>().index = i;
                    RecipeSlots[i].stackText.text = "1";
                }
            }
        }

    }

    #endregion Recipe

    #region Debugging
    void InspectHoveringItemPushingI()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (isHovering && currentHoverType == HoverType.Inventory)
            {
                Debug.Log(inventory[hoverIndex].ComponentCount);

                Debug.Log(inventory[hoverIndex].componentsHave[0].thisComponent);

                Debug.Log(inventory[hoverIndex].componentsHave[0].amountOfThisComponent);

                if (inventory[hoverIndex].Type == 1)
                {
                    Melee conversion;
                    conversion = inventory[hoverIndex] as Melee;

                    for (int i = 0; i < conversion.dmgProperties.primaryDamageAmounts.Length; i++)
                    {
                        Debug.Log(conversion.dmgProperties.primaryDamageTypes[i]);
                        Debug.Log(conversion.dmgProperties.primaryDamageAmounts[i]);
                    }
                }
            }

            if (isHovering && currentHoverType == HoverType.Container)
            {
                Debug.Log(containerMangerRef.currentlyOpenedContainer.containerContents[hoverIndex].ComponentCount);

                Debug.Log(containerMangerRef.currentlyOpenedContainer.containerContents[hoverIndex].componentsHave[0].thisComponent);

                Debug.Log(containerMangerRef.currentlyOpenedContainer.containerContents[hoverIndex].componentsHave[0].amountOfThisComponent);

                Debug.Log(containerMangerRef.currentlyOpenedContainer.containerContents[hoverIndex].ID);

                Debug.Log(containerMangerRef.currentlyOpenedContainer.containerContents[hoverIndex].StackSize);
            }
        }
    }

    #endregion Debugging

    #region InventoryInteractionFunctions
    public static void AddItemAnywhere(Item itemToAdd, int amount)
    {
        _InvRef.AddItem(itemToAdd, amount);
    }

    public void AddItem(Item itemToAdd, int amount)
    {
        //check to see if we have the same id in our inventory already
        if (hasSameID(itemToAdd.ID))
        {
            //we know we have the same item we are looking for that isnt max stacked, so find it in our inventory list
            for (int x = 0; x < inventory.Length; x++)
            {
                //we found the same item, and its not max stacked
                if (inventory[x].ID == itemToAdd.ID && inventory[x].StackSize < inventory[x].MaxStackSize)
                {
                    Item conversion;
                    conversion = TypeChecker(itemToAdd);
                    //if the ammount we are adding is more than what that slot can hold
                    if (amount > inventory[x].MaxStackSize - inventory[x].StackSize)
                    {
                        //set the item to max stack size since we know we have more, remove the max stack size from the ammount
                        int leftover;
                        leftover = amount - (inventory[x].MaxStackSize - inventory[x].StackSize);
                        inventory[x].StackSize = inventory[x].MaxStackSize;
                        UpdateSlotUIAtIndex(x, slots.ToArray(), inventory);
                        AddItem(itemToAdd, leftover);
                        return;
                    }

                    //if the amount we are adding, is less than the max we can add, then add what we have
                    else if (amount <= inventory[x].MaxStackSize - inventory[x].StackSize)
                    {
                        inventory[x].StackSize += amount;
                        UpdateSlotUIAtIndex(x, slots.ToArray(), inventory);
                        return;
                    }
                }
            }
        }
        //if we have an empty slot
        else if (hasEmptySlot())
        {
            //we know we have an empty slot, so find it in our inventory list
            for (int x = 0; x < inventory.Length; x++)
            {
                //we found the empty slot
                if (inventory[x].ID == -1)
                {
                    Item conversion;
                    conversion = TypeChecker(itemToAdd);
                    if (amount > itemToAdd.MaxStackSize)
                    {
                        //set the item to max stack size since we know we have more, remove the max stack size from the ammount
                        int leftover;
                        inventory[x] = conversion;
                        inventory[x].StackSize = itemToAdd.MaxStackSize;
                        leftover = amount - itemToAdd.MaxStackSize;
                        UpdateSlotUIAtIndex(x, slots.ToArray(), inventory);
                        AddItem(itemToAdd, leftover);
                        return;
                    }

                    //if the amount we are adding, is less than the max we can add, then add what we have
                    else if (amount <= itemToAdd.MaxStackSize)
                    {
                        inventory[x] = conversion;
                        inventory[x].StackSize = amount;
                        UpdateSlotUIAtIndex(x, slots.ToArray(), inventory);
                        return;
                    }
                }
            }
        }
        //if there wasn't already the same item there with space remaining, and there wasn't an empty spot, then we simply couldnt pick this item up
        //use this line of code to spit out the remaining ammount i guess.
        else
        {
            Debug.Log("Couldn't add " + itemToAdd.Title + amount);

            GameObject clone;
            clone = Instantiate(Resources.Load("DropModels/" + itemToAdd.DropModel, typeof(GameObject))) as GameObject;
            clone.GetComponent<AddOnClick>().thisItem = itemToAdd;
            clone.GetComponent<AddOnClick>().Amnt = amount;

            //where do we drop the item we couldnt pick up at?

            //clone.transform.position = CharacterMovement.thisInstance.hit.point;
        }
    }

    //used to shift click items to and from a chest
    public void TransferItem(Item itemToAdd, int amount, Item[] transferTo, Item[] transferFrom)
    {
        //check to see if we have the same id in our transferTo already
        if (hasSameID(itemToAdd.ID, transferTo))
        {
            //we know we have the same item we are looking for that isnt max stacked, so find it in our transferTo list
            for (int x = 0; x < transferTo.Length; x++)
            {
                //we found the same item, and its not max stacked
                if (transferTo[x].ID == itemToAdd.ID && transferTo[x].StackSize < transferTo[x].MaxStackSize)
                {
                    Item conversion;
                    conversion = TypeChecker(itemToAdd);
                    //if the ammount we are adding is more than what that slot can hold
                    if (amount > transferTo[x].MaxStackSize - transferTo[x].StackSize)
                    {
                        //set the item to max stack size since we know we have more, remove the max stack size from the ammount
                        int leftover;
                        leftover = amount - (transferTo[x].MaxStackSize - transferTo[x].StackSize);
                        transferTo[x].StackSize = transferTo[x].MaxStackSize;

                        transferFrom[hoverIndex].StackSize -= amount;

                        if (transferFrom[hoverIndex].StackSize <= 0)
                        {
                            transferFrom[hoverIndex] = new Item();
                        }

                        UpdateAllSlotsFromArray(slots.ToArray(), inventory);

                        containerMangerRef.RefreshContainer();

                        TransferItem(itemToAdd, leftover, transferTo, transferFrom);
                        return;
                    }

                    //if the amount we are adding, is less than the max we can add, then add what we have
                    else if (amount <= transferTo[x].MaxStackSize - transferTo[x].StackSize)
                    {
                        transferTo[x].StackSize += amount;

                        transferFrom[hoverIndex].StackSize -= amount;

                        if (transferFrom[hoverIndex].StackSize <= 0)
                        {
                            transferFrom[hoverIndex] = new Item();
                        }

                        UpdateAllSlotsFromArray(slots.ToArray(), inventory);

                        containerMangerRef.RefreshContainer();
                        return;
                    }
                }
            }
        }
        //if we have an empty slot
        else if (hasEmptySlot(transferTo))
        {
            //we know we have an empty slot, so find it in our transferTo list
            for (int x = 0; x < transferTo.Length; x++)
            {
                //we found the empty slot
                if (transferTo[x].ID == -1)
                {
                    Item conversion;
                    conversion = TypeChecker(itemToAdd);
                    if (amount > itemToAdd.MaxStackSize)
                    {
                        //set the item to max stack size since we know we have more, remove the max stack size from the ammount
                        int leftover;
                        transferTo[x] = conversion;
                        transferTo[x].StackSize = itemToAdd.MaxStackSize;
                        leftover = amount - itemToAdd.MaxStackSize;

                        containerMangerRef.RefreshContainer();

                        UpdateAllSlotsFromArray(slots.ToArray(), inventory);

                        TransferItem(itemToAdd, leftover, transferTo, transferFrom);

                        return;
                    }

                    //if the amount we are adding, is less than the max we can add, then add what we have
                    else if (amount <= itemToAdd.MaxStackSize)
                    {
                        transferTo[x] = conversion;
                        transferTo[x].StackSize = amount;

                        transferFrom[hoverIndex].StackSize -= amount;

                        if (transferFrom[hoverIndex].StackSize <= 0)
                        {
                            transferFrom[hoverIndex] = new Item();
                        }

                        UpdateAllSlotsFromArray(slots.ToArray(), inventory);

                        containerMangerRef.RefreshContainer();

                        return;
                    }
                }
            }
        }
        //if there wasn't already the same item there with space remaining, and there wasn't an empty spot, then we simply couldnt pick this item up
        //use this line of code to spit out the remaining ammount i guess.
        else
        {
            Debug.Log("Couldn't add " + itemToAdd.Title + amount);
        }
    }

    //Important when new item types are added
    //update for each item type we have
    public Item TypeChecker(Item itemToCheck)
    {
        if (itemToCheck.Type == 0)
        {
            return new Item(itemToCheck as Item);
        }
        else if (itemToCheck.Type == 1)
        {
            return new Melee(itemToCheck as Melee);
        }
        return null;
    }

    void UpdateSlotUIAtIndex(int index, Slot[] slotsToUpdate, Item[] itemsToUpdate)
    {
        if (itemsToUpdate[index].ID != -1)
        {
            slotsToUpdate[index].thisSlotsItem.enabled = true;
            slotsToUpdate[index].thisSlotsItem.sprite = Resources.Load<Sprite>("Sprites/" + itemsToUpdate[index].SpriteName) as Sprite;
            slotsToUpdate[index].stackText.text = itemsToUpdate[index].StackSize.ToString();
        }

        else
        {
            slotsToUpdate[index].thisSlotsItem.enabled = false;
            slotsToUpdate[index].stackText.text = "";
        }
    }

    public void UpdateAllSlotsFromArray (Slot[] slotsToUpdate, Item[] itemsToUpdate)
    {
        for (int i = 0; i < slotsToUpdate.Length; i++)
        {

            if (itemsToUpdate[i].ID != -1)
            {
                slotsToUpdate[i].thisSlotsItem.enabled = true;
                slotsToUpdate[i].thisSlotsItem.sprite = Resources.Load<Sprite>("Sprites/" + itemsToUpdate[i].SpriteName) as Sprite;
                slotsToUpdate[i].stackText.text = itemsToUpdate[i].StackSize.ToString();
            }

            else
            {
                slotsToUpdate[i].thisSlotsItem.enabled = false;
                slotsToUpdate[i].stackText.text = "";
            }
        }
    }
    #endregion InventoryInteractionFunctions

    #region InventoryCheckingFunctions
    public bool hasEmptySlot()
    {
        for (int x = 0; x < inventory.Length; x++)
        {
            if (inventory[x].ID == -1)
            {
                return true;
            }
        }
        return false;
    }

    public bool hasEmptySlot(Item[] listToCheck)
    {
        for (int x = 0; x < listToCheck.Length; x++)
        {
            if (listToCheck[x].ID == -1)
            {
                return true;
            }
        }
        return false;
    }

    public bool hasSameID(int id)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].ID == id && inventory[i].StackSize < inventory[i].MaxStackSize)
            {
                return true;
            }
        }
        return false;
    }

    public bool hasSameID(int id, Item[] listToCheck)
    {
        for (int i = 0; i < listToCheck.Length; i++)
        {
            if (listToCheck[i].ID == id && listToCheck[i].StackSize < listToCheck[i].MaxStackSize)
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region HoveringData
    public static void ToggleHoverWithPosition(HoverType newHoverType, Vector3 pos, bool hoverState)
    {
        _InvRef.currentHoverType = newHoverType;
        _InvRef.isHovering = hoverState;
        _InvRef.hovPosition = pos;
    }

    public static void ToggleHoverWithPositionAndWindow(HoverType newHoverType, Vector3 pos, bool hoverState, UIWindow window)
    {
        _InvRef.currentHoverType = newHoverType;
        _InvRef.isHovering = hoverState;
        _InvRef.hovPosition = pos;
        _InvRef.hoveringWindow = window;
    }


    public static void ToggleHover(HoverType newHoverType, int newHoverIndex, bool hoverState)
    {
        _InvRef.currentHoverType = newHoverType;
        _InvRef.isHovering = hoverState;


        if (_InvRef.isHovering == true)
        {
            _InvRef.hoverIndex = newHoverIndex;
        }
    }

    public static void ToggleHover(HoverType newHoverType, bool hoverState)
    {
        _InvRef.currentHoverType = newHoverType;
        _InvRef.isHovering = hoverState;
    }

    public void SetHover(bool boolToSetTo)
    {
        isHovering = boolToSetTo;
    }

    #endregion HoveringData

    #region Saving/Loading
    public void PrintInventory()
	{
		for (int x = 0; x < inventory.Length; x++)
		{
			if (inventory[x].Type == 1) 
			{
				Melee conversion;
				conversion = inventory[x] as Melee;
			}
		}
	}

    //save the inventory, create a custom text file using json
    //old style sorta worked, but not precise enough, so i sorta build my own using for loops
    public void SaveInventory(string directory)
    {
        StringBuilder sb = new StringBuilder();
        JsonWriter writer = new JsonWriter(sb);
        writer.WriteArrayStart();

        //for each item we have in the inventory we custom build them
        for (int i = 0; i < inventory.Length; i++)
        {
            writer.WriteObjectStart();

            writer.WritePropertyName("Type");
            writer.Write(inventory[i].Type);

            writer.WritePropertyName("ID");
            writer.Write(inventory[i].ID);

            //if its an empty spot, just cut out now, break out
            if (inventory[i].ID == -1)
            {
                writer.WriteObjectEnd();
               // Debug.Log(sb.ToString());
                continue;
            }

            writer.WritePropertyName("Title");
            writer.Write(inventory[i].Title);

            writer.WritePropertyName("SpriteName");
            writer.Write(inventory[i].SpriteName);

            writer.WritePropertyName("DropModel");
            writer.Write(inventory[i].DropModel);

            writer.WritePropertyName("Value");
            writer.Write(inventory[i].Value);

            writer.WritePropertyName("StackSize");
            writer.Write(inventory[i].StackSize);

            writer.WritePropertyName("MaxStackSize");
            writer.Write(inventory[i].MaxStackSize);

            writer.WritePropertyName("Description");
            writer.Write(inventory[i].Description);

            //if its a equipable weapons, add on the associated variables
            if (inventory[i].Type == 1)
            {
                Melee item;
                item = inventory[i] as Melee;

                writer.WritePropertyName("EquipPrefab");
                writer.Write(item.equipPrefab);

                writer.WritePropertyName("Range");
                writer.Write(item.range);

                writer.WritePropertyName("AttackSpeed");
                writer.Write(item.attackSpeed);



                for (int x = 0; x < item.dmgProperties.primaryDamageCount; x++)
                {
                    if (item.dmgProperties.primaryDamageTypes[x] == PrimaryDamageType.Melee)
                    {
                        writer.WritePropertyName("Melee");
                        writer.Write(item.dmgProperties.primaryDamageAmounts[x]);
                    }

                    if (item.dmgProperties.primaryDamageTypes[x] == PrimaryDamageType.Magic)
                    {
                        writer.WritePropertyName("Magic");
                        writer.Write(item.dmgProperties.primaryDamageAmounts[x]);
                    }

                    if (item.dmgProperties.primaryDamageTypes[x] == PrimaryDamageType.Ranged)
                    {
                        writer.WritePropertyName("Ranged");
                        writer.Write(item.dmgProperties.primaryDamageAmounts[x]);
                    }
                }
            }


            if (inventory[i].ComponentCount > 0)
            {
                writer.WritePropertyName("ComponentCount");
                writer.Write(inventory[i].ComponentCount);

                for (int c = 0; c < inventory[i].ComponentCount; c++)
                {
                    writer.WritePropertyName(c + "ComponentType");
                    writer.Write(inventory[i].componentsHave[c].thisComponent.ToString());

                    writer.WritePropertyName(c + "ComponentAmount");
                    writer.Write(inventory[i].componentsHave[c].amountOfThisComponent);
                }
                writer.WriteObjectEnd();
            }
            else
            {

                writer.WriteObjectEnd();
            }
           // Debug.Log(sb.ToString());
        }

        writer.WriteArrayEnd();
        
        ///write all the data down as a custom text file to the asset datapath.
        File.WriteAllText (directory, sb.ToString());
    }

    public void LoadInventory(string directory)
	{
        inventory = new Item[inventorySize];

		JsonData JsonToLoad = JsonMapper.ToObject (File.ReadAllText (directory));
		for (int i = 0; i < JsonToLoad.Count; i++) 
		{
			Item baseItem;

            ComponentTypeAndAmount[] tempList = new ComponentTypeAndAmount[1];
            int componentCount = 0;

            if (JsonToLoad[i].Keys.Contains("ComponentCount"))
            {
                componentCount = (int)JsonToLoad[i]["ComponentCount"];
                tempList = new ComponentTypeAndAmount[componentCount];
                for (int c = 0; c < componentCount; c++)
                {
                    tempList[c].thisComponent = (ComponentType)System.Enum.Parse(typeof(ComponentType), (string)JsonToLoad[i][c + "ComponentType"]);
                    tempList[c].amountOfThisComponent = (int)JsonToLoad[i][c + "ComponentAmount"];
                }
            }

            else
            {
                componentCount = 0;
                tempList[0].thisComponent = ComponentType.Nothing;
                tempList[0].amountOfThisComponent = (int)JsonToLoad[i][0];
            }

            if ((int)JsonToLoad[i]["ID"] == -1)
            {
                baseItem = new Item();
            }

            else
            {
                baseItem = new Item
                (
                    (int)JsonToLoad[i]["Type"],
                    (int)JsonToLoad[i]["ID"],
                    (string)JsonToLoad[i]["Title"],
                    (string)JsonToLoad[i]["SpriteName"],
                    (string)JsonToLoad[i]["DropModel"],
                    (int)JsonToLoad[i]["Value"],
                    (int)JsonToLoad[i]["StackSize"],
                    (int)JsonToLoad[i]["MaxStackSize"],
                    (string)JsonToLoad[i]["Description"],
                    componentCount,
                    tempList
                );
            }

            if ((int)JsonToLoad[i]["Type"] == 0)
            {
                inventory[i] = baseItem;
                UpdateSlotUIAtIndex(i, slots.ToArray(), inventory);
            }

            else if ((int)JsonToLoad[i]["Type"] == 1)
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

                conversion.equipPrefab = (string)JsonToLoad[i]["EquipPrefab"];

                conversion.range = (double)JsonToLoad[i]["Range"];

                conversion.attackSpeed = (double)JsonToLoad[i]["AttackSpeed"];


                conversion.dmgProperties.primaryDamageTypes = new PrimaryDamageType[conversion.dmgProperties.primaryDamageCount];

                conversion.dmgProperties.primaryDamageAmounts = new double[conversion.dmgProperties.primaryDamageCount];


                conversion.dmgProperties.thisAdditonalDamageTypes = new AdditionalDamageType[conversion.dmgProperties.additonalDamageCount];

                conversion.dmgProperties.additonalDamageAmounts = new double[conversion.dmgProperties.additonalDamageCount];


                int currentIndex = 0;
                //if it has the keyword blunt, add blunt damage to the list of damage we can do
                if (JsonToLoad[i].Keys.Contains("Melee"))
                {
                    conversion.dmgProperties.primaryDamageTypes[currentIndex] = PrimaryDamageType.Melee;
                    conversion.dmgProperties.primaryDamageAmounts[currentIndex] = (double)JsonToLoad[i]["Melee"];
                    currentIndex += 1;
                }

                if (JsonToLoad[i].Keys.Contains("Magic"))
                {
                    conversion.dmgProperties.primaryDamageTypes[currentIndex] = PrimaryDamageType.Magic;
                    conversion.dmgProperties.primaryDamageAmounts[currentIndex] = (double)JsonToLoad[i]["Magic"];
                    currentIndex += 1;
                }

                if (JsonToLoad[i].Keys.Contains("Ranged"))
                {
                    conversion.dmgProperties.primaryDamageTypes[currentIndex] = PrimaryDamageType.Ranged;
                    conversion.dmgProperties.primaryDamageAmounts[currentIndex] = (double)JsonToLoad[i]["Ranged"];
                    currentIndex += 1;
                }

                inventory[i] = conversion;
                UpdateSlotUIAtIndex(i, slots.ToArray(), inventory);
            }
		}
	}
    #endregion Saving/Loading
}

public enum UIInteractionType
{
    GrabItemNotDragging,
    DropItemNotDragging,
    GrabItemDragging,
    DropItemDragging,
    Transfer,
    Inventory,
    Hotbar,
    Ect
}

public enum UIinteractionCategory
{
    Inventory,
    Container,
    Shop,
    Crafting,
    Ect
}


