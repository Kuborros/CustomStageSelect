using BepInEx;
using BepInEx.Logging;
using CustomStageSelect.Patches;
using FP2Lib.Stage;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomStageSelect.Menus
{
    public class MenuCustomStageSelect : MonoBehaviour
    {
        private static readonly ManualLogSource MenuLogSource = CustomStageSelect.Logger;

        private FPObjectState state;
        private float genericTimer;
        private int buttonCount = 3;
        private float[] startX;
        private float[] targetX;
        private SpriteRenderer[] menuButtons;

        public Sprite[] menuSpritesRegular;
        public Sprite[] menuSpritesSelected;

        private int lastStageIndex = -1;
        private int selectedStageIndex = 0;
        private string selectedStageSceneName;

        private List<CustomStage> stages = [];

        [HideInInspector]
        public int menuSelection;
        public float xOffsetRegular;
        public float xOffsetSelected;

        [Header("Prefabs")]
        public MenuCursor cursor;
        public GameObject[] pfButtons;

        private void Start()
        {
            FPStage.currentStage.SetRequestDisablePausing(this);

            //Load FP2Lib provided stages
            foreach (CustomStage stage in StageHandler.Stages.Values) 
            {
                MenuLogSource.LogDebug("Parsing stage: " + stage.uid);
                //Only add stages which want to be shown, and are not HUB stages. Skip ones who have no scene string.
                if (!stage.isHUB && stage.showInCustomStageLoaders && !stage.sceneName.IsNullOrWhiteSpace())
                {
                    MenuLogSource.LogDebug("Adding stage: " + stage.name);
                    stages.Add(stage);
                }
            }
            cursor = gameObject.GetComponentInChildren<MenuCursor>();

            //Setup buttons
            pfButtons = new GameObject[] {
                gameObject.transform.GetChild(3).gameObject,
                gameObject.transform.GetChild(4).gameObject,
                gameObject.transform.GetChild(5).gameObject
            };

            menuSpritesRegular = new Sprite[]
            {
                CustomStageSelect.menuAssets.LoadAsset<Sprite>("lock"),
                CustomStageSelect.menuAssets.LoadAsset<Sprite>("play_off"),
                CustomStageSelect.menuAssets.LoadAsset<Sprite>("stop_off")
            };

            menuSpritesSelected = new Sprite[]
{
                CustomStageSelect.menuAssets.LoadAsset<Sprite>("lock"),
                CustomStageSelect.menuAssets.LoadAsset<Sprite>("play_on"),
                CustomStageSelect.menuAssets.LoadAsset<Sprite>("stop_on")
};

            startX = new float[pfButtons.Length];
            targetX = new float[pfButtons.Length];
            menuButtons = new SpriteRenderer[buttonCount];
            //Used for animations, maybe remove since we don't do that exactly?
            for (int i = 0; i < buttonCount; i++)
            {
                menuButtons[i] = pfButtons[i].GetComponent<SpriteRenderer>();
                startX[i] = pfButtons[i].transform.position.x;
                targetX[i] = pfButtons[i].transform.position.x;
            }
            //Default "Locked" sprite for "Play" button
            pfButtons[1].GetComponent<SpriteRenderer>().sprite = menuSpritesRegular[0];

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
                //Move cursor to selection
                if (i == menuSelection)
                {
                    if (i > 0)
                    {
                        cursor.transform.position = new Vector3(num2 - 32f, y, z);
                    }
                    else
                    {
                        cursor.transform.position = new Vector3(num2 - 96f, y, z);
                    }
                }
            }
            //Update button visuals
            switch (menuSelection)
            {
                case 0:
                    pfButtons[1].GetComponent<SpriteRenderer>().sprite = menuSpritesRegular[1];
                    pfButtons[2].GetComponent<SpriteRenderer>().sprite = menuSpritesRegular[2];
                    break;
                case 1:
                    pfButtons[1].GetComponent<SpriteRenderer>().sprite = menuSpritesSelected[1];
                    pfButtons[2].GetComponent<SpriteRenderer>().sprite = menuSpritesRegular[2];
                    break;
                case 2:
                    pfButtons[1].GetComponent<SpriteRenderer>().sprite = menuSpritesRegular[1];
                    pfButtons[2].GetComponent<SpriteRenderer>().sprite = menuSpritesSelected[2];
                    break;
            }
            //Set the lock icon
            if (stages.Count == 0)
            {
                pfButtons[1].GetComponent<SpriteRenderer>().sprite = menuSpritesRegular[0];
            }

            //Stage info
            //Update only when needed
            if (lastStageIndex != selectedStageIndex) 
            {
                GameObject customStageDisplay = GameObject.Find("CustomStageData");
                GameObject stageInfoBox = GameObject.Find("StageInfoBox");
                //Drop out if the custom stage display does not exist.
                if (customStageDisplay == null) return;

                int stageID = stages[selectedStageIndex].id;
                string stageName, stageTime, stageAuthor, stageDescription;
                int stageRank = 0;
                Sprite stageIcon = null;
                string destinationScene = "ErrorScene";
                //Make sure we did not get a cursed broken stage
                if (stageID <= FPSaveManager.timeRecord.Length + 1)
                {
                    stageName = stages[selectedStageIndex].name;
                    stageTime = FPStage.TimeToString(FPSaveManager.timeRecord[stageID]);
                    stageRank = FPSaveManager.timeRank[stageID];
                    stageAuthor = stages[selectedStageIndex].author;
                    stageDescription = stages[selectedStageIndex].description;
                    stageIcon = stages[selectedStageIndex].preview;
                    destinationScene = stages[selectedStageIndex].sceneName;

                    pfButtons[0].GetComponentInChildren<TextMesh>().text = "< Stage " + (selectedStageIndex + 1) + "/" + (stages.Count) + " >" ;

                    customStageDisplay.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = stageIcon;
                    customStageDisplay.transform.GetChild(1).GetComponent<TextMesh>().text = stageName;
                    customStageDisplay.transform.GetChild(3).GetComponent<TextMesh>().text = stageTime;
                    customStageDisplay.transform.GetChild(5).GetComponent<FPHudDigit>().SetDigitValue(stageRank - 1);

                    stageInfoBox.GetComponentInChildren<TextMesh>().text = "By: " + stageAuthor;
                    stageInfoBox.GetComponentInChildren<SuperTextMesh>().text = stageDescription;
                }

                MenuLogSource.LogDebug("Set destination scene to: " + destinationScene);
                selectedStageSceneName = destinationScene;
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
                if (menuSelection > 2)
                {
                    menuSelection = 2;
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
                    //If we only have one stage, we are on proper number already
                    if (stages.Count > 1)
                    {
                        if (selectedStageIndex < stages.Count - 1)
                        {
                            selectedStageIndex++;
                        }
                        else selectedStageIndex = 0;
                        FPAudio.PlayMenuSfx(1);
                    }
                }
                //Bottom buttons
                else if (menuSelection == 1)
                {
                    menuSelection++;
                    FPAudio.PlayMenuSfx(1);
                }
            }
            if (FPStage.menuInput.left)
            {
                //Level Selector
                if (menuSelection == 0)
                {
                    //Only process if there are stages to work with.
                    if (stages.Count > 1)
                    {
                        if (selectedStageIndex > 0)
                        {
                            selectedStageIndex--;
                        }
                        else selectedStageIndex = stages.Count - 1;
                        FPAudio.PlayMenuSfx(1);
                    }
                }
                //Bottom buttons
                else if (menuSelection <= 2)
                {
                    menuSelection--;
                    FPAudio.PlayMenuSfx(1);
                }
            }
            if (genericTimer > 0f)
            {
                genericTimer -= FPStage.deltaTime;
            }
            //Handle Play and Exit buttons.
            else if (FPStage.menuInput.confirm && menuSelection > 0)
            {
                //Case for no stages loaded
                if (stages.Count == 0 && menuSelection == 1)
                {
                    genericTimer = 0f;
                    FPAudio.PlayMenuSfx(21);
                    cursor.optionSelected = true;
                }
                //Proper stage or exit selected
                else
                {
                    genericTimer = 0f;
                    state = new FPObjectState(State_Transition);
                    cursor.optionSelected = true;
                    FPAudio.PlayMenuSfx(2);
                }
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
                    //Important! This is how modded stages actually know what ID they are!
                    FPSaveManager.debugStageID = stages[selectedStageIndex].id;
                    FPSaveManager.debugStageName = stages[selectedStageIndex].name;
                    //Set the destination to be the custom stage.
                    component.sceneToLoad = selectedStageSceneName;
                    //Set the exit to our custom stage select.
                    PatchStageExits.returnToLevelSelect = true;
                }
                // "Exit"
                else if (menuSelection == 2)
                {
                    //Just to be sure
                    PatchStageExits.returnToLevelSelect = false;
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
