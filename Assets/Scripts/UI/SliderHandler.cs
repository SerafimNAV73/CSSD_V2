using CSSD;
using FMODUnity;
using UnityEngine;
using UnityEngine.UI;

public class SliderHandler : MonoBehaviour
{
    [SerializeField]
    private InputField _field;

    [SerializeField]
    private Slider _slider;

    [SerializeField]
    private string _busPath = "";

    private FMOD.Studio.Bus _bus;

    public Slider CurSlider => _slider;

    private void Awake()
    {
        if (_busPath != "")
        {
            _bus = RuntimeManager.GetBus(_busPath);
        }
    }

    public void SetDefaultValue()
    {
        _bus.getVolume(out float volume);
        _slider.value = volume * _slider.maxValue;         

        UpdateSliderOutput();
    }

    public void UpdateSliderOutput()
    {
        if(_field != null && _slider != null)
        {
            _field.text = _slider.value.ToString();
            AudioManager._instance.PlaySliderEvent();
            _bus.setVolume(_slider.value / _slider.maxValue);
        }
    }

    public void UpdateFieldOutput()
    {
        if (_field != null && _slider != null)
        {
            if (float.TryParse(_field.text, out float value))    
                _slider.value = value;            
            
            UpdateSliderOutput();
        }
    }
}
