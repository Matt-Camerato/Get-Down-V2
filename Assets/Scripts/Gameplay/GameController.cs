using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    static public GameController instance;

    [Header("UI References")]
    public Transform playerPanels; //parent of player panels on HUD
    [SerializeField] private GameObject playerPanelPrefab; //prefab to spawn player panels on HUD
    [SerializeField] private Button confirmMoveButton;
    [SerializeField] private Button confirmAttacksButton;

    [Header("Current Game State Info")]
    public int roundNum = 1;
    public int turnNum = 1;
    public string currentPhase = "move"; //used to determine what should be shown on footer tab as well as help card when image is detected

    [HideInInspector] public bool lastRound = false; //used to determine if game is over and just needs to show recaps
    [HideInInspector] public bool gameOver = false;
    [HideInInspector] public PlayerManager currentPlayer;
    [HideInInspector] public bool tileSelectEnabled = true;

    [HideInInspector] public PlayerManager winner; //this will be set once winner has been determined (during first recap phase of the last round)

    private void Awake()
    {
        if (instance != null) return;
        instance = this; //create singleton instance of game controller that can be accessed from anywhere
    }

    private void Start() //on startup, instantiate player panels based on # of players in current game and set current player to the first one
    {
        for(int i = 0; i < GameSettings.instance.numPlayers; i++)
        {
            GameObject newPlayerPanel = Instantiate(playerPanelPrefab, playerPanels);
            newPlayerPanel.name = GameSettings.instance.playerNames[i]; //set panel name to "player name"
            newPlayerPanel.GetComponent<PlayerManager>().playerNum = i;
        }
    }

    public void StartTurn()
    {
        if (roundNum > 1) //if not on round 1, start on recap phase
        {
            currentPhase = "recap";
            //RecapManager.instance.StartRecap(); <--This is no longer done because game waits for player to start the recap by pressing "Begin Recap" button
            return;
        }

        //if still round 1, skip recap phase and start on move phase
        currentPhase = "move";
        StartMovePhase();
    }

    #region === Move Phase ===

    private bool moveSelected = false;
    private List<Vector2Int> currentPossibleMoves = new List<Vector2Int>();

    public void StartMovePhase()
    {
        tileSelectEnabled = true; //enable tile selection for move phase
        moveSelected = false; //reset move selected bool since no moves are initially selected
        currentPossibleMoves = currentPlayer.PossibleMoves(); //get current player's possible moves

        foreach(Vector2Int possibleMove in currentPossibleMoves)
        {
            GameboardManager.instance.ToggleIndicator(possibleMove); //turn on indicators for tile cubes that are possible moves 
            GameboardManager.instance.SetIndicatorColor(possibleMove, "unselected"); //also set indicators to unselected color
        }
    }

    public void HandleMoveSelect(Vector2Int TC) //called by tile select script when a tile is selected during move phase
    {
        if (!currentPossibleMoves.Contains(TC)) return; //if not a possible move, this tile cube cannot be selected

        if (!moveSelected)
        {
            confirmMoveButton.interactable = true; //if move hasn't been selected, make confirm move button interactable (this is first time player has tapped on tile in move phase)
        }
        else
        {
            //if move has been selected, unselect old move first
            Vector2Int oldTC = currentPlayer.destination;
            GameboardManager.instance.SetIndicatorColor(oldTC, "unselected"); //set old TC indicator back to unselected color
        }

        GameboardManager.instance.SetIndicatorColor(TC, "selected"); //set new TC indicator to selected color
        currentPlayer.destination = TC; //set selected tile cube as current player's destination

        moveSelected = true;
    }

    #endregion

    #region === Attack Phase ===

    public void StartAttackPhase()
    {
        currentPhase = "attack"; //update current phase

        currentPlayer.attacks.Clear(); //before start of attack phase, clear current player's attack list from previous rounds

        foreach (Vector2Int possibleMove in currentPossibleMoves) //loop through possible moves and turn off TC indicators (except the one for current player's destination)
        {
            if (possibleMove == currentPlayer.destination) continue; //skip over indicator for current player's destination

            GameboardManager.instance.ToggleIndicator(possibleMove); //if not current player's destination, turn off the indicator
        }
    }

    public void HandleAttackSelect(Vector2Int TC) //called by tile select script when a tile is selected during move phase
    {
        if (TC == currentPlayer.destination) return; //player can attack any space but their destination

        if (currentPlayer.attacks.Contains(TC)) //if player selects already chosen tile, deselect it as an attack
        {
            currentPlayer.attacks.Remove(TC); //remove TC from attacks list
            GameboardManager.instance.ToggleIndicator(TC); //turn off TC indicator
            confirmAttacksButton.interactable = false; //make confirm attacks button uninteractable if any attacks are deselected (meaning player hasn't selected max attacks)
            return;
        }

        //if not an already chosen tile, select it as an attack
        if (currentPlayer.attacks.Count >= GameSettings.instance.maxAttacks) return; //player can only select new attack if they dont have max attacks selected

        currentPlayer.attacks.Add(TC); //add TC to attacks list
        GameboardManager.instance.ToggleIndicator(TC); //turn on TC indicator
        GameboardManager.instance.SetIndicatorColor(TC, "playercolor"); //set TC indicator color to current player's color

        if (currentPlayer.attacks.Count == GameSettings.instance.maxAttacks) confirmAttacksButton.interactable = true; //make confirm attacks button interactable if max attacks are selected
    }

    #endregion

    #region === End of Turn ===

    public void EndTurn()
    {
        tileSelectEnabled = false; //disable tile selection after confirming attacks
       
        GameboardManager.instance.ResetIndicators(); //turn off any TC indicator that is still on at end of turn

        do IncrementTurnNum(); //increment turn number
        while (playerPanels.GetChild(turnNum - 1).GetComponent<PlayerManager>().health == 0); //if next player is already dead, keep incrementing turn number until on an alive player's turn
    }

    private void IncrementTurnNum()
    {
        turnNum++; //increment turn number
        if (turnNum <= GameSettings.instance.numPlayers) return; //return if not end of round

        if (lastRound) gameOver = true; //if last round is true at end of turn, game is over (winner variable is already set at this point)

        //if end of round, set turn number to 1 and increment round number instead
        turnNum = 1;
        roundNum++;

        RecapManager.instance.determinedHighestPlayer = false; //make sure highest player is recalculated on first recap phase of next round

        RecapManager.instance.UpdateSpawnedCubesList();
        GameboardManager.instance.SaveCubeCounts(); //save current cube counts of all TCs (so they can be reset for each recap phase)

        FixDestinations(); //before saving player info, fix any players who have the same destination
        foreach (Transform p in playerPanels)
        {
            if (p.GetComponent<PlayerManager>().health != 0) p.GetComponent<PlayerManager>().SaveRoundInfo(); //save all player info from this round onto player managers (if player isn't already dead)
        }
    }

    private void FixDestinations()
    {
        List<Vector2Int> destinations = new List<Vector2Int>(); //list of all destinations
        List<Vector2Int> duplicates = new List<Vector2Int>(); //list of duplicate destinations

        do
        {
            //reset lists at the start of each check
            destinations.Clear();
            duplicates.Clear();

            foreach (Transform p in playerPanels) //loop through players and setup list of duplicate destinations
            {
                PlayerManager pm = p.GetComponent<PlayerManager>(); //get player manager reference
                if (pm.savedHealth == 0) continue; //skip player if they are already dead or died this round (meaning their destination for next round is irrelevant)

                if (destinations.Contains(pm.destination)) duplicates.Add(pm.destination); //if destination already chosen, mark as duplicate
                else destinations.Add(pm.destination); //if not already chosen, add it to list of destinations
            }

            foreach (Vector2Int TC in duplicates) //once all duplicates are found, determine which player gets to keep the duplicate destination
            {
                List<PlayerManager> playersWithDuplicate = new List<PlayerManager>(); //create list for all players with this duplicate
                foreach (Transform p in playerPanels) if (p.GetComponent<PlayerManager>().destination == TC) playersWithDuplicate.Add(p.GetComponent<PlayerManager>()); //add players if they have this duplicate set as their destination
                FixDuplicate(TC, playersWithDuplicate); //call method to fix destinations of all players with this duplicate
            }
        } 
        while (duplicates.Count != 0); //after first duplicate check, keep checking until there are no more duplicates
    }

    private void FixDuplicate(Vector2Int TC, List<PlayerManager> players)
    {
        PlayerManager priorityPlayer = players[Random.Range(0, players.Count)]; //select random player with this duplicate and set them as priority player (meaning they will get to keep this destination)

        //check if any players have the same position as their destination (meaning they already failed this process and didn't get to keep their original destination)
        foreach (PlayerManager pm in players) if (pm.destination == pm.pos) priorityPlayer = pm; //if so, this player automatically gets to keep this destination

        //once priority player is determined, fix destinations accordingly
        foreach(PlayerManager pm in players)
        {
            if (pm == priorityPlayer) continue; //priority player is skipped since they will keep their destination
            pm.destination = pm.pos; //if not priority player, set their destination to their position (meaning they won't move anywhere)
        }
    }

    #endregion
}
