using UnityEngine;
using UnityEngine.UIElements;

public class ProjectionReferencePoint : MonoBehaviour
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

    public void InitializePoint(ReferencePointHandler spherePoint, Transform parent, Vector2 projectionScale, SphereGenerator sphereGenerator)
    {
        _ogPoint = spherePoint;
        _sphereGenerator = sphereGenerator;

        GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Color"));
        GetComponent<Renderer>().material.color = Color.black;

        transform.tag = Constants.REFERENCE_POINT_TAG;
        transform.parent = parent;

        _innerCircle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _innerCircle.transform.parent = transform;

        _innerCircle.transform.localScale = new Vector3(0.7f, 1f, 0.7f);
        _innerCircle.transform.position += new Vector3(0, 1f, 0);

        _innerCircleMaterial = _innerCircle.GetComponent<Renderer>().material;
        _innerCircleMaterial = new Material(Shader.Find("Unlit/Color"));
        _innerCircleMaterial.color = spherePoint.color;
        _innerCircle.GetComponent<Renderer>().material = _innerCircleMaterial;

        _projectionScale = projectionScale;

        _color = spherePoint.color;
        _sphereRadius = spherePoint.radius;

        SetPointRadius(spherePoint.pointRadius);
        SetSphericalPosition(spherePoint.sphericalPosition);

        transform.name = $"Reference Point [{_sphericalPosition.x}, {_sphericalPosition.y}]";
    }

    public void SetEuclideanPosition(Vector3 position)
    {
        _euclideanPosition = (position);
        _sphericalPosition = FromMercatorProjection(_euclideanPosition);
    }

    public void UpdatePosition()
    {
        Vector2 sphericalPosition = _ogPoint.sphericalPosition;
        SetSphericalPosition(sphericalPosition);
    }

    public void DragPointPosition(Vector3 pos)
    {
        transform.position = pos;
        _euclideanPosition = transform.localPosition;
        _sphericalPosition = FromMercatorProjection(_euclideanPosition);
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

    public void SetSphericalPosition(Vector2 position)
    {
        float theta = position.x % (Mathf.PI * 2f);
        theta = theta > 0 ? theta : (Mathf.PI * 2f) - theta;

        float phi = position.y % Mathf.PI;
        phi = phi > 0 ? phi : Mathf.PI - phi;

        _sphericalPosition = new Vector2(theta, phi);
        _euclideanPosition = ToMercatorProjection(_sphericalPosition);

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

    private Vector2 FromMercatorProjection(Vector3 mercatorPos)
    {
        float x = mercatorPos.x;
        float y = mercatorPos.z; // Assuming Z is north-south, matching your forward function

        // Undo X scaling and offset
        float theta = (x + _projectionScale.x * 0.25f) / (_projectionScale.x * 0.5f);
        theta *= 2f * Mathf.PI;

        // Undo Y scaling and inverse Mercator projection
        float unscaledY = Mathf.Clamp(y, -_projectionScale.y * 0.42f, _projectionScale.y * 0.42f); // same epsilon as forward
        float latitude = 2f * Mathf.Atan(Mathf.Exp(unscaledY)) - Mathf.PI / 2f;

        float phi = Mathf.PI / 2f - latitude;

        return new Vector2(theta, phi); // (theta, phi)
    }

    private Vector3 ToMercatorProjection(Vector2 sphercialPos)
    {
        float theta = sphercialPos.x;
        float phi = sphercialPos.y;

        float latitude = Mathf.PI / 2f - phi;
        float x = theta / (2f * Mathf.PI);
        float y = Mathf.Log(Mathf.Tan(Mathf.PI / 4f + latitude / 2f));
        // Scale to fit plane
        x *= _projectionScale.x * 0.5f;
        x -= _projectionScale.x * 0.25f;
        // Clamp at north / southpole to avoid exponantially exploding tan behaviour at 0pi / pi
        float epsilon = 0.08f;
        y = Mathf.Clamp(y, -_projectionScale.y * (0.5f - epsilon), _projectionScale.y * (0.5f - epsilon));

        Vector3 mercatorPos = new Vector3(x, 0f, y);
        return mercatorPos;
    }

    private void SetWorldPosition(Vector3 localPoint)
    {
        Quaternion rotation = transform.parent.localRotation;
        Vector3 scaledPoint = Vector3.Scale(localPoint, transform.parent.lossyScale);
        Vector3 rotatedPoint = transform.parent.rotation * scaledPoint;
        Vector3 newPoint = transform.parent.position + rotatedPoint;

        transform.position = newPoint;
        transform.rotation = rotation;
    }

    private void UpdateShader()
    {
        //_material.SetColor("_Color", _color);
        //_material.SetColor("_OutlineColor", Color.black); 
        //_material.SetFloat("_OutlineWidth", 0.05f);
    }
}
