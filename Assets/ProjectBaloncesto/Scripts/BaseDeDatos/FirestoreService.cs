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

    // =========================================================
    // CURRENT STUDENT
    // =========================================================

    private string currentStudentCode;

    public string CurrentStudentCode =>
        currentStudentCode;

    public bool HasCurrentStudent =>
        !string.IsNullOrEmpty(currentStudentCode);


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
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

    public void SetCurrentStudent(
        string studentCode)
    {
        currentStudentCode =
            NormalizeStudentCode(studentCode);

        Debug.Log(
            "[FIRESTORE] Current student set: " +
            currentStudentCode
        );
    }


    public void ClearCurrentStudent()
    {
        currentStudentCode = null;

        Debug.Log(
            "[FIRESTORE] Current student cleared."
        );
    }


    // =========================================================
    // STUDENTS
    // =========================================================

    public async Task<Dictionary<string, object>> GetStudent(
        string studentCode)
    {
        string normalizedCode =
            NormalizeStudentCode(studentCode);

        DocumentReference studentRef =
            db.Collection(STUDENTS_COLLECTION)
              .Document(normalizedCode);

        DocumentSnapshot snapshot =
            await studentRef.GetSnapshotAsync();

        if (!snapshot.Exists)
            return null;

        return snapshot.ToDictionary();
    }


    // =========================================================
    // CHECK STUDENT CODE
    // =========================================================

    public async Task<bool> StudentCodeExists(
        string studentCode)
    {
        string normalizedCode =
            NormalizeStudentCode(studentCode);

        DocumentReference studentRef =
            db.Collection(STUDENTS_COLLECTION)
              .Document(normalizedCode);

        DocumentSnapshot snapshot =
            await studentRef.GetSnapshotAsync();

        return snapshot.Exists;
    }


    // =========================================================
    // CREATE STUDENT
    // =========================================================

    public async Task<bool> CreateStudentIfNotExists(
        string name,
        string studentCode)
    {
        string normalizedName =
            name?.Trim();

        string normalizedCode =
            NormalizeStudentCode(studentCode);


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
                {
                    "name",
                    normalizedName
                },

                {
                    "studentCode",
                    normalizedCode
                },

                {
                    "createdAt",
                    Timestamp.GetCurrentTimestamp()
                }
            };


        await studentRef.SetAsync(data);


        Debug.Log(
            "[FIRESTORE] Student created: " +
            normalizedCode
        );


        return true;
    }


    // =========================================================
    // GET ALL STUDENTS
    // =========================================================

    public async Task<List<Dictionary<string, object>>>
        GetAllStudents()
    {
        QuerySnapshot snapshot =
            await db.Collection(STUDENTS_COLLECTION)
                    .GetSnapshotAsync();


        List<Dictionary<string, object>> students =
            new List<Dictionary<string, object>>();


        foreach (DocumentSnapshot document
                 in snapshot.Documents)
        {
            Dictionary<string, object> student =
                document.ToDictionary();


            student["id"] =
                document.Id;


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
            exerciseId =
                MOVEMENT_EXERCISE_ID,

            levels =
                new List<MovementLevel>()
        };
    }


    public MovementSession AddLevelToSession(
        MovementSession session,
        MovementLevel levelData)
    {
        if (session == null ||
            session.exerciseId !=
            MOVEMENT_EXERCISE_ID)
        {
            throw new Exception(
                "La sesión de reacción de movimiento no es válida."
            );
        }


        if (session.levels.Count >= 3)
        {
            throw new Exception(
                "La sesión ya tiene los tres niveles registrados."
            );
        }


        levelData.level =
            session.levels.Count + 1;


        ValidateLevel(levelData);


        session.levels.Add(
            levelData
        );


        return session;
    }


    // =========================================================
    // SAVE ATTEMPT
    // =========================================================

    public async Task<string> SaveCompletedAttempt(
        string studentCode,
        MovementSession session)
    {
        string normalizedCode =
            NormalizeStudentCode(studentCode);


        if (session == null ||
            session.exerciseId !=
            MOVEMENT_EXERCISE_ID)
        {
            throw new Exception(
                "Solo está implementado reaction-movement."
            );
        }


        if (session.levels == null ||
            session.levels.Count != 3)
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
                {
                    "exerciseId",
                    session.exerciseId
                },

                {
                    "completedAt",
                    Timestamp.GetCurrentTimestamp()
                },

                {
                    "levels",
                    ConvertLevelsToFirestore(
                        session.levels
                    )
                }
            };


        await attemptsRef.SetAsync(data);


        Debug.Log(
            "[FIRESTORE] Intento guardado. ID: " +
            attemptsRef.Id
        );


        return attemptsRef.Id;
    }


    // =========================================================
    // GET ATTEMPTS
    // =========================================================

    public async Task<List<Dictionary<string, object>>>
        GetStudentAttempts(
            string studentCode,
            string exerciseId = null)
    {
        string normalizedCode =
            NormalizeStudentCode(studentCode);


        CollectionReference attemptsRef =
            db.Collection(STUDENTS_COLLECTION)
              .Document(normalizedCode)
              .Collection("attempts");


        Query query =
            attemptsRef.OrderBy(
                "completedAt"
            );


        QuerySnapshot snapshot =
            await query.GetSnapshotAsync();


        List<Dictionary<string, object>> attempts =
            new List<Dictionary<string, object>>();


        foreach (DocumentSnapshot document
                 in snapshot.Documents)
        {
            Dictionary<string, object> data =
                document.ToDictionary();


            data["id"] =
                document.Id;


            if (!string.IsNullOrEmpty(exerciseId))
            {
                if (!data.ContainsKey("exerciseId"))
                    continue;


                if (data["exerciseId"].ToString()
                    != exerciseId)
                {
                    continue;
                }
            }


            attempts.Add(data);
        }


        return attempts;
    }


    // =========================================================
    // VALIDATION
    // =========================================================

    private string NormalizeStudentCode(
        string studentCode)
    {
        string normalizedCode =
            studentCode?.Trim();


        if (string.IsNullOrEmpty(normalizedCode))
        {
            throw new Exception(
                "El código universitario es obligatorio."
            );
        }


        return normalizedCode;
    }


    private void ValidateLevel(
        MovementLevel level)
    {
        int totalHandAttempts =
            level.leftHandAttempts +
            level.rightHandAttempts;


        if (level.level < 1 ||
            level.level > 3)
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


    // =========================================================
    // FIRESTORE CONVERSION
    // =========================================================

    private List<Dictionary<string, object>>
        ConvertLevelsToFirestore(
            List<MovementLevel> levels)
    {
        List<Dictionary<string, object>> result =
            new List<Dictionary<string, object>>();


        foreach (MovementLevel level
                 in levels)
        {
            int totalHandAttempts =
                level.leftHandAttempts +
                level.rightHandAttempts;


            Dictionary<string, object> levelData =
                new Dictionary<string, object>
                {
                    {
                        "level",
                        level.level
                    },

                    {
                        "averageReaction",
                        level.averageReaction
                    },

                    {
                        "correctAnswers",
                        level.correctAnswers
                    },

                    {
                        "totalAttempts",
                        level.totalAttempts
                    },

                    {
                        "leftHandReaction",
                        level.leftHandReaction
                    },

                    {
                        "rightHandReaction",
                        level.rightHandReaction
                    },

                    {
                        "leftHandCorrect",
                        level.leftHandCorrect
                    },

                    {
                        "leftHandAttempts",
                        level.leftHandAttempts
                    },

                    {
                        "rightHandCorrect",
                        level.rightHandCorrect
                    },

                    {
                        "rightHandAttempts",
                        level.rightHandAttempts
                    },

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
                            level.leftHandCorrect +
                            level.rightHandCorrect,

                            totalHandAttempts
                        )
                    }
                };


            result.Add(
                levelData
            );
        }


        return result;
    }


    private float CalculateAccuracy(
        int correct,
        int attempts)
    {
        if (attempts <= 0)
            return 0f;


        return
            (float)correct /
            attempts *
            100f;
    }


    // =========================================================
    // DATA CLASSES
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
}