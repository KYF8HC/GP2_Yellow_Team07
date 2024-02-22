using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
//using UnityEngine.UIElements;
using UnityEngine.UI;
using static MyUtility.Utility;

public class CoordinatorHUD : Entity {

    
    private Player playerRef;
    private Coordinator coordinatorRef;
    private Vector3 daredevilPosition = Vector3.zero;

    private Slider boostCrankSlider;
    private Image batteryBar;
    private TMP_Text scoreText;
    private TMP_Text timeLimitText;

    private const float distanceTrashHold = 10f;


    public override void Initialize(GameInstance game) {
        if (initialized)
            return;


        SetupReferences();
        gameInstanceRef = game;
        initialized = true;
    }
    public override void Tick() {
        if (!initialized)
            return;

        UpdateRadar();
        UpdateTimeLimit();
    }
    private void SetupReferences()
    {

        //Right Window
        Transform rightDragWindowTransform = transform.Find("DragWindow_1");
        Validate(rightDragWindowTransform, "RightDragWindow transform not found!", ValidationLevel.ERROR, true);

        //BoostCrank//
        Transform boostCrankTransform = transform.Find("BoostCrank");
        Validate(boostCrankTransform, "BoostCrank transform not found!", ValidationLevel.ERROR, true);
        boostCrankSlider = boostCrankTransform.GetComponent<Slider>();
        Validate(boostCrankSlider, "BoostCrank slider component not found!", ValidationLevel.ERROR, true);


        //BatteryBar batteryBar
        Transform batteryBarTransform = transform.Find("BatteryBar");
        Validate(batteryBarTransform, "BatteryBar transform not found!", ValidationLevel.ERROR, true);
        Transform batteryBarFillTransform = batteryBarTransform.Find("BatteryBarFill");
        Validate(batteryBarFillTransform, "BatteryBarFill transform not found!", ValidationLevel.ERROR, true);
        batteryBar = batteryBarFillTransform.GetComponent<Image>();
        Validate(batteryBar, "batteryBar component not found!", ValidationLevel.ERROR, true);

        //Dragwindow
        Transform dragWindowTransform = transform.Find("DragWindow");
        Validate(dragWindowTransform, "DragWindowTransform transform not found!", ValidationLevel.ERROR, true);
        Transform dragWindowBodyTransform = dragWindowTransform.transform.Find("DragWindowBody");
        Validate(dragWindowBodyTransform, "DragWindowBodyTransform transform not found!", ValidationLevel.ERROR, true);
        //TimeLimit inside dragwindow
        Transform timeLimitTextTransform = dragWindowBodyTransform.transform.Find("TimeLimitTMP");
        Validate(timeLimitTextTransform, "TimeLimitTextTransform transform not found!", ValidationLevel.ERROR, true);
        
        
    }



    //getting the current time limit from calling time in scoreSystem
    public void UpdateTimeLimit()
    {
        timeLimitText.text = "Seconds Passed: " + gameInstanceRef.GetScoreSystem().GetTimer();
    }

    public void SetupStartState() {
        batteryBar.fillAmount = coordinatorRef.GetStats().batteryLimit; //Could be changed to starting battery limit later!
    }

    public void SetPlayerReference(Player player) {
        playerRef = player;
        if (playerRef)
            coordinatorRef = playerRef.GetCoordinatorData();
    }

    public void RelayBoostState(bool state) {
        Log("Boost : " + state);
        gameInstanceRef.GetRPCManagement().SetBoostStateServerRpc(Netcode.GetClientID(), state);
    }
    public void UpdatePowerBar(float percentage) {
        batteryBar.fillAmount = percentage;
    }



   public void ObstacleActivationStateButton(int index) {
        if (index < 0) {
            Warning("Invalid index received at ObstacleActivationStateButton: " + index);
            return;
        }

        var currentLevel = gameInstanceRef.GetLevelManagement().GetCurrentLoadedLevel();
        if (!currentLevel) {
            Warning("ObstacleActivationStateButton was called while no level was loaded!");
            return;
        }
        if (currentLevel.currentObstacleState == (Obstacle.ObstacleActivationState)index)
            return;

        gameInstanceRef.GetRPCManagement().SetObstacleActivationStateServerRpc(Netcode.GetClientID(), (Obstacle.ObstacleActivationState)index);
   }

    public void BoostCrankSlider() {
        float value = boostCrankSlider.value;
        if (value == boostCrankSlider.maxValue) { //If boost is not activated
            RelayBoostState(true);
            coordinatorRef.SetBoostState(true);
        }
        else if (value == boostCrankSlider.minValue) { //If boost is activated
            RelayBoostState(false);
            coordinatorRef.SetBoostState(false);
        }
    }
    
    public void SetDaredevilPosition(Vector3 position) {
        daredevilPosition = position;
    }

    private void UpdateRadar() {
        var playerPosition = daredevilPosition;
        var currentLevel = gameInstanceRef.GetLevelManagement().GetCurrentLoadedLevel();
        foreach (var obstacle in currentLevel.registeredObstacles) {
            if (!obstacle)
                continue;
            var distance = Vector3.Distance(playerPosition, obstacle.transform.position);

            if (distance < distanceTrashHold) {
                PingRadar(daredevilPosition);
            }
        }
    }

    private void PingRadar(Vector3 position) {
        Log("Pinging radar!");
    }

    public void PauseMenuButton()
    {
        gameInstanceRef.PauseGame();
    }
}
