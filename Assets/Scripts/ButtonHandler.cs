using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    private GameObject _controlButton;
    private List<GameObject> _buttons;
    private List<GameObject> _pauseImages; //0: Pause. 1: Play.
    private List<GameObject> _controlImages; //0: Buttons. 1: Swipe.

    private const string StartFreeButtonName = "StartFree";
    private const string StartLadderButtonName = "StartLadder";
    private const string BackgroundButtonName = "Background";
    private const string GradientButtonName = "Gradient";
    private const string QuitButtonName = "Quit";
    private const string HelpButtonName = "Help Page";
    private const string CreditsButtonName = "Credits";
    private const string PauseButtonName = "Pause";
    private const string ControlButtonName = "Buttons/Swipe";
    private const string UpLeftButtonName = "Up Left";
    private const string UpRightButtonName = "Up Right";
    private const string DownLeftButtonName = "Down Left";
    private const string DownRightButtonName = "Down Right";
    private const string UpLeftSwipe = "Up Left Swipe";
    private const string UpRightSwipe = "Up Right Swipe";
    private const string DownLeftSwipe = "Down Left Swipe";
    private const string DownRightSwipe = "Down Right Swipe";

    public static bool StartFreeButton, StartLadderButton, BackgroundButton, GradientButton, QuitButton, PauseButton, HelpButton, CreditsButton;
    public static bool ControlButton; //True = Buttons. False = Swipe.
    public static bool UpLeftButton, UpRightButton, DownLeftButton, DownRightButton;

    void Start ()
    {
        _pauseImages = new List<GameObject>(GameObject.FindGameObjectsWithTag("Pause"));
        PauseButton = false;

        _controlImages = new List<GameObject>(GameObject.FindGameObjectsWithTag("Controls"));

        SetupButtonTriggers();
        EnableGameplayButtons(false);
        EnableStartButtons(true);
        EnableSwipeButtons(false);
        SwitchPauseImage(PauseButton);
        SwitchControlsImage(true);
    }

    void Update ()
    {
        switch(GameMode.CurrentGameModeState)
        {
            case GameMode.GameModeState.None:
                break;
            case GameMode.GameModeState.Start:
                if (!_buttons.Find(x => x.name == StartLadderButtonName).activeSelf)
                    EnableStartButtons(true);
                break;
            case GameMode.GameModeState.Info:
                EnableOnlyQuitButton(false);
                break;
            case GameMode.GameModeState.SetupGameplay:
                EnableStartButtons(false);
                EnableGameplayButtons(true);
                EnableSwipeButtons(false);
                break;
        }
    }

    private void EnableGameplayButtons(bool enable)
    {
        foreach (GameObject go in _buttons)
        {
            if(go.name != QuitButtonName && !go.name.Contains("Start") && go.name != BackgroundButtonName && go.name != GradientButtonName &&
                go.name != HelpButtonName && go.name != CreditsButtonName)
                go.SetActive(enable);
        }
    }
    private void EnableStartButtons(bool enable)
    {
        foreach (GameObject go in _buttons)
        {
            if (go.name.Contains("Start") || go.name == BackgroundButtonName || go.name == GradientButtonName || go.name == HelpButtonName || 
                go.name == CreditsButtonName)
                go.SetActive(enable);
        }
    }
    private void EnablePlayingButtons(bool enable)
    {
        foreach (GameObject go in _buttons)
        {
            if (go.name != QuitButtonName && !go.name.Contains("Start") &&
                go.name != BackgroundButtonName && go.name != PauseButtonName &&
                go.name != GradientButtonName && go.name != ControlButtonName &&
                go.name != HelpButtonName && go.name != CreditsButtonName)
                go.SetActive(enable);
        }
    }
    private void EnableSwipeButtons(bool enable)
    {
        foreach (GameObject go in _buttons)
        {
            if (go.name == UpLeftSwipe || go.name == UpRightSwipe ||
                go.name == DownLeftSwipe || go.name == DownRightSwipe)
                go.SetActive(enable);
        }
    }
    private void EnableOnlyQuitButton(bool enable)
    {
        foreach (GameObject go in _buttons)
        {
            if (go.name != QuitButtonName)
                go.SetActive(enable);
        }
    }

    private void SwitchPauseImage(bool paused)
    {
        SwitchImages(_pauseImages, !paused);
    }
    private void SwitchControlsImage(bool buttons)
    {
        SwitchImages(_controlImages, !buttons);
    }
    private void SwitchImages(List<GameObject> images, bool enable)
    {
        images[0].gameObject.SetActive(enable);
        images[1].gameObject.SetActive(!enable);
    }

    private void HandleControlButton()
    {
        float alpha = _buttons[0].GetComponent<Button>().colors.normalColor.a;
        Color newColor = ControlButton ? new Color(0, 1, 0, alpha) : new Color(0, 0, 1, alpha);
        ChangeControlButton(newColor);
        EnablePlayingButtons(ControlButton);
    }

    private void ChangeControlButton(Color color)
    {
        ColorBlock cb = _controlButton.GetComponent<Button>().colors;
        cb.normalColor = color;
        cb.highlightedColor = new Color(color.r, color.g, color.b, cb.pressedColor.a);
        _controlButton.GetComponent<Button>().colors = cb;
    }

    private void SetupButtonTriggers()
    {
        _buttons = new List<GameObject>(GameObject.FindGameObjectsWithTag("Button"));
        foreach (GameObject go in _buttons)
        {
            EventTrigger et = go.GetComponent<EventTrigger>();
            EventTrigger.Entry entryDown = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entryDown.callback.AddListener(x => { OnPointerDown((PointerEventData)x); });
            et.triggers.Add(entryDown);

            EventTrigger.Entry entryUp = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };
            entryUp.callback.AddListener(x => { OnPointerUp((PointerEventData)x); });
            et.triggers.Add(entryUp);

            if (go.name == ControlButtonName)
            {
                _controlButton = go;
                ControlButton = true;
                HandleControlButton();
            }
        }
    }
    private void OnPointerDown(PointerEventData data)
    {
        HandleButtonClick(data, true);
        HandleOnClick(data);
    }
    private void OnPointerUp(PointerEventData data)
    {
        HandleButtonClick(data, false);
        EventSystem.current.SetSelectedGameObject(null);
    }
    private void HandleButtonClick(PointerEventData data, bool enable)
    {
        if (!data.selectedObject)
            return;
        string button = data.selectedObject.name;

        DisableAllButtons();
        switch (button)
        {
            case "":
                Debug.Log("Nothing");
                break;
            case QuitButtonName:
                QuitButton = enable;
                break;
            case StartFreeButtonName:
                StartFreeButton = enable;
                break;
            case StartLadderButtonName:
                StartLadderButton = enable;
                break;
            case BackgroundButtonName:
                BackgroundButton = enable;
                break;
            case GradientButtonName:
                GradientButton = enable;
                break;
            case HelpButtonName:
                HelpButton = enable;
                break;
            case CreditsButtonName:
                CreditsButton = enable;
                break;
            case UpLeftButtonName:
                UpLeftButton = enable;
                break;
            case UpRightButtonName:
                UpRightButton = enable;
                break;
            case DownLeftButtonName:
                DownLeftButton = enable;
                break;
            case DownRightButtonName:
                DownRightButton = enable;
                break;
        }
    }
    private void HandleOnClick(PointerEventData data)
    {
        string button = data.selectedObject.name;
        
        switch (button)
        {
            case "":
                Debug.Log("Nothing");
                break;
            case ControlButtonName:
                ControlButton = !ControlButton;
                HandleControlButton();
                EnableSwipeButtons(!ControlButton);
                SwitchControlsImage(ControlButton);
                break;
            case PauseButtonName:
                PauseButton = !PauseButton;
                SwitchPauseImage(PauseButton);
                break;
        }
    }

    public static void DisableAllButtons()
    {
        StartFreeButton = StartLadderButton = QuitButton = HelpButton = CreditsButton =
            UpLeftButton = UpRightButton = DownLeftButton = DownRightButton =
            false;
    }
}
