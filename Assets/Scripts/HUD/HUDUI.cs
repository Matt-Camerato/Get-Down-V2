using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class HUDUI : MonoBehaviour
{
    [Header("AR Setup References")]
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private ARTapToPlace tapToPlace;
    [SerializeField] private GameObject gameboardPrefab;

    [Header("UI References")]
    [SerializeField] private GameObject passScreen;
    [SerializeField] private GameObject beginRecapButton;
    [SerializeField] private GameObject skipRecapButton;
    [SerializeField] private GameObject damagePanel;

    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void ConfirmARPlacement()
    {
        anim.SetTrigger("ARSetupFinished"); //trigger UI animation to get off of AR Placement Screen

        planeManager.enabled = false; //disable plane manager
        foreach (var plane in planeManager.trackables) { plane.gameObject.SetActive(false); } //disable all current planes

        tapToPlace.enabled = false; //disable movement of game marker
        Vector3 gamePos = tapToPlace.spawnedGameMarker.transform.position; //save position of game marker
        Quaternion gameRot = tapToPlace.spawnedGameMarker.transform.rotation; //save rotation of game marker
        Destroy(tapToPlace.spawnedGameMarker); //delete game marker object

        GameObject gameboard = Instantiate(gameboardPrefab, gamePos, gameRot); //instantiate game at markers position
    }

    public void UpdateHeaderEvent() //trigger when header moves up off screen in order to change the display (after AR placement confirmed and between turns)
    {
        Transform currentPlayer = null;
        Transform nextPlayer = GameController.instance.playerPanels.GetChild(0); //if no panels are visible, first player will be next player

        foreach (Transform child in GameController.instance.playerPanels)
        {
            if (child.GetComponent<CanvasGroup>().alpha == 0) continue; //if player panel isn't visible, check next panel

            currentPlayer = child; //current player is only visible child object
            int nextPlayerIndex = child.GetSiblingIndex() + 1; //set next player index
            if (nextPlayerIndex == child.parent.childCount) nextPlayerIndex = 0; //if current player was last player in turn order, set next player index to 0 (first player in turn order)
            while (child.parent.GetChild(nextPlayerIndex).GetComponent<PlayerManager>().health == 0) //if next player is already dead, skip to following player until an alive player is found
            {
                //increment to next player until an alive one is found
                nextPlayerIndex++;
                if (nextPlayerIndex == child.parent.childCount) nextPlayerIndex = 0;
            }
            nextPlayer = child.parent.GetChild(nextPlayerIndex);
            break;
        }

        if (currentPlayer != null) currentPlayer.GetComponent<CanvasGroup>().alpha = 0; //make current player's panel invisible (if possible)
        nextPlayer.GetComponent<CanvasGroup>().alpha = 1; //make next player's panel visible
        GameController.instance.currentPlayer = nextPlayer.GetComponent<PlayerManager>(); //update current player on game controller instance

        if (GameController.instance.roundNum == 1)
        {
            if (GameController.instance.turnNum == 1) return; //if first turn of game, don't enable pass screen
        }
        else
        {
            GameboardManager.instance.ResetGameboard(); //if not first round of game, reset gameboard for recap phase
        }

        passScreen.SetActive(true); //turn on pass screen when header changes
    }

    public void StartTurn() //starts turn when player taps on pass screen (or presses okay button on welcome message)
    {
        if (GameController.instance.gameOver) //if game is over when player taps on pass screen, transition directly to game over animation
        {
            anim.SetTrigger("GameOver");
            return;
        }

        if (GameController.instance.roundNum > 1)
        {
            anim.SetBool("Recap", true); //if not round 1 anymore, turn will start with recap phase instead of move phase
            skipRecapButton.SetActive(false);
            beginRecapButton.SetActive(true);
        }
        anim.SetTrigger("StartTurn"); //start turn animation (for either recap or move phase, depending on "Recap" bool)

        GameController.instance.StartTurn(); //call separate start turn logic on game controller (to start either recap or move phase)
    }

    public void BeginRecap() => RecapManager.instance.StartRecap(); //called when player presses "Begin Recap" button to make sure they are watching gameboard before starting recap animation

    public void ShowDamagePanel() => damagePanel.SetActive(true); //if current player was damaged during recap, damage panel is activated

    public void ShowDeathScreen() => anim.SetTrigger("PlayerDied"); //called if current player dies during recap in order to display death screen
    public void RemoveDeathScreen()
    {
        GameController.instance.EndTurn(); //call method on game controller to handle end of turn

        anim.SetTrigger("EndTurn"); //called on death screen to end turn
    }

    public void EndRecap()
    {
        anim.SetBool("SkippingTurn", GameController.instance.lastRound); //if on last round, set bool to true so rest of turn is skipped after recap phase
        anim.SetTrigger("EndRecap"); //trigger animation for end of recap phase (automatically ends turn if skipping turn or else transitions to move phase)
    }

    public void EndRecapEvent()
    {
        skipRecapButton.SetActive(false);

        if (anim.GetBool("SkippingTurn")) //if skipping rest of turn, call end of turn logic
        {
            GameController.instance.EndTurn(); //call method on game controller to handle end of turn
            return; //don't need to do anything else
        }

        //if not, continue to move phase
        GameController.instance.currentPhase = "move";
        GameController.instance.StartMovePhase();
    }

    public void ConfirmMove()
    {
        anim.SetTrigger("ConfirmMove"); //trigger animation transition from move phase to attack phase

        GameController.instance.StartAttackPhase(); //call method on game controller to initialize attack phase
    }

    public void ConfirmAttacks()
    {
        GameController.instance.EndTurn(); //call method on game controller to handle end of turn

        anim.SetTrigger("ConfirmAttacks"); //trigger end of turn animation (update header animation plays after this is finished)
    }

    public void QuitToMainMenu()
    {
        GameSettings.instance.ResetGameInfo(); //before quitting, game settings are reset so another game can be played
        SceneManager.LoadScene(0);
    }
}
