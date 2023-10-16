using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class F_Rope : MonoBehaviour
{
    public Transform object1; 
    public Transform object2;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] int _slackPoints;
    [SerializeField] float _slackAmount;

    void Start()
    {
        lineRenderer.positionCount = _slackPoints;

        if (object2 == null)
            object2 = Player.Instance.transform;
    }

    void Update()
    {
        if (object1 != null && object2 != null)
        {
            Vector3[] ropePoints = new Vector3[lineRenderer.positionCount];
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                float t = i / (float)(lineRenderer.positionCount - 1);
                ropePoints[i] = Vector3.Lerp(object1.position, object2.position, t);
                ropePoints[i].y -= Mathf.Abs(Mathf.Sin(t * Mathf.PI)) * _slackAmount; 
            }

            lineRenderer.SetPositions(ropePoints);
        }
    }
}
