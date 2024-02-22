using UnityEngine;

public class WinStateTrigger : Entity
{
    [SerializeField] GameInstance.GameState gameState = GameInstance.GameState.WIN_MENU;
    [SerializeField] bool sfxON = false;
    [SerializeField] string sFXKey;
    public GameObject winMenu;
    SoundSystem soundSystem;
   
    
    public override void Initialize(GameInstance game)
    {
        if (initialized)
        {
            return;
        }
        gameInstanceRef = game;
        initialized = true;
        soundSystem = gameInstanceRef.GetSoundSystem();
        winMenu = gameInstanceRef.GetWinMenu();
    }
    
    public void OnTriggerEnter(Collider other) {
        
        if (!other.CompareTag("Player")) return;

        if(sfxON)
        {
            soundSystem.PlayLocalSFX(sFXKey, gameObject);
        }
        gameInstanceRef.GetWinMenuScript().UpdateTimeText();
        gameInstanceRef.SetGameState(gameState);
    }
}
