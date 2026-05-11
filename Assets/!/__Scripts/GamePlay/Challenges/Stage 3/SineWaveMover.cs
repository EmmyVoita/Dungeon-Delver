using UnityEngine;

public class SineWaveMover : MonoBehaviour, IReversible
{
    [SerializeField] private float speed;
    [SerializeField] private float reverseSpeedMult = 1.25f;

    private float _amplitude;
    private float _frequency;
    private float _phaseOffset;
    private float _x;

    private float _time;
    private float _direction = 1f;

    private bool _initialized = false;
    private float _waveDir = 1;

    public void Initialize(float amplitude, float frequency, float waveDir, float x, float startTime)
    {
        _amplitude = amplitude;
        _frequency = frequency;
        _waveDir = waveDir;
        _x = x;

        _time = startTime;
        _initialized = true;
    }

    public float GetInitialY()
    {
        return _waveDir * _amplitude * Mathf.Sin(_frequency * (_x - _waveDir * _time));
    }

    void Update()
    {
        if (!_initialized) return;

        _time += Time.deltaTime * _direction * speed;

        float y = _waveDir * _amplitude * Mathf.Sin(_frequency * (_x - _waveDir * _time));

        transform.localPosition = new Vector3(_x, y, transform.position.z);
    }

    public void Reverse()
    {
        _direction *= -1f;
        speed *= reverseSpeedMult;
    }
}