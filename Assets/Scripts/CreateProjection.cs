using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreateProjection : MonoBehaviour
{
    [Header("Projection Settings")]
    public GameObject sphere;

    private int _pointCount = 0;
    private float _refPointRadius = 0;

    private float _scaleWidth = 10f;
    private float _scaleHeight = 5f;

    private Material projectionMaterial;

    private EMetricType _metricType = EMetricType.Spherical;
    private bool _useClosestDistance = true;
    private bool _showCoordGrid = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        projectionMaterial = GetComponent<Renderer>().material;
        _scaleWidth = transform.lossyScale.x;
        _scaleHeight = transform.lossyScale.z;
    }

    // Update is called once per frame
    void Update()
    {
        int pointCount = sphere.GetComponent<SphereGenerator>().numOfPoints;
        if (_pointCount != pointCount)
        {
            _pointCount = pointCount;
            DeleteChildObjects();
            //GeneratePoints();
        }

        float refPointRadius = sphere.GetComponent<SphereGenerator>().refPointRadius;
        if (_refPointRadius != refPointRadius)
        {
            _refPointRadius = refPointRadius;
            ResizePoints();
        }

        float scaleWidth = transform.lossyScale.x;
        float scaleHeight = transform.lossyScale.z;
        if (scaleWidth != _scaleWidth || scaleHeight != _scaleHeight)
        {
            _scaleHeight = scaleHeight;
            _scaleWidth = scaleWidth;
            RepositionPoints();
        }

        EMetricType metricType = sphere.GetComponent<SphereGenerator>().metricType;
        bool useClosestDistance = sphere.GetComponent<SphereGenerator>().useClosestDistance;
        bool showCoordGrid = sphere.GetComponent<SphereGenerator>().showCoordGrid;

        if (
            useClosestDistance != _useClosestDistance ||
            metricType != _metricType ||
            showCoordGrid != _showCoordGrid)
        {
            _useClosestDistance = useClosestDistance;
            _metricType = metricType;
            _showCoordGrid = showCoordGrid;
            SetShaderMetricProperties();
        }
    }

    void DeleteChildObjects()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }

    void ResizePoints()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.transform.localScale = new Vector3(
                _refPointRadius / transform.lossyScale.x * 0.8f,
                _refPointRadius / transform.lossyScale.y * 0.8f,
                _refPointRadius / transform.lossyScale.z * 0.8f
            );
        }
    }

    void RepositionPoints()
    {
        projectionMaterial.SetVector("_Scale", new Vector2(_scaleWidth, _scaleHeight));
    }

    private void GeneratePoints()
    {
        // TODO: 

        List<ReferencePointHandler> pointHandlers = sphere
            .GetComponent<SphereGenerator>()
            .GetReferencePoints()
            .Select((point) => point.GetComponent<ReferencePointHandler>())
            .ToList();

        List<Vector4> pointPositionsSpherical = pointHandlers.Select((p) => new Vector4(p.radius, p.sphericalPosition.x, p.sphericalPosition.y, 0)).ToList();
        List<Color> colors = pointHandlers.Select((p) => p.color).ToList();

        _pointCount = pointPositionsSpherical.Count;

        for (int i = 0; i < pointPositionsSpherical.Count; i++)
        {
            Vector3 localMercatorPos = ToMercatorProjection(pointPositionsSpherical[i]);
            Vector3 mercatorPos = transform.TransformPoint(localMercatorPos);

            //p/ointObject.transform.localScale = new Vector3(refPointRadius / 10, refPointRadius / 10, (_scaleWidth / _scaleHeight) * refPointRadius / 10);

            GameObject pointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointObject.transform.parent = transform;
            pointObject.transform.position = mercatorPos;
            pointObject.transform.localScale = new Vector3(
                _refPointRadius / transform.lossyScale.x * 0.8f,
                _refPointRadius / transform.lossyScale.y * 0.8f,
                _refPointRadius / transform.lossyScale.z * 0.8f
            );
            pointObject.GetComponent<Renderer>().material.color = colors[i];
            pointObject.name = $"Reference Point [{i}] <{pointPositionsSpherical[i].y}, {pointPositionsSpherical[i].z}>";
        }
        float radius = sphere.GetComponent<SphereGenerator>().refPointRadius;

        if (_pointCount != 0)
        {
            ComputeBuffer sphericalCoordsBuffer = new ComputeBuffer(pointPositionsSpherical.Count, sizeof(float) * 4);
            ComputeBuffer colorsBuffer = new ComputeBuffer(pointPositionsSpherical.Count, sizeof(float) * 4);
            sphericalCoordsBuffer.SetData(pointPositionsSpherical);
            colorsBuffer.SetData(colors);

            projectionMaterial.SetInt("_PointCount", pointPositionsSpherical.Count);
            projectionMaterial.SetBuffer("_PointSphericalCoords", sphericalCoordsBuffer);
            projectionMaterial.SetBuffer("_Colors", colorsBuffer);
            projectionMaterial.SetFloat("_Radius", radius);
            projectionMaterial.SetVector("_Scale", new Vector2(_scaleWidth, _scaleHeight));
            SetShaderMetricProperties();
        }
    }

    private Vector3 ToMercatorProjection(Vector4 sphercialPos)
    {
        float theta = sphercialPos.y;
        float phi = sphercialPos.z;

        float latitude = Mathf.PI / 2f - phi;
        float x = theta / (2f * Mathf.PI);
        float y = Mathf.Log(Mathf.Tan(Mathf.PI / 4f + latitude / 2f));
        // Scale to fit plane
        x *= _scaleWidth * 0.5f;
        x -= _scaleWidth * 0.25f;
        // Clamp at north / southpole to avoid exponantially exploding tan behaviour at -90deg / 90deg
        y = Mathf.Clamp(y, -_scaleHeight * 0.445f, _scaleHeight * 0.445f);

        Vector3 mercatorPos = new Vector3(x, 0f, y);
        return mercatorPos;
    }

    private void SetShaderMetricProperties()
    {
        projectionMaterial.SetFloat("_ClosestDistance", _useClosestDistance ? 1f : 0f);
        projectionMaterial.SetFloat("_MetricType", (float)_metricType);
        projectionMaterial.SetFloat("_ShowGrid", _showCoordGrid ? 1f : 0f);
    }
}
