using UnityEngine;

public class PointDragger : MonoBehaviour
{
    private Camera cam;
    private bool isDragging = false;
    private Transform selectedChild;

    void Start()
    {
        cam = Camera.main;
    }

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
                Vector3 direction = (hit.point - transform.position).normalized;
                float radius = transform.localScale.x * 0.5f; 
                //selectedChild.position = transform.position + direction * radius;
                hit.transform.GetComponent<ReferencePointHandler>().SetEuclideanPosition(transform.position + direction * radius);
            }
        }
    }
}
