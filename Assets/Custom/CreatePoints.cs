using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SphereGenerator : MonoBehaviour
{
    public int numOfPoints = 0;
    private int _prevNumOfPoints = 0;

    public float refPointRadius = 0.1f;
    private float _prevRefPointRadius = 0.1f;

    private Material sphereMaterial;
    private Vector4[] pointPositions;
    private Vector4[] pointPositionsSpherical;

    private Color[] colors;

    private float radius;
    private int frame = 0;

    void Start()
    {
        sphereMaterial = GetComponent<Renderer>().material;
        radius = transform.localScale.x * 0.5f;


        GeneratePoints();
    }

    void Update()
    {
        if (numOfPoints != _prevNumOfPoints)
        {
            DeleteChildObjects();
            GeneratePoints();
            _prevNumOfPoints = numOfPoints;
        }
        if ( refPointRadius != _prevRefPointRadius) 
        {
            ResizePoints();
            _prevRefPointRadius = refPointRadius;
        }
    }

    void GeneratePoints()
    {
        pointPositions = new Vector4[numOfPoints];
        pointPositionsSpherical = new Vector4[numOfPoints];
        colors = new Color[numOfPoints];

        for (int i = 0; i < numOfPoints; i++)
        {
            float phi = Random.Range(0f, Mathf.PI);
            float theta = Random.Range(0f, Mathf.PI * 2f);

            Vector3 newPointSpherical = new Vector4(radius, theta, phi);
            pointPositionsSpherical[i] = newPointSpherical;

            Quaternion rotation = transform.localRotation;
            Vector3 localPoint = ConvertToEuclidean(newPointSpherical);
            Vector3 rotatedPoint = transform.rotation * localPoint;
            Vector3 newPoint = transform.position + rotatedPoint;
            pointPositions[i] = newPoint;

            Color newPointColor = new Color(Random.value, Random.value, Random.value, 1);
            colors[i] = newPointColor;

            GameObject pointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointObject.transform.parent = transform;
            pointObject.transform.position = newPoint;
            pointObject.transform.rotation = rotation;
            pointObject.transform.localScale = new Vector3(refPointRadius/100, refPointRadius / 100, refPointRadius / 100);
            pointObject.name = $"Reference Point [{i}]";
            pointObject.GetComponent<Renderer>().material.color = newPointColor;
        }

        if (pointPositions.Length > 0)
        {
            sphereMaterial.SetInt("_PointCount", pointPositions.Length);
            sphereMaterial.SetVectorArray("_PointSphericalCoords", pointPositionsSpherical);
            sphereMaterial.SetColorArray("_Colors", colors);
            sphereMaterial.SetFloat("_Radius", radius);
        }
    }

    void ResizePoints()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            // ignore north and sout pole pointer objects
            if (child.name != "NorthPole" && child.name != "SouthPole")
            {
                child.transform.localScale = new Vector3(refPointRadius / 100, refPointRadius / 100, refPointRadius / 100);
            }
        }
    }

    void DeleteChildObjects()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            // ignore north and sout pole pointer objects
            if (child.name != "NorthPole" && child.name != "SouthPole")
            {
                // Destroy each child object
                Destroy(child.gameObject);
            }
        }
    }

    //Vector4 ConvertToSpherical(Vector4 euclidPos)
    //{
    //    float r = euclidPos.magnitude;
    //    float theta = Mathf.Atan2(euclidPos.z, euclidPos.x);
    //    float phi = Mathf.Acos(euclidPos.y / r);

    //    return new Vector4(r, theta, phi, 0);
    //}

    Vector3 ConvertToEuclidean(Vector3 sphericalPos)
    {
        float x = sphericalPos.x * Mathf.Sin(sphericalPos.z) * Mathf.Cos(sphericalPos.y);
        float y = sphericalPos.x * Mathf.Sin(sphericalPos.z) * Mathf.Sin(sphericalPos.y);
        float z = sphericalPos.x * Mathf.Cos(sphericalPos.z);

        return new Vector3(x, y, z);
    }
}
