using UnityEngine;

public class VoronoiSphere : MonoBehaviour
{
    public Material material;
    public Vector3[] points; // Array of points to be passed to the shader

    private ComputeBuffer buffer;
    private bool dataDirty = true; // flag for changes

    void Start()
    {
        // Initialize the Compute Buffer with the points
        buffer = new ComputeBuffer(points.Length, sizeof(float) * 3);
        buffer.SetData(points);

        // Set the buffer and buffer length in the shader
        material.SetBuffer("_PointBuffer", buffer);
        material.SetInt("_BufferLength", points.Length);
    }
    void Update()
    {
        if (dataDirty)
        {
            buffer.SetData(points); // just update data, no reallocation
            material.SetBuffer("_PointBuffer", buffer);
            material.SetInt("_BufferLength", points.Length);
            dataDirty = false;
        }
    }

    public void UpdatePoints(Vector3[] newPoints)
    {
        // update array and mark dirty
        points = newPoints;
        dataDirty = true;
    }

    void OnDestroy()
    {
        // Release the Compute Buffer when not needed anymore
        buffer.Release();
    }
}
