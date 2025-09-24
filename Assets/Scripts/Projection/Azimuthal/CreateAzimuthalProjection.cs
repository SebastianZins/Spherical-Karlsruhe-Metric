using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreateAzimuthalProjection : MonoBehaviour
{
    [Header("Projection Settings")]
    public GameObject sphere;

    private Material _projectionMaterial;
    public Shader voronoiShader;

    private EMetricType _metricType = EMetricType.Spherical;
    private bool _useClosestDistance = true;
    private bool _showCoordGrid = true;
    public bool isNorthCenter = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _projectionMaterial = GetComponent<Renderer>().material;
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

    public void UpdateGrowAnimation(float maxDistancePercent)
    {
        _projectionMaterial.SetFloat("_MaxDistancePercentage", maxDistancePercent);
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
                .AddComponent<AzimuthalReferencePoint>()
                .InitializePoint(spherePoint, transform, projectionScale, sphereGenerator, isNorthCenter);
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

    public void ResizePoints(float refPointRadius)
    {
        foreach (Transform point in GetReferencePoints())
        {
            point.GetComponent<AzimuthalReferencePoint>().SetPointRadius(refPointRadius);
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

    public List<AzimuthalReferencePoint> GetReferencePointsHandlers()
    {
        List<AzimuthalReferencePoint> spherePoints = GetReferencePoints()
            .Select(point => point.GetComponent<AzimuthalReferencePoint>())
            .ToList();
        return spherePoints;
    }

    public void SetPointPositionShaderData()
    {
        List<AzimuthalReferencePoint> pointHandlers = GetReferencePoints()
            .Select((point) => point.GetComponent<AzimuthalReferencePoint>())
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
        List<AzimuthalReferencePoint> pointHandlers = GetReferencePoints()
            .Select((point) => point.GetComponent<AzimuthalReferencePoint>())
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
        _projectionMaterial.SetFloat("_IsNorthCenter", isNorthCenter ? 1f : 0f);
        UpdateGrowAnimation(0f);
    }
}
