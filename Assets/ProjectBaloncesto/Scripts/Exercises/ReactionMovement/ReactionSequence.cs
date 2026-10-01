using UnityEngine;

public class ReactionSequence : MonoBehaviour
{
    [Header("Sequence Settings")]
    [SerializeField] private ReactionLevelController levelController;

    [SerializeField]
    private int[] totalReactionsByLevel =
    {
        10, 10, 10
    };

    [SerializeField] private int maxConsecutiveReactions = 3;


    private ReactionExerciseController.Hand[] sequence;

    private int currentIndex;


    // =====================================================
    // CREATE
    // =====================================================

    public void CreateSequence()
    {
        int totalReactions = GetTotalReactions();

        sequence =
            new ReactionExerciseController.Hand[totalReactions];


        for (int i = 0; i < totalReactions; i++)
        {
            sequence[i] = GetRandomHand(i);
        }


        currentIndex = 0;


        Debug.Log(
            $"[SEQUENCE] Created with {totalReactions} reactions."
        );

        Debug.Log(
            $"[SEQUENCE] {string.Join(", ", sequence)}"
        );
    }


    // =====================================================
    // RANDOM HAND
    // =====================================================

    private ReactionExerciseController.Hand GetRandomHand(int index)
    {
        // Primera reacción completamente aleatoria.
        if (index == 0)
        {
            return GetRandomHand();
        }


        ReactionExerciseController.Hand lastHand =
            sequence[index - 1];


        int consecutiveCount =
            GetConsecutiveCount(index);


        // Si ya alcanzó el máximo,
        // obligatoriamente cambia de lado.
        if (consecutiveCount >= maxConsecutiveReactions)
        {
            return GetOppositeHand(lastHand);
        }


        // Si todavía puede repetir,
        // elegimos aleatoriamente.
        return GetRandomHand();
    }


    // =====================================================
    // RANDOM
    // =====================================================

    private ReactionExerciseController.Hand GetRandomHand()
    {
        return Random.value > 0.5f
            ? ReactionExerciseController.Hand.Left
            : ReactionExerciseController.Hand.Right;
    }


    // =====================================================
    // OPPOSITE
    // =====================================================

    private ReactionExerciseController.Hand GetOppositeHand(
        ReactionExerciseController.Hand hand)
    {
        return hand ==
               ReactionExerciseController.Hand.Left
            ? ReactionExerciseController.Hand.Right
            : ReactionExerciseController.Hand.Left;
    }


    // =====================================================
    // CONSECUTIVE COUNT
    // =====================================================

    private int GetConsecutiveCount(int index)
    {
        ReactionExerciseController.Hand currentHand =
            sequence[index - 1];

        int count = 1;


        for (int i = index - 2; i >= 0; i--)
        {
            if (sequence[i] != currentHand)
                break;

            count++;
        }


        return count;
    }


    // =====================================================
    // NEXT
    // =====================================================

    public bool HasNext()
    {
        return sequence != null &&
               currentIndex < sequence.Length;
    }


    public ReactionExerciseController.Hand GetNext()
    {
        if (!HasNext())
            return ReactionExerciseController.Hand.Left;

        return sequence[currentIndex++];
    }


    // =====================================================
    // RESET
    // =====================================================

    public void ResetSequence()
    {
        currentIndex = 0;
    }


    // =====================================================
    // TOTAL REACTIONS BY LEVEL
    // =====================================================

    private int GetTotalReactions()
    {
        if (totalReactionsByLevel == null ||
            totalReactionsByLevel.Length == 0)
        {
            return 10;
        }

        if (levelController == null)
        {
            return Mathf.Max(1, totalReactionsByLevel[0]);
        }

        int index = (int)levelController.CurrentLevel;

        if (index < 0 || index >= totalReactionsByLevel.Length)
        {
            return Mathf.Max(1, totalReactionsByLevel[
                totalReactionsByLevel.Length - 1
            ]);
        }

        return Mathf.Max(1, totalReactionsByLevel[index]);
    }


#if UNITY_EDITOR

    private void OnValidate()
    {
        int levelCount =
            System.Enum.GetValues(
                typeof(
                    ReactionLevelController.ReactionLevel
                )
            ).Length;


        if (totalReactionsByLevel == null ||
            totalReactionsByLevel.Length != levelCount)
        {
            int[] newTotals =
                new int[levelCount];


            for (int i = 0;
                 i < levelCount;
                 i++)
            {
                if (totalReactionsByLevel != null &&
                    i < totalReactionsByLevel.Length)
                {
                    newTotals[i] =
                        totalReactionsByLevel[i];
                }
                else
                {
                    newTotals[i] =
                        10;
                }
            }


            totalReactionsByLevel =
                newTotals;
        }
    }

#endif
}