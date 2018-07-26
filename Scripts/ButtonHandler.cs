using System.Collections;
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

    private const string _START_FREE_BUTTON_NAME = "StartFree";
    private const string _START_LADDER_BUTTON_NAME = "StartLadder";
    private const string _BACKGROUND_BUTTON_NAME = "Background";
    private const string _QUIT_BUTTON_NAME = "Quit";
    private const string _PAUSE_BUTTON_NAME = "Pause";
    private const string _CONTROL_BUTTON_NAME = "Buttons/Swipe";
    private const string _UP_LEFT_BUTTON_NAME = "Up Left";
    private const string _UP_RIGHT_BUTTON_NAME = "Up Right";
    private const string _DOWN_LEFT_BUTTON_NAME = "Down Left";
    private const string _DOWN_RIGHT_BUTTON_NAME = "Down Right";
    private const string _UP_LEFT_SWIPE = "Up Left Swipe";
    private const string _UP_RIGHT_SWIPE = "Up Right Swipe";
    private const string _DOWN_LEFT_SWIPE = "Down Left Swipe";
    private const string _DOWN_RIGHT_SWIPE = "Down Right Swipe";

    public static bool StartFreeButton, StartLadderButton, BackgroundButton, QuitButton, PauseButton;
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
            if(go.name != _QUIT_BUTTON_NAME && !go.name.Contains("Start") && go.name != _BACKGROUND_BUTTON_NAME)
                go.SetActive(enable);
        }
    }
    private void EnableStartButtons(bool enable)
    {
        foreach (GameObject go in _buttons)
        {
            if (go.name.Contains("Start") || go.name == _BACKGROUND_BUTTON_NAME)
                go.SetActive(enable);
        }
    }
    private void EnablePlayingButtons(bool enable)
    {
        foreach (GameObject go in _buttons)
        {
            if (go.name != _QUIT_BUTTON_NAME && !go.name.Contains("Start") &&
                go.name != _BACKGROUND_BUTTON_NAME && go.name != _PAUSE_BUTTON_NAME && 
                go.name != _CONTROL_BUTTON_NAME)
                go.SetActive(enable);
        }
    }
    private void EnableSwipeButtons(bool enable)
    {
        foreach (GameObject go in _buttons)
        {
            if (go.name == _UP_LEFT_SWIPE || go.name == _UP_RIGHT_SWIPE ||
                go.name == _DOWN_LEFT_SWIPE || go.name == _DOWN_RIGHT_SWIPE)
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

            if (go.name == _CONTROL_BUTTON_NAME)
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
        string button = data.selectedObject.name;

        DisableAllButtons();
        switch (button)
        {
            case "":
                Debug.Log("Nothing");
                break;
            case _QUIT_BUTTON_NAME:
                QuitButton = enable;
                break;
            case _START_FREE_BUTTON_NAME:
                StartFreeButton = enable;
                break;
            case _START_LADDER_BUTTON_NAME:
                StartLadderButton = enable;
                break;
            case _BACKGROUND_BUTTON_NAME:
                BackgroundButton = enable;
                break;
            case _UP_LEFT_BUTTON_NAME:
                UpLeftButton = enable;
                break;
            case _UP_RIGHT_BUTTON_NAME:
                UpRightButton = enable;
                break;
            case _DOWN_LEFT_BUTTON_NAME:
                DownLeftButton = enable;
                break;
            case _DOWN_RIGHT_BUTTON_NAME:
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
            case _CONTROL_BUTTON_NAME:
                ControlButton = !ControlButton;
                HandleControlButton();
                EnableSwipeButtons(!ControlButton);
                SwitchControlsImage(ControlButton);
                break;
            case _PAUSE_BUTTON_NAME:
                PauseButton = !PauseButton;
                SwitchPauseImage(PauseButton);
                break;
        }
    }

    public static void DisableAllButtons()
    {
        StartFreeButton = StartLadderButton = QuitButton =
            UpLeftButton = UpRightButton = DownLeftButton = DownRightButton =
            false;
    }
}
