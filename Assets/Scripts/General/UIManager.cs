using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Sphere")]
    [SerializeField] public GameObject sphereObject;
    [SerializeField] private SphereGenerator _sphereGenerator;
    [SerializeField] private GrowVoronoi _growAnimator;
    [SerializeField] private RotateSphere _rotateAnimator;

	[Header("Display Settings")]
    [SerializeField] public Toggle showPolesInput;
    [SerializeField] public Toggle showCoordGridInput;
    [SerializeField] public Toggle rotateSphereInput;
    [SerializeField] public TMP_InputField rotationSpeedInput;

    [Header("Generator Settings")]
    [SerializeField] public TMP_InputField numOfPointsInput;
    [SerializeField] public TMP_InputField pointRadiusInput;

    [Header("Metric Settings")]
    [SerializeField] public TMP_Dropdown metricTypeInput;
    [SerializeField] public Toggle useClosestDistanceInput;
    [SerializeField] public Toggle growAnimationActiveInput;
    [SerializeField] public TMP_InputField growSpeedInput;

    void Start()
    {
        _sphereGenerator = sphereObject.GetComponent<SphereGenerator>();
        _growAnimator = sphereObject.GetComponent<GrowVoronoi>();
        _rotateAnimator = sphereObject.GetComponent<RotateSphere>();

        Initialize();
        AddListeners();
    }

	public void Quit()
	{
		Application.Quit();
	}

	private void Initialize()
	{
		showPolesInput.isOn = _sphereGenerator.showPoles;
		showCoordGridInput.isOn = _sphereGenerator.showCoordGrid;
		rotateSphereInput.isOn = _rotateAnimator.rotateSphere;
		rotationSpeedInput.text = _rotateAnimator.rotationSpeed.ToString();

		numOfPointsInput.text = _sphereGenerator.numOfPoints.ToString();
		pointRadiusInput.text = _sphereGenerator.refPointRadius.ToString();

		metricTypeInput.value = (int)_sphereGenerator.metricType;
		metricTypeInput.RefreshShownValue();
		useClosestDistanceInput.isOn = _sphereGenerator.useClosestDistance;
		growAnimationActiveInput.isOn = _growAnimator.growVoronoi;
		growSpeedInput.text = _growAnimator.growSpeed.ToString();
	}

	private void AddListeners()
	{
		showPolesInput.onValueChanged.AddListener(OnShowPolesChanged);
		showCoordGridInput.onValueChanged.AddListener(OnShowCoordGridChanged);
		rotateSphereInput.onValueChanged.AddListener(OnRotateSphereActiveChanged);
		rotationSpeedInput.onValueChanged.AddListener(OnRotationSpeedChanged);

		numOfPointsInput.onValueChanged.AddListener(OnNumberOfPointsChanged);
		pointRadiusInput.onValueChanged.AddListener(OnPointRadiusChanged);

		metricTypeInput.onValueChanged.AddListener(OnMetricTypeChanged);
		useClosestDistanceInput.onValueChanged.AddListener(OnUseClosestDistanceChanged);
		growAnimationActiveInput.onValueChanged.AddListener(OnGrowAnimationActiveChanged);
		growSpeedInput.onValueChanged.AddListener(OnGrowAnimationSpeedChanged);

		metricTypeInput.RefreshShownValue();
	}

	private void OnShowPolesChanged(bool value)
	{
		_sphereGenerator.showPoles = value;
		Debug.Log("Set show poles flag: " + value);
	}

	private void OnShowCoordGridChanged(bool value)
	{
		_sphereGenerator.showCoordGrid = value;
		Debug.Log("Show grid flag changed: " + value);
	}

	private void OnRotateSphereActiveChanged(bool value)
	{
		_rotateAnimator.rotateSphere = value;
		Debug.Log("Rotatation flag changed: " + value);
	}

	private void OnRotationSpeedChanged(string value)
	{
		if (float.TryParse(value, out float rotationSpeed))
		{
			Debug.Log("Rotation speed changed: " + rotationSpeed);
		}
		else
		{
			Debug.LogWarning("Invalid number input: " + value);
		}
		_rotateAnimator.rotationSpeed = rotationSpeed;
	}

	private void OnNumberOfPointsChanged(string value)
	{
		if (int.TryParse(value, out int numOfPoints))
		{
			Debug.Log("Number of points changed: " + numOfPoints);
		}
		else
		{
			Debug.LogWarning("Invalid number input: " + value);
		}
		_sphereGenerator.SetPointCount(numOfPoints);
	}

	private void OnPointRadiusChanged(string value)
	{
		if (float.TryParse(value, out float refPointRadius))
		{
			Debug.Log("Point radius changed: " + refPointRadius);
		}
		else
		{
			Debug.LogWarning("Invalid number input: " + value);
		}
		_sphereGenerator.SetPointRadius(refPointRadius);
	}

	private void OnMetricTypeChanged(int value)
	{
		_sphereGenerator.metricType = (EMetricType)value;
		Debug.Log("Metric type changed: " + value);
	}

	private void OnUseClosestDistanceChanged(bool value)
	{
		_sphereGenerator.useClosestDistance = value;
		Debug.Log("Use closest distance metric changed: " + value);
	}

	private void OnGrowAnimationActiveChanged(bool value)
	{
		_growAnimator.ActivateGrowAnimation(value);
		Debug.Log("Use closest distance metric changed: " + value);
	}

	private void OnGrowAnimationSpeedChanged(string value)
	{
		if (float.TryParse(value, out float growSpeed))
		{
			Debug.Log("Grow speed changed: " + growSpeed);
		}
		else
		{
			Debug.LogWarning("Invalid number input: " + value);
		}
		_growAnimator.growSpeed = growSpeed;
	}
}
