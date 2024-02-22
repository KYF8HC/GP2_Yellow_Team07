using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugHUD : MonoBehaviour {


    private TextMeshProUGUI fpsText;
    private TextMeshProUGUI refreashRateText;


    void Start() {
        SetupReferences();
    }
    private void SetupReferences() {
        fpsText = transform.Find("FPS").GetComponent<TextMeshProUGUI>();
        refreashRateText = transform.Find("RefreshRate").GetComponent<TextMeshProUGUI>();
    }
    void Update() {

        if (fpsText)
            fpsText.text = "FPS " + (int)(1.0f / Time.unscaledDeltaTime);

        if (refreashRateText)
            refreashRateText.text = "Refresh Rate " + Application.targetFrameRate;

    }
}
