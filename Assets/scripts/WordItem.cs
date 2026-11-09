using UnityEngine;

public class WordItem : MonoBehaviour
{
    [Header("Word")]
    public string englishWord = "banana";

    [Header("Score")]
    public int scoreValue = 10;

    private bool collected = false;

    public void Collect()
    {
        if (collected)
            return;

        collected = true;

        EnglishRunnerGame.Instance.AddScore(scoreValue);

        EnglishRunnerGame.Instance.ShowWord(englishWord);

        Destroy(gameObject);
    }
}