using UnityEngine;
using TMPro;

public class EnglishRunnerUI : MonoBehaviour
{
    [Header("HUD")]
    public TMP_Text scoreText;
    public TMP_Text bestScoreText;

    [Header("Word")]
    public GameObject wordPanel;
    public TMP_Text wordText;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public TMP_Text finalBestScoreText;

    [Header("Pause")]
    public GameObject pausePanel;

    private void Start()
    {
        if (wordPanel != null) wordPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    public void UpdateBestScore(int bestScore)
    {
        if (bestScoreText != null)
        {
            bestScoreText.text = "Best: " + bestScore;
        }
    }

    public void ShowWord(string word)
    {
        if (wordPanel != null)
        {
            wordPanel.SetActive(true);

            if (wordText != null)
            {
                wordText.text = word;
            }

            CancelInvoke(nameof(HideWord));
            Invoke(nameof(HideWord), 2f);
        }
    }

    private void HideWord()
    {
        if (wordPanel != null)
        {
            wordPanel.SetActive(false);
        }
    }

    public void ShowGameOver(int score, int bestScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (finalScoreText != null) finalScoreText.text = "Score\n" + score;
            if (finalBestScoreText != null) finalBestScoreText.text = "Best Score\n" + bestScore;
        }
    }

    public void ShowPause()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    public void HidePause()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }
}