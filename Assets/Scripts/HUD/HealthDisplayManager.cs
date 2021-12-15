using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplayManager : MonoBehaviour
{
    [SerializeField] private Sprite emptyHeartSprite;

    public void LoseHealth() //called whenever on this player's turn during recap phase and they are damaged
    {
        switch (transform.parent.GetComponent<PlayerManager>().savedHealth)
        {
            case 2:
                transform.GetChild(2).GetComponent<Image>().sprite = emptyHeartSprite;
                break;
            case 1:
                transform.GetChild(1).GetComponent<Image>().sprite = emptyHeartSprite;
                break;
            case 0:
                transform.GetChild(0).GetComponent<Image>().sprite = emptyHeartSprite;
                break;
            default:
                Debug.Log("Cannot set health display with negative saved health value!");
                break;
        }
    }
}
