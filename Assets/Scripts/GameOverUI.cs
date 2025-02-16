using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;
    [SerializeField] private Color tieColor;
    [SerializeField] private Button rematchButton;

    private void Awake()
    {
        rematchButton.onClick.AddListener(() =>
        {
            GameManager.Instance.RematchRpc();
        });
    }

    private void Start()
    {
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnGameRematch += GameManager_OnGameRematch;
        GameManager.Instance.OnGameTied += GameManager_OnGameTied;

        Hide();
    }

    private void GameManager_OnGameTied(object sender, System.EventArgs e)
    {
        text.text = "TIE!";
        text.color = tieColor;

        Show();
    }

    private void GameManager_OnGameRematch(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if(e.winPlayerType == GameManager.Instance.GetLocalPlayerType())
        {
            text.text = "YOU WIN!";
            text.color = winColor;
        }
        else
        {
            text.text = "YOU LOSE!";
            text.color = loseColor;
        }

        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
