using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VLAT_MenuNavigator : MonoBehaviour
{

    // VLAT_MenuNavigator allows menu navigation via VERA controls


    #region VARIABLES


    //[Tooltip("If true, the menu navigator will attempt to automatically detect UI elements and add them to the orderedMenuItems list on runtime")]
    //[SerializeField] private bool autoFindItems = false;
    [Tooltip("A list of the interactable UI elements in this menu, in the order as they should be navigated")]
    [SerializeField] private List<GameObject> orderedMenuItems = new List<GameObject>();
    [SerializeField] private bool hideVLATMenuOnNav = true;

    private bool controllingMenu;
    private int currentlyHighlightedItem = 0;
    private NewMenuNavigation menuNavigation;

    private bool allowExit;

    public UnityEvent onChangeHighlightedItem;

    private bool loggedVlatMenuWarning = false;


    #endregion


    #region MONOBEHAVIOUR


    // Manually sets up the menu navigator
    //--------------------------------------//
    public void ManualSetup()
    //--------------------------------------//
    {
        if (menuNavigation == null)
            menuNavigation = FindObjectOfType<NewMenuNavigation>();
        if (onChangeHighlightedItem == null)
            onChangeHighlightedItem = new UnityEvent();

        if (menuNavigation == null)
        {
            LogVlatNotPresentWarning();
            return;
        }

    } // END ManualSetup


    // Start
    //--------------------------------------//
    private void Start()
    //--------------------------------------//
    {
        ManualSetup();
    
    } // END Start


    // Logs that the VLAT menu is not present (but only does so once to not spam)
    //--------------------------------------//
    private void LogVlatNotPresentWarning()
    //--------------------------------------//
    {
        if (!loggedVlatMenuWarning)
        {
            loggedVlatMenuWarning = true;
            Debug.LogWarning("VLAT_MenuNavigator is in scene, but no VLAT_Menu prefab is present. " +
                "Add a VLAT_Menu prefab to enable menu navigation.");
        }
        
    } // END LogVlatNotPresentWarning


    #endregion


    #region ON/OFF


    // Starts menu navigation
    //--------------------------------------//
    public void StartMenuNavigation(bool allowExitNav)
    //--------------------------------------//
    {
        if (menuNavigation == null)
        {
            menuNavigation = FindObjectOfType<NewMenuNavigation>();
            if (menuNavigation == null)
            {
                LogVlatNotPresentWarning();
                return;
            }
        }

        allowExit = allowExitNav;

        menuNavigation.StartNavigateExternalMenu(this, hideVLATMenuOnNav);
        currentlyHighlightedItem = 0;
        HighlightItem(currentlyHighlightedItem);

    } // END StartMenuNavigation


    // Override of above
    public void StartMenuNavigation()
    {
        StartMenuNavigation(false);
    }


    // Stops menu navigation
    //--------------------------------------//
    public void StopMenuNavigation()
    //--------------------------------------//
    {
        if (menuNavigation == null)
        {
            menuNavigation = FindObjectOfType<NewMenuNavigation>();
            if (menuNavigation == null)
            {
                LogVlatNotPresentWarning();
                return;
            }
        }
        StopHighlightItem(currentlyHighlightedItem);
        menuNavigation.StopNavigateExternalMenu();

    } // END StopMenuNavigation


    #endregion


    #region SET NAVIGATABLE ITEMS


    // Gets the currently highlighted item
    //--------------------------------------//
    public GameObject GetHighlightedItem()
    //--------------------------------------//
    {
        if (currentlyHighlightedItem < 0 || currentlyHighlightedItem >= orderedMenuItems.Count || menuNavigation == null)
            return null;
        else
            return orderedMenuItems[currentlyHighlightedItem];

    } // END GetHighlightedItem


    // Sets the navigatable items of this menu
    //--------------------------------------//
    public void SetNavigatableItems(List<GameObject> navigatableItems)
    //--------------------------------------//
    {
        StopHighlightItem(currentlyHighlightedItem);
        orderedMenuItems = navigatableItems;
        currentlyHighlightedItem = 0;
        HighlightItem(currentlyHighlightedItem);

    } // END SetNavigatableItems


    // Clears the navigatable items of this menu
    //--------------------------------------//
    public void ClearNavigatableItems()
    //--------------------------------------//
    {
        StopHighlightItem(currentlyHighlightedItem);
        orderedMenuItems.Clear();
        currentlyHighlightedItem = 0;

    } // END ClearNavigatableItems


    // Adds an item to the navigatable items of this menu
    //--------------------------------------//
    public void AddNavigatableItem(GameObject itemToAdd)
    //--------------------------------------//
    {
        orderedMenuItems.Add(itemToAdd);
        if (orderedMenuItems.Count == 1)
        {
            currentlyHighlightedItem = 0;
            HighlightItem(currentlyHighlightedItem);
        }
        
    } // END AddNavigatableItems


    #endregion


    #region TABBING


    // Tabs to next item
    //--------------------------------------//
    public void TabNext()
    //--------------------------------------//
    {
        StopHighlightItem(currentlyHighlightedItem);
        currentlyHighlightedItem++;

        if (currentlyHighlightedItem >= orderedMenuItems.Count) 
        {
            currentlyHighlightedItem = 0;
        }

        HighlightItem(currentlyHighlightedItem);

    } // END TabNext


    // Tabs to previous item
    //--------------------------------------//
    public void TabPrevious()
    //--------------------------------------//
    {
        StopHighlightItem(currentlyHighlightedItem);
        currentlyHighlightedItem--;

        if (currentlyHighlightedItem < 0)
        {
            currentlyHighlightedItem = orderedMenuItems.Count - 1;
        }

        HighlightItem(currentlyHighlightedItem);

    } // END TabPrevious


    // Selects current item
    //--------------------------------------//
    public void SelectItem()
    //--------------------------------------//
    {
        if (currentlyHighlightedItem < 0 || currentlyHighlightedItem >= orderedMenuItems.Count || menuNavigation == null)
            return;

        Toggle toggleCheck = orderedMenuItems[currentlyHighlightedItem].GetComponent<Toggle>();
        Slider sliderCheck = orderedMenuItems[currentlyHighlightedItem].GetComponent<Slider>();
        Button buttonCheck = orderedMenuItems[currentlyHighlightedItem].GetComponent<Button>();

        // Toggle
        if (toggleCheck != null)
        {
            toggleCheck.isOn = !toggleCheck.isOn;
        }
        // Slider
        else if (sliderCheck != null)
        {
            // Whole numbers
            if (sliderCheck.wholeNumbers)
            {
                // Small increments
                if (sliderCheck.maxValue - sliderCheck.minValue <= 10)
                {
                    if (sliderCheck.value >= sliderCheck.maxValue)
                    {
                        sliderCheck.value = sliderCheck.minValue;
                    }
                    else
                    {
                        sliderCheck.value = sliderCheck.value + 1;
                    }
                }
                // Big increments
                else
                {
                    if (sliderCheck.value >= sliderCheck.maxValue)
                    {
                        sliderCheck.value = sliderCheck.minValue;
                    }
                    else
                    {
                        float increment = (sliderCheck.maxValue - sliderCheck.minValue) / 10f;
                        if (sliderCheck.value + increment > sliderCheck.maxValue)
                        {
                            sliderCheck.value = sliderCheck.maxValue;
                        }
                        else
                        {
                            sliderCheck.value = sliderCheck.value + increment;
                        }
                    }
                }
            }
            // Decimal numbers
            else
            {
                if (sliderCheck.value == sliderCheck.maxValue)
                {
                    sliderCheck.value = sliderCheck.minValue;
                }
                else
                {
                    float increment = (sliderCheck.maxValue - sliderCheck.minValue) / 10f;
                    if (sliderCheck.value + increment > sliderCheck.maxValue)
                    {
                        sliderCheck.value = sliderCheck.maxValue;
                    }
                    else
                    {
                        sliderCheck.value = sliderCheck.value + increment;
                    }
                }
            }
        }
        else if (buttonCheck != null)
        {
            if (buttonCheck.interactable)
                buttonCheck.onClick.Invoke();
        }
        else
        {
            LogSupportError();
        }

    } // END SelectItem


    // Goes back
    //--------------------------------------//
    public void BackButton()
    //--------------------------------------//
    {
        if (allowExit)
            StopMenuNavigation();

    } // END BackButton


    // Logs an error about unsupported ui type
    //--------------------------------------//
    private void LogSupportError()
    //--------------------------------------//
    {
        Debug.LogError("UI Element type not yet supported by VLAT menu navigator (support currently includes toggles, sliders, and buttons)");

    } // END LogSupportError


    #endregion


    #region HIGHLIGHT CONTROL


    // Highlights an item
    //--------------------------------------//
    private void HighlightItem(int itemToHighlight)
    //--------------------------------------//
    {
        if (itemToHighlight < 0 || itemToHighlight >= orderedMenuItems.Count || menuNavigation == null)
            return;

        HighlightableTab newTab = null;

        Toggle toggleCheck = orderedMenuItems[itemToHighlight].GetComponent<Toggle>();
        Slider sliderCheck = orderedMenuItems[itemToHighlight].GetComponent<Slider>();
        Button buttonCheck = orderedMenuItems[itemToHighlight].GetComponent<Button>();

        if (toggleCheck != null)
        {
            newTab = GameObject.Instantiate(menuNavigation.toggleHighlightPrefab, orderedMenuItems[itemToHighlight].transform, false);
            newTab.transform.SetAsFirstSibling();

            foreach(Transform child in orderedMenuItems[itemToHighlight].transform)
            {
                if (child.GetComponent<HighlightableTab>() == null)
                {
                    RectTransform targetTrans = child.gameObject.GetComponent<RectTransform>();
                    RectTransform tabTrans = newTab.GetComponent<RectTransform>();
                    tabTrans.sizeDelta = targetTrans.sizeDelta + new Vector2(10, 10);
                    tabTrans.localPosition = targetTrans.localPosition;

                    break;
                }
            }            
        }
        else if (sliderCheck != null)
        {
            newTab = GameObject.Instantiate(menuNavigation.sliderHighlightPrefab, orderedMenuItems[itemToHighlight].transform, false);
            newTab.transform.SetAsFirstSibling();
            RectTransform targetTrans = sliderCheck.gameObject.GetComponent<RectTransform>();
            RectTransform tabTrans = newTab.GetComponent<RectTransform>();
            //tabTrans.sizeDelta = targetTrans.sizeDelta + new Vector2(10, 10);
            tabTrans.position = targetTrans.position;
        }
        else if (buttonCheck != null)
        {
            newTab = GameObject.Instantiate(menuNavigation.buttonHighlightPrefab, orderedMenuItems[itemToHighlight].transform, false);
            newTab.transform.SetAsFirstSibling();
            RectTransform targetTrans = buttonCheck.gameObject.GetComponent<RectTransform>();
            RectTransform tabTrans = newTab.GetComponent<RectTransform>();
            tabTrans.sizeDelta = targetTrans.sizeDelta + new Vector2(10, 10);
        }
        else
        {
            LogSupportError();
        }

        newTab.StartHighlight();
        onChangeHighlightedItem?.Invoke();

    } // END HighlightItem


    // Stops highlighting an item
    //--------------------------------------//
    private void StopHighlightItem(int itemToStopHighlight)
    //--------------------------------------//
    {
        if (itemToStopHighlight < 0 || itemToStopHighlight >= orderedMenuItems.Count || menuNavigation == null)
            return;

        HighlightableTab tab = orderedMenuItems[itemToStopHighlight].transform.GetChild(0).GetComponent<HighlightableTab>();
        
        if (tab != null)
        {
            tab.EndHighlight();
            GameObject.Destroy(tab.gameObject, .25f);
        }

    } // END StopHighlightItem


    #endregion


} // END VLAT_MenuNavigator
