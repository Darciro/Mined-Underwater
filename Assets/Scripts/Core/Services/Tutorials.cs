using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central manager that holds references to all tutorial frames
/// and exposes methods to show each one using Feel feedbacks.
/// Only one tutorial frame can be visible at a time.
/// The first tutorial waits a configurable delay before appearing;
/// each subsequent tutorial also waits the same delay after the previous one closes.
/// </summary>
public class Tutorials : MonoBehaviour
{
    #region Inspector Fields

    [Header("Tutorial Frames")]
    [SerializeField]
    [Tooltip("Tutorial explaining the start of the game")]
    private TutorialFrame startTutorial;

    [SerializeField]
    [Tooltip("Tutorial explaining player movement")]
    private TutorialFrame movementTutorial;

    [SerializeField]
    [Tooltip("Tutorial explaining the health bar")]
    private TutorialFrame healthbarTutorial;

    [SerializeField]
    [Tooltip("Tutorial explaining the bubble shot")]
    private TutorialFrame bubbleshotTutorial;

    [SerializeField]
    [Tooltip("Tutorial explaining your resources")]
    private TutorialFrame resourcesTutorial;

    [SerializeField]
    [Tooltip("Tutorial explaining stage requirements")]
    private TutorialFrame requirementsTutorial;

    [Header("Feedbacks")]
    [SerializeField]
    [Tooltip("Feedback played when showing a tutorial frame")]
    private MMF_Player showFeedback;

    [Header("Timing")]
    [SerializeField]
    [Tooltip("Seconds to wait before showing a tutorial")]
    private float delayBeforeShow = 3f;

    [SerializeField]
    [Tooltip("Overlay canvas group to block interactions when a tutorial is active")]
    private GameObject overlayTutorial;

    #endregion

    #region Private Fields

    private TutorialFrame activeTutorial;
    private Coroutine pendingShowCoroutine;
    private TutorialFrame[] tutorialSequence;
    private int currentTutorialIndex;

    /// <summary>
    /// Returns true while a tutorial frame is currently being shown.
    /// </summary>
    public bool IsTutorialActive => activeTutorial != null;

    #endregion

    #region Unity Callbacks

    private void Start()
    {
        // Build the ordered sequence of tutorials to show
        tutorialSequence = new[] { startTutorial, movementTutorial, healthbarTutorial, bubbleshotTutorial, resourcesTutorial, requirementsTutorial };
        currentTutorialIndex = 0;

        if (overlayTutorial != null)
            overlayTutorial.SetActive(false);

        // Hide all frames initially
        foreach (var frame in tutorialSequence)
        {
            if (frame != null)
                frame.gameObject.SetActive(false);
        }

        // Start the first tutorial after the delay
        ShowNextInSequence();
    }

    #endregion

    #region Public Methods

    public void ShowStartTutorial()
    {
        RequestShow(startTutorial);
    }

    public void ShowHealthbarTutorial()
    {
        RequestShow(healthbarTutorial);
    }

    public void ShowResourcesTutorial()
    {
        RequestShow(resourcesTutorial);
    }

    public void ShowMovementTutorial()
    {
        RequestShow(movementTutorial);
    }

    public void ShowBubbleshotTutorial()
    {
        RequestShow(bubbleshotTutorial);
    }

    #endregion

    #region Private Methods

    private void RequestShow(TutorialFrame tutorial)
    {
        if (tutorial == null) return;

        // Ignore if this tutorial is already active
        if (activeTutorial == tutorial) return;

        // Cancel any pending delayed show
        if (pendingShowCoroutine != null)
            StopCoroutine(pendingShowCoroutine);

        // If another tutorial is visible, close it first then wait before showing the new one
        if (activeTutorial != null)
        {
            if (overlayTutorial != null)
                overlayTutorial.SetActive(false);
            activeTutorial.OnClosed.RemoveListener(OnActiveTutorialClosed);
            activeTutorial.gameObject.SetActive(false);
            activeTutorial = null;
        }

        pendingShowCoroutine = StartCoroutine(ShowAfterDelay(tutorial));
    }

    private IEnumerator ShowAfterDelay(TutorialFrame tutorial)
    {
        yield return new WaitForSeconds(delayBeforeShow);

        activeTutorial = tutorial;
        tutorial.OnClosed.AddListener(OnActiveTutorialClosed);

        if (overlayTutorial != null)
            overlayTutorial.SetActive(true);
        tutorial.transform.localScale = Vector3.zero;
        tutorial.gameObject.SetActive(true);

        if (showFeedback != null)
            showFeedback.PlayFeedbacks();

        // Pause the game while a tutorial is active
        if (GameManager.Instance != null)
            GameManager.Instance.ChangeState(GameStateEnum.Paused);

        pendingShowCoroutine = null;
    }

    private void OnActiveTutorialClosed()
    {
        if (activeTutorial != null)
        {
            activeTutorial.OnClosed.RemoveListener(OnActiveTutorialClosed);
            activeTutorial = null;
        }

        // Hide the overlay and resume gameplay
        if (overlayTutorial != null)
            overlayTutorial.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.ChangeState(GameStateEnum.Playing);

        // Automatically show the next tutorial in the sequence
        currentTutorialIndex++;
        ShowNextInSequence();
    }

    private void ShowNextInSequence()
    {
        if (tutorialSequence == null) return;

        // Skip null entries
        while (currentTutorialIndex < tutorialSequence.Length && tutorialSequence[currentTutorialIndex] == null)
            currentTutorialIndex++;

        if (currentTutorialIndex < tutorialSequence.Length)
            RequestShow(tutorialSequence[currentTutorialIndex]);
    }

    #endregion
}
