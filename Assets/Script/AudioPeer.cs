using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPeer : MonoBehaviour
{

    /// <summary>
    /// CREDIT TO Peter Olthof/ Peer Play.
    /// https://www.youtube.com/watch?v=5pmoP1ZOoNs
    /// https://forum.unity.com/threads/audio-visualization-tutorial-unity-c-q-a.432461/
    /// </summary>

    [SerializeField]
    bool liveAudio = false;
    AudioSource audioSource;

    [SerializeField]
    int frequency;
    private  float[] _samplesLeft = new float[512];
    private  float[] _samplesRight= new float[512];

    private float[] _frequencyBand = new float[8];
    private float[] _bandBuffer = new float[8];
    private float[] _freqBandHighest = new float[8];
    private float[] _bufferDecrease = new float[8];


    private static float[] _bandBuffer64 = new float[64];
    private static float[] _frequencyBand64 = new float[64];
    private float[] _freqBandHighest64 = new float[64];
    private float[] _bufferDecrease64 = new float[64];


    public float[] _audioBand, _audioBandBuffer;

    public float[] _audioBand64, _audioBandBuffer64;

    public float _audioProfile;
    public enum _channel { Stereo,  Left, Right};
    public _channel channel = new _channel();
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        AudioProfile(_audioProfile);
        if (liveAudio)
        {
            audioSource.clip = Microphone.Start(Microphone.devices[0], true, 1, frequency);
            while (!(Microphone.GetPosition(Microphone.devices[0]) > 0)) { } // wait until the recording has started
        }
        audioSource.Play(); // Play the audio source!
    }

    void Update()
    {
        GetSpectrumAudioSource();
        MakeFrequencyBands();
        MakeFrequencyBands64();
        BandBuffer();
        BandBuffer64();
        CreateAudioBands();
        CreateAudioBands64();
    }
    void AudioProfile(float audioProfile)
    {
        for (int i=0;i<8;i++)
        {
            _freqBandHighest[i] = audioProfile;
        }
    }
    void CreateAudioBands()
    {// create values between zero and one that can be apllied to a lot of different outputs
        for (int i = 0; i < 8; i++)
        {
            if (_frequencyBand[i] > _freqBandHighest[i])
            {
                _freqBandHighest[i] = _frequencyBand[i];
            }
            _audioBand[i] = (_frequencyBand[i] / _freqBandHighest[i]);
            _audioBandBuffer[i] = (_bandBuffer[i] / _freqBandHighest[i]);
        }
    }
    void CreateAudioBands64()
    {// create values between zero and one that can be apllied to a lot of different outputs
        for (int i = 0; i < 64; i++)
        {
            if (_frequencyBand64[i] > _freqBandHighest64[i])
            {
                _freqBandHighest64[i] = _frequencyBand64[i];
            }
            _audioBand64[i] = (_frequencyBand64[i] / _freqBandHighest64[i]);
            _audioBandBuffer64[i] = (_bandBuffer64[i] / _freqBandHighest64[i]);
        }
    }

    void BandBuffer()
    {
        for (int g = 0; g < 8; g++)
        {
            if (_frequencyBand[g] > _bandBuffer[g])
            {
                _bandBuffer[g] = _frequencyBand[g];
                _bufferDecrease[g] = .005f;
            }
            if (_frequencyBand[g] < _bandBuffer[g])
            {
                _bandBuffer[g] -= _bufferDecrease[g];
                _bufferDecrease[g] *= 1.2f;
            }


        }
    }

    void BandBuffer64()
    {
        for (int g = 0; g < 64; g++)
        {
            if (_frequencyBand64[g] > _bandBuffer64[g])
            {
                _bandBuffer64[g] = _frequencyBand64[g];
                _bufferDecrease64[g] = .005f;
            }
            if (_frequencyBand64[g] < _bandBuffer64[g])
            {
                _bandBuffer64[g] -= _bufferDecrease64[g];
                _bufferDecrease64[g] *= 1.2f;
            }


        }
    }
    void GetSpectrumAudioSource()
    {
        audioSource.GetSpectrumData(_samplesLeft, 0, FFTWindow.Blackman);
        audioSource.GetSpectrumData(_samplesRight, 1, FFTWindow.Blackman);
    }

    void MakeFrequencyBands()
    {
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            float average = 0;
            int sampleCount = (int)Mathf.Pow(2, i) * 2;
            if (i == 7)
            {
                sampleCount += 2;
            }
            for (int j = 0; j < sampleCount; j++)
            {
                //average += samples[count] * (count + 1);
                if (channel == _channel.Stereo)
                {
                    average += (_samplesLeft[count] + _samplesRight[count]);
                }
                if (channel == _channel.Left)
                {
                    average += _samplesLeft[count] * (count + 1);
                }
                if (channel == _channel.Right)
                {
                    average += _samplesRight[count] * (count + 1);
                }
                count++;
            }
            average /= count;
            _frequencyBand[i] = average * 10;
        }
    }

    void MakeFrequencyBands64()
    {
        int count = 0;
        int sampleCount = 1;
        int power = 0;
        for (int i = 0; i < 64; i++)
        {
            float average = 0;
            //int sampleCount = (int)Mathf.Pow(2, i) * 2;
            if (i == 16 || i==32 || i==40 || i==48 || i==56)
            {
                power++;
                sampleCount = (int)Mathf.Pow(2, power);
                if (power == 3)
                {
                    sampleCount -= 2;
                }
            }
            for (int j = 0; j < sampleCount; j++)
            {
                if (channel == _channel.Stereo)
                {
                    average += (_samplesLeft[count] + _samplesRight[count]);
                }
                if (channel == _channel.Left)
                {
                    average += _samplesLeft[count] * (count + 1);
                }
                if (channel == _channel.Right)
                {
                    average += _samplesRight[count] * (count + 1);
                }

                count++;
            }
            average /= count;
            _frequencyBand64[i] = average * 80;
        }
    }
}