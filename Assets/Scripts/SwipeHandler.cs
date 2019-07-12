using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeHandler : MonoBehaviour
{
    private Vector2 _touchPosition, _midScreenPosition;
    public static bool LeftSide, RightSide, UpSwipe, DownSwipe;

    void Start ()
    {
        _midScreenPosition = new Vector2(Screen.width / 2, Screen.height / 2);
    }
    
    void Update ()
    {
        switch (GameMode.CurrentGameModeState)
        {
            case GameMode.GameModeState.None:
                break;
            case GameMode.GameModeState.Gameplay:
                    DisableAllSwipes();
                if (Input.GetMouseButtonDown(0))
                    _touchPosition = Input.mousePosition;
                if (Input.GetMouseButton(0))
                {
                    Vector2 newPosition = Input.mousePosition;
                    Swiping(newPosition.y);
                }
                break;
        }
    }
    
    private void Swiping(float y)
    {
        if (_touchPosition.x > _midScreenPosition.x)
            LeftSide = true;
        if (_touchPosition.x < _midScreenPosition.x)
            RightSide = true;
        if (_touchPosition.y > y)
            UpSwipe = true;
        if (_touchPosition.y < y)
            DownSwipe = true;
    }

    private static void DisableAllSwipes()
    {
        LeftSide = RightSide = UpSwipe = DownSwipe = false;
    }
}
