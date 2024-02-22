using System;
using UnityEngine;
using static MyUtility.Utility;


[Serializable]
public struct RampBoost {
    public float duration;
    public float maxSpeed;
    public float accelerationRate;
    public float decelerationRate;
}

public enum TurnAnimation {
    Left,
    Right
}


public class Daredevil {
    bool initialized = false;


    GameInstance gameInstanceRef;
    Player playerRef;
    DaredevilStats stats;
    Rigidbody playerRigidbody;

    private string sFXKeyAccelerate = "AccelSFX";
    private string sFXKeyBrake = "BreakSFX";

    private float currentSpeed = 0.0f;
    private RampBoost currentRampBoost;
    private float soundTimer = 0.0f;
    private float soundCooldown = 3.0f;
    private float rampBoostTimer = 0.0f;

    private bool isGrounded = true;
    private bool isMoving = false;
    private bool isReversingAndBraking = false;
    private bool isBoosting = false;
    private bool isRampBoosted = false;
    private bool isTurningLeftWithButton = false;
    private bool isTurningRightWithButton = false;
    private bool isTurning = false;
    private bool turningAnimWasTriggered = false;
    private bool turningButtons = true;

    private Vector3 direction = Vector3.forward;
    private int currentTiltIndex = 0;
    private float tiltSpectrum = 0.0f;
    private float gyroResetTimer = 0.0f;
    private float tiltRate = 0.0f;
    private float hitStopTimer = 0.0f;

    private GameObject frontWheelMesh;
    private GameObject backWheelMesh;
    private GameObject engineMesh;
    private GameObject seatMesh;
    private GameObject turnButtons;

    private WheelCollider frontWheelColliderComp;
    private WheelCollider backWheelColliderComp;

    private ParticleSystem landingDustParticle;

    private DaredevilAnimationController daredevilAnimationController = new DaredevilAnimationController();


    public void Initialize(GameInstance game, Player player) {
        if (initialized)
            return;

        playerRef = player;
        stats = playerRef.GetDaredevilStats();
        playerRigidbody = playerRef.GetRigidbody();
        SetupReferences();

        gameInstanceRef = game;
        initialized = true;
    }

    public void SetupStartState() {
        gyroResetTimer = stats.gyroResetDuration;
        daredevilAnimationController.SetupReferences(playerRef);
    }

    public void SetupReferences() {
        var landingDustGameObject = playerRef.transform.Find("Landing_Dust");
        var meshTransform = playerRef.transform.Find("Mesh");
        var bikeTransform = meshTransform.Find("Bike").Find("SeparatedBike");

        frontWheelColliderComp = bikeTransform.Find("FrontWheelCollider").GetComponent<WheelCollider>();
        backWheelColliderComp = bikeTransform.Find("BackWheelCollider").GetComponent<WheelCollider>();
        frontWheelMesh = frontWheelColliderComp.transform.Find("FrontWheel").gameObject;
        backWheelMesh = backWheelColliderComp.transform.Find("BackWheel").gameObject;

        engineMesh = bikeTransform.Find("Engine").gameObject;
        seatMesh = bikeTransform.Find("Seat").gameObject;

        if (landingDustGameObject != null) {
            landingDustParticle =
                landingDustGameObject
                    .GetComponent<
                        ParticleSystem>(); //prolly not supposed to be here but i wasn't sure where to put it //Chriss
        }

        Validate(frontWheelMesh, "FrontWheel go", ValidationLevel.ERROR);
        Validate(backWheelMesh, "Backwheel go", ValidationLevel.ERROR);
        Validate(engineMesh, "Engine", ValidationLevel.ERROR);
        Validate(seatMesh, "Seat", ValidationLevel.ERROR);

        Validate(frontWheelColliderComp, "FrontWheel", ValidationLevel.ERROR);
        Validate(backWheelColliderComp, "Backwheel", ValidationLevel.ERROR);

        

    }

    public void Tick() {
        if (!initialized) {
            Warning("Player attempted to tick daredevil while it was not initialized");
            return;
        }

        UpdateGroundedState();



        UpdateTurning();
        UpdateDebugTilt();
        
        UpdateMovement();
        UpdateRampBoost();
        //HitstopTimer();
        TriggerTurningAnimation();
        SoundCountdown();
    }

    public void FixedTick() {
        if (!initialized) {
            Warning("Player attempted to fixed tick daredevil while it was not initialized");
            return;
        }


        UpdateDirection();
        UpdateWheels();
        //UpdateGravity();
        UpdateVelocity();

        UpdatePositionOnServer();
    }

    private void UpdatePositionOnServer() {
        gameInstanceRef.GetRPCManagement()
            .SetDaredevilTransformServerRpc(Netcode.GetClientID(), playerRef.transform.position);
    }


    private void UpdateWheels() {
        backWheelColliderComp.GetWorldPose(out var frontWheelPosition, out var frontWheelRotation);
        backWheelMesh.transform.rotation = frontWheelRotation;
        backWheelMesh.transform.position = frontWheelPosition;

        frontWheelColliderComp.GetWorldPose(out var backWheelPosition, out var backWheelRotation);
        frontWheelMesh.transform.rotation = backWheelRotation;
        frontWheelMesh.transform.position = backWheelPosition;
    }

    public void SetBoostState(bool state) {
        isBoosting = state;
    }

    public void SetMovementState(bool state) {
        isMoving = state;
    }

    public void SetBrakeState(bool state) {
        isReversingAndBraking = state;
    }

    public void SetLeftButton(bool state) {
        isTurningLeftWithButton = state;
    }

    public void SetRightButton(bool state) {
        isTurningRightWithButton = state;
    }

    private void CheckTerrain() {
        //Check if object is infront by doing 
        Vector3 position = playerRef.transform.position + playerRef.GetCapsuleCollider().center;
        Vector3 size = new Vector3(10.0f, 10.0f, 10.0f); //playerRef.GetCapsuleCollider().size;
        float offset = 0.1f;

        position.y += offset;
        RaycastHit raycastHitData;

        bool results = Physics.BoxCast(position, size / 2, -playerRef.transform.up, out raycastHitData,
            playerRef.transform.rotation, offset * 2.0f, LayerMask.GetMask("Ramp"));
        if (results) {
            float dot = Vector3.Dot(playerRef.transform.up, raycastHitData.normal);
            //Log("DOT! " + dot);
            //Log("Angle is " + Mathf.Cos(stats.terrainAdjustmentAngle * Mathf.Deg2Rad));
            if (dot <= Mathf.Cos(stats.terrainAdjustmentAngle * Mathf.Deg2Rad) ||
                dot >= -Mathf.Cos(stats.terrainAdjustmentAngle * Mathf.Deg2Rad)) {
                //Not sure fully

                if (IsInBounds(playerRef.GetCapsuleCollider().bounds, raycastHitData.collider.bounds)) {
                    Log("IS �N BOUNDS!");
                }

                playerRef.transform.up = raycastHitData.transform.up;
                //playerRef.GetBoxCollider().size = new Vector3(0.75f, 1, 1); // otherwise z is 2
                //playerRef.GetBoxCollider().center = new Vector3(0.0f, 0.5f, 1.0f); //Otherwise z is 0

                Log("Hit with " + raycastHitData.collider.name);
                return;
            }
        }
        else {
            //There is one frame where it doesnt hit. It is consistent too so reset this after landing probably or when is not ground and the forcedboost bool is on
            //playerRef.GetBoxCollider().size = new Vector3(0.75f, 1, 2);
            //playerRef.GetBoxCollider().center = new Vector3(0.0f, 0.5f, 0.0f);
        }
        //playerRef.GetBoxCollider().size = new Vector3(0.75f, 1, 2); // otherwise z is 2
    }

    private void UpdateGroundedState() {
        Vector3 position = playerRef.transform.position;
        Vector3 size = new Vector3(1.0f, 1.0f, 1.0f); //playerRef.GetCapsuleCollider().size;
        float offset = 0.6f;
        position.y += offset;


        //Debug.Log(isGrounded);
        bool results = Physics.BoxCast(position, size / 2, -playerRef.transform.up, playerRef.transform.rotation,
            offset * 2.0f);
        if (!isGrounded && results) {
            //HitStop();

            Debug.Log(landingDustParticle);
            if (landingDustParticle != null) //didn't work properly so will check it out on wednesday //Chriss
            {
                Debug.Log("landing particle activated");
                landingDustParticle.Play(); //Landing Dust particle plays when the player hits the ground
            }

            else {
                Debug.Log("LandingDustParticle is null");
            }
        }

        isGrounded = results; //Separted to player vfx on landing! if(!ground && results)
    }

    private void UpdateGravity() {
        if (isGrounded)
            return;

        float gravity = stats.gravity * Time.fixedDeltaTime;
        playerRigidbody.velocity += new Vector3(0.0f, -gravity, 0.0f);
    }

    private void UpdateRampBoost() {
        if (!isRampBoosted)
            return;

        if (rampBoostTimer > 0.0f) {
            //currentSpeed += currentRampBoost.accelerationRate * Time.deltaTime;
            UpdateSpeed(currentRampBoost.accelerationRate, currentRampBoost.maxSpeed, null, true);
            if (currentSpeed >= currentRampBoost.maxSpeed) {
                //currentSpeed = currentRampBoost.maxSpeed;
                rampBoostTimer -= Time.deltaTime; //create function and add it as action callback
                Log("Boost timer " + rampBoostTimer);
                if (rampBoostTimer <= 0.0f) {
                    rampBoostTimer = 0.0f;
                    Log("Boost timer over");

                    //Disable any vfx
                }
            }
        }
        else {
            //THIS HERE IS KINDA WEiRD!
            currentSpeed -= currentRampBoost.decelerationRate * Time.deltaTime;
            if (isBoosting) {
                if (currentSpeed <= stats.maxSpeed) {
                    isRampBoosted = false;
                    Log("Forced boost deactivated at boost speed decel");
                }
            }
            else {
                if (currentSpeed <= stats.maxBoostSpeed) {
                    isRampBoosted = false;
                    Log("Forced boost deactivated at normal speed decel"); //Once you see this and confirm that it works. Delete the logs!
                }
            }
        }
    }

    private void UpdateTurning() {
        if (SystemInfo.supportsGyroscope) {
            UpdateTilt();
            DeactiveTurningButtons();
        }
        else {
            LeftAndRightButtons(); 
        }

        
    }

    private void DeactiveTurningButtons() {
        if (turningButtons == false)
            return;

        Debug.Log("turning off buttons");
        gameInstanceRef.GetDaredevilHUD().GetTurnButtons().gameObject.SetActive(false);
        turningButtons = false;
    }

    private void LeftAndRightButtons() {
        if (isTurningRightWithButton && !isTurningLeftWithButton) {
            tiltRate += stats.TiltRate * Time.deltaTime;
            if (tiltRate > 1.0f)
                tiltRate = 1.0f;
        }
        else if (isTurningLeftWithButton && !isTurningRightWithButton) {
            tiltRate -= stats.TiltRate * Time.deltaTime;
            if (tiltRate < -1.0f)
                tiltRate = -1.0f;
        }
        else {
            if (tiltRate > 0.0f) {
                tiltRate -= stats.TiltRate * Time.deltaTime;
                if (tiltRate < 0.0f)
                    tiltRate = 0.0f;
            }
            else if (tiltRate < 0.0f) {
                tiltRate += stats.TiltRate * Time.deltaTime;
                if (tiltRate > 0.0f)
                    tiltRate = 0.0f;
            }
        }

        if (tiltRate != 0.0f)
            isTurning = true;
    }

    public void ReduceSpeed(float speed) { currentSpeed -= speed; }

    private void UpdateMovement() {
        if (isReversingAndBraking) {
            if (currentSpeed > 0.0f)
                Brake();
            else if (currentSpeed <= 0.0f)
                Reverse();
        }
        else {
            if (isMoving)
                Accelerate();
            else if (!isMoving && isGrounded)
            {
                playerRef.transform.eulerAngles =
                new Vector3(playerRef.transform.eulerAngles.x, playerRef.transform.eulerAngles.y, 0.0f); //Kepp oit
                Decelerate();
            }
        }

        //Log(currentSpeed);
    }

    private void UpdateVelocity() {
        if (!isGrounded)
            return;

        Vector3 velocity = direction * (currentSpeed * Time.deltaTime);
        //velocity.y = playerRigidbody.velocity.y;
        
        playerRigidbody.velocity = velocity;
        if (currentSpeed != 0.0f)
            playerRef.transform.forward = direction;
    }

    private void UpdateDirection() {
        if (!isReversingAndBraking)
            direction = (playerRef.transform.forward +
                         playerRef.transform.right * (stats.driftRate * tiltRate * GetCurrentSpeedPercentage()))
                .normalized;
        else if (isReversingAndBraking)
            direction = (playerRef.transform.forward +
                         playerRef.transform.right * (stats.driftRate * -tiltRate * GetCurrentSpeedPercentage()))
                .normalized;
    }


    private void UpdateDebugTilt() {
        if (Input.GetKey(KeyCode.D)) {
            tiltRate += stats.debugTiltRate * Time.deltaTime;
            if (tiltRate > 1.0f)
                tiltRate = 1.0f;
        }

        if (Input.GetKey(KeyCode.A)) {
            tiltRate -= stats.debugTiltRate * Time.deltaTime;
            if (tiltRate < -1.0f)
                tiltRate = -1.0f;
        }
    }

    private void UpdateTilt() {
        tiltRate = Input.gyro.gravity.x;
        if (tiltRate != 0.0f) {
            isTurning = true;
        }
        else {
            isTurning = false;
            turningAnimWasTriggered = false;
        }
    }


    private void UpdateSpeed(float rate, float limit, Action callback, bool additive = true) {
        if (additive) {
            currentSpeed += rate * Time.deltaTime;
            if (currentSpeed >= limit) {
                currentSpeed = limit;
                if (callback != null)
                    callback.Invoke();
            }
        }
        else if (!additive) {
            currentSpeed -= rate * Time.deltaTime;
            if (currentSpeed <= limit) {
                currentSpeed = limit;
                if (callback != null)
                    callback.Invoke();
            }
        }
    }



    //Brake
    private void Brake() {
        if (!isGrounded)
            return;
        
        //currentSpeed -= stats.brakeRate * Time.deltaTime;
        UpdateSpeed(stats.brakeRate, 0.0f, null, false);
        //Log("Brake");
        //if (currentSpeed <= 0.0f) {
        //currentSpeed = 0.0f;
        //}
        
        if (soundTimer == 0)
        {
            gameInstanceRef.GetSoundSystem().PlayLocalSFX(sFXKeyBrake, gameInstanceRef.GetPlayerGameObject());
            soundTimer = soundCooldown/2;
        }

    }

    private void Reverse() {
        if (!isGrounded)
            return;

        UpdateSpeed(stats.reverseRate, -stats.maxReverseSpeed, null, false);
        //Log("Reverse");
        //if (currentSpeed <= -stats.maxReverseSpeed) {
        //currentSpeed = -stats.maxReverseSpeed;
        //}
    }


    public void Accelerate() {
        if (isBoosting) {
            //currentSpeed += stats.boostAccelerationRate * Time.deltaTime;
            UpdateSpeed(stats.boostAccelerationRate, stats.maxBoostSpeed, null, true);
            //Log("Boost Accelerate");
            //if (currentSpeed >= stats.maxBoostSpeed) {
            //currentSpeed = stats.maxBoostSpeed;
            //}

        }
        else if (!isBoosting)
        {
            if (currentSpeed > stats.maxSpeed)
            {
                //Boost recovery
                //currentSpeed -= stats.boostDecelerationRate * Time.deltaTime;
                UpdateSpeed(stats.boostDecelerationRate, stats.maxSpeed, null, false);
                //Log("Boost Recovery");
                //if (currentSpeed <= stats.maxSpeed) {
                //currentSpeed = stats.maxSpeed;
                //}
                if (soundTimer == 0)
                {
                    gameInstanceRef.GetSoundSystem().PlayLocalSFX(sFXKeyAccelerate, gameInstanceRef.GetPlayerGameObject());
                    soundTimer = soundCooldown;
                }

            }
            else if (currentSpeed < stats.maxSpeed) {
                //currentSpeed += stats.accelerationRate * Time.deltaTime;
                if (soundTimer == 0)
                {
                    gameInstanceRef.GetSoundSystem().PlayLocalSFX(sFXKeyAccelerate, gameInstanceRef.GetPlayerGameObject());
                    soundTimer = soundCooldown;
                }

                UpdateSpeed(stats.accelerationRate, stats.maxSpeed, null, true);
                //Log("Accelerate");
                //if (currentSpeed >= stats.maxSpeed) {
                //currentSpeed = stats.maxSpeed;
                //}



            }
        }
    }

    public void SoundCountdown() {
        if (soundTimer == 0.0f)
            return;
        //count down the timer in unscaled time until timeScale can be reset
        soundTimer -= Time.unscaledDeltaTime;
        if (soundTimer <= 0.0f)
        {
            Time.timeScale = 1.0f;
            soundTimer = 0.0f;
        }
    }

    public void Decelerate() {
        //TODO: Add unique behavior for if was boosting
        if (currentSpeed > 0.0f) {
            //currentSpeed -= stats.decelerationRate * Time.deltaTime;
            UpdateSpeed(stats.decelerationRate, 0.0f, null, false);
            //Log("Normal Decelrate");
            //if (currentSpeed <= 0.0f) {
            //currentSpeed = 0.0f;
            //}
        }
        else {
            if (currentSpeed < 0.0f) {
                //currentSpeed += stats.decelerationRate * Time.deltaTime; //COuld be something else. Reverse deceleraton rate
                UpdateSpeed(stats.decelerationRate, 0.0f, null, true);
                //Log("Reverse Decelrate");
                //if (currentSpeed > 0.0f) {
                //currentSpeed = 0.0f;
                //}
            }
        }
    }

    public void HitStop() {
        //pause timeScale and set a timer
        Time.timeScale = 0.0f;
        hitStopTimer = stats.hitStopDuration;
    }

    public void HitstopTimer() {
        if (hitStopTimer == 0.0f)
            return;
        //count down the timer in unscaled time until timeScale can be reset
        hitStopTimer -= Time.unscaledDeltaTime;
        if (hitStopTimer <= 0.0f) {
            Time.timeScale = 1.0f;
            hitStopTimer = 0.0f;
        }
    }

    private void TriggerTurningAnimation() {
        
        if (tiltRate is < 0.09f and > -0.09f) {
            turningAnimWasTriggered = false;
            isTurning = false;
            
            daredevilAnimationController.TriggerTurningAnimation(TurnAnimation.Left, false);
            daredevilAnimationController.TriggerTurningAnimation(TurnAnimation.Right, false);
        }

        if (turningAnimWasTriggered)
            return;
        switch (tiltRate) {
            case > 0.0f: {
                daredevilAnimationController.TriggerTurningAnimation(TurnAnimation.Right, isTurning);
                if (isTurning)
                    turningAnimWasTriggered = true;
                break;
            }
            case < 0.0f: {
                daredevilAnimationController.TriggerTurningAnimation(TurnAnimation.Left, isTurning);
                if (isTurning)
                    turningAnimWasTriggered = true;
                break;
            }
        }
    }


    public void ApplyRampBoost(RampBoost boost) {
        if (boost.duration == 0.0f || boost.maxSpeed == 0.0f)
            return;

        isRampBoosted = true; //Mainly in case of anything else needing the data. Could still use Timer alone for that.
        currentRampBoost = boost;
        rampBoostTimer = boost.duration;


        //Do unconditional timed boost.
        //Func takes time, max speed and rate, and maybe decelrate too so might as well be a struct of data.
        //Bool is checked and special func is called
    }


    public float GetCurrentSpeed() {
        return currentSpeed;
    }

    public float GetCurrentSpeedPercentage() {
        return currentSpeed / stats.maxSpeed;
    }
}