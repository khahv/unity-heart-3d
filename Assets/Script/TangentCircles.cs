using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TangentCircles : CircleTangent
{
    [Header("Setup")]
    public GameObject _circlePrefab;
    private GameObject _innerCircleGO, _outterCircleGO;
    public Vector4 _innerCircle, _outerCircle;
    private Vector4[] _tangentCircle;
    private GameObject[] _tangentObject;
    [Range(1,64)]
    public int _circleAmount;
    public float _innerCircleRadius, _outerCircleRadius;

    [Header("Audio Visuals")]
    public AudioPeer _audioPeer;
    public Material _materialBase;
    private Material[] _material;
    public Gradient _gradient;
    public float _emissionMultiplier;
    public bool _emissionBuffer;
    [Range(0, 1)]
    public float _thresholdEmission;



    //public float _degree;
    // Start is called before the first frame update
    void Start()
    {
        _innerCircle = new Vector4(0, 0, 0, _innerCircleRadius);
        _outerCircle = new Vector4(0, 0, 0, _outerCircleRadius);
         _tangentCircle = new Vector4[_circleAmount];
        _tangentObject = new GameObject[_circleAmount];
        _material = new Material[_circleAmount];


        for (int i = 0; i < _circleAmount; i++)
        {
            GameObject tangentInstance = (GameObject)Instantiate(_circlePrefab);
            _tangentObject[i] = tangentInstance;
            _tangentObject[i].transform.parent = this.transform;
            _material[i] = new Material(_materialBase);
            _material[i].EnableKeyword("_EMISSION");
            _material[i].SetColor("_Color", new Color(0, 0, 0));
            _tangentObject[i].GetComponent<MeshRenderer>().material = _material[i];
        }
       // _tangentCircleGO = (GameObject)Instantiate(_circlePrefab);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _circleAmount; i++)
        {
            _tangentCircle[i] = FindTangentCircle(_outerCircle, _innerCircle, (360f / _circleAmount) * i);
            _tangentObject[i].transform.position = new Vector3(_tangentCircle[i].x, _tangentCircle[i].y, _tangentCircle[i].z);
            _tangentObject[i].transform.localScale = new Vector3(_tangentCircle[i].w, _tangentCircle[i].w, _tangentCircle[i].w) * 2;
            if (_audioPeer._audioBandBuffer64[i] > _thresholdEmission)
            {
                if (_emissionBuffer)
                {
                    _material[i].SetColor("_EmissionColor", _gradient.Evaluate((1f/_circleAmount) * i) * _audioPeer._audioBandBuffer64[i] * _emissionMultiplier);
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
        }
    }
}
