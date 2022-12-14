using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDrawShape : MonoBehaviour
{
    public GameObject _circlePrefab;
    private GameObject[] _tangentObject;
    public Material _materialBase;
    public int _circleAmount;
    private Vector3[] _tangentCircle;
    private Material[] _material;
    public Gradient _gradient;
    public float _emissionMultiplier;
    public bool _emissionBuffer;
    [Range(0, 1)]
    public float _thresholdEmission;
    [Header("Audio Visuals")]
    public AudioPeer _audioPeer;
    // Start is called before the first frame update
    void Start()
    {
        _tangentObject = new GameObject[_circleAmount];
        _tangentCircle = new Vector3[_circleAmount];
        _material = new Material[_circleAmount];
        for (int i = 0; i < _circleAmount; i++)
        {
            GameObject tangentInstance = (GameObject)Instantiate(_circlePrefab);
            _tangentObject[i] = tangentInstance;
            _tangentObject[i].transform.parent = this.transform;

            _material[i] = new Material(_materialBase);
            _material[i].EnableKeyword("_EMISSION");
            _material[i].SetColor("_Color", new Color(0, 0, 0));
            _tangentObject[i].GetComponentInChildren<MeshRenderer>().material = _material[i];
        }
    }

    private float currentCircleAmount = 0;
    private bool isInitEffectRunning = true;
    public float initEffectSpeed = 100;
    void InitEffect()
    {

        UpdateShape(_circleAmount);
        if (isInitEffectRunning)
        {
            for (int i = 0; i < _circleAmount; i++)
            {
                _tangentObject[i].SetActive(false);
            }
          

            for (int i = 0; i < (int)currentCircleAmount; i++)
            {
                _tangentObject[i].SetActive(true);
            }
            currentCircleAmount += Time.deltaTime  * initEffectSpeed;
            initEffectSpeed += 1;
            Debug.Log("currentCircleAmount: " + currentCircleAmount);
            if (currentCircleAmount > _circleAmount)
            {
                currentCircleAmount = _circleAmount;
                //isInitEffectRunning = false;
            }
        }
    }

    void UpdateShape(int _circleAmount)
    {
        
        float stepAngle = 0f;
        for (int i = 0; i < _circleAmount; i++)
        {

            _tangentCircle[i] = CalculateCirclePosition(stepAngle, _circleAmount);
            _tangentObject[i].transform.position = _tangentCircle[i];


            if (_audioPeer._audioBandBuffer64[i] > _thresholdEmission)
            {
                if (_emissionBuffer)
                {
                    _material[i].SetColor("_EmissionColor", _gradient.Evaluate((1f / _circleAmount) * i) * _audioPeer._audioBandBuffer64[i] * _emissionMultiplier * 5);
                    _tangentObject[i].transform.localScale = new Vector3(1, 1 * _audioPeer._audioBandBuffer64[i] * _emissionMultiplier + 1, 1);
                }
                else
                {
                    _material[i].SetColor("_EmissionColor", _gradient.Evaluate((1f / _circleAmount) * i) * _audioPeer._audioBand[i] * _emissionMultiplier);

                }
            }
            else
            {
                _material[i].SetColor("_EmissionColor", new Color(0, 0, 0));
            }
            stepAngle += 360f / _circleAmount;
        }
    }
    // Update is called once per frame
    void Update()
    {
        InitEffect();

    }
    //y = x*2
    private Vector3 CalculateCirclePosition(float step, int amount)
    {
        Vector3 a = new Vector3(0, 0, 0);
        //a.x = step;
        //a.z = a.x * 2;
        float angle = step * Mathf.Deg2Rad;
        a.x = 16 * Mathf.Pow(Mathf.Sin(angle), 3);
        a.z = 13 * Mathf.Cos(angle) - 5 * Mathf.Cos(2 * angle) - 2 * Mathf.Cos(3 * angle) - Mathf.Cos(4 * angle);
        return a;
    }
}
