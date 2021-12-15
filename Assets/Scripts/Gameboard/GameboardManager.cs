using System.Collections.Generic;
using UnityEngine;

public class GameboardManager : MonoBehaviour
{
    [Header("Gameboard Settings")]
    public int size = 5;
    [SerializeField] private Color tcIndicatorColor;
    [SerializeField] private Color tcSelectedColor;

    [Header("Prefabs")]
    [SerializeField] private GameObject tileCubePrefab;
    [SerializeField] private GameObject attackCubePrefab;
    [SerializeField] private GameObject playerPiecePrefab;

    [HideInInspector] public GameObject[,] tiles;
    [HideInInspector] public List<GameObject> playerPieces;

    private int[,] cubeCounts;

    static public GameboardManager instance;

    private void Awake()
    {
        if (instance != null) return;
        instance = this; //create singleton instance of gameboard manager that can be accessed from anywhere
    }

    private void Start() //spawn game tiles on startup
    {
        tiles = new GameObject[size, size]; //gameboard is 5x5 grid only (FOR NOW)
        cubeCounts = new int[size, size]; //also setup 2D array to save cube counts

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                GameObject currentTileCube = Instantiate(tileCubePrefab, Vector3.zero, transform.rotation, transform.GetChild(1));
                currentTileCube.transform.localPosition = new Vector3((x * 0.16f) - 0.32f, 0, (y * 0.16f) - 0.32f);
                currentTileCube.name = "Tile Cube (" + x + "," + y + ")"; //set tile name to "Tile Cube (x, y)"

                //do some math to set speed of tile startup animation (center is fastest and tiles get slower as x and y increase)

                int xFromMid = Mathf.Abs(x - 2); //2, 1, 0, 1, 2
                int yFromMid = Mathf.Abs(y - 2); //2, 1, 0, 1, 2
                float speed = 5 - (xFromMid + yFromMid); //center tile is speed 5, corner tile is speed 1
                speed /= 5; //center tile is speed 1, corner tile is speed 0.2f
                speed += (1f - speed) / 2; //lastly, speeds become 0.6, 0.7, 0.8, 0.9, and 1
                currentTileCube.GetComponent<Animator>().SetFloat("Speed", speed);

                tiles[x, y] = currentTileCube; //add tile to 2D tile array
                cubeCounts[x, y] = 0; //set saved cube count of current TC position to 0 (no TC starts with cubes)

                TileCubeManager currentTileCubeManager = currentTileCube.GetComponent<TileCubeManager>();
                currentTileCubeManager.pos = new Vector2Int(x, y);
                
            }
        }

        SpawnPlayerPieces(); //spawn player pieces after gameboard has been fully spawned
    }

    private void SpawnPlayerPieces()
    {
        List<int> randoms = new List<int>();
        for(int r = 0; r < 8; r++) randoms.Add(r); //setup random list with integers 0 through 7

        for(int i = 0; i < GameSettings.instance.numPlayers; i++)
        {
            int randomIndex = Random.Range(0, randoms.Count); //get random index of 0-7 list
            int random = randoms[randomIndex]; //set variable to random number
            randoms.RemoveAt(randomIndex); //remove random number from list (so all players get different random numbers from 0 to 7)
            Vector2Int spawnPos = GetSpawnPos(random); //use random number to get spawn position

            PlayerManager newPlayer = GameController.instance.playerPanels.GetChild(i).GetComponent<PlayerManager>();
            newPlayer.pos = spawnPos; //set player manager position to random spawn position

            Transform parentTile = tiles[spawnPos.x, spawnPos.y].transform.GetChild(0); //get transform of TC that player piece will spawn on
            GameObject newPlayerPiece = Instantiate(playerPiecePrefab, parentTile); //spawn player piece and parent it to (mesh of) TC of spawn position
            newPlayerPiece.name = newPlayer.playerName + "'s Player Piece";
            newPlayerPiece.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = newPlayer.playerColor; //set player piece color
            //newPlayerPiece.transform.GetChild(1).GetComponent<ParticleSystem>().startColor = newPlayer.playerColor; //set player piece particles color
            newPlayerPiece.GetComponent<PlayerPieceManager>().playerManager = newPlayer; //set player manager reference on new player piece

            playerPieces.Add(newPlayerPiece); //add spawned player piece to list of player pieces
        }
    }

    private Vector2Int GetSpawnPos(int random)
    {
        switch (random)
        {
            case 0:
                return new Vector2Int(0, 0); //top left corner
            case 1:
                return new Vector2Int(size - 1, size - 1); //bottom right corner
            case 2:
                return new Vector2Int(size - 1, 0); //top right corner
            case 3:
                return new Vector2Int(0, size - 1); //bottom left corner
            case 4:
                return new Vector2Int(0, Mathf.FloorToInt((size - 1) / 2)); //left middle 
            case 5:
                return new Vector2Int(size - 1, Mathf.FloorToInt((size - 1) / 2)); //right middle 
            case 6:
                return new Vector2Int(Mathf.FloorToInt((size - 1) / 2), 0); //top middle
            case 7:
                return new Vector2Int(Mathf.FloorToInt((size - 1) / 2), size - 1); //bottom middle
            default:
                Debug.Log("random integer doesn't contain possible spawn position");
                return Vector2Int.zero;
        }
    }

    public void ToggleIndicator(Vector2Int coords)
    {
        Animator anim = tiles[coords.x, coords.y].GetComponent<Animator>(); //get animator of given tile cube
        anim.SetBool("Selected", !anim.GetBool("Selected")); //toggle tile cube's indicator
    }

    public void SetIndicatorColor(Vector2Int coords, string color) 
    {
        Transform TI = tiles[coords.x, coords.y].transform.GetChild(1);
        MeshRenderer renderer = TI.GetComponent<MeshRenderer>();
        //ParticleSystem ps = TI.GetChild(0).GetComponent<ParticleSystem>();

        switch (color)
        {
            case "unselected":
                //set indicator to unselected color
                //ps.startColor = tcIndicatorColor; //set particles color
                renderer.material.color = tcIndicatorColor; //set mesh color
                break;
            case "selected":
                //set indicator to selected color
                //ps.startColor = tcSelectedColor; //set particles color
                renderer.material.color = tcSelectedColor; //set mesh color
                break;
            case "playercolor":
                //set indicator to current player's color
                Color pc = GameController.instance.currentPlayer.playerColor; //get current player's color
                //ps.startColor = pc; //set particles color
                renderer.material.color = pc; //set mesh color
                break;
            default:
                Debug.Log("Color given is not a possible indicator color!");
                break;
        }
    }

    public void ResetIndicators() //loop through all TC indicators and shut off any that are still on
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Animator anim = tiles[x, y].GetComponent<Animator>();
                Vector2Int TC = new Vector2Int(x, y);
                if (anim.GetBool("Selected")) ToggleIndicator(TC); //if indicator is on, turn it off
            }
        }
    }

    public GameObject SpawnAttackCube(Vector2Int coords, Color playerColor)
    {
        GameObject newCube = Instantiate(attackCubePrefab, tiles[coords.x, coords.y].transform); //spawn new attack cube and parent it to given TC
        float y = -0.04f - (tiles[coords.x, coords.y].GetComponent<TileCubeManager>().cubeCount * 0.1f); //determine y value of new attack cube based on current cube count of given TC
        newCube.transform.localPosition = new Vector3(0, y, 0); //set y value of new attack cube
        newCube.transform.GetChild(0).GetComponent<MeshRenderer>().materials[0].color = playerColor; //set material color for cube border to attacking player's color

        tiles[coords.x, coords.y].GetComponent<TileCubeManager>().cubeCount++; //increment cube count of given TC
        return newCube;
    }

    public void SaveCubeCounts()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                cubeCounts[x, y] = tiles[x, y].GetComponent<TileCubeManager>().cubeCount;
            }
        }
    }

    public void ResetGameboard()
    {
        foreach(GameObject pp in playerPieces) //loop through all player pieces and reset them to saved position
        {
            PlayerManager pm = pp.GetComponent<PlayerPieceManager>().playerManager;
            if (pm.health == 0) continue; //skip player piece if player is already dead

            Vector2Int savedPos = pm.savedPos; //get saved position of player piece
            pp.transform.SetParent(tiles[savedPos.x, savedPos.y].transform.GetChild(0), false); //set player piece's parent to (mesh of) old TC

            if (pm.savedHealth == 0) //if player died this round, make their player piece visible again 
            {
                foreach (Transform child in pp.transform) child.gameObject.SetActive(true); //turn on all child objects of player piece (mesh and particles)
            }
        }

        RecapManager.instance.DeleteSpawnedCubes(); //delete spawned cube objects

        //reset cube counts of all tiles (so TC's go back to original position)
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                tiles[x, y].GetComponent<TileCubeManager>().cubeCount = cubeCounts[x, y];
            }
        }
    }
}
