using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SphereGenerator : MonoBehaviour
{
    private bool _showCoordGrid = true;
    private int _numOfPoints = 100;
    private float _refPointRadius = 2f;
    private float _radius;

    public EMetricType _metricType = EMetricType.Spherical;
    private bool _useClosestDistance = true;
    private bool _showPoles = true;

    public Shader voronoiShader;
    private Material _sphereMaterial;

    public GameObject mercatorProjection;
    private CreateProjection _mercatorProjectionData;

    [HideInInspector]
    public bool showCoordGrid
    {
        get { return _showCoordGrid; }
        set { SetShowCoordGrid(value); }
    }
    [HideInInspector]
    public int numOfPoints
    {
        get { return _numOfPoints; }
        set { SetPointCount(value); }
    }
    [HideInInspector]
    public float refPointRadius
    {
        get { return _refPointRadius; }
        set { SetPointRadius(value); }
    }

    [HideInInspector]
    public EMetricType metricType
    {
        get { return _metricType; }
        set { SetMetricType(value); }
    }
    [HideInInspector]
    public bool useClosestDistance
    {
        get { return _useClosestDistance; }
        set { SetUseClosestDistance(value); }
    }
    [HideInInspector]
    public bool showPoles
    {
        get { return _showPoles; }
        set { SetShowPoles(value); }
    }


    void Start()
    {
        _sphereMaterial = GetComponent<Renderer>().material;
        _radius = transform.localScale.x * 0.5f;

        GeneratePoints();

        List<ReferencePointHandler> spherePoints = GetReferencePointsHandlers();
        _mercatorProjectionData = mercatorProjection.GetComponent<CreateProjection>();
        _mercatorProjectionData.InitializeProjection(_refPointRadius, _radius, spherePoints);
    }

    public void SetPointCount(int countString)
    {
        _numOfPoints = countString;
        DeleteChildObjects();
        GeneratePoints();

        List<ReferencePointHandler> spherePoints = GetReferencePointsHandlers();
        _mercatorProjectionData.UpdatePoints(_refPointRadius, _radius, spherePoints);
    }

    public void SetPointRadius(float radiusString)
    {
        _refPointRadius = _radius;
        ResizePoints();

        _mercatorProjectionData.SetPointRadius(_radius);
    }

    public void SetUseClosestDistance(bool useClosest)
    {
        _useClosestDistance = useClosest;
        SetShaderMetricProperties();

        _mercatorProjectionData.SetUseClosestDistance(useClosest);
    }

    public void SetShowCoordGrid(bool showCoordGrid)
    {
        _showCoordGrid = showCoordGrid;
        SetShaderMetricProperties();

        _mercatorProjectionData.SetShowCoordGrid(showCoordGrid);
    }

    public void SetMetricType(EMetricType metricType)
    {
        _metricType = metricType;
        SetShaderMetricProperties();

        _mercatorProjectionData.SetMetricType(_metricType);
    }

    public void SetShowPoles(bool showPoles)
    {
        _showPoles = showPoles;
        SwitchPolesVisibility();
    }

    public void UpdateGrowAnimation(float maxDistancePercent)
    {
        _sphereMaterial.SetFloat("_MaxDistancePercentage", maxDistancePercent);
    }

    public void UpdatePointPositionSpherical()
    {
        SetPointPositionShaderData();

        List<ReferencePointHandler> spherePoints = GetReferencePointsHandlers();
        _mercatorProjectionData.UpdatePointPositions();
    }

    public void UpdatePointColor()
    {
        SetPointColorShaderData();
    }

    private void GeneratePoints()
    {
        Renderer renderer = GetComponent<Renderer>();
        Destroy(renderer.material);

        for (int i = 0; i < _numOfPoints; i++)
        {
            GameObject pointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointObject.transform.parent = transform;
            pointObject.tag = Constants.REFERENCE_POINT_TAG;
            ReferencePointHandler pointHandler = pointObject.AddComponent<ReferencePointHandler>();
            pointHandler.InitializePoint(_radius, _refPointRadius);

            pointObject.name = $"Reference Point [{pointHandler.sphericalPosition.x}, {pointHandler.sphericalPosition.y}]";
        }

        if (_numOfPoints > 0)
        {
            _sphereMaterial = new Material(voronoiShader);
            _sphereMaterial.SetInt("_PointCount", _numOfPoints);
            _sphereMaterial.SetFloat("_Radius", _radius);
            SetPointPositionShaderData();
            SetPointColorShaderData();
            SetShaderMetricProperties();
            renderer.material = _sphereMaterial;
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

    public List<ReferencePointHandler> GetReferencePointsHandlers()
    {
        List<ReferencePointHandler> spherePoints = GetReferencePoints()
            .Select(point => point.GetComponent<ReferencePointHandler>())
            .ToList();
        return spherePoints;
    }

    private void ResizePoints()
    {
        foreach (Transform point in GetReferencePoints())
        {
            point.GetComponent<ReferencePointHandler>().SetPointRadius(_refPointRadius);
        }
    }

    private void DeleteChildObjects()
    {
        foreach (Transform point in GetReferencePoints())
        {
            DestroyImmediate(point.gameObject);
        }
    }

    private void SetPointPositionShaderData()
    {
        List<ReferencePointHandler> pointHandlers = GetReferencePoints()
            .Select((point) => point.GetComponent<ReferencePointHandler>())
            .ToList();
        ComputeBuffer sphericalCoordsBuffer = new ComputeBuffer(pointHandlers.Count, sizeof(float) * 4);
        sphericalCoordsBuffer
            .SetData(
                pointHandlers
                    .Select((p) => new Vector4(p.radius, p.sphericalPosition.x, p.sphericalPosition.y, 0))
                    .ToList()
            );
        _sphereMaterial.SetBuffer("_PointSphericalCoords", sphericalCoordsBuffer);
    }

    private void SetPointColorShaderData()
    {
        List<ReferencePointHandler> pointHandlers = GetReferencePoints()
            .Select((point) => point.GetComponent<ReferencePointHandler>())
            .ToList();
        ComputeBuffer colorsBuffer = new ComputeBuffer(pointHandlers.Count, sizeof(float) * 4);
        colorsBuffer
            .SetData(
                pointHandlers
                    .Select((p) => p.color)
                    .ToList()
            );
        _sphereMaterial.SetBuffer("_Colors", colorsBuffer);
    }

    private void SetShaderMetricProperties()
    {
        _sphereMaterial.SetFloat("_ClosestDistance", _useClosestDistance ? 1f : 0f);
        _sphereMaterial.SetFloat("_MetricType", (float)_metricType);
        _sphereMaterial.SetFloat("_ShowGrid", _showCoordGrid ? 1f : 0f);
        UpdateGrowAnimation(0f);
    }

    private void SwitchPolesVisibility()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.CompareTag(Constants.POLE_TAG))
            {
                child.gameObject.SetActive(_showPoles);
            }
        }
    }
}
