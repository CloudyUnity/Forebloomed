using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class A_IntroManager : MonoBehaviour
{
    [SerializeField] Image _fader;
    [SerializeField] TMP_Text _txt;
    [SerializeField] GameObject _glowFish;
    [SerializeField] List<Pest> _pests = new List<Pest>();
    [SerializeField] GameObject _pestParent;
    [SerializeField] GameObject _scene;
    [SerializeField] GameObject _lightning;
    [SerializeField] GameObject _mainHedge;
    [SerializeField] List<Image> _hedgeRendParts = new List<Image>();
    [SerializeField] GameObject _eyebrows;
    [SerializeField] GameObject _skipWheelGreen;
    [SerializeField] GameObject _skipWheelGrey;

    [SerializeField] Image _color;

    [SerializeField] Color _cA;
    [SerializeField] Color _cB;
    [SerializeField] Color _cC;

    const float GFRange = 600;
    const float ZOOM_AMOUNT = 1.8f;

    const string CHANGE_SFX = "Trans";

    [System.Serializable]
    public struct Pest
    {
        public GameObject go;
        public Vector2 position;
    }

    private void Start()
    {
        StartCoroutine(C_IntroFull());
    }

    float _skipTimer;

    private void Update()
    {
        if (_skipTimer < -990)
            return;

        if (Input.GetKey(KeyCode.Space))
            _skipTimer += Time.deltaTime;
        else 
            _skipTimer = 0;

        _skipWheelGreen.transform.localScale = Vector2.one * Mathf.Lerp(0, 1, _skipTimer / 1.5f);
        _skipWheelGrey.transform.localScale = Vector2.Lerp(_skipWheelGrey.transform.localScale, _skipTimer == 0 ? Vector2.zero : Vector2.one, Time.deltaTime * 15);

        if (_skipTimer > 1.5f)
        {
            LoadMainMenu();
            _skipTimer = -999;
        }          
    }

    bool IsInput()
    {
        return Input.anyKeyDown && !Input.GetKey(KeyCode.Space);
    }
    IEnumerator C_IntroFull()
    {
        float elapsed = 0;
        float curved = 0;

        _color.color = _cA;
        
        // Fade in
        while (elapsed < 2f)
        {
            curved = A_Extensions.CosCurve(elapsed / 2);

            _fader.SetAlpha(1 - curved);

            elapsed += Time.deltaTime;
            yield return null;
        }

        while (!IsInput())
            yield return null;
        //===========================================
        A_EventManager.InvokePlaySFX(CHANGE_SFX);
        _txt.text = "One day, the world was visited by envoys of the stars.";

        _glowFish.SetActive(true);

        // Lerp in glowfish
        elapsed = 0;
        while (elapsed < 2)
        {
            curved = A_Extensions.CosCurve(elapsed / 2);

            _glowFish.transform.localPosition = Vector3.Lerp(new Vector3(-GFRange, -GFRange), Vector3.zero, curved);

            elapsed += Time.deltaTime;
            yield return null;
        }

        while (!IsInput())
            yield return null;
        //===========================================
        A_EventManager.InvokePlaySFX("Detrans");
        _txt.text = "Seeing the endless growth, they yearned for balance and order.";

        // Lerp out glowfish
        elapsed = 0;
        while (elapsed < 2)
        {
            curved = A_Extensions.CosCurve(elapsed / 2);
            _glowFish.transform.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(GFRange, GFRange), curved);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _pestParent.SetActive(true);

        // Grow all pests, flip them back and forth randomly
        
        elapsed = 0;
        while (elapsed < 1.5f)
        {
            curved = A_Extensions.CosCurve(elapsed / 1.5f);

            _color.color = Color.Lerp(_cA, _cB, elapsed);
            
            foreach (Pest pest in _pests)
            {
                pest.go.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, curved);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        A_EventManager.InvokePlaySFX("EnemyDie");

        elapsed = 0;
        while (!IsInput())
        {
            if (elapsed > 1)
            {
                foreach (Pest pest in _pests)
                {
                    pest.go.transform.rotation = Quaternion.Euler(0, pest.go.transform.rotation.eulerAngles.y + 180, 0);
                }
                elapsed = 0;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        //===========================================
        A_EventManager.InvokePlaySFX(CHANGE_SFX);
        _txt.text = "And so they sent their spawn to cull the Gardens.";


        Dictionary<GameObject, Vector2> _startPos = new Dictionary<GameObject, Vector2>();
        foreach (Pest pest in _pests)
        {
            _startPos.Add(pest.go, pest.go.transform.localPosition);
        }

        A_EventManager.InvokePlaySFX("Worm");

        // Move pests into hedges
        elapsed = 0;
        while (elapsed < 1)
        {
            curved = A_Extensions.CosCurve(elapsed / 1);

            foreach (Pest pest in _pests)
            {
                pest.go.transform.localPosition = Vector2.Lerp(_startPos[pest.go], pest.position, curved);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        while (!IsInput())
            yield return null;
        //===========================================
        A_EventManager.InvokePlaySFX(CHANGE_SFX);
        _txt.text = "But the Gardens refused to simply die away.";

        // Zoom into main hedge
        elapsed = 0;
        while (elapsed < 2)
        {
            curved = A_Extensions.CosCurve(elapsed / 2);

            _color.color = Color.Lerp(_cB, _cC, elapsed);

            _scene.transform.localScale = Vector2.one * Mathf.Lerp(1, ZOOM_AMOUNT, curved);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Lightning
        _lightning.SetActive(true);

        A_EventManager.InvokePlaySFX("Dig");

        elapsed = 0;
        while (elapsed < 0.1f)
        {
            curved = A_Extensions.CosCurve(elapsed / 0.3f);

            _lightning.transform.localPosition = Vector3.Lerp(new Vector3(125, 283), new Vector3(45, 133), curved);

            elapsed += Time.deltaTime;
            yield return null;
        }

        A_EventManager.InvokePlaySFX("Portal");

        // Shake lightning
        elapsed = 0;
        while (elapsed < 0.5f)
        {
            _lightning.transform.localPosition = new Vector3(45, 133) + new Vector3(Random.Range(-3, 3), Random.Range(-3, 3));

            elapsed += Time.deltaTime;
            yield return null;
        }

        _lightning.SetActive(false);

        // Shake hedge
        while (!IsInput())
        {
            _mainHedge.transform.localPosition = new Vector3(Random.Range(-3, 3), Random.Range(-3, 3));

            elapsed += Time.deltaTime;
            yield return null;
        }
        //===========================================
        A_EventManager.InvokePlaySFX(CHANGE_SFX);
        _txt.text = "The plants were granted mind, will, and a purpose.";

        // Grow Hedge revealing Thicket
        A_EventManager.InvokePlaySFX("Crack");

        elapsed = 0;
        while (elapsed < 1f)
        {
            curved = A_Extensions.CosCurve(elapsed / 1f);

            _mainHedge.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(15, 15), curved);

            foreach (Image rend in _hedgeRendParts)
            {
                rend.SetAlpha(1 - curved);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        while (!IsInput())
            yield return null;
        //===========================================
        A_EventManager.InvokePlaySFX(CHANGE_SFX);
        _txt.text = "To fight, and to protect their home.";

        _eyebrows.SetActive(true);

        A_EventManager.InvokePlaySFX("Blank");

        yield return new WaitForSecondsRealtime(2f);

        // Fade to black
        while (elapsed < 4f)
        {
            curved = A_Extensions.CosCurve(elapsed / 4);

            _fader.SetAlpha(curved);

            elapsed += Time.deltaTime;
            yield return null;
        }

        LoadMainMenu();

        yield break;
    }

    void LoadMainMenu()
    {
        A_EventManager.InvokeLoadWorld(-1);
    }
}
