using UnityEngine;
using TMPro;
using static MyUtility.Utility;
using System;

public class DaredevilHUD : Entity {

    public enum DaredevilKeyCode
    {
        MoveLeft = 0,
        MoveRight = 1,
        Accelerate = 2,
        Decelerate = 3
    }
    
    private Player playerRef;
    private Daredevil daredevilRef;

    private TMP_Text scoreText;
    private TMP_Text timeLimitText;

    public Transform turnButtons;
   

    public override void Initialize(GameInstance game) {
        if (initialized)
            return;




        gameInstanceRef = game;
        
        SetupReferences();
        initialized = true;
    }

    public override void Tick() {
        if (!initialized)
            return;

        
        UpdateTimeLimit();
    }
    public void SetupStartState() {

    }

    public Transform GetTurnButtons() { return turnButtons; }

    public void SetPlayerReference(Player player) {
        playerRef = player;
        if (playerRef)
            daredevilRef = playerRef.GetDaredevilData();
    }

    private void SetupReferences()
    {

        //finding the text for the time limit
        Transform timeLimitBackgroundTransform = transform.Find("TimeLimit");
        Validate(timeLimitBackgroundTransform, "ScoreTransform transform not found!", ValidationLevel.ERROR, true);
        Transform timeLimitTextTransform = timeLimitBackgroundTransform.transform.Find("TimeLimitTMP");
        Validate(timeLimitTextTransform, "TimeLimitTransform transform not found!", ValidationLevel.ERROR, true);
        timeLimitText = timeLimitTextTransform.GetComponent<TMP_Text>();
        Validate(timeLimitText, "TimeLimitText transform not found!", ValidationLevel.ERROR, true);

        turnButtons = transform.Find("TurningButtons");

    }



    //getting the current time limit from calling time in scoreSystem 
    public void UpdateTimeLimit()
    {
        
        timeLimitText.text = "Seconds Passed: " + gameInstanceRef.GetScoreSystem().GetTimer();
    }

    public void LeftButtonOnEvent()
    {
        daredevilRef.SetLeftButton(true);
    }
    public void LeftButtonOffEvent()
    {
        daredevilRef.SetLeftButton(false);
    }
    public void RightButtonOnEvent()
    {
        daredevilRef.SetRightButton(true);
    }
    public void RightButtonOffEvent()
    {
        daredevilRef.SetRightButton(false);
    }

    public void BrakeOnEvent() {
        daredevilRef.SetBrakeState(true);
    }
    public void BrakeOffEvent() {
        daredevilRef.SetBrakeState(false);
    }
    public void GasOnEvent() {
        daredevilRef.SetMovementState(true);
    }
    public void GasOffEvent() {
        daredevilRef.SetMovementState(false);
    }

    public void PauseMenuButton()
    {
        gameInstanceRef.PauseGame();
    }
}
