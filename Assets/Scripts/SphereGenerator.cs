using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SphereGenerator : MonoBehaviour
{
    public bool showCoordGrid = true;
    public int numOfPoints = 100;
    public float refPointRadius = 2f;
    [HideInInspector]
    public float radius;

    public EMetricType metricType = EMetricType.Spherical;
    public bool useClosestDistance = true;
    private bool _showPoles = true;

    private Material _sphereMaterial;
    public Shader voronoiShader;
    public GameObject mercatorProjection;
    private CreateProjection _mercatorProjectionData;


    void Start()
    {
        _sphereMaterial = GetComponent<Renderer>().material;
        radius = transform.localScale.x * 0.5f;

        GeneratePoints();

        List<ReferencePointHandler> spherePoints = GetReferencePointsHandlers();
        _mercatorProjectionData = mercatorProjection.GetComponent<CreateProjection>();
        _mercatorProjectionData.InitializeProjection(refPointRadius, radius, spherePoints);
    }

    public void SetPointCount(string countString)
    {
        if (int.TryParse(countString, out int count))
        {
            numOfPoints = count;
            DeleteChildObjects();
            GeneratePoints();

            List<ReferencePointHandler> spherePoints = GetReferencePointsHandlers();
            _mercatorProjectionData.UpdatePoints(refPointRadius, radius, spherePoints);
            Debug.Log("Number of points changed: " + count);
        }
        else
        {
            Debug.LogWarning("Invalid number input: " + countString);
        }
    }

    public void SetPointRadius(string radiusString)
    {
        if (float.TryParse(radiusString, out float radius))
        {
            refPointRadius = radius;
            ResizePoints();

            _mercatorProjectionData.SetPointRadius(radius);
            Debug.Log("Point radius changed: " + radius);
        }
        else
        {
            Debug.LogWarning("Invalid number input: " + radiusString);
        }
    }

    public void SetUseClosestDistance(bool useClosest)
    {
        useClosestDistance = useClosest;
        SetShaderMetricProperties();

        _mercatorProjectionData.SetUseClosestDistance(useClosest);
        Debug.Log("Use closest distance metric changed: " + useClosest);
    }

    public void SetShowCoordGrid(bool showCoordGrid)
    {
        this.showCoordGrid = showCoordGrid;
        SetShaderMetricProperties();

        _mercatorProjectionData.SetShowCoordGrid(showCoordGrid);
        Debug.Log("Show grid flag changed: " + showCoordGrid);
    }

    public void SetMetricType(int metricType)
    {
        this.metricType = (EMetricType)metricType;
        SetShaderMetricProperties();

        _mercatorProjectionData.SetMetricType(this.metricType);
        Debug.Log("Metric type changed: " + metricType);
    }

    public void SetshowPoles(bool showPoles)
    {
        _showPoles = showPoles;
        SwitchPolesVisibility();
        Debug.Log("Set show poles flag: " + showPoles);
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

        for (int i = 0; i < numOfPoints; i++)
        {
            GameObject pointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointObject.transform.parent = transform;
            pointObject.tag = Constants.REFERENCE_POINT_TAG;
            ReferencePointHandler pointHandler = pointObject.AddComponent<ReferencePointHandler>();
            pointHandler.InitializePoint(radius, refPointRadius);

            pointObject.name = $"Reference Point [{pointHandler.sphericalPosition.x}, {pointHandler.sphericalPosition.y}]";
        }

        if (numOfPoints > 0)
        {
            _sphereMaterial = new Material(voronoiShader);
            _sphereMaterial.SetInt("_PointCount", numOfPoints);
            _sphereMaterial.SetFloat("_Radius", radius);
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
            point.GetComponent<ReferencePointHandler>().SetPointRadius(refPointRadius);
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
        _sphereMaterial.SetFloat("_ClosestDistance", useClosestDistance ? 1f : 0f);
        _sphereMaterial.SetFloat("_MetricType", (float)metricType);
        _sphereMaterial.SetFloat("_ShowGrid", showCoordGrid ? 1f : 0f);
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
