using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class O_SolStatue : MonoBehaviour
{
    bool _broken;
    [SerializeField] SpriteRenderer _solRend, _rend;
    [SerializeField] Animator _solAnim;
    [SerializeField] GameObject _eV, _eD, _eG, _eM;
    [SerializeField] SpriteRenderer _fV, _fD, _fG, _fM;
    [SerializeField] ParticleSystem _ps;
    [SerializeField] Material _baseMat, _selMat;

    private void Start()
    {
        if (A_SaveMetaManager.SolanaUnlocked)
            Destroy(transform.parent.gameObject);

        _fV.enabled = (A_SaveMetaManager.instance.EmblemsPlaced.Contains(0));
        _fD.enabled = (A_SaveMetaManager.instance.EmblemsPlaced.Contains(1));
        _fG.enabled = (A_SaveMetaManager.instance.EmblemsPlaced.Contains(2));
        _fM.enabled = (A_SaveMetaManager.instance.EmblemsPlaced.Contains(3));        
    }

    public void Update()
    {       
        _solRend.sortingOrder = _rend.sortingOrder;
        _fV.sortingOrder = _rend.sortingOrder + 1;
        _fD.sortingOrder = _rend.sortingOrder + 1;
        _fM.sortingOrder = _rend.sortingOrder + 1;
        _fG.sortingOrder = _rend.sortingOrder + 1;

        Vector2 target = Player.Selected == gameObject ? Vector2.one * 1.1f : Vector2.one;
        transform.parent.localScale = Vector2.Lerp(transform.parent.localScale, target, Time.deltaTime * 5);
        _rend.material = Player.Selected == gameObject ? _selMat : _baseMat;

        if (Player.Selected != gameObject || _broken)
            return;        

        if (!Input.GetKeyDown(A_InputManager.Instance.Key("Interact")))
            return;

        var indexes = ItemManager.Instance.AllItemIndexes();

        bool readyForV = indexes.Contains(77) && !A_SaveMetaManager.instance.EmblemsPlaced.Contains(0);
        bool readyForD = indexes.Contains(74) && !A_SaveMetaManager.instance.EmblemsPlaced.Contains(1);
        bool readyForG = indexes.Contains(75) && !A_SaveMetaManager.instance.EmblemsPlaced.Contains(2);
        bool readyForM = indexes.Contains(76) && !A_SaveMetaManager.instance.EmblemsPlaced.Contains(3);

        bool hasAnEmblemInInv = readyForD || readyForV || readyForM || readyForG;

        if (!hasAnEmblemInInv)
        {
            V_HUDManager.Instance.AssignErrorHelpMessage("Missing emblems");
            return;
        }

        List<int> ids = new List<int>();
        if (readyForV)
            ids.Add(0);
        if (readyForD)
            ids.Add(1);
        if (readyForG)
            ids.Add(2);
        if (readyForM)
            ids.Add(3);

        StartCoroutine(C_GrabEmblems(ids));

        var list = ItemManager.Instance.AllItems;
        for (int i = 0; i < list.Count; i++)
            if (list[i].Name == "Dermal Emblem" && readyForD)
            {
                list.RemoveAt(i);
                break;
            }
        for (int i = 0; i < list.Count; i++)
            if (list[i].Name == "Vascular Emblem" && readyForV)
            {
                list.RemoveAt(i);
                break;
            }
        for (int i = 0; i < list.Count; i++)
            if (list[i].Name == "Ground Emblem" && readyForG)
            {
                list.RemoveAt(i);
                break;
            }
        for (int i = 0; i < list.Count; i++)
            if (list[i].Name == "Meristematic Emblem" && readyForM)
            {
                list.RemoveAt(i);
                break;
            }
        ItemManager.Instance.AllItems = list;
    }

    void BreakOpen()
    {       
        Player.Selected = null;
        _broken = true;
        A_SaveMetaManager.SolanaUnlocked = true;

        _solAnim.SetTrigger("Break");
        _ps.Play();
        A_EventManager.InvokeUnlock("Solana");
    }

    IEnumerator C_GrabEmblems(List<int> emblemIDs)
    {
        bool VascularPlace = emblemIDs.Contains(0);
        bool DermalPlace = emblemIDs.Contains(1);
        bool GroundPlace = emblemIDs.Contains(2);
        bool MeriPlace = emblemIDs.Contains(3);        

        float e = 0;
        float d = 1f;
        Vector2 start = Player.Instance.transform.position;
        Vector2 endV = transform.position + new Vector3(-0.22f, 0.22f);
        Vector2 endD = transform.position + new Vector3(0.22f, 0.22f);
        Vector2 endM = transform.position + new Vector3(-0.22f, -0.11f);
        Vector2 endG = transform.position + new Vector3(0.22f, -0.11f);

        float startScale = 0.5f;
        float endScale = 0.3f;

        while (e < d)
        {
            float c = A_Extensions.CosCurve(e / d);

            if (VascularPlace)
                _eV.transform.position = Vector2.Lerp(start, endV, c);
            if (DermalPlace)
                _eD.transform.position = Vector2.Lerp(start, endD, c);
            if (MeriPlace)
                _eM.transform.position = Vector2.Lerp(start, endM, c);
            if (GroundPlace)
                _eG.transform.position = Vector2.Lerp(start, endG, c);

            _eV.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, c);
            _eD.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, c);
            _eM.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, c);
            _eG.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, c);

            e += Time.deltaTime;
            yield return null;
        }

        Destroy(_eV);
        Destroy(_eD);
        Destroy(_eM);
        Destroy(_eG);
        A_EventManager.InvokePlaySFX("1UP");

        if (VascularPlace) _fV.enabled = true;
        if (DermalPlace) _fD.enabled = true;
        if (MeriPlace) _fM.enabled = true;
        if (GroundPlace) _fG.enabled = true;

        A_SaveMetaManager.instance.SaveEmblemPlaced(emblemIDs);

        bool allPlaced = A_SaveMetaManager.instance.EmblemsPlaced.Contains(0)
            && A_SaveMetaManager.instance.EmblemsPlaced.Contains(1)
            && A_SaveMetaManager.instance.EmblemsPlaced.Contains(2)
            && A_SaveMetaManager.instance.EmblemsPlaced.Contains(3);

        if (allPlaced)
            BreakOpen();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (_broken || Player.Selected != null)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        Player.Selected = gameObject;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (Player.Selected == gameObject && collision.gameObject.tag == "Player")
            Player.Selected = null;
    }
}
