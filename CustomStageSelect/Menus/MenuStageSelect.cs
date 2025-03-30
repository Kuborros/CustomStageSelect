using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomStageSelect.Menus
{
    public class MenuStageSelect : MonoBehaviour
    {

        private FPObjectState state;
        private float genericTimer;
        private int buttonCount;

        private string selectedStageSceneName;

        [HideInInspector]
        public int menuSelection;

        public MenuOption[] menuOptions;
        public MenuCursor cursor;


        private void Start()
        {
            buttonCount = menuOptions.Length;
            FPStage.currentStage.SetRequestDisablePausing(this);




            state = new FPObjectState(State_Main);
        }

        private void Update()
        {
            if (FPStage.state != FPStageState.STATE_PAUSED)
            {
                FPStage.UpdateMenuInput(false);
            }
            if (FPStage.objectsRegistered && state != null)
            {
                state();
            }

        }

        private void State_Main()
        {


        }

        /// <summary>
        /// Warp to selected stage, or back to parent Adventure/Classic Menu
        /// </summary>
        private void State_Transition()
        {
            if (genericTimer < 30f)
            {
                genericTimer += FPStage.deltaTime;
            }
            else
            {
                FPScreenTransition component = GameObject.Find("Screen Transition").GetComponent<FPScreenTransition>();
                component.transitionType = FPTransitionTypes.WIPE;
                component.transitionSpeed = 48f;
                // "Play"
                if (menuSelection == 2)
                {
                    //Set to return to this menu instead of world map
                    FPSaveManager.previousStage = SceneManager.GetActiveScene().name;
                    //Set the destination to be the custom stage.
                    component.sceneToLoad = selectedStageSceneName;
                }
                // "Exit"
                else if (this.menuSelection == 3)
                {
                    //Set appropriate destination map.
                    if (FPSaveManager.gameMode == FPGameMode.ADVENTURE)
                    {
                        component.sceneToLoad = "AdventureMenu";
                        FPSaveManager.menuToLoad = 4;
                    }
                    else if (FPSaveManager.gameMode == FPGameMode.CLASSIC)
                    {
                        component.sceneToLoad = "ClassicMenu";
                        FPSaveManager.menuToLoad = 5;
                    }
                }
                component.SetTransitionColor(0f, 0f, 0f);
                //Whoosh to destination
                component.BeginTransition();
                FPAudio.PlayMenuSfx(3);
                base.enabled = false;
            }
        }

    }
}
