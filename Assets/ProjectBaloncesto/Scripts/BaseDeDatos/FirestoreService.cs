
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirestoreService : MonoBehaviour
{
    public static FirestoreService Instance { get; private set; }

    private FirebaseFirestore db;

    private const string STUDENTS_COLLECTION = "students";
    private const string MOVEMENT_EXERCISE_ID = "reaction-movement";
    private const string LIGHTS_EXERCISE_ID = "reaction-lights";
    private const string PASSES_EXERCISE_ID = "reaction-passes";

    // =========================================================
    // CURRENT STUDENT
    // =========================================================

    private string currentStudentCode;

    public string CurrentStudentCode => currentStudentCode;

    public bool HasCurrentStudent =>
        !string.IsNullOrEmpty(currentStudentCode);

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        db = FirebaseFirestore.DefaultInstance;
    }

    // =========================================================
    // CURRENT STUDENT
    // =========================================================

    public void SetCurrentStudent(string studentCode)
    {
        currentStudentCode = NormalizeStudentCode(studentCode);

        Debug.Log(
            "[FIRESTORE] Current student set: " +
            currentStudentCode
        );
    }

    public void ClearCurrentStudent()
    {
        currentStudentCode = null;

        Debug.Log("[FIRESTORE] Current student cleared.");
    }

    // =========================================================
    // STUDENTS
    // =========================================================

    public async Task<Dictionary<string, object>> GetStudent(
        string studentCode)
    {
        string normalizedCode = NormalizeStudentCode(studentCode);

        DocumentReference studentRef =
            db.Collection(STUDENTS_COLLECTION)
              .Document(normalizedCode);

        DocumentSnapshot snapshot =
            await studentRef.GetSnapshotAsync();

        if (!snapshot.Exists)
            return null;

        return snapshot.ToDictionary();
    }

    public async Task<bool> StudentCodeExists(string studentCode)
    {
        string normalizedCode = NormalizeStudentCode(studentCode);

        DocumentReference studentRef =
            db.Collection(STUDENTS_COLLECTION)
              .Document(normalizedCode);

        DocumentSnapshot snapshot =
            await studentRef.GetSnapshotAsync();

        return snapshot.Exists;
    }

    public async Task<bool> CreateStudentIfNotExists(
        string name,
        string studentCode)
    {
        string normalizedName = name?.Trim();
        string normalizedCode = NormalizeStudentCode(studentCode);

        if (string.IsNullOrEmpty(normalizedName))
        {
            throw new Exception(
                "El nombre del estudiante es obligatorio."
            );
        }

        DocumentReference studentRef =
            db.Collection(STUDENTS_COLLECTION)
              .Document(normalizedCode);

        DocumentSnapshot existingStudent =
            await studentRef.GetSnapshotAsync();

        if (existingStudent.Exists)
            return false;

        Dictionary<string, object> data =
            new Dictionary<string, object>
            {
                { "name", normalizedName },
                { "studentCode", normalizedCode },
                { "createdAt", Timestamp.GetCurrentTimestamp() }
            };

        await studentRef.SetAsync(data);

        Debug.Log(
            "[FIRESTORE] Student created: " + normalizedCode
        );

        return true;
    }

    public async Task<List<Dictionary<string, object>>> GetAllStudents()
    {
        QuerySnapshot snapshot =
            await db.Collection(STUDENTS_COLLECTION)
                    .GetSnapshotAsync();

        List<Dictionary<string, object>> students =
            new List<Dictionary<string, object>>();

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            Dictionary<string, object> student =
                document.ToDictionary();

            student["id"] = document.Id;
            students.Add(student);
        }

        return students;
    }

    // =========================================================
    // MOVEMENT SESSION
    // =========================================================

    public MovementSession CreateMovementSession()
    {
        return new MovementSession
        {
            exerciseId = MOVEMENT_EXERCISE_ID,
            levels = new List<MovementLevel>()
        };
    }

    public MovementSession AddLevelToSession(
        MovementSession session,
        MovementLevel levelData)
    {
        if (session == null ||
            session.exerciseId != MOVEMENT_EXERCISE_ID)
        {
            throw new Exception(
                "La sesión de reacción de movimiento no es válida."
            );
        }

        if (session.levels == null)
            session.levels = new List<MovementLevel>();

        if (session.levels.Count >= 3)
        {
            throw new Exception(
                "La sesión ya tiene los tres niveles registrados."
            );
        }

        if (levelData == null)
        {
            throw new Exception(
                "Los datos del nivel no pueden ser nulos."
            );
        }

        levelData.level = session.levels.Count + 1;

        ValidateLevel(levelData);

        session.levels.Add(levelData);

        return session;
    }

    // =========================================================
    // LIGHTS SESSION
    // =========================================================

    public LightsSession CreateLightsSession()
    {
        return new LightsSession
        {
            exerciseId = LIGHTS_EXERCISE_ID,
            levels = new List<LightsLevel>()
        };
    }

    public LightsSession AddLightsLevelToSession(
        LightsSession session,
        LightsLevel levelData)
    {
        if (session == null ||
            session.exerciseId != LIGHTS_EXERCISE_ID)
        {
            throw new Exception(
                "La sesión de reacción por luces no es válida."
            );
        }

        if (session.levels == null)
            session.levels = new List<LightsLevel>();

        if (session.levels.Count >= 3)
        {
            throw new Exception(
                "La sesión ya tiene los tres niveles registrados."
            );
        }

        if (levelData == null)
        {
            throw new Exception(
                "Los datos del nivel no pueden ser nulos."
            );
        }

        levelData.level = session.levels.Count + 1;

        if (levelData.correctAnswers < 0)
        {
            throw new Exception(
                "Los aciertos no pueden ser negativos."
            );
        }

        ValidateAccuracy(levelData.accuracy);
        ValidateReaction(levelData.averageReaction);

        session.levels.Add(levelData);

        return session;
    }

    // =========================================================
    // SAVE LIGHTS ATTEMPT
    // =========================================================

    public async Task<string> SaveLightsAttempt(
        string studentCode,
        LightsSession session)
    {
        string normalizedCode = NormalizeStudentCode(studentCode);

        if (session == null ||
            session.exerciseId != LIGHTS_EXERCISE_ID)
        {
            throw new Exception(
                "La sesión no corresponde a reaction-lights."
            );
        }

        if (session.levels == null || session.levels.Count != 3)
        {
            throw new Exception(
                "La sesión debe tener exactamente tres niveles."
            );
        }

        List<Dictionary<string, object>> levelsData =
            new List<Dictionary<string, object>>();

        foreach (LightsLevel level in session.levels)
        {
            if (level == null || level.level < 1 || level.level > 3)
            {
                throw new Exception(
                    "Se encontró un nivel inválido."
                );
            }

            levelsData.Add(
                new Dictionary<string, object>
                {
                    { "level", level.level },
                    { "correctAnswers", level.correctAnswers },
                    { "accuracy", level.accuracy },
                    { "averageReaction", level.averageReaction }
                }
            );
        }

        DocumentReference attemptRef =
            db.Collection(STUDENTS_COLLECTION)
              .Document(normalizedCode)
              .Collection("attempts")
              .Document();

        Dictionary<string, object> data =
            new Dictionary<string, object>
            {
                { "exerciseId", LIGHTS_EXERCISE_ID },
                { "completedAt", Timestamp.GetCurrentTimestamp() },
                { "levels", levelsData }
            };

        await attemptRef.SetAsync(data);

        Debug.Log(
            "[FIRESTORE] Intento de luces guardado. ID: " +
            attemptRef.Id
        );

        return attemptRef.Id;
    }

    // =========================================================
    // PASSES AND REACTION SESSION
    // =========================================================

    public PassesSession CreatePassesSession()
    {
        return new PassesSession
        {
            exerciseId = PASSES_EXERCISE_ID,
            levels = new List<PassesLevel>()
        };
    }

    public PassesSession AddPassesLevelToSession(
        PassesSession session,
        PassesLevel levelData)
    {
        if (session == null ||
            session.exerciseId != PASSES_EXERCISE_ID)
        {
            throw new Exception(
                "La sesión de Pases y Reacción no es válida."
            );
        }

        if (session.levels == null)
            session.levels = new List<PassesLevel>();

        if (session.levels.Count >= 3)
        {
            throw new Exception(
                "La sesión ya tiene los tres niveles registrados."
            );
        }

        if (levelData == null)
        {
            throw new Exception(
                "Los datos del nivel no pueden ser nulos."
            );
        }

        levelData.level = session.levels.Count + 1;

        ValidatePassesLevel(levelData);

        session.levels.Add(levelData);

        return session;
    }

    // =========================================================
    // SAVE PASSES ATTEMPT
    // =========================================================

    public async Task<string> SavePassesAttempt(
        string studentCode,
        PassesSession session)
    {
        string normalizedCode = NormalizeStudentCode(studentCode);

        if (session == null ||
            session.exerciseId != PASSES_EXERCISE_ID)
        {
            throw new Exception(
                "La sesión no corresponde a reaction-passes."
            );
        }

        if (session.levels == null || session.levels.Count != 3)
        {
            throw new Exception(
                "La sesión debe tener exactamente tres niveles."
            );
        }

        List<Dictionary<string, object>> levelsData =
            new List<Dictionary<string, object>>();

        foreach (PassesLevel level in session.levels)
        {
            if (level == null || level.level < 1 || level.level > 3)
            {
                throw new Exception(
                    "Se encontró un nivel de pases inválido."
                );
            }

            ValidatePassesLevel(level);

            Dictionary<string, object> levelData =
                new Dictionary<string, object>
                {
                    { "level", level.level },
                    { "correctAnswers", level.correctAnswers },
                    { "totalAttempts", level.totalAttempts },
                    { "accuracy", level.accuracy },
                    { "averageReaction", level.averageReaction },
                    { "caught", level.caught },
                    { "missed", level.missed },
                    { "wrongCaught", level.wrongCaught },
                    { "correctPasses", level.correctPasses },
                    { "fastestReaction", level.fastestReaction },
                    { "slowestReaction", level.slowestReaction },
                    { "reactionTimes", level.reactionTimes ?? new List<float>() }
                };

            levelsData.Add(levelData);
        }

        DocumentReference attemptRef =
            db.Collection(STUDENTS_COLLECTION)
              .Document(normalizedCode)
              .Collection("attempts")
              .Document();

        Dictionary<string, object> data =
            new Dictionary<string, object>
            {
                { "exerciseId", PASSES_EXERCISE_ID },
                { "completedAt", Timestamp.GetCurrentTimestamp() },
                { "levels", levelsData }
            };

        await attemptRef.SetAsync(data);

        Debug.Log(
            "[FIRESTORE] Intento de Pases y Reacción guardado. ID: " +
            attemptRef.Id
        );

        return attemptRef.Id;
    }

    // =========================================================
    // SAVE MOVEMENT ATTEMPT
    // =========================================================

    public async Task<string> SaveCompletedAttempt(
        string studentCode,
        MovementSession session)
    {
        string normalizedCode = NormalizeStudentCode(studentCode);

        if (session == null ||
            session.exerciseId != MOVEMENT_EXERCISE_ID)
        {
            throw new Exception(
                "Solo está implementado reaction-movement para este método."
            );
        }

        if (session.levels == null || session.levels.Count != 3)
        {
            throw new Exception(
                "La sesión debe tener exactamente tres niveles."
            );
        }

        DocumentReference attemptsRef =
            db.Collection(STUDENTS_COLLECTION)
              .Document(normalizedCode)
              .Collection("attempts")
              .Document();

        Dictionary<string, object> data =
            new Dictionary<string, object>
            {
                { "exerciseId", session.exerciseId },
                { "completedAt", Timestamp.GetCurrentTimestamp() },
                { "levels", ConvertLevelsToFirestore(session.levels) }
            };

        await attemptsRef.SetAsync(data);

        Debug.Log(
            "[FIRESTORE] Intento guardado. ID: " + attemptsRef.Id
        );

        return attemptsRef.Id;
    }

    // =========================================================
    // GET ATTEMPTS
    // =========================================================

    public async Task<List<Dictionary<string, object>>> GetStudentAttempts(
        string studentCode,
        string exerciseId = null)
    {
        string normalizedCode = NormalizeStudentCode(studentCode);

        CollectionReference attemptsRef =
            db.Collection(STUDENTS_COLLECTION)
              .Document(normalizedCode)
              .Collection("attempts");

        Query query = attemptsRef.OrderBy("completedAt");

        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        List<Dictionary<string, object>> attempts =
            new List<Dictionary<string, object>>();

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            Dictionary<string, object> data = document.ToDictionary();

            data["id"] = document.Id;

            if (!string.IsNullOrEmpty(exerciseId))
            {
                if (!data.ContainsKey("exerciseId"))
                    continue;

                if (data["exerciseId"].ToString() != exerciseId)
                    continue;
            }

            attempts.Add(data);
        }

        return attempts;
    }

    // =========================================================
    // VALIDATION
    // =========================================================

    private string NormalizeStudentCode(string studentCode)
    {
        string normalizedCode = studentCode?.Trim();

        if (string.IsNullOrEmpty(normalizedCode))
        {
            throw new Exception(
                "El código universitario es obligatorio."
            );
        }

        return normalizedCode;
    }

    private void ValidateLevel(MovementLevel level)
    {
        if (level == null)
        {
            throw new Exception(
                "Los datos del nivel no pueden ser nulos."
            );
        }

        int totalHandAttempts =
            level.leftHandAttempts + level.rightHandAttempts;

        if (level.level < 1 || level.level > 3)
        {
            throw new Exception(
                "El nivel debe ser 1, 2 o 3."
            );
        }

        if (totalHandAttempts <= 0)
        {
            throw new Exception(
                "Los intentos de ambas manos deben ser mayores que cero."
            );
        }
    }

    private void ValidatePassesLevel(PassesLevel level)
    {
        if (level == null)
        {
            throw new Exception(
                "Los datos del nivel de pases no pueden ser nulos."
            );
        }

        if (level.level < 1 || level.level > 3)
        {
            throw new Exception(
                "El nivel de pases debe ser 1, 2 o 3."
            );
        }

        if (level.correctAnswers < 0 || level.totalAttempts < 0)
        {
            throw new Exception(
                "Los aciertos y los intentos no pueden ser negativos."
            );
        }

        if (level.correctAnswers > level.totalAttempts)
        {
            throw new Exception(
                "Los aciertos no pueden superar el total de intentos."
            );
        }

        if (level.caught < 0 ||
            level.missed < 0 ||
            level.wrongCaught < 0 ||
            level.correctPasses < 0)
        {
            throw new Exception(
                "Las estadísticas de pases no pueden ser negativas."
            );
        }

        ValidateAccuracy(level.accuracy);
        ValidateReaction(level.averageReaction);
        ValidateReaction(level.fastestReaction);
        ValidateReaction(level.slowestReaction);

        if (level.reactionTimes != null)
        {
            foreach (float reactionTime in level.reactionTimes)
            {
                ValidateReaction(reactionTime);
            }
        }
    }

    private void ValidateAccuracy(float accuracy)
    {
        if (float.IsNaN(accuracy) ||
            float.IsInfinity(accuracy) ||
            accuracy < 0f ||
            accuracy > 100f)
        {
            throw new Exception(
                "La precisión debe estar entre 0 y 100."
            );
        }
    }

    private void ValidateReaction(float reaction)
    {
        if (float.IsNaN(reaction) ||
            float.IsInfinity(reaction) ||
            reaction < 0f)
        {
            throw new Exception(
                "El tiempo de reacción debe ser válido."
            );
        }
    }

    // =========================================================
    // FIRESTORE CONVERSION - MOVEMENT
    // =========================================================

    private List<Dictionary<string, object>> ConvertLevelsToFirestore(
        List<MovementLevel> levels)
    {
        List<Dictionary<string, object>> result =
            new List<Dictionary<string, object>>();

        foreach (MovementLevel level in levels)
        {
            ValidateLevel(level);

            int totalHandAttempts =
                level.leftHandAttempts + level.rightHandAttempts;

            Dictionary<string, object> levelData =
                new Dictionary<string, object>
                {
                    { "level", level.level },
                    { "averageReaction", level.averageReaction },
                    { "correctAnswers", level.correctAnswers },
                    { "totalAttempts", level.totalAttempts },
                    { "leftHandReaction", level.leftHandReaction },
                    { "rightHandReaction", level.rightHandReaction },
                    { "leftHandCorrect", level.leftHandCorrect },
                    { "leftHandAttempts", level.leftHandAttempts },
                    { "rightHandCorrect", level.rightHandCorrect },
                    { "rightHandAttempts", level.rightHandAttempts },
                    {
                        "leftHandAccuracy",
                        CalculateAccuracy(
                            level.leftHandCorrect,
                            level.leftHandAttempts
                        )
                    },
                    {
                        "rightHandAccuracy",
                        CalculateAccuracy(
                            level.rightHandCorrect,
                            level.rightHandAttempts
                        )
                    },
                    {
                        "combinedAccuracy",
                        CalculateAccuracy(
                            level.leftHandCorrect + level.rightHandCorrect,
                            totalHandAttempts
                        )
                    }
                };

            result.Add(levelData);
        }

        return result;
    }

    private float CalculateAccuracy(int correct, int attempts)
    {
        if (attempts <= 0)
            return 0f;

        return (float)correct / attempts * 100f;
    }

    // =========================================================
    // DATA CLASSES - MOVEMENT
    // =========================================================

    [Serializable]
    public class MovementSession
    {
        public string exerciseId;

        public List<MovementLevel> levels =
            new List<MovementLevel>();
    }

    [Serializable]
    public class MovementLevel
    {
        public int level;
        public float averageReaction;
        public int correctAnswers;
        public int totalAttempts;
        public float leftHandReaction;
        public float rightHandReaction;
        public int leftHandCorrect;
        public int leftHandAttempts;
        public int rightHandCorrect;
        public int rightHandAttempts;
    }

    // =========================================================
    // DATA CLASSES - LIGHTS
    // =========================================================

    [Serializable]
    public class LightsSession
    {
        public string exerciseId;

        public List<LightsLevel> levels =
            new List<LightsLevel>();
    }

    [Serializable]
    public class LightsLevel
    {
        public int level;
        public int correctAnswers;
        public float accuracy;
        public float averageReaction;
    }

    // =========================================================
    // DATA CLASSES - PASSES AND REACTION
    // =========================================================

    [Serializable]
    public class PassesSession
    {
        public string exerciseId;

        public List<PassesLevel> levels =
            new List<PassesLevel>();
    }

    [Serializable]
    public class PassesLevel
    {
        public int level;

        // Estadísticas generales
        public int correctAnswers;
        public int totalAttempts;
        public float accuracy;
        public float averageReaction;

        // Estadísticas específicas de pases y atrapes
        public int caught;
        public int missed;
        public int wrongCaught;
        public int correctPasses;

        // Reacciones mínima y máxima
        public float fastestReaction;
        public float slowestReaction;

        // Tiempos de reacción individuales, en segundos
        public List<float> reactionTimes =
            new List<float>();
    }
}
