using System.Collections.Generic;
using UnityEngine;

public class LineGenerator : MechanismBase
{
    public Material dashedMat;
    public float lineWidth = 0.5f;
    public float repeatCount = 5f;
    public Color inactiveColor = Color.white;
    public Color activeColor = Color.green;

    private LineRenderer lineRenderer;
    private List<Transform> points = new List<Transform>();

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        if (dashedMat != null)
        {
            lineRenderer.material = dashedMat;
        }
    }

    void Start()
    {
        foreach (Transform child in transform)
        {
            points.Add(child);
        }
        lineRenderer.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i].position);
        }
        SetRepeatCount(repeatCount);
        SetLineColor(inactiveColor);
    }

    private void SetRepeatCount(float repCount)
    {
        Renderer rend = lineRenderer;
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        rend.GetPropertyBlock(mpb);
        mpb.SetFloat("_Rep", repCount);
        rend.SetPropertyBlock(mpb);
    }

    private void SetLineColor(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        if (lineRenderer.material != null)
        {
            lineRenderer.material.color = color;
        }
    }

    public override void TriggerActivate()
    {
        SetLineColor(activeColor);
    }

    public override void TriggerDeactivate()
    {
        SetLineColor(inactiveColor);
    }
}
