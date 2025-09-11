using UnityEngine;
using UnityEngine.Rendering;

public class GrowVoronoi : MonoBehaviour
{
    private bool _growVoronoi = false;
    private float _growSpeed = 0.05f;
    private float _growPercentage = 0f;
    private bool _growDirBigger = true;

    void Update()
    {
        if (_growVoronoi)
        {
            if (_growDirBigger)
            {
                _growPercentage += _growSpeed;
                if (_growPercentage >= 100f)
                {
                    _growDirBigger = false;
                    _growPercentage -= _growSpeed;
                }
            }
            else
            {
                _growPercentage -= _growSpeed;
                if (_growPercentage <= 0f)
                {
                    _growDirBigger = true;
                    _growPercentage += _growSpeed;
                }
            }
            transform.GetComponent<SphereGenerator>().UpdateGrowAnimation(_growPercentage);
        }
    }

    public void ActivateGrowAnimation(bool growVoronoi)
    {
        _growVoronoi = growVoronoi;
        _growPercentage = 0f;
        Debug.Log("Use closest distance metric changed: " + growVoronoi);
        if (!growVoronoi)
        {
            transform.GetComponent<SphereGenerator>().UpdateGrowAnimation(_growPercentage);
        }
    }

    public void SetGrowSpeed(string growSpeedString)
    {
        if (float.TryParse(growSpeedString, out float growSpeed))
        {
            _growSpeed = growSpeed;
            Debug.Log("Grow speed changed: " + growSpeed);
        }
        else
        {
            Debug.LogWarning("Invalid number input: " + growSpeed);
        }
    }
}
