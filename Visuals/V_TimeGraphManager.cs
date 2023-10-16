using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class V_TimeGraphManager : MonoBehaviour
{
    [SerializeField] LineRenderer _line;
    [SerializeField] Transform _bounds;
    [SerializeField] GameObject _NightBG;
    [SerializeField] GameObject _item;
    [SerializeField] bool _blockyGraph;
    [SerializeField] GameObject _offScreen;

    void OnEnable() => A_EventManager.OnDisplayTimeGraph += SetGraphData;
    void OnDisable() => A_EventManager.OnDisplayTimeGraph -= SetGraphData;  

    public void SetGraphData(TimeData data)
    {
        if (data.GoldPos.Count <= 0)
        {
            _offScreen.SetActive(true);
            return;
        }

        Vector2 pointMax = new Vector2(data.TimeSpent, data.GoldPeak);
        Vector2 pointMin = new Vector2(0, data.GoldValley);

        if (pointMax.y == pointMin.y)
            pointMax.y += 1;

        List<Vector3> translatedPos = new List<Vector3>();
        foreach(Vector3 pos in data.GoldPos)
        {
            Vector2 percentagePos = A_Extensions.InverseLerp(pos, pointMax, pointMin);
            percentagePos.x = percentagePos.x.RemoveNaN();
            percentagePos.y = percentagePos.y.RemoveNaN();

            Vector2 newPos = A_Extensions.LerpAxis(GetMaxBounds(), GetMinBounds(), percentagePos);

            if (translatedPos.Count > 0 && _blockyGraph)
                translatedPos.Add(new Vector3(newPos.x, translatedPos[translatedPos.Count - 1].y));

            translatedPos.Add(newPos);

            if (data.ItemPos.Count > 0 && pos.x >= data.ItemPos[0])
            {
                MakeItem(newPos, data.ItemSprites[0]);
                data.ItemPos.RemoveAt(0);
                data.ItemSprites.RemoveAt(0);
            }
        }

        float finalY = translatedPos[translatedPos.Count - 1].y;
        translatedPos.Add(new Vector2(GetMaxBounds().x, finalY));

        _line.positionCount = translatedPos.Count;
        _line.SetPositions(translatedPos.ToArray());

        float nightPerc = data.DayTime / data.TimeSpent;
        float x = _NightBG.transform.localScale.x * nightPerc;
        _NightBG.transform.localPosition = new Vector3(x, 0, 0);

        _line.Simplify(0.04f);
    }

    void MakeItem(Vector2 pos, Sprite sprite)
    {
        GameObject go = Instantiate(_item, pos, Quaternion.identity);
        Transform icon = go.transform.GetChild(0);
        icon.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    Vector2 GetMaxBounds() => transform.position + _bounds.transform.localScale / 2;
    Vector2 GetMinBounds() => transform.position - _bounds.transform.localScale / 2;
}

[System.Serializable]
public struct TimeData
{
    public List<Vector3> GoldPos;
    public List<float> ItemPos;
    public List<Sprite> ItemSprites;

    public float TimeSpent;
    public float DayTime;
    public float GoldPeak;
    public float GoldValley;
}
