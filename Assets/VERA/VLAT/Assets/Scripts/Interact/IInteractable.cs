using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{

    // IInteractable is an interface which provides general functionality for all interactables
    // All VERA Interactables inherit from this interface


    #region FUNCTIONS


    public List<string> GetInteractions();
    public void TriggerInteraction(string interaction);


    #endregion


} // END IInteractable.cs
