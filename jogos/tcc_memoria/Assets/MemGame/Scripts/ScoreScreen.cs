using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreScreen : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI responseTimeText;
    public TextMeshProUGUI errorCountText;

    void Start()
    {
        scoreText.text = "Pressione o Botão Preto";
        timerText.text = "";
        responseTimeText.text = "";
        errorCountText.text = "";
    }

    public void UpdateScore(int score)
    {
        scoreText.text = "Pontos: " + score;
    }

    public void UpdateTimer(float time)
    {
        timerText.text = "Tempo: " + time.ToString("F2") + "s";
    }

    public void UpdateResponseTime(float time)
    {
        responseTimeText.text = "Tempo Resp.: " + time.ToString("F2") + "s";
    }

    public void UpdateErrorCount(int errors)
    {
        errorCountText.text = "Erros: " + errors;
    }

    public void ShowFinalResults(int score, float totalTime, float responseTime, int errors)
    {
        scoreText.text = "Pontos: " + score;
        timerText.text = "Tempo Total: " + totalTime.ToString("F2") + "s";
        responseTimeText.text = "Tempo Resp. Total: " + responseTime.ToString("F2") + "s";
        errorCountText.text = "Total Erros: " + errors;
    }
}