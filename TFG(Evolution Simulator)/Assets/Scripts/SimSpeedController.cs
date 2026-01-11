using UnityEngine;

//This script is used to control the speed of the simulation put it on an empty game object in the scene to use it
//It has some issues but it works for the most part
public class SimSpeedController : MonoBehaviour
{
    float fpsAverage = 0f;
    private float timeSum = 0f;
    public bool autoAdjust = true;
    public float gameSpeed = 1f;
    [SerializeField] private float targetFPS = 60f;
    [SerializeField] private float minGameSpeed = 0.1f;
    [SerializeField] private float maxGameSpeed = 100f;



    void Update()
    {
        //if the space bar is pressed, change autoAdjust to the opposite of what it currently is
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleAutoAdjust();
        }

        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus))
        {
            IncreaseGameSpeed();
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            DecreaseGameSpeed();
        }

        UpdateFPSCalculation();

        if (timeSum > 1f)
        {
            timeSum = 0f;

            if (autoAdjust)
            {
                AdjustGameSpeed();
            }
            else
            {
                ApplyGameSpeed();
            }
        }
    }

    private void UpdateFPSCalculation()
    {
        float currentFPS = 1.0f / Time.unscaledDeltaTime;
        fpsAverage = (currentFPS + fpsAverage * 2f) / 3f;
        timeSum += Time.deltaTime;
    }

    void AdjustGameSpeed()
    {
        if (fpsAverage < targetFPS * 0.9f)
        {
            gameSpeed *= 0.9f;
        }
        else if (fpsAverage > targetFPS * 1.1f && gameSpeed < maxGameSpeed)
        {
            gameSpeed *= 1.1f;
        }

        ApplyGameSpeed();
    }

    private void ApplyGameSpeed()
    {
        gameSpeed = Mathf.Clamp(gameSpeed, minGameSpeed, maxGameSpeed);
        Time.timeScale = gameSpeed;
    }

    public void ToggleAutoAdjust()
    {
        autoAdjust = !autoAdjust;
        Debug.Log($"Auto-adjust: {autoAdjust}");
    }

    public void IncreaseGameSpeed()
    {
        gameSpeed = Mathf.Min(gameSpeed * 1.5f, maxGameSpeed);
        if (!autoAdjust) ApplyGameSpeed();
    }

    public void DecreaseGameSpeed()
    {
        gameSpeed = Mathf.Max(gameSpeed * 0.5f, minGameSpeed);
        if (!autoAdjust) ApplyGameSpeed();
    }

    public void SetGameSpeed(float speed)
    {
        gameSpeed = Mathf.Clamp(speed, minGameSpeed, maxGameSpeed);
        if (!autoAdjust) ApplyGameSpeed();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 200, 100));
        GUILayout.Label($"FPS: {fpsAverage:F1}");
        GUILayout.Label($"Game Speed: {gameSpeed:F1}x");
        GUILayout.Label($"Auto-Adjust: {autoAdjust}");
        GUILayout.EndArea();
    }
}
