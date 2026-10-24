using System;
using UnityEngine;

public class DribbleTutorialReaction : MonoBehaviour
{
    public enum TutorialRequirement
    {
        None,
        DribbleThreeSeconds,
        FrontPlate,
        ThreeHandChanges,
        ThreeOrangePlates,
        ThreeGreenPlates
    }

    [SerializeField] private FakeDribbleBall dribbleBall;
    [SerializeField] private DribbleBallStart ballStart;
    [SerializeField] private DribbleHandManager handManager;

    [Header("Configuración")]
    [SerializeField] private float requiredDribbleTime = 3f;
    [SerializeField] private int requiredHandChanges = 3;
    [SerializeField] private int requiredOrangeTouches = 3;
    [SerializeField] private int requiredGreenTouches = 3;

    private TutorialRequirement currentRequirement;

    private float dribbleTimer;

    private int handChanges;
    private int orangeTouches;
    private int greenTouches;

    private bool requirementCompleted;

    public event Action OnRequirementCompleted;

    public TutorialRequirement CurrentRequirement =>
        currentRequirement;

    private void OnEnable()
    {
        if (handManager != null)
        {
            handManager.OnSuccessfulHandChange +=
                RegisterHandChange;
        }
    }

    private void OnDisable()
    {
        if (handManager != null)
        {
            handManager.OnSuccessfulHandChange -=
                RegisterHandChange;
        }
    }

    private void Update()
    {
        if (requirementCompleted)
            return;

        if (currentRequirement !=
            TutorialRequirement.DribbleThreeSeconds)
        {
            return;
        }

        CheckDribbleTime();
    }

    public void StartRequirement(
        TutorialRequirement requirement)
    {
        currentRequirement = requirement;

        requirementCompleted = false;

        dribbleTimer = 0f;
        handChanges = 0;
        orangeTouches = 0;
        greenTouches = 0;
    }

    public void StopRequirement()
    {
        currentRequirement =
            TutorialRequirement.None;

        requirementCompleted = false;

        dribbleTimer = 0f;
        handChanges = 0;
        orangeTouches = 0;
        greenTouches = 0;
    }

    private void CheckDribbleTime()
    {
        if (dribbleBall == null)
            return;

        if (!dribbleBall.IsControlled)
        {
            dribbleTimer = 0f;
            return;
        }

        if (ballStart != null &&
            !ballStart.DribbleEnabled)
        {
            dribbleTimer = 0f;
            return;
        }

        dribbleTimer +=
            Time.deltaTime;

        if (dribbleTimer >=
            requiredDribbleTime)
        {
            CompleteRequirement();
        }
    }

    public void RegisterFrontPlate()
    {
        if (requirementCompleted)
            return;

        if (currentRequirement !=
            TutorialRequirement.FrontPlate)
        {
            return;
        }

        CompleteRequirement();
    }

    public void RegisterHandChange()
    {
        if (requirementCompleted)
            return;

        if (currentRequirement !=
            TutorialRequirement.ThreeHandChanges)
        {
            return;
        }

        handChanges++;

        Debug.Log(
            "Cambios de mano: " +
            handChanges +
            "/" +
            requiredHandChanges
        );

        if (handChanges >=
            requiredHandChanges)
        {
            CompleteRequirement();
        }
    }

    public void RegisterOrangePlate()
    {
        if (requirementCompleted)
            return;

        if (currentRequirement !=
            TutorialRequirement.ThreeOrangePlates)
        {
            return;
        }

        orangeTouches++;

        Debug.Log(
            "Naranjas: " +
            orangeTouches +
            "/" +
            requiredOrangeTouches
        );

        if (orangeTouches >=
            requiredOrangeTouches)
        {
            CompleteRequirement();
        }
    }

    public void RegisterGreenPlate()
    {
        if (requirementCompleted)
            return;

        if (currentRequirement !=
            TutorialRequirement.ThreeGreenPlates)
        {
            return;
        }

        greenTouches++;

        Debug.Log(
            "Verdes: " +
            greenTouches +
            "/" +
            requiredGreenTouches
        );

        if (greenTouches >=
            requiredGreenTouches)
        {
            CompleteRequirement();
        }
    }

    private void CompleteRequirement()
    {
        if (requirementCompleted)
            return;

        requirementCompleted = true;

        OnRequirementCompleted?.Invoke();
    }
}