using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TutorialFrame : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private GameObject[] tutorial;

    [Header("Navigation")]
    [SerializeField] private Button nextButton;

    [Header("Close Animation")]
    [SerializeField] private float closeDuration = 0.3f;

    [HideInInspector] public UnityEvent OnClosed = new UnityEvent();

    private int currentPage;
    private CanvasGroup canvasGroup;

    private Vector3 initialScale = Vector3.one;
    private float initialAlpha = 1f;
    private Coroutine closeRoutine;
    private Coroutine openRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Cache the "authoring" values from the prefab/scene,
        // but only if they are non-zero so that objects that start
        // in their closed state (scale 0 / alpha 0) still animate correctly.
        if (transform.localScale.sqrMagnitude > 0f)
            initialScale = transform.localScale;

        if (canvasGroup.alpha > 0f)
            initialAlpha = canvasGroup.alpha;
    }

    private void OnEnable()
    {
        // Always reset visuals when re-opened (prevents device-dependent weirdness)
        if (closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
            closeRoutine = null;
        }

        if (openRoutine != null)
        {
            StopCoroutine(openRoutine);
            openRoutine = null;
        }

        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        currentPage = 0;
        ShowPage(currentPage);

        openRoutine = StartCoroutine(AnimateOpen());

        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    private void OnDisable()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
    }

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
            if (tutorial[i] != null)
                tutorial[i].SetActive(i == index);
    }

    private IEnumerator AnimateOpen()
    {
        float elapsed = 0f;

        while (elapsed < closeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / closeDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = initialScale * smooth;
            canvasGroup.alpha = initialAlpha * smooth;

            yield return null;
        }

        transform.localScale = initialScale;
        canvasGroup.alpha = initialAlpha;
        openRoutine = null;
    }

    private void Close()
    {
        for (int i = 0; i < tutorial.Length; i++)
            if (tutorial[i] != null)
                tutorial[i].SetActive(false);

        closeRoutine = StartCoroutine(AnimateClose());
    }

    private IEnumerator AnimateClose()
    {
        float elapsed = 0f;

        while (elapsed < closeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / closeDuration);
            float smooth = Mathf.SmoothStep(1f, 0f, t);

            transform.localScale = initialScale * smooth;
            canvasGroup.alpha = initialAlpha * smooth;

            yield return null;
        }

        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        gameObject.SetActive(false);
        OnClosed.Invoke();
        closeRoutine = null;
    }
}