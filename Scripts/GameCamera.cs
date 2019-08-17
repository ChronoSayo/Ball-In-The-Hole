using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    private Transform _ball;
    private Vector3 _startPosition;

    void Start ()
    {
        _ball = GameObject.Find("Ball").transform;
        _startPosition = new Vector3(transform.position.x, _ball.position.y, transform.position.z);
        transform.position = _startPosition;
    }
    
    void Update ()
    {
        switch (GameMode.CurrentGameplayState)
        {
            case GameMode.GameplayState.None:
                break;
            case GameMode.GameplayState.Playing:
                //transform.position = new Vector3(transform.position.x, _ball.position.y, transform.position.z);
                transform.position = Vector3.Lerp(transform.position,
                    new Vector3(transform.position.x, _ball.position.y, transform.position.z), Time.deltaTime * 10);
                break;
            case GameMode.GameplayState.Resetting:
            case GameMode.GameplayState.Ending:
                transform.position = Vector3.Lerp(transform.position, _startPosition, 
                    Time.deltaTime * GameMode.ReturnSpeed * 0.3f);
                break;
            case GameMode.GameplayState.Reset:
                break;
        }
    }

    public bool Ready
    {
        get { return Vector3.Distance(transform.position, _startPosition) < 1; }
    }
}
