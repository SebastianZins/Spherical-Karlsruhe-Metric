using UnityEngine;

public class MouseInputSphereRotator : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 lastMousePosition;

    void Update()
    {
        // Detect mouse down on the sphere
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    isDragging = true;
                    lastMousePosition = Input.mousePosition;
                }
            }
        }

        // Rotate while dragging
        if (Input.GetMouseButton(1) && isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float rotationSpeed = 0.2f;

            // Rotate around Y and X axes
            transform.Rotate(Camera.main.transform.up, -delta.x * rotationSpeed, Space.World);
            transform.Rotate(Camera.main.transform.right, delta.y * rotationSpeed, Space.World);

            lastMousePosition = Input.mousePosition;
        }

        // Release drag
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }
}