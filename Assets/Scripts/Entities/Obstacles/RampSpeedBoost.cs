using UnityEngine;
using static MyUtility.Utility;


public class RampSpeedBoost : Obstacle {
    
    [SerializeField] public RampBoost RampBoost = new RampBoost();
    [SerializeField] private GameObject redStateObject = null;
    [SerializeField] private GameObject blueStateObject = null;

    private GameObject liveObject = null;
    private Animator rampAnimator = null;


    public override void Initialize(GameInstance game) {
        if (initialized)
            return;


        gameInstanceRef = game;
        initialized = true;

        rampAnimator = GetComponent<Animator>();
        redStateObject.SetActive(false);
        blueStateObject.SetActive(false);
        
        if (assignedActivationState == ObstacleActivationState.RED) {
            redStateObject.SetActive(true);
            liveObject = redStateObject;
        }
        else if (assignedActivationState == ObstacleActivationState.BLUE) {
            blueStateObject.SetActive(true);
            liveObject = blueStateObject;
        }
    }
    public override void Tick() {
        
        //Anything specific to this!
    }

    public override void SetActivationState(bool state) {
        rampAnimator.SetBool("IsActive", state);
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player") || !activated)
            return;

        Player player = other.GetComponent<Player>();
        player.GetDaredevilData().ApplyRampBoost(RampBoost);
        Log("Boosted!");
    }
}
