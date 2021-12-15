using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform playerListPanel; //parent of all list players
    [SerializeField] private GameObject addListPlayerButton; //used to spawn new list players
    [SerializeField] private GameObject listPlayerPrefab; //list players that are spawned

    [SerializeField] private Button beginButton;

    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    #region Main Menu - UI Methods

    public void StartButton()
    {
        anim.SetTrigger("StartButton"); //trigger transition animation to show game setup screen
    }

    public void SettingsButton()
    {
        //not currently implemented
    }

    public void QuitGame() => Application.Quit(); //used by quit button on title screen

    #endregion

    #region Setup Screen - UI Methods

    public void AddPlayer()
    {
        Instantiate(listPlayerPrefab, playerListPanel); //spawn new list player
        addListPlayerButton.transform.SetAsLastSibling(); //make sure add player button is at end of player list

        if (GameSettings.instance.numPlayers == 7) addListPlayerButton.SetActive(false); //if this will be 8th player, turn off add player button (will be turned on again if player is removed)

        GameSettings.instance.numPlayers++; //increase player count

        UpdateBeginButton(); //update interactability of begin button
    }

    public void UpdateBeginButton() //changes button interactability based on whether the game can begin or not
    {
        beginButton.interactable = false;

        if (GameSettings.instance.numPlayers < 2 || GameSettings.instance.numPlayers > 8) return; //if there isn't 2-8 players, can't press begin button

        for(int i = 0; i < playerListPanel.transform.childCount - 1; i++)
        {
            if (!playerListPanel.transform.GetChild(i).GetComponent<ListPlayerManager>().validName) return; //if there is a player with an invalid name, can't press begin button
        }

        beginButton.interactable = true;
    }

    public void TutorialCheck()
    {
        GameSettings.instance.SaveNamesAndColors(playerListPanel); //save current player info to game settings instance

        anim.SetTrigger("TutorialCheck"); //trigger tutorial check to see if new players are present and need to know how-to-play
    }

    public void YesButton() => anim.SetTrigger("ShowTutorial"); //show tutorial screen if player asks to see it

    public void TapToContinue() => anim.SetTrigger("TapToContinue"); //this allows player to progress through tutorial sequence

    public void NoButton() => anim.SetTrigger("BeginGame"); //cause fade out animation to play before game begins

    public void BeginGameEvent() => SceneManager.LoadScene(1); //when fade out is done, load the game scene

    public void BackButton() => anim.SetTrigger("BackButton"); //trigger transition animation to show main menu screen

    #endregion
}
