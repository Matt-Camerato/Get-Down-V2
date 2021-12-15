using System.Collections.Generic;
using UnityEngine;

public class RecapManager : MonoBehaviour
{
    [SerializeField] private HUDUI HUD;

    [HideInInspector] public bool determinedHighestPlayer;
    [HideInInspector] public List<PlayerManager> highestPlayer;

    private List<GameObject> spawnedCubes = new List<GameObject>();

    private float recapTimer = 0;
    private bool startedRecap = false;
    private bool movedPlayerPieces = false;
    private List<PlayerManager> attacksLeftToSpawn = new List<PlayerManager>();
    private bool playerDied = false;

    private bool damageDisplayed = false;

    static public RecapManager instance;

    private void Awake()
    {
        if (instance != null) return;
        instance = this; //create singleton instance of recap manager that can be accessed from anywhere
    }

    public void StartRecap() => startedRecap = true; //called by game controller to start recap phase

    private void Update()
    {
        if (!startedRecap) return; //only continue if recap phase has started

        RecapTimer(); //decrease recap timer throughout entire recap phase
        if (recapTimer > 0) return; //wait until recap timer hits 0 before doing next part of recap phase

        if (!movedPlayerPieces) MovePlayerPieces(); //! ===MOVE PLAYER PIECES===
        else if (attacksLeftToSpawn.Count != 0) SpawnAttacks(); //! ===SPAWN ATTACK CUBES===
        else if (!damageDisplayed) DisplayDamage(); //! ===DISPLAY DAMAGE===
        else EndRecapPhase(); //! ===END OF RECAP PHASE===
    }

    private void RecapTimer() => recapTimer -= Time.deltaTime;

    private void MovePlayerPieces()
    {
        for (int i = 0; i < GameboardManager.instance.playerPieces.Count; i++) //trigger moving of all player pieces based on their new positions
        {
            PlayerManager pm = GameController.instance.playerPanels.GetChild(i).GetComponent<PlayerManager>();
            if (pm.health == 0) continue; //skip player piece if player is already dead

            GameboardManager.instance.playerPieces[i].GetComponent<Animator>().SetTrigger("MovePiece"); //trigger animation that will move player piece to new position
            attacksLeftToSpawn.Add(pm); //for each player still alive, add them to attacks left to spawn list
        }
        movedPlayerPieces = true; //set this step of recap phase as completed
        recapTimer = 3; //set timer to 3 seconds until attacks are spawned
    }

    private void SpawnAttacks()
    {
        PlayerManager pm = attacksLeftToSpawn[0]; //get first player in attacks left to spawn list
        foreach (Vector2Int TC in pm.savedAttacks)
        {
            GameObject spawnedCube = GameboardManager.instance.SpawnAttackCube(TC, pm.playerColor); //spawn an attack cube with player's color for each of their saved attacks
            spawnedCubes.Add(spawnedCube); //add spawned attack cube to list of spawned cubes
        }
        attacksLeftToSpawn.Remove(pm); //remove player from list once their attacks are spawned
        recapTimer = 1; //set timer to 1 second until next attacks are spawned
        if (attacksLeftToSpawn.Count == 0) recapTimer = 1.5f; //if done spawning all attacks, set timer to 1.5 seconds until damage is displayed
    }

    private void DisplayDamage()
    {
        if (!determinedHighestPlayer)
        {
            DetermineHighest(); //before displaying damage, if this is the first recap phase of the round, highest player(s) must be determined and damaged
            determinedHighestPlayer = true; //set bool for determining highest player to true so this is only done once per round
        }

        foreach (PlayerManager pm in highestPlayer)
        {
            GameboardManager.instance.playerPieces[pm.playerNum].GetComponent<PlayerPieceManager>().Damage(); //display damage particles on highest player(s) playing piece
            if (pm == GameController.instance.currentPlayer)
            {
                pm.transform.GetChild(1).GetComponent<HealthDisplayManager>().LoseHealth(); //if this is current player, update on-screen health display 
                HUD.ShowDamagePanel(); //also indicate being damaged by turning red damage panel on that quickly fades away
            }

            if (pm.savedHealth != 0) continue; //continue to next highest player if player didn't die
            foreach (Transform child in GameboardManager.instance.playerPieces[pm.playerNum].transform) child.gameObject.SetActive(false); //if player died, make player piece invisible (mesh and particles)
            if (pm == GameController.instance.currentPlayer) playerDied = true;
        }
        damageDisplayed = true; //set this step of recap phase as completed
        recapTimer = 1.5f; //set timer to 1.5 seconds until end of recap phase
    }

    private void EndRecapPhase()
    {
        //reset private bools used for recap phase
        startedRecap = false;
        movedPlayerPieces = false;
        damageDisplayed = false;

        if(playerDied) HUD.ShowDeathScreen(); //if current player was one who died, display death screen
        else HUD.EndRecap(); //else trigger end of recap phase

        playerDied = false; //reset player died bool as well (after check)
    }

    private void DetermineHighest() //at the end of each round after all player information has been saved, this determines highest player which will be damaged during recap phase
    {
        int highestHeight = 0; //setup int to keep track of highest player height
        highestPlayer = new List<PlayerManager>(); //setup list to keep track of highest player(s)
        foreach (Transform child in GameController.instance.playerPanels) //loop through players to determine who is highest
        {
            PlayerManager pm = child.GetComponent<PlayerManager>();
            if (pm.health == 0) continue; //skip player if they are already dead

            int height = GameboardManager.instance.tiles[pm.pos.x, pm.pos.y].GetComponent<TileCubeManager>().cubeCount; //get current player's height
            if (height < highestHeight) continue; //if player is lower than highest height, check next player

            if (height > highestHeight) highestPlayer.Clear(); //if player is higher than highest height, clear any previous highest player(s)
            highestPlayer.Add(pm); //player's height is either higher or equal to the highest height, meaning they will be on the highest player list regardless
            highestHeight = height; //they will also have the new highest height, regardless of if it increases or remains the same
        }

        Debug.Log("Highest Player(s):");
        foreach (PlayerManager pm in highestPlayer) Debug.Log(pm.playerName);

        HandleDamage(); //once highest player(s) is found, determine if multiple players will be damaged or no one at all and deal this damage
    }

    private void HandleDamage() //determine and deal damage based on highest player(s)
    {
        //!IMPORTANT: In this version, all highest players will get damaged unless every (alive) player is tied (on highest player list). The variable to change this can be found on the GameSettings script and is currently being set in the SaveNamesAndColors() method.         
        List<PlayerManager> alivePlayers = new List<PlayerManager>(); //this is used to determine if this is last round
        foreach (Transform child in GameController.instance.playerPanels)
        {
            if (child.GetComponent<PlayerManager>().health != 0) alivePlayers.Add(child.GetComponent<PlayerManager>()); //loop through players to count how many are alive at the start of this round
        }

        if (highestPlayer.Count > GameSettings.instance.maxPlayersDamagedPerTurn) highestPlayer.Clear(); //if # of highest players is greater than max # of players that can be damaged per turn, don't damage anyone (clear highest player list)
        foreach (PlayerManager pm in highestPlayer) //loop through highest players and damage them
        {
            pm.savedHealth--; //damage player with saved health (will be updated to actual health at the end of the round)
            if (pm.savedHealth == 0) alivePlayers.Remove(pm); //remove player from alive players list if this causes player to die
        }
        if (alivePlayers.Count > 1) return; //if more than 1 player remains alive, this isn't last round
        GameController.instance.lastRound = true; //if not, this is the final round of the game
        GameController.instance.winner = alivePlayers[0]; //set last alive player to winner

        //?POSSIBLE FEATURE: whoever has a cube in highest player's tower gets a life back, or instead whoever attacked the highest player this round (possibly just last person to spawn attack cube in that position so everyone doesn't heal)
    }

    public void UpdateSpawnedCubesList()
    {
        Debug.Log("There are currently " + spawnedCubes.Count + " spawned cubes to be cleared");
        spawnedCubes.Clear(); //clear spawned cubes list so no cubes are removed when gameboard is reset
        Debug.Log("After clearing, there are now " + spawnedCubes.Count + " spawned cubes");
    }

    public void DeleteSpawnedCubes()
    {
        Debug.Log("There are currently " + spawnedCubes.Count + " spawned cubes to be deleted");
        foreach(GameObject cube in spawnedCubes)
        {
            Destroy(cube);
        }
        Debug.Log("After deleteing, there are now " + spawnedCubes.Count + " spawned cubes");
    }
}
