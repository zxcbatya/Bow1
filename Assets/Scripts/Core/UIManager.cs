namespace Core
{
    using UnityEngine;

    namespace Core
    {
        public class UIManager : MonoBehaviour
        {
            [SerializeField] private Canvas _mainMenuCanvas;
            [SerializeField] private Canvas _gameUICanvas;

            public void Initialize()
            {
                _mainMenuCanvas.gameObject.SetActive(true);
                _gameUICanvas.gameObject.SetActive(false);
            }

            public void SwitchToGameUI()
            {
                _mainMenuCanvas.gameObject.SetActive(false);
                _gameUICanvas.gameObject.SetActive(true);
            }

            public void SwitchToMainMenu()
            {
                _gameUICanvas.gameObject.SetActive(false);
                _mainMenuCanvas.gameObject.SetActive(true);
            }
        }
    }
}