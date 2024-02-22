using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem : Entity {

    //Taking the more dangerous route gives more score.

    [SerializeField] private int crashScorePenality = 100;
    [SerializeField] private int chaosScoreBonus = 10;
    public float timer = 0;

   

    private int currentScore = 0;

    public override void Initialize(GameInstance game) {
        if (initialized)
            return;


        gameInstanceRef = game;
        initialized = true;
    }
    public override void Tick() {
        if (!initialized)
            return;

        if(gameInstanceRef.currentGameState == GameInstance.GameState.PLAYING)
        {
            timer += Time.deltaTime;
        }

    }


    public void SetupStartState()
    {
        currentScore = 0;
        timer = 0;
    }


    public void RegisterCrashScorePenality() {
        currentScore -= crashScorePenality;
    }
    public void RegisterChaosScoreBonus() {
        currentScore += chaosScoreBonus;
    }

    public int GetCurrentScore() { return currentScore; }
    public float GetTimer() { return timer; }
}
