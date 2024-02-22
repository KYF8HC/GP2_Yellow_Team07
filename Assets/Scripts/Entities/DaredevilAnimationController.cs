using UnityEngine;

public class DaredevilAnimationController {

    private Player playerRef;
    private Animator bikeAnimator;
    private Animator daredevilAnimator;
    
    

    public void SetupReferences(Player player) {
        playerRef = player;
        bikeAnimator = playerRef.GetBikeAnimator();
        daredevilAnimator = playerRef.GetDaredevilAnimator();
    }

    public void TriggerTurningAnimation(TurnAnimation direction, bool state) {
        bikeAnimator.SetBool($"isTurning{direction}", state);
        daredevilAnimator.SetBool($"isTurning{direction}", state);
    }
}