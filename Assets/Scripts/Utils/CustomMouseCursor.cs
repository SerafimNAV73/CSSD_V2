using UnityEngine;

namespace CSSD
{
    public class CustomMouseCursor : MonoBehaviour
    {
        [SerializeField] private Texture2D _mouseCursor;

        private Vector2 _hotSpot = new Vector2(0, 0);
        private CursorMode _cursorMode = CursorMode.Auto;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (_mouseCursor != null)
            {
                _hotSpot = new Vector2(_mouseCursor.width / 2, _mouseCursor.height / 2);
                Cursor.SetCursor(_mouseCursor, _hotSpot, _cursorMode);
            }
        }
    }
}