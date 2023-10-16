using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class V_CameraManager : MonoBehaviour
{
    [SerializeField] Camera _cam;
    bool _shaking { get { return _curMag != 0; } }
    float _curMag;

    EBoss _boss;
    bool _break;
    bool _toad;

    PixelPerfectCamera _filter;

    [SerializeField] List<V_FocalPoint> _focalPoints = new List<V_FocalPoint>();
    private void OnEnable()
    {
        A_EventManager.OnCameraShake += ScreenShake;
        A_EventManager.OnBossSpawn += ReassignBoss;
        A_EventManager.OnAddFocalPoint += AddFocalPoint;
    }
    private void OnDisable()
    {
        A_EventManager.OnCameraShake -= ScreenShake;
        A_EventManager.OnBossSpawn -= ReassignBoss;
        A_EventManager.OnAddFocalPoint -= AddFocalPoint;
    }

    void ReassignBoss(EBoss boss)
    {
        _boss = boss;

        if (boss.EnemyName == "Wallum Toad")
            _toad = true;
    }

    private void Start()
    {
        SetAspect();

        _filter = GetComponent<PixelPerfectCamera>();

        if (A_LevelManager.Instance == null)
            return;

        _break = A_LevelManager.Instance.CurrentLevel.IsEven() && A_LevelManager.Instance.CurrentLevel != 24;
    }

    private void Update()
    {
        SetAspect();

        if (A_OptionsManager.Instance == null || _filter == null)
            return;

        if (A_LevelManager.Instance.CurrentLevel == 24 && A_LevelManager.Instance.SceneTime < 0.5f)
            return;

        _filter.enabled = A_OptionsManager.Instance.Current.PixelFilter;
        if (!_filter.enabled)
            _cam.orthographicSize = 4.5f;
    }

    private void LateUpdate()
    {
        if (_shaking || Player.Instance == null || (V_HUDManager.Instance != null && V_HUDManager.Instance.IsPaused) || Player.Instance.InCutscene)
            return;

        transform.position = GetPosition();
    }

    float _camSpeed = 0;

    Vector3 GetPosition()
    {
        if (!ReferenceEquals(_boss, null) && _boss != null && (_boss.EnemyName == "Concheror" || _boss.EnemyName == "Colossix"))
        {
            Vector3 target = _boss.transform.position + new Vector3(7, 0);
            target.z = -10;
            return Vector3.Lerp(transform.position, target, Time.deltaTime * 10);
        }

        List<Vector3> points = new List<Vector3>();
        for (int i = _focalPoints.Count - 1; i >= 0; i--)
        {
            if (_focalPoints[i] == null)
            {
                _focalPoints.RemoveAt(i);
                continue;
            }

            if (!A_OptionsManager.Instance.Current.CamLock)
                continue;

            if (_focalPoints[i].Bias < 1)
                continue;

            if (_focalPoints[i].Bias.Is(1, 2) && Vector3.Distance(Player.Instance.transform.position, _focalPoints[i].transform.position) > 3)
                continue;

            if (_focalPoints[i].Bias.Is(3, 4) && Vector3.Distance(Player.Instance.transform.position, _focalPoints[i].transform.position) > 5)
                continue;

            if (Vector3.Distance(Player.Instance.transform.position, _focalPoints[i].transform.position) < 2)
                points.Add(_focalPoints[i].transform.position);

            for (int j = 0; j < _focalPoints[i].Bias; j++)
                points.Add(_focalPoints[i].transform.position);
        }

        Vector3 average = AverageOfPoints(points);
        Vector3 dir = Vector3.zero;

        if (average.z != 999)
        {
            dir = average - Player.Instance.transform.position;
        }

        float angle = Vector3.Angle(dir.normalized, GetDir().normalized);
        if (angle > 110)
            dir = Vector3.zero;

        Vector3 pos = Player.Instance.transform.position + dir * 0.3f;
        pos.z = -10;

        if (_break)
        {
            pos.x = Mathf.Clamp(pos.x, -1, 1);
            pos.y = Mathf.Clamp(pos.y, -1.5f, 5);
        }

        if (_toad)
        {
            pos.x = Mathf.Clamp(pos.x, -4, 4);
            pos.y = Mathf.Clamp(pos.y, -4, 4);
        }

        float targetSpeed = points.Count > 0 ? 2.5f : 3.5f;

        if (targetSpeed < _camSpeed)
            _camSpeed = targetSpeed;

        _camSpeed = Mathf.Lerp(_camSpeed, targetSpeed, Time.deltaTime * 0.55f);

        if (Player.Instance.Teleporting || !A_OptionsManager.Instance.Current.CamLock)
            _camSpeed = 8;
        
        return Vector3.Lerp(transform.position, pos, Time.deltaTime * _camSpeed);
    }

    Vector3 AverageOfPoints(List<Vector3> points)
    {
        if (points.Count == 0)
            return new Vector3(float.NaN, float.NaN, 999);

        Vector3 average = Vector3.zero;
        foreach (Vector3 point in points)
        {
            average += point;
        }
        return average / points.Count;
    }

    void AddFocalPoint(V_FocalPoint point)
    {
        if (_focalPoints.Count >= 15)
            return;

        _focalPoints.Add(point);
    }

    public void ScreenShake(float mag, float dur)
    {
        if (mag <= _curMag || !A_OptionsManager.Instance.Current.Screenshake)
            return;

        StopAllCoroutines();
        StartCoroutine(C_ScreenShake(mag, dur));
    }

    IEnumerator C_ScreenShake(float mag, float dur)
    {
        _curMag = mag;

        float elapsed = 0;

        float seed = Random.Range(0, 1000f);

        while (elapsed < dur)
        {
            if (V_HUDManager.Instance == null || V_HUDManager.Instance.IsPaused || Player.Instance == null)
                break;

            Vector3 pos = GetPosition();

            pos.x += Mathf.PerlinNoise(seed, elapsed*4) * mag;
            pos.y += Mathf.PerlinNoise(seed + 1, elapsed*4 + 1) * mag;
            pos.z = -10;

            float rotation = Mathf.PerlinNoise(seed + 2, elapsed * 4 + 2) * mag;

            transform.position = pos;

            transform.rotation = Quaternion.Euler(0, 0, rotation);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        _curMag = 0;
    }

    void SetAspect()
    {
        float targetaspect = 16.0f / 9.0f;

        float windowaspect = (float)Screen.width / Screen.height;

        float scaleheight = windowaspect / targetaspect;

        // if scaled height is less than current height, add letterbox
        if (scaleheight < 1.0f)
        {
            Rect rect = _cam.rect;

            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;

            _cam.rect = rect;
        }
        else // add pillarbox
        {
            float scalewidth = 1.0f / scaleheight;

            Rect rect = _cam.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            _cam.rect = rect;
        }
    }

    Vector2 GetDir()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
        return (worldPosition - (Vector2)Player.Instance.transform.position).normalized;

    }
}
