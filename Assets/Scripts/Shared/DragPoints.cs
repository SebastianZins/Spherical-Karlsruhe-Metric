using UnityEngine;

public class PointDragger : MonoBehaviour
{
    [SerializeField] public Camera cam;
    private bool isDragging = false;
    private Transform selectedChild;

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform != null && hit.transform.CompareTag(Constants.REFERENCE_POINT_TAG))
                {
                    selectedChild = hit.transform;
                    isDragging = true;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            selectedChild = null;
        }

        if (isDragging && selectedChild != null)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {

                ReferencePointHandler spherePoint = selectedChild.GetComponent<ReferencePointHandler>();
                if (spherePoint != null)
                {
                    Vector3 direction = (hit.point - transform.position).normalized;
                    float radius = transform.localScale.x * 0.5f;
                    spherePoint.SetEuclideanPosition(transform.position + direction * radius);
                }

                AzimuthalReferencePoint mercatorProjectionPoint = selectedChild.GetComponent<AzimuthalReferencePoint>();
                if (mercatorProjectionPoint != null)
                {
                    Vector3 localHitPoint = transform.InverseTransformPoint(hit.point);

                    float halfWidth = transform.localScale.x * 0.5f;
                    float halfHeight = transform.localScale.z * 0.5f;
                    localHitPoint.x = Mathf.Clamp(localHitPoint.x, -halfWidth, halfWidth);
                    localHitPoint.z = Mathf.Clamp(localHitPoint.z, -halfHeight, halfHeight);
                    localHitPoint.y = 1f;

                    Vector3 clampedWorldPoint = transform.TransformPoint(localHitPoint);

                    mercatorProjectionPoint.DragPointPosition(clampedWorldPoint);
                }
            }
        }
    }
}
