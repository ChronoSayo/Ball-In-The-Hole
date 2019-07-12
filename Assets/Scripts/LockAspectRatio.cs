using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LockAspectRatio : MonoBehaviour {

    private float _targetAspectX;
    private float _targetAspectY;
    private Vector2 _storedAspect;

    //Restarts the game to fix the resolution.
    private static bool _Restarted = false;

    void Start()
    {
        Camera camera = GetComponent<Camera>();
        _targetAspectX = 9;
        _targetAspectY = 16;

        Application.targetFrameRate = 60;

        int width = 576;
        int height = 1024;

        Screen.SetResolution(width, height, Application.platform == RuntimePlatform.Android);

        if (_storedAspect.x != Screen.width || _storedAspect.y != Screen.height)
        {
            RatioFix(camera);
            _storedAspect = new Vector2(Screen.width, Screen.height);

        }

        if (!_Restarted)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            _Restarted = true;
        }
    }

    public void RatioFix(Camera camera)
    {
        float targetAspect = _targetAspectX / _targetAspectY;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            Rect rect = camera.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            camera.rect = rect;
        }
        else
        {
            float scalewidth = 1.0f / scaleHeight;

            Rect rect = camera.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            camera.rect = rect;


        }
    }
}
