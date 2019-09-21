namespace JumpGame
{
    using UnityEngine;

    public class GameInitializer : MonoBehaviour
    {
        [SerializeField]
        private GuiManager guiManager;

        // Start is called before the first frame update
        void Start()
        {
            guiManager?.ShowWindow("UI_MAIN_MENU");
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}