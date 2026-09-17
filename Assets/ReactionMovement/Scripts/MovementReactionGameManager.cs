using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementReactionGameManager : MonoBehaviour
{
    // =====================================================
    // TARGETS
    // =====================================================

    [Header("Targets")]
    [SerializeField] private MovementReactionTarget leftTarget;
    [SerializeField] private MovementReactionTarget rightTarget;


    // =====================================================
    // JUGADOR Y CENTRO
    // =====================================================

    [Header("Jugador y zona central")]
    [SerializeField] private Transform playerHead;
    [SerializeField] private Transform centralZone;

    [Tooltip("Radio horizontal para considerar al jugador dentro del centro.")]
    [SerializeField] private float centerRadius = 0.50f;


    // =====================================================
    // CONFIGURACIÓN
    // =====================================================

    [Header("Configuración del juego")]
    [SerializeField] private float gameDuration = 60f;

    [Tooltip("Tiempo después de estar en el centro antes de encender la siguiente luz.")]
    [SerializeField] private float delayAfterReturning = 1f;


    // =====================================================
    // BOTONES
    // =====================================================

    [Header("Botones")]
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject exitButton;

    [Tooltip("Tiempo para mostrar INICIAR después de cargar la escena.")]
    [SerializeField] private float startButtonDelay = 33f;


    // =====================================================
    // AUDIO
    // =====================================================

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip startSound;
    [SerializeField] private AudioClip targetOnSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip centerConfirmSound;
    [SerializeField] private AudioClip finishSound;


    // =====================================================
    // ESTADO
    // =====================================================

    [Header("Estado")]
    [SerializeField] private bool gameRunning = false;
    [SerializeField] private bool gameFinished = false;

    [SerializeField] private bool waitingForCenter = false;
    [SerializeField] private bool targetActive = false;

    [SerializeField] private float timeRemaining = 0f;
    [SerializeField] private int score = 0;


    // =====================================================
    // ESTADÍSTICAS
    // =====================================================

    [Header("Estadísticas")]
    [SerializeField] private float averageReactionTime = 0f;
    [SerializeField] private float averageReturnTime = 0f;

    [SerializeField] private float lastReactionTime = 0f;
    [SerializeField] private float lastReturnTime = 0f;

    [SerializeField] private int leftHits = 0;
    [SerializeField] private int rightHits = 0;


    // =====================================================
    // VARIABLES INTERNAS
    // =====================================================

    private MovementReactionTarget currentTarget;

    private float targetActivationTime;
    private float returnStartTime;

    // Indica si estamos esperando el centro
    // porque se acaba de tocar una luz.
    private bool measuringReturn = false;

    // Evita lanzar varias corrutinas al mismo tiempo.
    private bool nextTargetScheduled = false;

    private Coroutine nextTargetCoroutine;
    private Coroutine startButtonCoroutine;

    private readonly List<float> reactionTimes =
        new List<float>();

    private readonly List<float> returnTimes =
        new List<float>();


    // =====================================================
    // PROPIEDADES PÚBLICAS
    // =====================================================

    public bool GameRunning => gameRunning;
    public bool GameFinished => gameFinished;

    public float TimeRemaining => timeRemaining;
    public float GameDuration => gameDuration;

    public int Score => score;

    public float AverageReactionTime => averageReactionTime;
    public float AverageReturnTime => averageReturnTime;

    public float LastReactionTime => lastReactionTime;
    public float LastReturnTime => lastReturnTime;

    public int LeftHits => leftHits;
    public int RightHits => rightHits;


    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        if (leftTarget != null)
        {
            leftTarget.Setup(this);
            leftTarget.Deactivate();
        }

        if (rightTarget != null)
        {
            rightTarget.Setup(this);
            rightTarget.Deactivate();
        }


        gameRunning = false;
        gameFinished = false;

        waitingForCenter = false;
        targetActive = false;

        timeRemaining = 0f;
        score = 0;


        // INICIAR oculto durante los primeros 33 segundos.
        if (startButton != null)
        {
            startButton.SetActive(false);

            startButtonCoroutine =
                StartCoroutine(ShowStartButtonAfterDelay());
        }


        // SALIR disponible.
        if (exitButton != null)
        {
            exitButton.SetActive(true);
        }
    }


    // =====================================================
    // MOSTRAR INICIAR
    // =====================================================

    private IEnumerator ShowStartButtonAfterDelay()
    {
        yield return new WaitForSeconds(startButtonDelay);

        if (!gameRunning &&
            !gameFinished &&
            startButton != null)
        {
            startButton.SetActive(true);
        }

        startButtonCoroutine = null;
    }


    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        if (!gameRunning)
            return;


        // =========================
        // CRONÓMETRO
        // =========================

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;

            EndGame();

            return;
        }


        // =========================
        // ESPERANDO CENTRO
        // =========================

        if (waitingForCenter)
        {
            CheckCenter();
        }
    }


    // =====================================================
    // INICIAR
    // =====================================================

    public void StartGame()
    {
        if (gameRunning)
            return;


        // Detener temporizador inicial por seguridad.
        if (startButtonCoroutine != null)
        {
            StopCoroutine(startButtonCoroutine);
            startButtonCoroutine = null;
        }


        // Detener cualquier corrutina vieja.
        if (nextTargetCoroutine != null)
        {
            StopCoroutine(nextTargetCoroutine);
            nextTargetCoroutine = null;
        }


        gameFinished = false;


        // =========================
        // BOTONES
        // =========================

        if (startButton != null)
        {
            startButton.SetActive(false);
        }

        if (exitButton != null)
        {
            exitButton.SetActive(false);
        }


        // =========================
        // AUDIO
        // =========================

        if (audioSource != null && startSound != null)
        {
            audioSource.PlayOneShot(startSound);
        }


        // =========================
        // REINICIAR ESTADÍSTICAS
        // =========================

        score = 0;

        leftHits = 0;
        rightHits = 0;

        averageReactionTime = 0f;
        averageReturnTime = 0f;

        lastReactionTime = 0f;
        lastReturnTime = 0f;

        reactionTimes.Clear();
        returnTimes.Clear();


        // =========================
        // ESTADO
        // =========================

        timeRemaining = gameDuration;

        currentTarget = null;

        waitingForCenter = false;
        targetActive = false;

        measuringReturn = false;
        nextTargetScheduled = false;


        if (leftTarget != null)
        {
            leftTarget.Deactivate();
        }

        if (rightTarget != null)
        {
            rightTarget.Deactivate();
        }


        gameRunning = true;


        Debug.Log("REACCIÓN EN MOVIMIENTO INICIADA");


        // =========================
        // PRIMER TARGET
        // =========================

        // Si ya está en el centro:
        // programamos la primera luz.
        //
        // Si no está:
        // esperamos hasta que entre.
        if (IsPlayerInCenter())
        {
            ScheduleNextTarget();
        }
        else
        {
            waitingForCenter = true;
            measuringReturn = false;

            Debug.Log(
                "Esperando que el jugador entre al centro..."
            );
        }
    }


    // =====================================================
    // COMPROBAR CENTRO
    // =====================================================

    private void CheckCenter()
    {
        if (!IsPlayerInCenter())
            return;


        // Ya entró correctamente al centro.
        waitingForCenter = false;


        // =========================
        // SI VENÍA DE TOCAR UN TARGET
        // =========================

        if (measuringReturn)
        {
            lastReturnTime =
                Time.time - returnStartTime;


            returnTimes.Add(lastReturnTime);

            CalculateAverageReturnTime();


            if (
                audioSource != null &&
                centerConfirmSound != null
            )
            {
                audioSource.PlayOneShot(
                    centerConfirmSound
                );
            }


            Debug.Log(
                "Regreso al centro: " +
                lastReturnTime.ToString("F3") +
                " segundos"
            );


            measuringReturn = false;
        }
        else
        {
            Debug.Log(
                "Jugador detectado en el centro."
            );
        }


        // Programar siguiente target.
        ScheduleNextTarget();
    }


    // =====================================================
    // PROGRAMAR SIGUIENTE TARGET
    // =====================================================

    private void ScheduleNextTarget()
    {
        if (!gameRunning)
            return;


        // No permitir dos corrutinas simultáneas.
        if (nextTargetScheduled)
            return;


        // No encender otro target si ya hay uno activo.
        if (targetActive)
            return;


        nextTargetScheduled = true;


        nextTargetCoroutine =
            StartCoroutine(
                ActivateNextTargetAfterDelay()
            );
    }


    // =====================================================
    // ESPERAR Y ENCENDER
    // =====================================================

    private IEnumerator ActivateNextTargetAfterDelay()
    {
        yield return new WaitForSeconds(
            delayAfterReturning
        );


        nextTargetScheduled = false;
        nextTargetCoroutine = null;


        if (!gameRunning)
            yield break;


        // IMPORTANTE:
        // El jugador puede haberse movido durante
        // ese segundo.
        //
        // Si ya no está en el centro, no bloqueamos
        // el juego. Simplemente esperamos que vuelva.
        if (!IsPlayerInCenter())
        {
            waitingForCenter = true;
            measuringReturn = false;

            Debug.Log(
                "Jugador salió del centro antes del target. Esperando regreso..."
            );

            yield break;
        }


        ActivateRandomTarget();
    }


    // =====================================================
    // TARGET ALEATORIO
    // =====================================================

    private void ActivateRandomTarget()
    {
        if (!gameRunning)
            return;


        if (targetActive)
            return;


        int randomSide = Random.Range(0, 2);


        if (randomSide == 0)
        {
            currentTarget = leftTarget;
        }
        else
        {
            currentTarget = rightTarget;
        }


        // Seguridad en caso de referencia perdida.
        if (currentTarget == null)
        {
            Debug.LogWarning(
                "No se encontró el target seleccionado. Se intentará nuevamente."
            );

            currentTarget = null;
            targetActive = false;

            ScheduleNextTarget();

            return;
        }


        currentTarget.Activate();

        targetActive = true;


        // Sonido target encendido.
        if (
            audioSource != null &&
            targetOnSound != null
        )
        {
            audioSource.PlayOneShot(
                targetOnSound
            );
        }


        // Iniciar tiempo de reacción.
        targetActivationTime = Time.time;


        Debug.Log(
            randomSide == 0
            ? "TARGET IZQUIERDO ENCENDIDO"
            : "TARGET DERECHO ENCENDIDO"
        );
    }


    // =====================================================
    // TARGET TOCADO
    // =====================================================

    public void TargetTouched(
        MovementReactionTarget touchedTarget
    )
    {
        if (!gameRunning)
            return;


        if (!targetActive)
            return;


        if (touchedTarget == null)
            return;


        if (touchedTarget != currentTarget)
            return;


        // =========================
        // REACCIÓN
        // =========================

        lastReactionTime =
            Time.time - targetActivationTime;


        reactionTimes.Add(lastReactionTime);

        CalculateAverageReactionTime();


        // =========================
        // ACIERTO
        // =========================

        score++;


        if (
            touchedTarget.Side ==
            MovementReactionTarget.TargetSide.Left
        )
        {
            leftHits++;
        }
        else
        {
            rightHits++;
        }


        // =========================
        // AUDIO
        // =========================

        if (
            audioSource != null &&
            hitSound != null
        )
        {
            audioSource.PlayOneShot(hitSound);
        }


        // =========================
        // APAGAR
        // =========================

        touchedTarget.Deactivate();

        currentTarget = null;

        targetActive = false;


        // =========================
        // REGRESO
        // =========================

        returnStartTime = Time.time;

        measuringReturn = true;

        waitingForCenter = true;


        Debug.Log(
            "Acierto. Reacción: " +
            lastReactionTime.ToString("F3") +
            " segundos. Esperando regreso..."
        );


        // CASO DE SEGURIDAD:
        //
        // Si por alguna razón el target está tan cerca
        // que al tocarlo el headset ya está dentro
        // del radio central, registramos inmediatamente.
        if (IsPlayerInCenter())
        {
            CheckCenter();
        }
    }


    // =====================================================
    // ¿ESTÁ EN EL CENTRO?
    // =====================================================

    private bool IsPlayerInCenter()
    {
        if (playerHead == null)
        {
            Debug.LogWarning(
                "Player Head no está asignado."
            );

            return false;
        }


        if (centralZone == null)
        {
            Debug.LogWarning(
                "Central Zone no está asignado."
            );

            return false;
        }


        Vector3 playerPosition =
            playerHead.position;


        Vector3 centerPosition =
            centralZone.position;


        // Solo X/Z.
        playerPosition.y = 0f;
        centerPosition.y = 0f;


        float distance =
            Vector3.Distance(
                playerPosition,
                centerPosition
            );


        return distance <= centerRadius;
    }


    // =====================================================
    // PROMEDIO REACCIÓN
    // =====================================================

    private void CalculateAverageReactionTime()
    {
        if (reactionTimes.Count == 0)
        {
            averageReactionTime = 0f;
            return;
        }


        float total = 0f;


        foreach (float value in reactionTimes)
        {
            total += value;
        }


        averageReactionTime =
            total / reactionTimes.Count;
    }


    // =====================================================
    // PROMEDIO REGRESO
    // =====================================================

    private void CalculateAverageReturnTime()
    {
        if (returnTimes.Count == 0)
        {
            averageReturnTime = 0f;
            return;
        }


        float total = 0f;


        foreach (float value in returnTimes)
        {
            total += value;
        }


        averageReturnTime =
            total / returnTimes.Count;
    }


    // =====================================================
    // FINALIZAR
    // =====================================================

    private void EndGame()
    {
        if (!gameRunning)
            return;


        gameRunning = false;
        gameFinished = true;

        waitingForCenter = false;
        measuringReturn = false;

        targetActive = false;
        nextTargetScheduled = false;


        // =========================
        // DETENER CORRUTINA
        // =========================

        if (nextTargetCoroutine != null)
        {
            StopCoroutine(nextTargetCoroutine);

            nextTargetCoroutine = null;
        }


        // =========================
        // APAGAR TARGETS
        // =========================

        if (leftTarget != null)
        {
            leftTarget.Deactivate();
        }

        if (rightTarget != null)
        {
            rightTarget.Deactivate();
        }


        currentTarget = null;


        // =========================
        // SONIDO FINAL
        // =========================

        if (
            audioSource != null &&
            finishSound != null
        )
        {
            audioSource.PlayOneShot(
                finishSound
            );
        }


        // =========================
        // BOTONES
        // =========================

        if (startButton != null)
        {
            startButton.SetActive(true);
        }

        if (exitButton != null)
        {
            exitButton.SetActive(true);
        }


        // =========================
        // RESULTADOS
        // =========================

        Debug.Log(
            "REACCIÓN EN MOVIMIENTO FINALIZADA"
        );

        Debug.Log(
            "Aciertos: " + score
        );

        Debug.Log(
            "Izquierda: " + leftHits +
            " | Derecha: " + rightHits
        );

        Debug.Log(
            "Promedio reacción: " +
            averageReactionTime.ToString("F3") +
            " segundos"
        );

        Debug.Log(
            "Promedio regreso: " +
            averageReturnTime.ToString("F3") +
            " segundos"
        );
    }
}