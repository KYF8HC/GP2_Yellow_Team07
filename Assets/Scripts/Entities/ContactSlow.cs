using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using static UnityEngine.Tilemaps.Tile;
using UnityEngine.UIElements;

public class ContactSlow : Entity
{
    [SerializeField] private float reduceSpeedBy = 2.0f;
    [SerializeField] private bool sfxOn = false;
    [SerializeField] private float sfxDuration;
    [SerializeField] private string sfxKey;
    
    [SerializeField] private GameObject particleEffect;
    
    private SoundSystem soundSystem;
    private float sfxTimer;
    private GameObject sfxObject;

    public override void Initialize(GameInstance game)
    {
        if (initialized)
        {
            return;
        }
        gameInstanceRef = game;
        initialized = true;
        soundSystem = gameInstanceRef.GetSoundSystem();
        sfxTimer = sfxDuration;
        
    }
    public override void Tick()
    {
        if(!initialized)
        {
            if(sfxTimer < sfxDuration)
            {
                sfxTimer += Time.deltaTime;
                if(sfxTimer >= sfxDuration)
                {
                    Destroy(sfxObject.GetComponent<AudioSource>());
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("ObstacleSlow"))
            return;

        if (sfxOn)
        {
            if(sfxObject != null)
            {
                Destroy(sfxObject.GetComponent<AudioSource>());
                sfxObject = null;
            }
            sfxObject = other.gameObject;
            //soundSystem.PlayLocalSFX(sfxKey, sfxObject);
            sfxTimer = 0;
        }
        var player = GetComponent<Player>();
        player.GetDaredevilData().ReduceSpeed(reduceSpeedBy);
        
        //Particle effect
        var effectTransform = Instantiate(particleEffect, other.transform.position, Quaternion.identity).transform;

        var particle = effectTransform.GetComponent<ParticleSystem>();

        var particleRenderer = effectTransform.GetComponent<ParticleSystemRenderer>();

        //particleRenderer.SetMeshes(new Mesh[] {other.GetComponent<MeshFilter>().mesh});
        particle.Play();
        
        Destroy(other.gameObject);
    }
}
