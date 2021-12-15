using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    [Header("Game Settings")]
    public int maxAttacks = 3; //max attacks per turn
    public int maxPlayersDamagedPerTurn;

    [Header("Current Game Info")]
    public int numPlayers = 0;
    public List<string> playerNames;
    public List<Color> playerColors;

    static public GameSettings instance; //singleton intance of current game settings
    
    private void Awake()
    {
        if(instance != null) return;
        instance = this; //create singleton instance of game settings that can be accessed from anywhere
        DontDestroyOnLoad(gameObject); //also allow instance to persist across scenes
    }

    public void SaveNamesAndColors(Transform playerListParent) //save player info to game settings before game begins (also determine max players damaged per turn based on # of players)
    {
        for(int i = 0; i < playerListParent.childCount - 1; i++)
        {
            ListPlayerManager currentPlayer = playerListParent.GetChild(i).GetComponent<ListPlayerManager>();
            playerNames.Add(currentPlayer.playerName);
            playerColors.Add(currentPlayer.playerColor);
        }

        maxPlayersDamagedPerTurn = numPlayers - 1; //max # of players that can be damaged each turn is one less than the # of players currently playing
    }

    public void ResetGameInfo()
    {
        numPlayers = 0;
        playerNames.Clear();
        playerColors.Clear();
    }
}
