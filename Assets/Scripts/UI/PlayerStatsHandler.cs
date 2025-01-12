using UnityEngine;
using UnityEngine.UI;

namespace CSSD 
{
    public class PlayerStatsHandler : MonoBehaviour
    {
        [SerializeField] private PlayerStats _stats;
        [SerializeField] private WeaponHandler.Settings _weaponSettings;
        [SerializeField] private int _racID;
        [SerializeField] private MainMenu _menu;

        private Button _button;

        private void Start()
        {
            _button = GetComponent<Button>();
        }

        private void OnValidate()
        {
            RuntimeAnimatorController[] animators = Resources.LoadAll<RuntimeAnimatorController>("RuntimeAnimatorControllers/");

            if (_racID > animators.Length - 1) _racID = animators.Length - 1;
            if (_racID < 0) _racID = 0;
        }

        public void OnCharacterChoose()
        {
            _menu.SetPlayerStats(_stats, _weaponSettings/*_stats.MaxHP, _stats.walkSpeed, _stats.damageModifier, _stats.reloadModifier*/);
            _menu.SetPlayerControllerID(_racID);
            AudioManager._instance.PlayClickEvent();
            _menu.SetDefaultColorsToButton();
            _menu.SetChoosenCharacterColor(this);
        }

        public void SetColor(Color color)
        {
            ColorBlock colorBlock = _button.colors;
            colorBlock.normalColor = color;
            _button.colors = colorBlock;
        }
    }
}