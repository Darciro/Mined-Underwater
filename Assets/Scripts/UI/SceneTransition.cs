using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    private Animator _animator;
    [SerializeField] private float _transitionTime = 1f;
    [SerializeField] private GameObject _objectivePanel;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        if (_objectivePanel != null)
        {
            _animator = _objectivePanel.GetComponent<Animator>();
            StartCoroutine(EndObjectiveTransitionCoroutine());
        }
    }

    public void StartSceneTransition(string sceneName)
    {
        if (_animator != null)
        {
            StartCoroutine(SceneTransitionCoroutine(sceneName));
        }
    }

    private IEnumerator SceneTransitionCoroutine(string sceneName)
    {
        _animator.SetTrigger("Start");
        yield return new WaitForSeconds(_transitionTime);
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator EndObjectiveTransitionCoroutine()
    {
        yield return new WaitForSeconds(3f);
        _animator.SetTrigger("EndTransition");
    }

}
