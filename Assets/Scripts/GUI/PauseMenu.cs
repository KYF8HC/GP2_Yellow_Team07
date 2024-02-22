using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PauseMenu : Entity
{
    [SerializeField] GameInstance.GameState OptionsMenuState = GameInstance.GameState.OPTIONS_MENU;
    public override void Initialize(GameInstance game)
    {
        if (initialized)
            return;


        gameInstanceRef = game;
        initialized = true;
    }

    public void ResumeButton()
    {
        gameInstanceRef.UnpauseGame();
    }

    /*public void RestartButton()
    {
        string levelkey = gameInstanceRef.GetLevelManagement().GetCurrentLoadedLevel().gameObject.name;
        //gameInstanceRef.StartGame(levelkey);
        gameInstanceRef.GetLevelManagement().UnloadLevel();
        Debug.Log(levelkey);
    }*/

    public void OptionsButton()
    {
        gameInstanceRef.SetGameState(OptionsMenuState, GameInstance.GameState.PAUSED);
    }

    public void MainMenuButton()
    {
        gameInstanceRef.GetPlayerGameObject().transform.rotation = Quaternion.identity;
        gameInstanceRef.GetNetcode().StopNetworking();
        gameInstanceRef.InterruptGame();
    }






}
