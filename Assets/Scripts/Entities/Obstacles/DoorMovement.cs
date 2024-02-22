using System;
using UnityEngine;

public class DoorMovement : Obstacle {
    
    private Animator gateAnimator = null;
    
    public override void Initialize(GameInstance game) {
        if (initialized) {
            return;
        }
        gameInstanceRef = game;
        initialized = true;
        
        gateAnimator = GetComponent<Animator>();
    }

    public override void Tick() {
        if (!initialized)
            return;
        
        
    }
    
    public override void SetActivationState(bool state) {
        gateAnimator.SetBool("IsActive", state);
    }
}