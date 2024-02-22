using Unity.VisualScripting;
using UnityEngine;
using static MyUtility.Utility;


public class Player : NetworkedEntity {
    public enum Identity {
        NONE = 0,
        DAREDEVIL = 1, //Daredevil
        COORDINATOR = 2 //Coordinator
    }

    [SerializeField] private DaredevilStats daredevilStats;
    [SerializeField] private CoordinatorStats coordinatorStats;

    private Identity assignedPlayerIdentity = Identity.NONE;

    private Daredevil daredevilData = new Daredevil();
    private Coordinator coordinatorData = new Coordinator();
    

    private CoordinatorHUD coordinatorHUD;
    private DaredevilHUD daredevilHUD;

    private Rigidbody rigidbodyComp;

    private CapsuleCollider capsuleColliderComp;

    private Animator bikeAnimator;
    private Animator daredevilAnimator;
    
    private PlayerSFX playerSFX;
    private ContactSlow contactSlow;

    public override void Initialize(GameInstance game)
    {
        if (initialized)
            return;


        SetupReference();

        daredevilData.Initialize(game, this);
        coordinatorData.Initialize(game, this);

        gameInstanceRef = game;
        initialized = true;
    }
    public override void Tick() {
        if (!initialized) {
            Warning("Attempted to tick player before it was initialized!");
            return;
        }

        if (assignedPlayerIdentity == Identity.DAREDEVIL)
        {
            daredevilData.Tick();
            daredevilHUD.Tick();
            playerSFX.Tick();
        }
        else if (assignedPlayerIdentity == Identity.COORDINATOR)
        {
            coordinatorData.Tick();
            coordinatorHUD.Tick();
        }
    }
    public override void FixedTick() {
        if (!initialized) {
            Warning("Attempted to fixed tick player before it was initialized!");
            return;
        }

        if (assignedPlayerIdentity == Identity.DAREDEVIL) {
            daredevilData.FixedTick();
            daredevilHUD.FixedTick();
        }
        else if (assignedPlayerIdentity == Identity.COORDINATOR) {
            coordinatorData.FixedTick();
            coordinatorHUD.FixedTick();
        }
    }
    private void SetupReference()
    {
        rigidbodyComp = GetComponent<Rigidbody>();
        Validate(rigidbodyComp, "Failed to get reference to Rigidbody component!", ValidationLevel.ERROR, true);

        capsuleColliderComp = GetComponent<CapsuleCollider>();
        Validate(capsuleColliderComp, "Failed to get reference to CapsuleCollider component!", ValidationLevel.ERROR, true);
        
        var mesh = transform.Find("Mesh");
        Validate(mesh, "Failed to get reference to Mesh transform!", ValidationLevel.ERROR, true);
        var bike = mesh.Find("Bike");
        Validate(bike, "Failed to get reference to Bike transform!", ValidationLevel.ERROR, true);
        bikeAnimator = bike.Find("AnimatedBike").GetComponent<Animator>();
        Validate(bikeAnimator, "Failed to get reference to Bike Animator component!", ValidationLevel.ERROR, true);
        
        daredevilAnimator = mesh.Find("Daredevil").GetComponent<Animator>();
        Validate(daredevilAnimator, "Failed to get reference to Daredevil Animator component!", ValidationLevel.ERROR, true);
    }
    public void SetupStartState() {
        if (assignedPlayerIdentity == Identity.DAREDEVIL) { //Order matters due to stats being reset in data then HUD using those stats.
            daredevilData.SetupStartState();
            daredevilHUD.SetupStartState();
            
            playerSFX = GetComponent<PlayerSFX>();
            playerSFX.Initialize(gameInstanceRef);
            
            contactSlow = GetComponent<ContactSlow>();
            contactSlow.Initialize(gameInstanceRef);
        }
        else if (assignedPlayerIdentity == Identity.COORDINATOR) {
            coordinatorData.SetupStartState();
            coordinatorHUD.SetupStartState();
        }
    }




    public void AssignPlayerIdentity(Identity playerIdentity) { assignedPlayerIdentity = playerIdentity; }
    public void SetDaredevilHUD(DaredevilHUD hud) { daredevilHUD = hud; }
    public void SetCoordinatorHUD(CoordinatorHUD hud) { coordinatorHUD = hud; }


   


    public Identity GetPlayerIdentity() { return assignedPlayerIdentity; }

    public DaredevilStats GetDaredevilStats() { return daredevilStats; }
    public CoordinatorStats GetCoordinatorStats() { return coordinatorStats; }
    public DaredevilHUD GetDaredevilHUD() { return daredevilHUD; }
    public CoordinatorHUD GetCoordinatorHUD() { return coordinatorHUD; }
    public Daredevil GetDaredevilData() { return daredevilData; }
    public Coordinator GetCoordinatorData() { return coordinatorData; }

    public Rigidbody GetRigidbody() { return rigidbodyComp; }
    
    public Animator GetBikeAnimator() { return bikeAnimator; }
    public Animator GetDaredevilAnimator() { return daredevilAnimator; }
    
    public CapsuleCollider GetCapsuleCollider() { return capsuleColliderComp; }


}
