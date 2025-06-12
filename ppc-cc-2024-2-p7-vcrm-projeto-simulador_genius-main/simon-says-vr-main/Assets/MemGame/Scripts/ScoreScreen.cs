using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreScreen : MonoBehaviour
{

    public TextMeshProUGUI scoreText;

    void Start()
    {
        scoreText.text = "Pressione o Botão Preto";
    }

    public void UpdateScore(int score)
    {
        scoreText.text = "Pontos: " + score;
    }


}