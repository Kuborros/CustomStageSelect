using FP2Lib.Stage;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomStageSelect.Menus
{
    public class MenuStageSelect : MonoBehaviour
    {

        private FPObjectState state;
        private float genericTimer;
        private int buttonCount;
        private float[] startX;
        private float[] targetX;

        private int lastStageIndex;
        private int selectedStageIndex;
        private string selectedStageSceneName;

        private List<CustomStage> stages;

        [HideInInspector]
        public int menuSelection;

        public float xOffsetRegular;
        public float xOffsetSelected;

        [Header("Prefabs")]
        public MenuOption[] menuOptions;
        public MenuCursor cursor;
        public GameObject[] pfButtons;
        public GameObject pfTextBox;

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

        private void UpdateMenu()
        {
            float num = 5f * FPStage.frameScale;
            //Buttons
            for (int i = 0; i < buttonCount; i++)
            {
                float num2 = (pfButtons[i].transform.position.x * (num - 1f) + targetX[i]) / num;
                float y = pfButtons[i].transform.position.y;
                float z = pfButtons[i].transform.position.z;
                if (i == menuSelection)
                {
                    cursor.transform.position = new Vector3(num2 - 32f, y, z);
                }
                pfButtons[i].transform.position = new Vector3(num2, y, z);
                targetX[i] = 320f + xOffsetRegular;
                if (i == menuSelection)
                {
                    pfTextBox.transform.position = new Vector3(pfButtons[i].transform.position.x + 144f,pfButtons[i].transform.position.y, pfTextBox.transform.position.z);
                    targetX[menuSelection] = 320f + xOffsetSelected;
                }
            }
            //Stage info
            //Update only when needed
            if (lastStageIndex != selectedStageIndex) 
            {
                int stageID = stages[selectedStageIndex].id;
                string stageName, stageTime, stageAuthor, stageDescription;
                string destinationScene = "ErrorScene";
                //Make sure we did not get a cursed stage
                if (stageID <= FPSaveManager.timeRecord.Length)
                {
                    stageName = stages[selectedStageIndex].name;
                    stageTime = FPStage.TimeToString(stages[selectedStageIndex].parTime);
                    stageAuthor = stages[selectedStageIndex].author;
                    stageDescription = stages[selectedStageIndex].description;
                    destinationScene = stages[selectedStageIndex].sceneName;
                }
                
                
            

                lastStageIndex = selectedStageIndex;
            }
        }

        private void State_Main()
        {
            //Up-Down controls
            if (FPStage.menuInput.up)
            {
                menuSelection--;
                if (menuSelection < 0)
                {
                    menuSelection = 0;
                }
                else FPAudio.PlayMenuSfx(1);
            }
            else if (FPStage.menuInput.down)
            {
                menuSelection++;
                if (menuSelection >= buttonCount)
                {
                    menuSelection = buttonCount;
                }
                else FPAudio.PlayMenuSfx(1);
            }
            //Straight to 'Exit'
            else if (FPStage.menuInput.cancel)
            {
                menuSelection = 2;
                genericTimer = 10f;
                FPAudio.PlayMenuSfx(1);
            }

            //Left-Right controls
            if (FPStage.menuInput.right)
            {
                //Level Selector
                if (menuSelection == 0)
                {
                    //Only process if there are stages to work with.
                    if (stages.Count != 0)
                    {
                        if (selectedStageIndex < stages.Count)
                        {
                            selectedStageIndex++;
                        }
                        else selectedStageIndex = 0;
                    }
                }
                //Bottom buttons
                else if (menuSelection == 1)
                {
                    menuSelection++;
                }
                FPAudio.PlayMenuSfx(1);
            }
            if (FPStage.menuInput.left)
            {
                //Level Selector
                if (menuSelection == 0)
                {
                    //Only process if there are stages to work with.
                    if (stages.Count != 0)
                    {
                        if (selectedStageIndex > 0)
                        {
                            selectedStageIndex--;
                        }
                        else selectedStageIndex = stages.Count - 1;
                    }
                }
                //Bottom buttons
                else if (menuSelection == 2)
                {
                    menuSelection--;
                }
                FPAudio.PlayMenuSfx(1);
            }
            if (genericTimer > 0f)
            {
                genericTimer -= FPStage.deltaTime;
            }
            //Handle Play and Exit buttons.
            else if (FPStage.menuInput.confirm && menuSelection > 1)
            {
                genericTimer = 0f;
                state = new FPObjectState(State_Transition);
                cursor.optionSelected = true;
                FPAudio.PlayMenuSfx(2);
            }
            UpdateMenu();
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
                if (menuSelection == 1)
                {
                    //Set to return to this menu instead of world map
                    FPSaveManager.previousStage = SceneManager.GetActiveScene().name;
                    //Set the destination to be the custom stage.
                    component.sceneToLoad = selectedStageSceneName;
                }
                // "Exit"
                else if (menuSelection == 2)
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
                enabled = false;
            }
        }

    }
}
