using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : Entity {

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private GameInstance.GameState previousState = GameInstance.GameState.MAIN_MENU;
    public override void Initialize(GameInstance game) {
        if (initialized)
            return;


        gameInstanceRef = game;
        initialized = true;
    }

    public void BackButton() {
        gameInstanceRef.SetGameState(previousState);
    }
    
    public void SetPreviousState(GameInstance.GameState state) {
        previousState = state;
    }
    
    public void MasterVolumeSlider() {
        gameInstanceRef.GetSoundSystem().SetMasterVolume(masterVolumeSlider.value);
    }
    
    public void MusicVolumeSlider() {
       gameInstanceRef.GetSoundSystem().SetMusicVolume(musicVolumeSlider.value);
    }
    
    public void SFXVolumeSlider() {
        var soundSystem = gameInstanceRef.GetSoundSystem();
        soundSystem.SetSFXVolume(sfxVolumeSlider.value);
        
    }
}
