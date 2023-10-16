using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class V_TransitionManager : MonoBehaviour
{
    public static V_TransitionManager Instance;

    [SerializeField] GameObject _square1;
    [SerializeField] GameObject _square2;
    [SerializeField] Transform _parent;
    [SerializeField] Vector2 _amount;
    [SerializeField] GameObject _loading;
    [SerializeField] Canvas _transCanvas;
    public float Dur;
    public bool Transitioning;

    Vector2 _size;
    float _miniDur;

    private void OnEnable()
    {
        A_EventManager.OnTransition += Transition;
        SceneManager.sceneLoaded += Detransition;

        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(transform.root.gameObject);
    }

    private void OnDisable()
    {
        A_EventManager.OnTransition -= Transition;
        SceneManager.sceneLoaded -= Detransition;
    }

    void Start()
    {
        _size = new Vector2(Screen.width, Screen.height) / _amount / _transCanvas.transform.localScale;
        _miniDur = Dur / (_amount.x + _amount.y);
    }

    Vector2 ConvertPos(Vector2 pos) => pos * _size * _transCanvas.transform.localScale + _size / 2 * _transCanvas.transform.localScale;
    void Transition() => StartCoroutine(C_Transition());
    void Detransition(Scene scene, LoadSceneMode mode) => StartCoroutine(C_Detransition());

    IEnumerator C_Transition()
    {
        if (Transitioning)
            yield break;

        Transitioning = true;
        StartCoroutine(C_LoadingOn());
        A_EventManager.InvokePlaySFX("Trans");
        int counter = 0;

        while (counter < _amount.x + _amount.y)
        {
            foreach (Vector2 pos in Addends(counter))
            {
                GameObject square = (pos.x + pos.y).IsEven() ? _square1 : _square2;
                GameObject go = Instantiate(square, ConvertPos(pos), Quaternion.identity, _parent);
                StartCoroutine(C_SizeUpSquare(go));              
            }
            counter++;
            yield return new WaitForSecondsRealtime(_miniDur);
        }
        yield return new WaitForSecondsRealtime(Dur / 2);
        A_EventManager.InvokeTransitionEnd();
    }

    List<Vector2> Addends(int i)
    {
        List<Vector2> result = new List<Vector2>();
        Vector2 addend = new Vector2(0, i);
        while (addend.x <= i)
        {
            result.Add(addend);
            addend.x++;
            addend.y--;
        }
        return result;
    }

    IEnumerator C_Detransition()
    {
        if (!Transitioning)
            yield break;

        A_EventManager.InvokePlaySFX("Detrans");

        for (int i = 0; i < _parent.childCount; i++)
        {
            _parent.GetChild(i).name = i.ToString();
            StartCoroutine(C_SizeDownSquare(_parent.GetChild(i).gameObject));

            if (i == _parent.childCount - 1)
                break;

            if (_parent.GetChild(i).position.y > _parent.GetChild(i + 1).position.y)
                continue;

            yield return new WaitForSecondsRealtime(_miniDur);
        }

        StartCoroutine(C_LoadingOff());
        yield return new WaitForSecondsRealtime(Dur * 0.51f);
        _parent.DestroyChildren();
        Transitioning = false;
    }

    IEnumerator C_SizeUpSquare(GameObject go)
    {
        float elapsed = 0;
        float growDur = Dur / 2;

        while (elapsed < growDur)
        {
            if (go == null)
                yield break;

            float curved = A_Extensions.CosCurve(elapsed / growDur);
            go.transform.localScale = Vector2.Lerp(Vector2.zero, _size, curved);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (go == null)
            yield break;

        go.transform.localScale = _size;
    }

    IEnumerator C_SizeDownSquare(GameObject go)
    {
        float elapsed = 0;
        float growDur = Dur / 2;

        while (elapsed < growDur)
        {
            if (go == null)
                yield break;

            float curved = A_Extensions.CosCurve(elapsed / growDur);
            go.transform.localScale = Vector2.Lerp(_size, Vector2.zero, curved);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (go == null)
            yield break;

        go.transform.localScale = Vector2.zero;
    }


    IEnumerator C_LoadingOn()
    {
        float elapsed = 0;
        float dur = 0.5f;
        _loading.SetActive(true);

        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            _loading.transform.localScale = Vector2.one * Mathf.Lerp(0, 1, curved);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    IEnumerator C_LoadingOff()
    {
        float elapsed = 0;
        float dur = 0.5f;

        while (elapsed < dur)
        {
            float curved = A_Extensions.CosCurve(elapsed / dur);
            _loading.transform.localScale = Vector2.one * Mathf.Lerp(1, 0, curved);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        _loading.SetActive(false);
    }
}
