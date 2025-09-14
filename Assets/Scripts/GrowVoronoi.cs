using UnityEngine;

public class GrowVoronoi : MonoBehaviour
{
    public bool growVoronoi = false;
    public float growSpeed = 0.05f;
    private float _growPercentage = 0f;
    private bool _growDirBigger = true;

    public GameObject projection;
    private CreateProjection _projectionData;

    private void Start()
    {
        _projectionData = projection.GetComponent<CreateProjection>();
    }

    void Update()
    {
        if (growVoronoi)
        {
            if (_growDirBigger)
            {
                _growPercentage += growSpeed;
                if (_growPercentage >= 100f)
                {
                    _growDirBigger = false;
                    _growPercentage -= growSpeed;
                }
            }
            else
            {
                _growPercentage -= growSpeed;
                if (_growPercentage <= 0f)
                {
                    _growDirBigger = true;
                    _growPercentage += growSpeed;
                }
            }
            transform.GetComponent<SphereGenerator>().UpdateGrowAnimation(_growPercentage);
            _projectionData.UpdateGrowAnimation(_growPercentage);
        }
    }

    public void ActivateGrowAnimation(bool growVoronoiChanged)
    {
        growVoronoi = growVoronoiChanged;
        _growPercentage = 0f;
        if (!growVoronoiChanged)
        {
            transform.GetComponent<SphereGenerator>().UpdateGrowAnimation(_growPercentage);
            _projectionData.UpdateGrowAnimation(_growPercentage);
        }
    }
}
