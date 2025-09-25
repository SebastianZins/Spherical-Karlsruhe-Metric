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
            if (Physics.Raycast(ray, out RaycastHit hit) && cam.enabled)
            {
                var a = hit.collider.transform.tag;
                if (hit.transform != null && hit.collider.transform.CompareTag(Constants.REFERENCE_POINT_TAG))
                {
                    selectedChild = hit.collider.transform;
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

                MercatorReferencePoint mercatorProjectionPoint = selectedChild.GetComponent<MercatorReferencePoint>();
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

                AzimuthalReferencePoint azimuthalReferencePoint = selectedChild.GetComponent<AzimuthalReferencePoint>();
                if (azimuthalReferencePoint == null)
                {
                    azimuthalReferencePoint = selectedChild.parent.GetComponent<AzimuthalReferencePoint>();
                }
                if (azimuthalReferencePoint != null)
                {
                    Vector3 localHitPoint = transform.InverseTransformPoint(hit.point);

                    float halfWidth = transform.localScale.x * 0.5f;
                    float halfHeight = transform.localScale.z * 0.5f;
                    localHitPoint.x = Mathf.Clamp(localHitPoint.x, -halfWidth, halfWidth);
                    localHitPoint.z = Mathf.Clamp(localHitPoint.z, -halfHeight, halfHeight);
                    localHitPoint.y = 1f;

                    Vector3 clampedWorldPoint = transform.TransformPoint(localHitPoint);

                    azimuthalReferencePoint.DragPointPosition(clampedWorldPoint);
                }
            }
        }
    }
}
