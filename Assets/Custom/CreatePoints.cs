using UnityEngine;

public class SphereGenerator : MonoBehaviour
{
//    public int numRefPoints = 4;
//    private int prevNumRefPoints = 0;

//    public float refPointRadius = 0.1f;
//    private float prevRefPointRadius = 0f;

//    public int metricIndex = 0;
//    private int prevMetricIndex = 0;


    public int numOfPoints = 0;
    private int _prevNumOfPoints = 0;

    public float refPointRadius = 0.1f;
    private float _prevRefPointRadius = 0.1f;

    private Transform sphereTransform;
    private Transform _prevSphereTransform;

    private Material sphereMaterial;
    private Vector4[] pointPositions;
    private Vector4[] pointPositionsSpherical;
    private Color[] colors;

    private float radius;



    void Start()
    {
        sphereMaterial = GetComponent<Renderer>().material;
        radius = transform.localScale.x * 0.5f;

        GeneratePoints();
    }

    void Update()
    {
        bool doRefresh = false;
        if (numOfPoints != _prevNumOfPoints)
        {
            doRefresh = true;
            _prevNumOfPoints = numOfPoints;
        }
        if ( refPointRadius != _prevRefPointRadius) 
        {
            doRefresh = true;
            _prevRefPointRadius = refPointRadius;
        }

        if ( doRefresh)
        {
            DeleteChildObjects();
            GeneratePoints();
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

            Vector4 newPointSpherical = new Vector4(radius, theta, phi);
            pointPositionsSpherical[i] = newPointSpherical;

            Vector4 newPoint = ConvertToEuclidean(newPointSpherical);
            pointPositions[i] = newPoint;

            Color newPointColor = new Color(Random.value, Random.value, Random.value, 1);
            colors[i] = newPointColor;

            GameObject pointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointObject.transform.localScale = new Vector4(refPointRadius, refPointRadius, refPointRadius);
            pointObject.transform.position = newPoint;
            pointObject.transform.parent = transform;
            pointObject.name = "Reference Point";
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
    void DeleteChildObjects()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            // ignore north and sout pole pointer objects
            if (child.name != "NorthPole" && child.name != "SouthPole")
            {
                // Destroy each child object
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }

    Vector4 ConvertToSpherical(Vector4 euclidPos)
    {
        float r = euclidPos.magnitude;
        float theta = Mathf.Atan2(euclidPos.z, euclidPos.x);
        float phi = Mathf.Acos(euclidPos.y / r);

        return new Vector4(r, theta, phi, 0);
    }

    Vector4 ConvertToEuclidean(Vector4 sphericalPos)
    {
        float x = sphericalPos.x * Mathf.Sin(sphericalPos.z) * Mathf.Cos(sphericalPos.y);
        float y = sphericalPos.x * Mathf.Sin(sphericalPos.z) * Mathf.Sin(sphericalPos.y);
        float z = sphericalPos.x * Mathf.Cos(sphericalPos.z);

        return new Vector4(x, y, z, 0);
    }

    //private int frame = 0;

    //private struct PointStruct
    //{
    //    public GameObject point;
    //    public Vector3 sphericCoord;
    //    public Vector3 euclidCoord;
    //    public Vector2 dir;
    //}

    //private PointStruct[] points;

    //void Start()
    //{
    //    prevNumRefPoints = numRefPoints;
    //    prevRefPointRadius = refPointRadius;
    //    prevMetricIndex = metricIndex;

    //    //points = new PointStruct[numRefPoints];
    //    sphereMaterial = sphere.GetComponent<Renderer>().material;

    //    GenerateChildObjects();
    //}

    //void Update()
    //{


    //    if (numRefPoints == 0)
    //    {
    //        prevNumRefPoints = numRefPoints;
    //        return;
    //    }

    //    if (prevNumRefPoints != numRefPoints ||
    //    prevRefPointRadius != refPointRadius ||
    //    prevMetricIndex != metricIndex)
    //    {
    //        prevNumRefPoints = numRefPoints;
    //        prevRefPointRadius = refPointRadius;
    //        prevMetricIndex = metricIndex;

    //        DeleteChildObjects();

    //        GenerateChildObjects();
    //    } 
    //}

    //void DeleteChildObjects ()
    //{
    //    for (int i = 0; i < transform.childCount; i++)
    //    {
    //        // Destroy each child object
    //        Destroy(transform.GetChild(i).gameObject);
    //    }
    //}

    //void GenerateChildObjects()
    //{
    //    Transform parentTransform = gameObject.transform; // Get the parent transform of the script GameObject
    //    float parentRadius = parentTransform.localScale.x * 0.5f; // Assuming the parent's scale is uniform


    //    float[] points = new float[numRefPoints * 3];
    //    float[] pointColors = new float[numRefPoints * 3];

    //    //for (int i = 0; i < numRefPoints; i++)
    //    //{
    //    //    float theta = UnityEngine.Random.Range(0f,  Mathf.PI);
    //    //    float phi = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

    //    //    points[i * 2] = theta;
    //    //    points[i * 2 + 1] = phi;
    //    //    Debug.Log(points[i * 2] + " " + points[i * 2 + 1]);

    //    //    float x = parentRadius * Mathf.Sin(theta) * Mathf.Cos(phi);
    //    //    float y = parentRadius * Mathf.Sin(theta) * Mathf.Sin(phi);
    //    //    float z = parentRadius * Mathf.Cos(theta);

    //    //    pointColors[i * 3] = UnityEngine.Random.value;
    //    //    pointColors[i * 3 + 1] = UnityEngine.Random.value;
    //    //    pointColors[i * 3 + 2] = UnityEngine.Random.value;

    //    //    GameObject pointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere); 
    //    //    pointObject.transform.localScale = new Vector3(refPointRadius, refPointRadius, refPointRadius); 
    //    //    pointObject.transform.position = new Vector3(x,y,z);
    //    //    pointObject.transform.parent = transform; 
    //    //    pointObject.name = "Reference Point";
    //    //    pointObject.GetComponent<Renderer>().material.color = new Color(pointColors[i*3], pointColors[i * 3+1], pointColors[i * 3+2]);

    //    //}

    //    //MaterialPropertyBlock props = new MaterialPropertyBlock();
    //    //props.SetFloatArray("_PointArray", points);
    //    //props.SetFloatArray("_ColorArray", pointColors);
    //    //props.SetInt("_ColorArraySize", pointColors.Length);
    //    //props.SetFloat("_SphereRadius", parentRadius);
    //    //props.SetInt("_MetricIndex", metricIndex);
    //    //props.SetInt("_NumRefPoints", numRefPoints);
    //    //GetComponent<Renderer>().SetPropertyBlock(props);


    //    MaterialPropertyBlock props = new MaterialPropertyBlock();
    //    props.SetFloatArray("_PointArray", new float[1]);
    //    props.SetFloatArray("_ColorArray", new float[1]);
    //    props.SetInt("_ColorArraySize", 0);
    //    props.SetFloat("_SphereRadius", parentRadius);
    //    props.SetInt("_MetricIndex", 4);
    //    props.SetInt("_NumRefPoints", 0);
    //    GetComponent<Renderer>().SetPropertyBlock(props);

    //    float theta = Mathf.PI * 0.6f;
    //    float phi = Mathf.PI * 2f * 0.6f;
    //    Debug.Log(theta + " " + phi);

    //    float x = parentRadius * Mathf.Sin(theta) * Mathf.Cos(phi);
    //    float y = parentRadius * Mathf.Sin(theta) * Mathf.Sin(phi);
    //    float z = parentRadius * Mathf.Cos(theta);
    //    Debug.Log(x + " " + y + " " + z);

    //    GameObject pointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //    pointObject.transform.localScale = new Vector3(refPointRadius, refPointRadius, refPointRadius);
    //    pointObject.transform.position = new Vector3(x, y, z);
    //    pointObject.transform.parent = transform;
    //    pointObject.name = "Reference Point";
    //    pointObject.GetComponent<Renderer>().material.color = new Color(255, 0, 0);

    //}

}
