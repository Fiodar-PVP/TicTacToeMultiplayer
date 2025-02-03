using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one GameManager!");
        }

        Instance = this;
    }

    public void ClickedOnGridPosition(int x, int y)
    {
        Debug.Log("Clicked on Grid Position: " + x + ", " + y);
    }
}
