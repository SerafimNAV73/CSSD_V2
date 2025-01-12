using System.ComponentModel;
using UnityEngine;
using UnityWeld.Binding;

namespace CSSD
{
    [Binding]
    public class ViewModel : MonoBehaviour, INotifyPropertyChanged
    {
        private string _curHealth = "";
        private string _maxHealth = "";
        private float _ratioHealth = 0;

        private string _ammoCount = "";
        private string _waveCount = "";

        public event PropertyChangedEventHandler PropertyChanged;

        [Binding]
        public string CurHealth
        {
            get => _curHealth;
            set
            {
                if (_curHealth.Equals(value)) return;
                _curHealth = value;
                OnPropertyChanged("CurHealth");
            }
        }

        [Binding]
        public string MaxHealth
        {
            get => _maxHealth;
            set
            {
                if (_maxHealth.Equals(value)) return;
                _maxHealth = value;
                OnPropertyChanged("MaxHealth");
            }
        }

        [Binding]
        public float RatioHealth
        {
            get => _ratioHealth;
            set
            {
                if (_ratioHealth == value) return;
                _ratioHealth = value;
                OnPropertyChanged("RatioHealth");
            }
        }

        [Binding]
        public string AmmoCount
        {
            get => _ammoCount;
            set
            {
                if (_ammoCount == value) return;
                _ammoCount = value;
                OnPropertyChanged("AmmoCount");
            }
        }

        [Binding]
        public string WaveCount
        {
            get => _waveCount;
            set
            {
                if (_waveCount == value) return;
                _waveCount = value;
                OnPropertyChanged("WaveCount");
            }
        }


        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
