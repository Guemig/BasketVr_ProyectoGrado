using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionGameManager : MonoBehaviour
{
    [Header("Configuración del juego")]
    [SerializeField] private List<ReactionTarget> targets;
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private float delayBetweenTargets = 1f;

    [Header("Botón de inicio")]
    [SerializeField] private GameObject startButton;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip startSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip finishSound;

    [Header("Estado del juego")]
    [SerializeField] private bool gameRunning = false;
    [SerializeField] private float timeRemaining = 0f;
    [SerializeField] private int score = 0;

    [Header("Estadísticas")]
    [SerializeField] private float averageReactionTime = 0f;
    [SerializeField] private float lastReactionTime = 0f;

    private ReactionTarget currentTarget;
    private ReactionTarget previousTarget;

    private Coroutine nextTargetCoroutine;

    
    private float targetActivationTime;

    
    private List<float> reactionTimes = new List<float>();


   
    public bool GameRunning => gameRunning;
    public float TimeRemaining => timeRemaining;
    public int Score => score;
    public float AverageReactionTime => averageReactionTime;
    public float LastReactionTime => lastReactionTime;


    private void Start()
    {
        // Configurar todos los targets
        foreach (ReactionTarget target in targets)
        {
            if (target != null)
            {
                target.Setup(this);
                target.Deactivate();
            }
        }

        // Estado inicial
        gameRunning = false;
        timeRemaining = 0f;
        score = 0;

        averageReactionTime = 0f;
        lastReactionTime = 0f;

        // Mostrar botón de inicio
        if (startButton != null)
        {
            startButton.SetActive(true);
        }
    }


    private void Update()
    {
        if (!gameRunning)
            return;

        // Restar tiempo
        timeRemaining -= Time.deltaTime;

        // Comprobar final de partida
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;

            EndGame();
        }
    }


    public void StartGame()
    {
        // Evitar iniciar dos partidas
        if (gameRunning)
            return;


        if (audioSource != null && startSound != null)
        {
            audioSource.PlayOneShot(startSound);
        }


        // =========================
        // OCULTAR BOTÓN
        // =========================

        if (startButton != null)
        {
            startButton.SetActive(false);
        }


        // =========================
        // REINICIAR ESTADÍSTICAS
        // =========================

        score = 0;

        timeRemaining = gameDuration;

        averageReactionTime = 0f;

        lastReactionTime = 0f;

        reactionTimes.Clear();


        currentTarget = null;

        previousTarget = null;


        // =========================
        // APAGAR TODOS LOS TARGETS
        // =========================

        foreach (ReactionTarget target in targets)
        {
            if (target != null)
            {
                target.Deactivate();
            }
        }


        // =========================
        // INICIAR PARTIDA
        // =========================

        gameRunning = true;

        Debug.Log("PARTIDA INICIADA");


        nextTargetCoroutine =
            StartCoroutine(ActivateNextTarget());
    }


    private IEnumerator ActivateNextTarget()
    {
        // Esperar antes de encender el siguiente target
        yield return new WaitForSeconds(delayBetweenTargets);


        if (!gameRunning)
            yield break;


        if (targets == null || targets.Count == 0)
            yield break;


        // =========================
        // ELEGIR TARGET ALEATORIO
        // =========================

        int randomIndex;

        do
        {
            randomIndex =
                Random.Range(0, targets.Count);
        }
        while (
            targets.Count > 1 &&
            targets[randomIndex] == previousTarget
        );


        currentTarget = targets[randomIndex];

        previousTarget = currentTarget;


        // =========================
        // ENCENDER TARGET
        // =========================

        if (currentTarget != null)
        {
            currentTarget.Activate();

            // Comenzar a medir reacción
            targetActivationTime = Time.time;
        }
    }


    public void TargetTouched(ReactionTarget touchedTarget)
    {
        if (!gameRunning)
            return;


        // Solo aceptar el target encendido
        if (touchedTarget != currentTarget)
            return;


        // =========================
        // SONIDO DE ACIERTO
        // =========================

        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }


        // =========================
        // CALCULAR REACCIÓN
        // =========================

        float reactionTime =
            Time.time - targetActivationTime;


        lastReactionTime = reactionTime;

        reactionTimes.Add(reactionTime);


        // =========================
        // CALCULAR PROMEDIO
        // =========================

        float total = 0f;

        foreach (float time in reactionTimes)
        {
            total += time;
        }


        averageReactionTime =
            total / reactionTimes.Count;


        // =========================
        // SUMAR ACIERTO
        // =========================

        score++;


        Debug.Log(
            "Reacción: " +
            reactionTime.ToString("F3") +
            " segundos"
        );

        Debug.Log(
            "Promedio: " +
            averageReactionTime.ToString("F3") +
            " segundos"
        );


        // =========================
        // APAGAR TARGET
        // =========================

        touchedTarget.Deactivate();

        currentTarget = null;


        // =========================
        // PREPARAR SIGUIENTE
        // =========================

        nextTargetCoroutine =
            StartCoroutine(ActivateNextTarget());
    }


    private void EndGame()
    {
        if (!gameRunning)
            return;


        // =========================
        // TERMINAR PARTIDA
        // =========================

        gameRunning = false;


        // =========================
        // DETENER CORRUTINA
        // =========================

        if (nextTargetCoroutine != null)
        {
            StopCoroutine(nextTargetCoroutine);

            nextTargetCoroutine = null;
        }


        // =========================
        // APAGAR TODOS LOS TARGETS
        // =========================

        foreach (ReactionTarget target in targets)
        {
            if (target != null)
            {
                target.Deactivate();
            }
        }


        currentTarget = null;


        // =========================
        // SONIDO DE FINALIZACIÓN
        // =========================

        if (audioSource != null && finishSound != null)
        {
            audioSource.PlayOneShot(finishSound);
        }


        // =========================
        // RESULTADOS
        // =========================

        Debug.Log("PARTIDA TERMINADA");

        Debug.Log(
            "Puntuación final: " + score
        );

        Debug.Log(
            "Promedio final: " +
            averageReactionTime.ToString("F3") +
            " segundos"
        );


        // =========================
        // MOSTRAR BOTÓN
        // =========================

        if (startButton != null)
        {
            startButton.SetActive(true);
        }
    }
}