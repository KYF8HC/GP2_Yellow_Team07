using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static MyUtility.Utility;

public class WinMenu : Entity
{
    private TMP_Text scoreText;
    private TMP_Text finalTimeText;


    private ScoreSystem scoreSystemRef;
    public override void Initialize(GameInstance game)
    {
        if (initialized)
            return;

        SetupReferences();
        gameInstanceRef = game;
        initialized = true;
    }

    private void SetupReferences()
    {




        //finding the transform for the time text and then the text component
        Transform timeLimitTextTransform = transform.Find("SecondsLeft");
        Validate(timeLimitTextTransform, "TimeLimitTransform transform not found!", ValidationLevel.ERROR, true);
        finalTimeText = timeLimitTextTransform.GetComponent<TextMeshProUGUI>();
        Validate(finalTimeText, "TimeLimitText transform not found!", ValidationLevel.ERROR, true);

        
        
    }


    public void UpdateTimeText()
    {
        float totalTime = gameInstanceRef.GetScoreSystem().GetTimer();
        TimeSpan timeSpan = TimeSpan.FromSeconds(totalTime);
        string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:000}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds); 
        finalTimeText.text = "You took " + elapsedTime;
    }
}
