using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStates : int
{
    Idle = 1,
    PlayingSequence = 2,
    ArrangeItem = 3,
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
    public AudioClip errorSound;

    public List<Transform> spawnPoints;

    public int currentScore = 0;
    public List<ItemCode> trueSequence = new List<ItemCode>();

    public ScoreScreen scoreScreen;

    private AudioSource audioSource;
    private int currentResponseIndex = 0;

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
    }

    void Update()
    {
        if (currentState != GameStates.Idle)
        {
            // Atualiza o cronômetro do teste
            currentTestTime = Time.time - testStartTime;
            scoreScreen.UpdateTimer(currentTestTime);
        }
    }

    public void OnButtonPressed()
    {
        if (currentState == GameStates.Idle)
        {
            // Inicia o cronômetro do teste
            testStartTime = Time.time;
            currentTestTime = 0f;
            totalResponseTime = 0f;
            scoreScreen.UpdateTimer(0f);
            scoreScreen.UpdateResponseTime(0f);

            scoreScreen.UpdateScore(currentScore);
            currentState = GameStates.PlayingSequence;
            StartCoroutine(PlaySequenceInteractive());
        }
    }

    public void OnEmergencyButtonPressed()
    {
        if (currentState == GameStates.ArrangeItem)
        {
            currentScore = Mathf.Max(0, currentScore - 1);
            currentState = GameStates.Idle;
            OnButtonPressed();
        }
    }

    IEnumerator PlaySequenceInteractive()
    {
        GenerateRandomSequence();

        foreach (Table table in tables)
        {
            table.SetColor(GetColorByItemCode(ItemCode.Black));
        }

        MoveItemsToSpawnPosition();
        yield return new WaitForSeconds(0.5f);

        currentResponseIndex = 0;
        currentState = GameStates.ArrangeItem;

        ShowNextStep(); // Mostra a primeira cor
    }

    void ShowNextStep()
    {
        if (currentResponseIndex >= trueSequence.Count)
        {
            // Teste terminado, mostra resultados finais
            scoreScreen.ShowFinalResults(currentScore, currentTestTime, totalResponseTime);
            currentState = GameStates.Idle;
            return;
        }

        ItemCode nextCode = trueSequence[currentResponseIndex];
        tables[currentResponseIndex].SetCode(nextCode);
        tables[currentResponseIndex].SetColor(GetColorByItemCode(nextCode));
        audioSource.clip = GetToneByCode(nextCode);
        audioSource.Play();

        // Registra o momento em que a cor foi mostrada
        lastColorShowTime = Time.time;
    }

    void MoveItemsToSpawnPosition()
    {
        List<Transform> positions = new List<Transform>(spawnPoints);

        for (int i = 0; i < items.Count; i++)
        {
            int idx = Random.Range(0, positions.Count);
            Vector3 randomPosition = positions[idx].position;
            positions.RemoveAt(idx);
            items[i].transform.localPosition = randomPosition;
            items[i].transform.localRotation = Quaternion.identity;
            items[i].GetComponent<Renderer>().enabled = false;
        }
    }

    public void OnRedButtonPressed() => OnColorButtonPressed(ItemCode.Red);
    public void OnGreenButtonPressed() => OnColorButtonPressed(ItemCode.Green);
    public void OnBlueButtonPressed() => OnColorButtonPressed(ItemCode.Blue);
    public void OnYellowButtonPressed() => OnColorButtonPressed(ItemCode.Yellow);

    public void OnColorButtonPressed(ItemCode selectedCode)
    {
        if (currentState != GameStates.ArrangeItem || currentResponseIndex >= trueSequence.Count)
            return;

        audioSource.Stop();

        ItemCode expectedCode = trueSequence[currentResponseIndex];

        if (selectedCode == expectedCode)
        {
            // Calcula o tempo de resposta e acumula
            audioSource.clip = successSound;
            float responseTime = Time.time - lastColorShowTime;
            totalResponseTime += responseTime;
            scoreScreen.UpdateResponseTime(totalResponseTime);

            currentScore++;
            scoreScreen.UpdateScore(currentScore);

            audioSource.clip = successSound;
            audioSource.Play();

            currentResponseIndex++;
            Invoke(nameof(ShowNextStep), 1f);
        }
        else
        {
            audioSource.clip = errorSound;
            audioSource.Play();

            currentScore = Mathf.Max(0, currentScore - 1);
            scoreScreen.UpdateScore(currentScore);
        }
    }

    public Material GetColorByItemCode(ItemCode code) => colors[(int)code];
    public AudioClip GetToneByCode(ItemCode code) => tones[(int)code];

    void GenerateRandomSequence()
    {
        trueSequence.Clear();

        List<ItemCode> sequence = new List<ItemCode>
        {
            ItemCode.Red,
            ItemCode.Green,
            ItemCode.Blue,
            ItemCode.Yellow
        };

        while (sequence.Count > 0)
        {
            int idx = Random.Range(0, sequence.Count);
            trueSequence.Add(sequence[idx]);
            sequence.RemoveAt(idx);
        }
    }

    public void CheckSequence() { }
}