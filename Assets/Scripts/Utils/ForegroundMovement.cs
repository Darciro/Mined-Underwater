
using System.Collections;
using UnityEngine;

public class ForegroundMovement : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private Vector3 fromPosition = Vector3.zero;
    [SerializeField] private Vector3 toPosition = Vector3.right * 5f;

    [Header("Timing")]
    [SerializeField] private float duration = 3f;
    [SerializeField] private bool playOnEnable = true;

    [Header("Behavior")]
    [SerializeField] private bool loop = true;
    [SerializeField] private bool pingPong = false;
    [SerializeField] private bool useLocalPosition = false;

    private Coroutine moveRoutine;

    private void OnEnable()
    {
        if (playOnEnable)
        {
            StartMovement();
        }
    }

    private void OnDisable()
    {
        StopMovement();
    }

    public void StartMovement()
    {
        StopMovement();

        if (duration <= 0f)
        {
            SetPosition(toPosition);
            return;
        }

        SetPosition(fromPosition);
        moveRoutine = StartCoroutine(Move());
    }

    public void StopMovement()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
    }

    private IEnumerator Move()
    {
        while (true)
        {
            yield return LerpPositions(fromPosition, toPosition);

            if (!loop)
            {
                break;
            }

            if (pingPong)
            {
                // swap endpoints for the next leg
                (fromPosition, toPosition) = (toPosition, fromPosition);
            }
            else
            {
                SetPosition(fromPosition);
            }
        }
    }

    private IEnumerator LerpPositions(Vector3 start, Vector3 end)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 pos = Vector3.Lerp(start, end, t);
            SetPosition(pos);
            elapsed += Time.deltaTime;
            yield return null;
        }

        SetPosition(end);
    }

    private void SetPosition(Vector3 pos)
    {
        if (useLocalPosition)
        {
            transform.localPosition = pos;
        }
        else
        {
            transform.position = pos;
        }
    }
}
