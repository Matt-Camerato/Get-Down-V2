using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelectorManager : MonoBehaviour
{
    [HideInInspector] public Image currentPlayerColorDisplay;

    public void PickColor(Image image) //selects the color of whatever button was pressed and sets it to this player's color
    {
        currentPlayerColorDisplay.color = image.color; //set color for current player's display
        currentPlayerColorDisplay.transform.parent.parent.GetComponent<ListPlayerManager>().playerColor = image.color; //set color on current player manager

        gameObject.SetActive(false); //turn off color selection screen
    }
}
