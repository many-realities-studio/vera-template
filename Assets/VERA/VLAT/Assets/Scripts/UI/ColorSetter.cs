using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSetter : MonoBehaviour
{
    
    // ColorSetter sets the image color to whatever color type specified


    #region VARIABLES


    [SerializeField] private GlobalColorManager.ColorType colorType;


    #endregion


    #region MONOBEHAVIOUR


    // Start
    //--------------------------------------//
    void Start()
    //--------------------------------------//
    {
        StartCoroutine(SetColorCoroutine());

    } // END Start


    // Sets the color
    //--------------------------------------//
    private IEnumerator SetColorCoroutine()
    //--------------------------------------//
    {
        yield return null;

        Image img = GetComponent<Image>();

        if (img != null)
        {
            switch (colorType)
            {
                case GlobalColorManager.ColorType.Look:
                    img.color = GlobalColorManager.Instance.lookColorLight;
                    break;
                case GlobalColorManager.ColorType.Move:
                    img.color = GlobalColorManager.Instance.moveColorLight;
                    break;
                case GlobalColorManager.ColorType.Interact:
                    img.color = GlobalColorManager.Instance.interactColorLight;
                    break;
                case GlobalColorManager.ColorType.Settings:
                    img.color = GlobalColorManager.Instance.settingsColorLight;
                    break;
                case GlobalColorManager.ColorType.Highlight:
                    img.color = GlobalColorManager.Instance.highlightColor;
                    break;
            }
        }

    } // END SetColorCoroutine


    #endregion


} // END ColorSetter.cs
