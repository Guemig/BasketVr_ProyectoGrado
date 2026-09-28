using UnityEngine;

public class ReactionSequence : MonoBehaviour
{
    [Header("Sequence Settings")]
    [SerializeField] private int totalReactions = 10;

    [SerializeField] private int maxConsecutiveReactions = 3;


    private ReactionExerciseController.Hand[] sequence;

    private int currentIndex;


    // =====================================================
    // CREATE
    // =====================================================

    public void CreateSequence()
    {
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
}