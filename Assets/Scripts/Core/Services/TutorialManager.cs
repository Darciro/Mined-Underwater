using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central manager that holds references to all tutorial frames
/// and exposes methods to show each one using Feel feedbacks.
/// Only one tutorial frame can be visible at a time.
/// The first tutorial waits a configurable delay before appearing;
/// each subsequent tutorial also waits the same delay after the previous one closes.
/// </summary>
public class TutorialManager : MonoBehaviour
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

    [SerializeField]
    [Tooltip("Tutorial explaining air bubbles")]
    private TutorialFrame airBubblesTutorial;

    [SerializeField]
    [Tooltip("Tutorial explaining collectibles and their benefits")]
    private TutorialFrame collectiblesTutorial;

    [SerializeField]
    [Tooltip("Tutorial explaining how eggs work")]
    private TutorialFrame eggsTutorial;

    [Tooltip("Main spawner used in the tutorial to show how spawning works")]
    [SerializeField]
    private GameObject tutorialSpawners;

    [Header("Timing")]
    [SerializeField]
    [Tooltip("Seconds to wait before showing a tutorial")]
    private float delayBeforeShow = 5f;

    [SerializeField]
    [Tooltip("Seconds to wait before showing the final (eggs) tutorial")]
    private float delayBeforeEndTutorial = 10f;

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
        tutorialSequence = new[] { startTutorial, movementTutorial, healthbarTutorial, bubbleshotTutorial, resourcesTutorial, requirementsTutorial, airBubblesTutorial, collectiblesTutorial, eggsTutorial };
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
        float delay = tutorial == eggsTutorial ? delayBeforeEndTutorial : delayBeforeShow;
        yield return new WaitForSeconds(delay);

        activeTutorial = tutorial;
        tutorial.OnClosed.AddListener(OnActiveTutorialClosed);

        if (overlayTutorial != null)
            overlayTutorial.SetActive(true);
        tutorial.transform.localScale = Vector3.zero;
        tutorial.gameObject.SetActive(true);

        // Pause the game while a tutorial is active
        if (GameManager.Instance != null)
            GameManager.Instance.ChangeState(GameStateEnum.Paused);

        pendingShowCoroutine = null;
    }

    private void OnActiveTutorialClosed()
    {
        TutorialFrame closedTutorial = activeTutorial;
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
