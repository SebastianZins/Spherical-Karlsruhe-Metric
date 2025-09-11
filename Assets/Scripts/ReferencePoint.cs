using UnityEngine;
using Random = UnityEngine.Random;

public class ReferencePointHandler : MonoBehaviour
{
    public Vector3 euclideanPosition;
    private Vector3 _oldEuclideanPosition;
    public float radius;
    public Vector2 sphericalPosition;
    private Vector2 _oldSphericalPosition;
    public Color color;
    private Color _oldColor;
    public float pointRadius;
    private float _oldPointRadius;

    void Update()
    {
        if (euclideanPosition != _oldEuclideanPosition)
        {
            SetEuclideanPosition(euclideanPosition);
        }

        if (sphericalPosition != _oldSphericalPosition)
        {
            SetSphericalPosition(radius, sphericalPosition);
        }

        color = GetComponent<Renderer>().material.color;
        if (color != _oldColor)
        {
            SetColor(color);
            transform.parent.GetComponent<SphereGenerator>().UpdatePointColor();
        }

        if (pointRadius != _oldPointRadius)
        {
            SetPointRadius(pointRadius);
        }
    }

    public void SetEuclideanPosition(Vector3 position)
    {
        _oldEuclideanPosition = position;
        euclideanPosition = position;

        sphericalPosition = ConvertToSpherical(euclideanPosition);
        _oldSphericalPosition = sphericalPosition;
        transform.parent.GetComponent<SphereGenerator>().UpdatePointPositionSpherical();
    }

    public void SetSphericalPosition(float radius, Vector2 position, bool init = false)
    {
        this.radius = radius;

        float theta = position.x % (Mathf.PI * 2f);
        theta = theta > 0 ? theta : (Mathf.PI * 2f) - theta;

        float phi = position.y % Mathf.PI;
        phi = phi > 0 ? phi : Mathf.PI - phi;

        sphericalPosition = new Vector2(theta, phi);
        _oldSphericalPosition = sphericalPosition;

        euclideanPosition = ConvertToEuclidean(radius, sphericalPosition);
        _oldEuclideanPosition = euclideanPosition;

        SetWorldPosition(euclideanPosition);
        if (!init)
        {
            transform.parent.GetComponent<SphereGenerator>().UpdatePointPositionSpherical();
        }
    }

    public void SetColor(Color color)
    {
        this.color = color;
        _oldColor = color;
        GetComponent<Renderer>().material.color = color;
    }

    public void SetPointRadius(float pointRadius)
    {
        this.pointRadius = pointRadius;
        _oldPointRadius = pointRadius;

        transform.localScale = new Vector3(
            pointRadius / transform.parent.transform.lossyScale.x,
            pointRadius / transform.parent.transform.lossyScale.y,
            pointRadius / transform.parent.transform.lossyScale.z
        );
    }

    public void SetRandomSphericalPosition(float radius, bool init = false)
    {
        float phi = Random.Range(0, Mathf.PI);
        float theta = Random.Range(0, Mathf.PI * 2f);
        SetSphericalPosition(radius, new Vector2(theta, phi), init);
    }

    public void SetRandomColor()
    {
        SetColor(new Color(Random.value, Random.value, Random.value, 1));
    }

    private static Vector2 ConvertToSpherical(Vector3 euclidPos)
    {
        float r = euclidPos.magnitude;
        float theta = Mathf.Atan2(euclidPos.z, euclidPos.x);
        float phi = Mathf.Acos(euclidPos.y / r);

        return new Vector2(theta, phi);
    }

    private static Vector3 ConvertToEuclidean(float radius, Vector2 sphericalPos)
    {
        float x = radius * Mathf.Sin(sphericalPos.y) * Mathf.Cos(sphericalPos.x);
        float y = radius * Mathf.Sin(sphericalPos.y) * Mathf.Sin(sphericalPos.x);
        float z = radius * Mathf.Cos(sphericalPos.y);

        return new Vector3(x, y, z);
    }

    private void SetWorldPosition(Vector3 localPoint)
    {
        Quaternion rotation = transform.parent.transform.localRotation; ;
        Vector3 rotatedPoint = transform.parent.transform.rotation * localPoint;
        Vector3 newPoint = transform.parent.transform.position + rotatedPoint;

        transform.position = newPoint;
        transform.rotation = rotation;
    }
}
