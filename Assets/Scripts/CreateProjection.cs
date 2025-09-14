using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CreateProjection : MonoBehaviour
{
    [Header("Projection Settings")]
    public GameObject sphere;

    private Material _projectionMaterial;
    public Shader voronoiShader;

    private EMetricType _metricType = EMetricType.Spherical;
    private bool _useClosestDistance = true;
    private bool _showCoordGrid = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _projectionMaterial = GetComponent<Renderer>().material;
    }

    public void UpdatePointPosition()
    {

    }

    public void UpdatePointColor()
    {

    }

    public void SetPointRadius(float radius)
    {
        ResizePoints(radius);
    }

    public void SetUseClosestDistance(bool useClosest)
    {
        _useClosestDistance = useClosest;
        SetShaderMetricProperties();
    }

    public void SetShowCoordGrid(bool showCoordGrid)
    {
        _showCoordGrid = showCoordGrid;
        SetShaderMetricProperties();
    }

    public void SetMetricType(EMetricType metricType)
    {
        _metricType = metricType;
        SetShaderMetricProperties();
    }

    // Update is called once per frame
    void Update()
    {
        //int pointCount = sphere.GetComponent<SphereGenerator>().numOfPoints;
        //if (_pointCount != pointCount)
        //{
        //    _pointCount = pointCount;
        //    DeleteChildObjects();
        //    //GeneratePoints();
        //}

        //float refPointRadius = sphere.GetComponent<SphereGenerator>().refPointRadius;
        //if (_refPointRadius != refPointRadius)
        //{
        //    _refPointRadius = refPointRadius;
        //    ResizePoints();
        //}

        //float scaleWidth = transform.lossyScale.x;
        //float scaleHeight = transform.lossyScale.z;
        //if (scaleWidth != _scaleWidth || scaleHeight != _scaleHeight)
        //{
        //    _scaleHeight = scaleHeight;
        //    _scaleWidth = scaleWidth;
        //    RepositionPoints();
        //}

        //EMetricType metricType = sphere.GetComponent<SphereGenerator>().metricType;
        //bool useClosestDistance = sphere.GetComponent<SphereGenerator>().useClosestDistance;
        //bool showCoordGrid = sphere.GetComponent<SphereGenerator>().showCoordGrid;

        //if (
        //    useClosestDistance != _useClosestDistance ||
        //    metricType != _metricType ||
        //    showCoordGrid != _showCoordGrid)
        //{
        //    _useClosestDistance = useClosestDistance;
        //    _metricType = metricType;
        //    _showCoordGrid = showCoordGrid;
        //    SetShaderMetricProperties();
        //}
    }

    public void InitializeProjection(float refPointRadius, float radius, List<ReferencePointHandler> spherePoints)
    {
        GeneratePoints(refPointRadius, radius, spherePoints);
    }

    public void UpdatePoints(float refPointRadius, float radius, List<ReferencePointHandler> spherePoints)
    {
        DeleteChildObjects();
        GeneratePoints(refPointRadius, radius, spherePoints);
    }

    public void UpdatePointPositions()
    {
        foreach (var point in GetReferencePointsHandlers())
        {
            point.UpdatePosition();
        }
        SetPointPositionShaderData();
    }

    private void GeneratePoints(float refPointRadius, float radius, List<ReferencePointHandler> spherePoints)
    {
        Renderer renderer = GetComponent<Renderer>();
        Destroy(renderer.material);

        Vector2 projectionScale = new Vector2(transform.lossyScale.x, transform.lossyScale.z);

        foreach (var spherePoint in spherePoints)
        {
            GameObject pointObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            SphereGenerator sphereGenerator = sphere.GetComponent<SphereGenerator>();
            pointObject
                .AddComponent<ProjectionReferencePoint>()
                .InitializePoint(spherePoint, transform, projectionScale, sphereGenerator);
        }

        if (spherePoints.Count > 0)
        {
            _projectionMaterial = new Material(voronoiShader);
            _projectionMaterial.SetInt("_PointCount", spherePoints.Count);
            _projectionMaterial.SetFloat("_Radius", radius);
            _projectionMaterial.SetVector("_Scale", projectionScale);
            SetPointPositionShaderData();
            SetPointColorShaderData();
            SetShaderMetricProperties();
            renderer.material = _projectionMaterial;
        }
    }

    void DeleteChildObjects()
    {
        foreach (var point in GetReferencePoints())
        {
            DestroyImmediate(point.gameObject);
        }
    }

    //void RepositionPoints()
    //{
    //    projectionMaterial.SetVector("_Scale", new Vector2(_scaleWidth, _scaleHeight));
    //}

    public void ResizePoints(float refPointRadius)
    {
        foreach (Transform point in GetReferencePoints())
        {
            point.GetComponent<ProjectionReferencePoint>().SetPointRadius(refPointRadius);
        }
    }

    public List<Transform> GetReferencePoints()
    {
        List<Transform> points = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.CompareTag(Constants.REFERENCE_POINT_TAG))
            {
                points.Add(child);
            }
        }
        return points;
    }

    public List<ProjectionReferencePoint> GetReferencePointsHandlers()
    {
        List<ProjectionReferencePoint> spherePoints = GetReferencePoints()
            .Select(point => point.GetComponent<ProjectionReferencePoint>())
            .ToList();
        return spherePoints;
    }

    public void SetPointPositionShaderData()
    {
        List<ProjectionReferencePoint> pointHandlers = GetReferencePoints()
            .Select((point) => point.GetComponent<ProjectionReferencePoint>())
            .ToList();
        ComputeBuffer sphericalCoordsBuffer = new ComputeBuffer(pointHandlers.Count, sizeof(float) * 4);
        sphericalCoordsBuffer
            .SetData(
                pointHandlers
                    .Select((p) => p.GetSphericalPosition4())
                    .ToList()
            );
        _projectionMaterial.SetBuffer("_PointSphericalCoords", sphericalCoordsBuffer);
    }

    private void SetPointColorShaderData()
    {
        List<ProjectionReferencePoint> pointHandlers = GetReferencePoints()
            .Select((point) => point.GetComponent<ProjectionReferencePoint>())
            .ToList();
        ComputeBuffer colorsBuffer = new ComputeBuffer(pointHandlers.Count, sizeof(float) * 4);
        colorsBuffer
            .SetData(
                pointHandlers
                    .Select((p) => p.GetColor())
                    .ToList()
            );
        _projectionMaterial.SetBuffer("_Colors", colorsBuffer);
    }

    private void SetShaderMetricProperties()
    {
        _projectionMaterial.SetFloat("_ClosestDistance", _useClosestDistance ? 1f : 0f);
        _projectionMaterial.SetFloat("_MetricType", (float)_metricType);
        _projectionMaterial.SetFloat("_ShowGrid", _showCoordGrid ? 1f : 0f);
    }
}
