using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// A tutorial frame that displays pages of text one at a time.
/// Each page is a separate GameObject (text child) assigned from the editor.
/// A button advances to the next page, or closes the frame on the last page.
/// </summary>
public class TutorialFrame : MonoBehaviour
{
    #region Inspector Fields

    [Header("Pages")]
    [SerializeField]
    [Tooltip("Tutorial pages to display in order. Each element is a GameObject that represents a single page.")]
    private GameObject[] tutorial;

    [Header("Navigation")]
    [SerializeField]
    [Tooltip("Button used to advance to the next page or close the tutorial on the last page.")]
    private Button nextButton;

    [Header("Close Animation")]
    [SerializeField]
    [Tooltip("Duration of the close animation in seconds")]
    private float closeDuration = 0.3f;

    #endregion

    #region Events

    /// <summary>
    /// Invoked when this tutorial frame is closed.
    /// </summary>
    [HideInInspector]
    public UnityEvent OnClosed = new UnityEvent();

    #endregion

    #region Private Fields

    private int currentPage;
    private CanvasGroup canvasGroup;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        currentPage = 0;
        ShowPage(currentPage);

        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    private void OnDisable()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
    }

    #endregion

    #region Private Methods

    private void OnNextButtonClicked()
    {
        if (currentPage < tutorial.Length - 1)
        {
            currentPage++;
            ShowPage(currentPage);
        }
        else
        {
            Close();
        }
    }

    private void ShowPage(int index)
    {
        for (int i = 0; i < tutorial.Length; i++)
        {
            if (tutorial[i] != null)
                tutorial[i].SetActive(i == index);
        }
    }

    private void Close()
    {
        // Hide all pages immediately
        for (int i = 0; i < tutorial.Length; i++)
        {
            if (tutorial[i] != null)
                tutorial[i].SetActive(false);
        }

        StartCoroutine(AnimateClose());
    }

    private IEnumerator AnimateClose()
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < closeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / closeDuration;
            float smooth = Mathf.SmoothStep(1f, 0f, t);

            transform.localScale = startScale * smooth;
            canvasGroup.alpha = startAlpha * smooth;

            yield return null;
        }

        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        gameObject.SetActive(false);
        OnClosed.Invoke();
    }

    #endregion
}
