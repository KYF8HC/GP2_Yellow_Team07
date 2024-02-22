using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class DynamicSound : Entity
{
    /*[SerializeField] AudioMixer audioMixer;
    [SerializeField] public string type;

    public void setVolume(float volume)
    {
        audioMixer.SetFloat(type, Mathf.Log10(volume) * 20);
    }*/ //Ref from volume slider script I previously made - Johan.

    [SerializeField] float volume, pitch, speed;//Placeholder for values from other sources.
    [SerializeField] float volumeMin, volumeMax;
    [SerializeField] float pitchMin, pitchMax;
    [SerializeField] float pitchMod, volumeMod;
    public bool dynamSoundActive = false;
    AudioSource source;
    public override void Initialize(GameInstance game)
    {
        if (initialized)
        {
            return;
        }
        gameInstanceRef = game;
        initialized = true;
    }
    public override void Tick()
    {
        if (initialized)
        {
            if (dynamSoundActive)
            {
                speed = gameInstanceRef.GetPlayer().GetDaredevilData().GetCurrentSpeed();
                SpeedVolumeDynamics();
            }
        }

    }
    public void SpeedVolumeDynamics()
    {
        source = gameObject.GetComponent<AudioSource>();
        volume += speed * volumeMod;
        pitch += speed * pitchMod;


        if(volume < volumeMin) volume = volumeMin;
        else if(volume > volumeMax) volume = volumeMax;
        if(pitch < pitchMin) pitch = pitchMin;
        else if(pitch > pitchMax) pitch = pitchMax;

        source.volume = volume;
        source.pitch = pitch;
        source.loop = true;
        //Update Pitch and volume
    }
    



}
