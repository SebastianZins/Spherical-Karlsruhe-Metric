using UnityEngine;

public class ZoomWithTransform : MonoBehaviour
{
    private float _zoomSpeed = 10f;
    private float _minDistance = 2f;
    private float _maxDistance = 20f;
    [SerializeField] public GameObject sphere;

    private void Start()
    {
        float distance = Vector3.Distance(transform.position, sphere.transform.position);
        _minDistance = sphere.transform.lossyScale.x * 0.2f;
        _maxDistance = _minDistance + 2 * distance;
        _zoomSpeed = (_maxDistance - _minDistance) / 10;
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f && sphere.transform != null)
        {
            Vector3 direction = (transform.position - sphere.transform.position).normalized;
            float distance = Vector3.Distance(transform.position, sphere.transform.position);

            // Calculate new position
            float newDistance = Mathf.Clamp(distance + (-scroll * _zoomSpeed), _minDistance, _maxDistance);

            sphere.transform.position = sphere.transform.position + direction * (distance - newDistance);
        }
    }
}