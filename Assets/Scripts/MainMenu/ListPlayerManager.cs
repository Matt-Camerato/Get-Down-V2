using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListPlayerManager : MonoBehaviour
{
    public bool validName = false;

    public string playerName;
    public Color playerColor;

    private GameObject colorSelector;
    private Image colorDisplay;
    

    private void Start()
    {
        colorSelector = transform.parent.parent.GetChild(transform.parent.parent.childCount - 1).gameObject;
        colorDisplay = transform.GetChild(0).GetChild(0).GetComponent<Image>();

        AssignRandomColor();
    }

    private void AssignRandomColor()
    {
        List<Button> availableColors = new List<Button>();

        foreach (Transform child in colorSelector.transform.GetChild(0)) //loop through all color buttons to determine which ones are available to be automatically assigned
        {
            if (!child.GetComponent<Button>().interactable) continue; //if color is already selected, check next color

            availableColors.Add(child.GetComponent<Button>()); //if not, add button to available color list
        }

        Button randomColor = availableColors[Random.Range(0, availableColors.Count)]; //select random color from available colors
        colorDisplay.color = randomColor.GetComponent<Image>().color; //set color display for this player
        playerColor = randomColor.GetComponent<Image>().color; //set color variable for this player

        randomColor.interactable = false; //make button for selected color uninteractable on color selection screen
        randomColor.transform.GetChild(0).gameObject.SetActive(true); //turn on X overlay for selected color button on color selection screen
    }

    public void SelectColor()
    {
        colorSelector.SetActive(true); //turn on color selection screen
        colorSelector.GetComponent<ColorSelectorManager>().currentPlayerColorDisplay = colorDisplay; //set this player as one currently selecting a color

        foreach (Transform child in colorSelector.transform.GetChild(0)) //loop through all color buttons to find one that matches this player's current color
        {
            if (child.GetComponent<Image>().color == colorDisplay.color) //if colors match, this is current player's color button
            {
                //if this is player's current color, re-enable button and disable X overlay
                child.GetComponent<Button>().interactable = true;
                child.GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    public void EditName(string name)
    {
        playerName = name;
        validName = false;

        //simple check to make sure name has at least 1 non-space character (meaning it isn't blank)
        for (int i = 0; i < name.Length; i++)
        {
            if(name[i] != ' ')
            {
                validName = true;
                break;
            }
        }

        transform.parent.parent.parent.GetComponent<MainMenuUI>().UpdateBeginButton(); //update interactability of begin button
    }

    public void RemovePlayer()
    {
        GameSettings.instance.numPlayers--; //decrease player count

        foreach (Transform child in colorSelector.transform.GetChild(0)) //loop through all color buttons to find one that matches this player's current color
        {
            if (child.GetComponent<Image>().color == colorDisplay.color) //if colors match, this is current player's color button
            {
                //if this is player's current color, re-enable button and disable X overlay
                child.GetComponent<Button>().interactable = true;
                child.GetChild(0).gameObject.SetActive(false);
            }
        }

        transform.parent.GetChild(transform.parent.childCount - 1).gameObject.SetActive(true); //activate add new player button whenever a player is removed, as there should always be room for a new one after removing one

        Destroy(gameObject); //destroy this list player prefab
    }
}
