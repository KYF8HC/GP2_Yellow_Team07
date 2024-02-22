using System.Xml.XPath;
using Cinemachine;
using UnityEngine;


public class CameraShakeType {
    public float intensity;
    public float duration;
    public Vector3 direction;
}

public class MainCamera : Entity {
    private CinemachineVirtualCamera virtualCamera;
    private Player playerRef;

    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        gameInstanceRef = game;
        initialized = true;
        
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public override void Tick() {
        if (!initialized)
            return;

    }

    public override void FixedTick() {
        if (!initialized)
            return;
        
    }
    
    public void SetPlayerReference(Player player)
    {
        playerRef = player;
        virtualCamera.Follow = playerRef.transform;
        virtualCamera.LookAt = playerRef.transform;
    }
}