using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Info")]
    [ReadOnly] public string playerName;
    [ReadOnly] public int playerNum;
    [ReadOnly] public Color playerColor;
    [ReadOnly] public int health = 3;
    [ReadOnly] public Vector2Int pos;
    [ReadOnly] public Vector2Int destination;
    [ReadOnly] public List<Vector2Int> attacks = new List<Vector2Int>();

    [Header("Saved Round Info")]
    [ReadOnly] public int savedHealth = 3;
    [ReadOnly] public Vector2Int savedPos;
    [ReadOnly] public List<Vector2Int> savedAttacks;

    private void Start() //on startup, setup panel and variables based on saved game settings (from main menu scene)
    {
        playerName = GameSettings.instance.playerNames[playerNum]; //set player name variable
        playerColor = GameSettings.instance.playerColors[playerNum]; //set player color variable
        transform.GetChild(0).GetComponent<TMP_Text>().text = playerName; //set panel text to player name
        GetComponent<Image>().color = playerColor; //set panel color to player color

        if (playerNum == 0) GameController.instance.currentPlayer = this; //if this is first player, set them as current player when spawned

        GetComponent<CanvasGroup>().alpha = 0; //all player panels start invisible and are turned on during update header animation
    }

    public List<Vector2Int> PossibleMoves()
    {
        List<Vector2Int> possibleMoves = new List<Vector2Int>();

        //move right
        Vector2Int rightMove = pos + Vector2Int.right;
        if (rightMove.x <= GameboardManager.instance.size - 1) possibleMoves.Add(rightMove);

        //move left
        Vector2Int leftMove = pos + Vector2Int.left;
        if (leftMove.x >= 0) possibleMoves.Add(leftMove);

        //move up
        Vector2Int upMove = pos + Vector2Int.down;
        if (upMove.y >= 0) possibleMoves.Add(upMove);

        //move down
        Vector2Int downMove = pos + Vector2Int.up;
        if (downMove.y <= GameboardManager.instance.size - 1) possibleMoves.Add(downMove);

        return possibleMoves;
    }

    public void SaveRoundInfo()
    {
        health = savedHealth; //health is backwards because player will die from saved health before updating actual health

        //save current position and attacks for use in recap phase
        savedPos = pos;
        savedAttacks = new List<Vector2Int>(); //reset saved attacks list
        foreach(Vector2Int TC in attacks) savedAttacks.Add(TC); //save each attack individually

        pos = destination; //once position is saved, it is updated to player's selected destination
    }
}