using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFX : Entity {
    [SerializeField] bool sfx, track, sfxLoop;
    [SerializeField] string sfxKey, trackKey;
    SoundSystem soundSystem;
    bool active = false;
    DynamicSound dynamicSound;

    private AudioSource currentAudioSource;

    public override void Initialize(GameInstance game) {
        if (initialized) {
            return;
        }

        gameInstanceRef = game;
        initialized = true;
        soundSystem = gameInstanceRef.GetSoundSystem();
        dynamicSound = GetComponent<DynamicSound>();
        Debug.Log("Player SFX Initialized");
    }

    public override void Tick() {
        if (gameInstanceRef.currentGameState == GameInstance.GameState.PLAYING && !active) {
            if (sfx) {
                Debug.Log("Player SFX On");
                soundSystem.PlayLocalSFX(sfxKey, gameObject);
                dynamicSound.dynamSoundActive = true;
                currentAudioSource = GetComponent<AudioSource>();
                currentAudioSource.loop = sfxLoop;
            }

            if (track) {
                Debug.Log("Track On");
                soundSystem.PlayTrack(trackKey);
            }

            active = true;
        }
        
        GetComponent<AudioSource>().volume = gameInstanceRef.GetSoundSystem().GetSFXVolume() * gameInstanceRef.GetSoundSystem().GetMasterVolume();
        
        if (gameInstanceRef.currentGameState != GameInstance.GameState.PLAYING && active) {
            if (sfx) {
                Debug.Log("Player SFX Off");
                dynamicSound.dynamSoundActive = false;
                currentAudioSource = null;
                Destroy(GetComponent<AudioSource>());
            }

            if (track) {
                Debug.Log("track off");
                soundSystem.StopTrack();
            }

            active = false;
        }
        
    }
}