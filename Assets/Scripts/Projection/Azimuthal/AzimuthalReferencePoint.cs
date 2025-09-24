using UnityEngine;
using UnityEngine.UIElements;

public class AzimuthalReferencePoint : MonoBehaviour
{
    private Vector3 _euclideanPosition;
    private Vector2 _sphericalPosition;
    private Vector2 _projectionScale;
    private Color _color;
    private float _sphereRadius;
    private GameObject _innerCircle;
    private Material _innerCircleMaterial;
    private ReferencePointHandler _ogPoint;
    public SphereGenerator _sphereGenerator;
    private bool _isNorthCenter;

    public void InitializePoint(ReferencePointHandler spherePoint, Transform parent, Vector2 projectionScale, SphereGenerator sphereGenerator, bool isNorthCenter)
    {
        _isNorthCenter = isNorthCenter;
        _ogPoint = spherePoint;
        _sphereGenerator = sphereGenerator;

        GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Color"));
        GetComponent<Renderer>().material.color = Color.black;

        transform.tag = Constants.REFERENCE_POINT_TAG;
        transform.parent = parent;

        _innerCircle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _innerCircle.transform.parent = transform;

        _innerCircle.transform.localScale = new Vector3(0.7f, 1f, 0.7f);
        _innerCircle.transform.position += new Vector3(0, 0.5f, 0);

        _innerCircleMaterial = _innerCircle.GetComponent<Renderer>().material;
        _innerCircleMaterial = new Material(Shader.Find("Unlit/Color"));
        _innerCircleMaterial.color = spherePoint.color;
        _innerCircle.GetComponent<Renderer>().material = _innerCircleMaterial;

        _projectionScale = projectionScale;

        _color = spherePoint.color;
        _sphereRadius = spherePoint.radius;

        SetPointRadius(spherePoint.pointRadius);
        UpdatePosition();

        transform.name = $"Reference Point [{_sphericalPosition.x}, {_sphericalPosition.y}]";
    }

    public void SetEuclideanPosition(Vector3 position)
    {
        _euclideanPosition = (position);
        _sphericalPosition = FromAzimuthalProjection();
        _sphericalPosition = normalizeSphericalCoord(_sphericalPosition);
    }

    public void UpdatePosition()
    {
        _sphericalPosition = _ogPoint.sphericalPosition;
        SetSphericalPosition();
    }

    public void DragPointPosition(Vector3 pos)
    {
        transform.position = pos;
        _euclideanPosition = transform.localPosition; 
        _sphericalPosition = FromAzimuthalProjection();
        _sphericalPosition = normalizeSphericalCoord(_sphericalPosition);
        _ogPoint.SetSphericalPosition(_ogPoint.radius, _sphericalPosition);
        _sphereGenerator.UpdatePointPositionSpherical();
    }

    public Vector4 GetSphericalPosition4()
    {
        return new Vector4(_sphereRadius, _sphericalPosition.x, _sphericalPosition.y, 0);
    }

    public Color GetColor()
    {
        return _color;
    }

    public void SetSphericalPosition()
    {
        _euclideanPosition = ToAzimuthalProjection();

        SetWorldPosition(_euclideanPosition);
    }

    public void SetPointRadius(float pointRadius)
    {
        transform.localScale = new Vector3(
            pointRadius / _projectionScale.x,
            0.1f,
            pointRadius / _projectionScale.y
        );
    }

    Vector2 FromAzimuthalProjection()
    {
        float x = _euclideanPosition.x / 0.5f;
        float z = _euclideanPosition.z / 0.5f;

        float rProj = Mathf.Sqrt(x * x + z * z);
        float theta = Mathf.Atan2(z, x);
        float phi = Mathf.PI - Mathf.Asin(rProj);

        if (!_isNorthCenter)
        {
            theta = 2f * Mathf.PI - theta;
            SwitchVisibility(phi > Mathf.PI / 2);
        }
        else
        {
            phi = Mathf.PI - phi;
            SwitchVisibility(phi <= Mathf.PI / 2);
        }

        return new Vector2(theta, phi);
    }

    Vector3 ToAzimuthalProjection()
    {
        float theta = _sphericalPosition.x;
        float phi = _sphericalPosition.y;

        if (!_isNorthCenter)
        {
            theta = 2f * Mathf.PI - theta;
            SwitchVisibility(phi > Mathf.PI / 2);
        }
        else
        {
            SwitchVisibility(phi <= Mathf.PI / 2);
        }

        float x = Mathf.Sin(phi) * Mathf.Cos(theta) * 0.5f;
        float z = Mathf.Sin(phi) * Mathf.Sin(theta) * 0.5f;

        return new Vector3(x, 1f, z);
    }

    private void SetWorldPosition(Vector3 localPoint)
    {
        transform.localPosition = localPoint;
    }

    private void SwitchVisibility(bool active)
    {
        transform.gameObject.SetActive(active);
    }

    private Vector2 normalizeSphericalCoord(Vector2 sphereCords)
    {
        float theta = sphereCords.x;
        if (theta < 0)
        {
            theta = Mathf.PI * 2f + theta % (Mathf.PI * 2f);
        }
        theta = theta % (Mathf.PI * 2f);

        float phi = sphereCords.y;
        if (phi < 0)
        {
            phi = Mathf.PI + phi % Mathf.PI;
        }
        phi = phi % Mathf.PI;

        return new Vector2(theta, phi);
    }
}
