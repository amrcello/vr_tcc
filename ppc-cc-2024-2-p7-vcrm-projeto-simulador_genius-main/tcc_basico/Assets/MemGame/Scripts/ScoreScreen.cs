using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreScreen : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI responseTimeText;

    void Start()
    {
        scoreText.text = "Pressione o Botão Preto";
        timerText.text = "";
        responseTimeText.text = "";
    }

    public void UpdateScore(int score)
    {
        scoreText.text = "Pontos: " + score;
    }

    public void UpdateTimer(float time)
    {
        timerText.text = "Tempo de teste: " + time.ToString("F2") + "s";
    }

    public void UpdateResponseTime(float time)
    {
        responseTimeText.text = "Tempo de resposta: " + time.ToString("F2") + "s";
    }

    public void ShowFinalResults(int score, float totalTime, float responseTime)
    {
        scoreText.text = "Pontuação Final: " + score;
        timerText.text = "Tempo Total: " + totalTime.ToString("F2") + "s";
        responseTimeText.text = "Tempo de Resposta Total: " + responseTime.ToString("F2") + "s";
    }
}