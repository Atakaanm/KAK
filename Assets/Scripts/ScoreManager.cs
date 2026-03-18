using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public float scorePerSecond = 10f;

    private float currentScore = 0f;

    public int ScoreInt => Mathf.FloorToInt(currentScore);

    void Update()
    {
        currentScore += scorePerSecond * Time.deltaTime;

        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + ScoreInt.ToString();
        }
    }
}