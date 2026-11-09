using UnityEngine;
using UnityEngine.SceneManagement;

public class EnglishRunnerGame : MonoBehaviour
{
    public static EnglishRunnerGame Instance;

    [Header("Score")]
    public int score = 0;

    public int bestScore = 0;

    [Header("UI")]
    public EnglishRunnerUI ui;

    private bool gameOver = false;

    private void Awake()
    {
        Instance = this;

        bestScore = PlayerPrefs.GetInt("BestScore", 0);
    }

    private void Start()
    {
        Time.timeScale = 1f;

        ui.UpdateScore(score);
        ui.UpdateBestScore(bestScore);
    }

    public void AddScore(int amount)
    {
        if (gameOver)
            return;

        score += amount;

        if (score > bestScore)
        {
            bestScore = score;

            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
        }

        ui.UpdateScore(score);
        ui.UpdateBestScore(bestScore);
    }

    public void ShowWord(string word)
    {
        ui.ShowWord(word);
    }

    public void GameOver()
    {
        if (gameOver)
            return;

        gameOver = true;

        Time.timeScale = 0f;

        ui.ShowGameOver(score, bestScore);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;

        ui.ShowPause();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        ui.HidePause();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }
}