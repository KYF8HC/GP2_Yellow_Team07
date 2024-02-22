using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static MyUtility.Utility;

[Serializable]
public struct LevelOption {
    public int index;
    public Image progressBar;
    public Image previewImage;
    public Button button;
    public string name;
    public string levelKey;
}


public class LevelSelectMenu : Entity {
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private Image voteCheckMark1;
    [SerializeField] private Image voteCheckMark2;
    [SerializeField] private Image readyCheckMark1;
    [SerializeField] private Image readyCheckMark2;

    //Ref
    public LevelOption[] levelOptions;

    private LevelsBundle levelsBundle;
    private ScrollRect scrollRectComp;
    private HorizontalLayoutGroup layoutGroupComp;

    private Slider timeBar;

    //Ready checks
    public bool client1Ready = false;
    public bool client2Ready = false;

    //Level votes
    public string client1LevelVote = "";
    public string client2LevelVote = "";

    //Holding button
    private bool isButtonHeld = false;
    private float timer = 0.0f;
    private float timeToHold = 1.0f;

    private int selectedElementIndex = 0;


    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        gameInstanceRef = game;
        SetupReferences();

        initialized = true;
    }

    public override void Tick() {
        if (!initialized) {
            Error("");
            return;
        }

        if (Input.GetKeyDown(KeyCode.W)) {
            scrollRectComp.content.localPosition = Vector2.zero;
        }

        ButtonTimer();
        UpdateTimerBar(selectedElementIndex);
    }

    public void SetupMenuStartingState() {
        client1Ready = false;
        client2Ready = false;

        readyCheckMark1.gameObject.SetActive(false);
        readyCheckMark2.gameObject.SetActive(false);
        voteCheckMark1.gameObject.SetActive(false);
        voteCheckMark2.gameObject.SetActive(false);

        //Menu gets reconstructed on each opening of the menu
        levelsBundle = gameInstanceRef.GetLevelManagement().GetLevelsBundle();
        SetupGUIElements();
    }

    private void SetupReferences() {
        var scrollViewTransform = transform.Find("ScrollView");
        scrollRectComp = scrollViewTransform.GetComponent<ScrollRect>();
        layoutGroupComp = scrollRectComp.content.gameObject.GetComponent<HorizontalLayoutGroup>();
    }

    private void SetupGUIElements() {
        if (!levelsBundle) {
            Warning("Invalid levels bundle!\nUnable to construct GUI elements.");
            return;
        }

        levelOptions = new LevelOption[levelsBundle.Entries.Length];
        for (var i = 0; i < levelsBundle.Entries.Length + 1; i++) {
            var newOption = Instantiate(buttonPrefab, scrollRectComp.content);
            var levelButton = newOption.transform.Find("LevelButton");
            if (i == levelsBundle.Entries.Length) {
                newOption.GetComponent<Image>().enabled = false;
                levelButton.GetComponent<Image>().enabled = false;
                newOption.name = "EmptyOption";
                continue;
            }


            var levelOption = new LevelOption {
                progressBar = newOption.GetComponent<Image>(),
                previewImage = levelButton.GetComponent<Image>(),
                button = levelButton.GetComponent<Button>(),
                name = levelsBundle.Entries[i].name,
                levelKey = levelsBundle.Entries[i].key
            };

            levelOption.progressBar.fillAmount = 0.0f;
            levelOption.progressBar.sprite = levelsBundle.Entries[i].preview;
            levelOption.previewImage.sprite = levelsBundle.Entries[i].preview;
            levelOption.index = i;

            var eventTrigger = levelButton.GetComponent<EventTrigger>();

            var onPointerEnter = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerDown
            };
            onPointerEnter.callback.AddListener((eventData) => { OnStartClicking(levelOption.index); });

            var onPointerExit = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerUp
            };
            onPointerExit.callback.AddListener((eventData) => { OnEndClicking(levelOption.index); });

            eventTrigger.triggers.Add(onPointerEnter);
            eventTrigger.triggers.Add(onPointerExit);

            levelOptions[i] = levelOption;
        }
    }

    //Button holding
    private void ButtonTimer() {
        if (!isButtonHeld)
            return;

        timer += Time.deltaTime;

        if (timer >= timeToHold) {
            levelOptions[selectedElementIndex].progressBar.fillAmount = 1.0f;
            levelNameText.text = levelOptions[selectedElementIndex].name;
            timer = 0.0f;
            isButtonHeld = false;
            TimerOver();
        }
    }

    private void TimerOver() {
        FinalizeVote();
    }

    private void FinalizeVote() {
        client1LevelVote = levelOptions[selectedElementIndex].levelKey;
        Debug.Log($"{client1LevelVote} +  {client2LevelVote}");
        gameInstanceRef.GetRPCManagement().UpdateLevelSelectionServerRpc(Netcode.GetClientID(), client1LevelVote);
        UpdateGUI();
    }

    private void UpdateGUI() {
        if (string.IsNullOrEmpty(client1LevelVote))
            voteCheckMark1.gameObject.SetActive(false);
        if (string.IsNullOrEmpty(client2LevelVote))
            voteCheckMark2.gameObject.SetActive(false);

        foreach (var level in levelOptions) {
            if (level.levelKey == client1LevelVote) {
                voteCheckMark1.gameObject.SetActive(true);
                voteCheckMark1.rectTransform.SetParent(level.progressBar.rectTransform);
                voteCheckMark1.rectTransform.anchoredPosition = new Vector3(0, 0, 0);
            }

            if (level.levelKey == client2LevelVote) {
                voteCheckMark2.gameObject.SetActive(true);
                voteCheckMark2.rectTransform.SetParent(level.progressBar.rectTransform);
                voteCheckMark2.rectTransform.anchoredPosition = new Vector3(0, 0, 0);
            }

            if (client1LevelVote == client2LevelVote) {
                voteCheckMark1.rectTransform.anchoredPosition = new Vector3(150, 0, 0);
                voteCheckMark2.rectTransform.anchoredPosition = new Vector3(-150, 0, 0);
            }
        }

        if (client1Ready)
            readyCheckMark1.gameObject.SetActive(true);
        else if (readyCheckMark1.IsActive())
            readyCheckMark1.gameObject.SetActive(false);

        if (client2Ready)
            readyCheckMark2.gameObject.SetActive(true);
        else if (readyCheckMark2.IsActive())
            readyCheckMark2.gameObject.SetActive(false);
    }

    public void ReceiveLevelSelectionRPC(string value) {
        client2LevelVote = value;
        UpdateGUI();
        Debug.Log($"{client1LevelVote}  +  {client2LevelVote}");
    }

    public void ReceiveReadyCheckRPC(bool value) {
        client2Ready = value;
        UpdateGUI();
        CheckReadyStatus();
    }

    private void UpdateTimerBar(int index) {
        if (!isButtonHeld)
            return;
        levelOptions[index].progressBar.fillAmount = timer / timeToHold;
    }

    public void DebugLevelButton() {
        gameInstanceRef.StartGame("DebugLevel");
    }

    private void CheckReadyStatus() {
        Debug.Log($"{client1Ready}  +  {client2Ready}");
        if (client1Ready && client2Ready) {
            if (gameInstanceRef.GetNetcode().IsHost()) {
                var clientID = Netcode.GetClientID();
                var levelKey = client1LevelVote;

                if (string.IsNullOrEmpty(client2LevelVote) && string.IsNullOrEmpty(client1LevelVote)) {
                    levelKey = levelsBundle.Entries[^1].key;
                }

                if (client1LevelVote != client2LevelVote) {
                    if (string.IsNullOrEmpty(client1LevelVote)) {
                        levelKey = client2LevelVote;
                    }

                    else if (string.IsNullOrEmpty(client2LevelVote)) {
                        levelKey = client1LevelVote;
                    }

                    else {
                        var random = UnityEngine.Random.Range(0.0f, 1.0f);
                        levelKey = random > 0.5f ? client1LevelVote : client2LevelVote;
                    }
                }

                gameInstanceRef.GetRPCManagement().ConfirmLevelSelectionServerRpc((ulong)clientID, levelKey);
                gameInstanceRef.GetLevelManagement().QueueLevelLoadKey(levelKey);
                gameInstanceRef.StartGame();
            }
        }
    }

    public void ReadyButton() {
        ulong clientID = Netcode.GetClientID();
        if (client1Ready)
            client1Ready = false;
        else if (!client1Ready)
            client1Ready = true;

        readyCheckMark1.gameObject.SetActive(client1Ready);
        gameInstanceRef.GetRPCManagement().UpdateLevelReadyCheckServerRpc((ulong)clientID, client1Ready);
        CheckReadyStatus();
    }

    public void OnStartClicking(int index) {
        var option = levelOptions[index];

        var middleIndex = (float)levelOptions.Length / 2;
        var indexDiff = middleIndex - index - .5f;

        var spacingTotal = layoutGroupComp.spacing * indexDiff + 1;
        var totalWidth = option.progressBar.rectTransform.rect.width * indexDiff + 1;

        var scrollRectPosition = scrollRectComp.content.localPosition;
        var resultRect = new Vector3(spacingTotal + totalWidth, scrollRectPosition.y, scrollRectPosition.z);
        scrollRectComp.content.localPosition = resultRect;
        scrollRectComp.velocity = Vector2.zero;

        selectedElementIndex = index;
        isButtonHeld = true;
    }

    public void OnEndClicking(int index) {
        levelOptions[index].progressBar.fillAmount = 0.0f;
        isButtonHeld = false;
    }
}