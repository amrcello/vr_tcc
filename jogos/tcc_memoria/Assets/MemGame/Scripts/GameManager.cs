using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStates : int
{
    Idle = 1,
    PlayingSequence = 2,
    AwaitingPlayerInput = 3,
    GameOver = 4
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameStates currentState;

    public List<Table> tables;
    public List<GameObject> items;
    public List<Material> colors;
    public List<AudioClip> tones;
    public AudioClip successSound;

    public List<Transform> spawnPoints;
    public ScoreScreen scoreScreen;

    public int currentScore = 0;
    public int errorCount = 0; // Contador de erros

    private List<ItemCode> trueSequence = new List<ItemCode>();
    private List<ItemCode> playerInput = new List<ItemCode>();
    private AudioSource audioSource;

    private int inputIndex = 0;
    private int currentStep = 1;
    private const int maxSteps = 4;

    // Variáveis para os cronômetros
    private float testStartTime;
    private float currentTestTime;
    private float totalResponseTime;
    private float lastColorShowTime;

    void Start()
    {
        currentState = GameStates.Idle;
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        totalResponseTime = 0f;
        errorCount = 0;
    }

    void Update()
    {
        if (currentState != GameStates.Idle && currentState != GameStates.GameOver)
        {
            // Atualiza o cronômetro do teste
            currentTestTime = Time.time - testStartTime;
            scoreScreen.UpdateTimer(currentTestTime);
        }
    }

    public void OnButtonPressed()
    {
        if (currentState == GameStates.Idle || currentState == GameStates.GameOver)
        {
            // Inicia os cronômetros e contadores
            testStartTime = Time.time;
            currentTestTime = 0f;
            totalResponseTime = 0f;
            errorCount = 0;

            scoreScreen.UpdateTimer(0f);
            scoreScreen.UpdateResponseTime(0f);
            scoreScreen.UpdateErrorCount(0);

            currentScore = 0;
            currentStep = 1;
            trueSequence.Clear();
            playerInput.Clear();
            inputIndex = 0;

            // Reseta visualmente todas as mesas
            foreach (Table table in tables)
            {
                table.SetColor(colors[(int)ItemCode.Black]);
            }

            scoreScreen.UpdateScore(currentScore);
            currentState = GameStates.PlayingSequence;
            AddToSequence();
            StartCoroutine(PlaySequence());
        }
    }

    public void OnEmergencyButtonPressed()
    {
        if (currentState == GameStates.AwaitingPlayerInput)
        {
            errorCount++;
            currentScore = Mathf.Max(0, currentScore - 1);
            scoreScreen.UpdateScore(currentScore);
            scoreScreen.UpdateErrorCount(errorCount);
            StartCoroutine(PlaySequence()); // repete a mesma sequência
        }
    }

    IEnumerator PlaySequence()
    {
        currentState = GameStates.PlayingSequence;
        playerInput.Clear();
        inputIndex = 0;

        // Garante tamanho mínimo da sequência
        while (trueSequence.Count < currentStep)
        {
            AddToSequence();
        }

        yield return new WaitForSeconds(1f);

        // Apaga todas as mesas
        foreach (Table table in tables)
        {
            table.SetColor(colors[(int)ItemCode.Black]);
        }

        yield return new WaitForSeconds(0.5f);

        // Reproduz a sequência
        for (int i = 0; i < currentStep; i++)
        {
            ItemCode code = trueSequence[i];
            tables[i % tables.Count].SetCode(code);
            tables[i % tables.Count].SetColor(colors[(int)code]);
            audioSource.clip = tones[(int)code];
            audioSource.Play();

            // Registra o momento em que a cor foi mostrada (para o tempo de resposta)
            lastColorShowTime = Time.time;

            yield return new WaitForSeconds(1f);

            // Só apaga se o passo ainda não foi atingido corretamente
            if (i >= playerInput.Count || playerInput[i] != code)
            {
                tables[i % tables.Count].SetColor(colors[(int)ItemCode.Black]);
            }

            yield return new WaitForSeconds(0.3f);
        }

        currentState = GameStates.AwaitingPlayerInput;
    }

    void AddToSequence()
    {
        ItemCode randomCode = (ItemCode)Random.Range(0, 4); // 0 to 3 (Red, Green, Blue, Yellow)
        trueSequence.Add(randomCode);
    }

    public void OnRedButtonPressed() => OnColorButtonPressed(ItemCode.Red);
    public void OnGreenButtonPressed() => OnColorButtonPressed(ItemCode.Green);
    public void OnBlueButtonPressed() => OnColorButtonPressed(ItemCode.Blue);
    public void OnYellowButtonPressed() => OnColorButtonPressed(ItemCode.Yellow);

    public void OnColorButtonPressed(ItemCode selectedCode)
    {
        if (currentState != GameStates.AwaitingPlayerInput || inputIndex >= currentStep)
            return;

        ItemCode expectedCode = trueSequence[inputIndex];

        if (selectedCode == expectedCode)
        {
            // Calcula e acumula o tempo de resposta
            float responseTime = Time.time - lastColorShowTime;
            totalResponseTime += responseTime;
            scoreScreen.UpdateResponseTime(totalResponseTime);

            playerInput.Add(selectedCode);

            audioSource.clip = successSound;
            audioSource.Play();

            // Deixa a mesa acesa com a cor correta
            tables[inputIndex % tables.Count].SetColor(colors[(int)selectedCode]);

            inputIndex++;

            if (inputIndex >= currentStep)
            {
                currentScore++;
                scoreScreen.UpdateScore(currentScore);

                if (currentStep >= maxSteps)
                {
                    currentState = GameStates.GameOver;
                    // Mostra todos os resultados finais
                    scoreScreen.ShowFinalResults(currentScore, currentTestTime, totalResponseTime, errorCount);
                }
                else
                {
                    currentStep++; // Avança para próxima rodada
                    currentState = GameStates.Idle;
                    StartCoroutine(PlaySequence());
                }
            }
        }
        else
        {
            errorCount++;
            currentScore = Mathf.Max(0, currentScore - 1);
            scoreScreen.UpdateScore(currentScore);
            scoreScreen.UpdateErrorCount(errorCount);
            currentState = GameStates.Idle;
            StartCoroutine(PlaySequence()); // repete a mesma rodada
        }
    }

    public Material GetColorByItemCode(ItemCode code) => colors[(int)code];
    public AudioClip GetToneByCode(ItemCode code) => tones[(int)code];

    public void CheckSequence() { } // compatibilidade com Table.cs
}