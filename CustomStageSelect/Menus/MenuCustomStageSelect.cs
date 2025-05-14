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
        private readonly int buttonCount = 3;
        private float[] startX;
        private float[] targetX;
        private SpriteRenderer[] menuButtons;

        private static bool introPlayed = false;
        private static int previousRunStageIndex = -1;

        private int lastStageIndex = -1;
        private int selectedStageIndex = 0;
        private string selectedStageSceneName;

        private List<CustomStage> stages = [];

        [HideInInspector]
        public int menuSelection;
        public float xOffsetRegular;
        public float xOffsetSelected;

        [Header("Prefabs")]
        public MenuConsoleCursor cursor;
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
            //Sort by stage id
            stages.Sort((x, y) => x.id.CompareTo(y.id));

            cursor = gameObject.transform.GetChild(1).gameObject.AddComponent<MenuConsoleCursor>();

            //Setup buttons
            pfButtons = new GameObject[] {
                gameObject.transform.GetChild(3).gameObject,
                gameObject.transform.GetChild(4).gameObject,
                gameObject.transform.GetChild(5).gameObject
            };

            startX = new float[pfButtons.Length];
            targetX = new float[pfButtons.Length];
            menuButtons = new SpriteRenderer[buttonCount];
            //Used for animations
            for (int i = 0; i < buttonCount; i++)
            {
                menuButtons[i] = pfButtons[i].GetComponent<SpriteRenderer>();
                startX[i] = pfButtons[i].transform.position.x;
                targetX[i] = pfButtons[i].transform.position.x;
            }
            //Restore previous index
            if (previousRunStageIndex >= 0)
                selectedStageIndex = previousRunStageIndex;

            //Show the loading screen if not seen in this 'session'
            if (!introPlayed)
            {
                genericTimer = 200f;
                gameObject.transform.GetChild(7).gameObject.SetActive(true);
                introPlayed = true;
                UpdateMenu();
                state = new FPObjectState(State_Intro);
            }
            else
            {
                gameObject.transform.GetChild(7).gameObject.SetActive(false);
                UpdateMenu();
                state = new FPObjectState(State_Main);
            }
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
                        cursor.transform.position = new Vector3(200, y, z);
                    }
                }
            }
            //Update button visuals
            switch (menuSelection)
            {
                case 0:
                    pfButtons[1].GetComponentInChildren<SuperTextMesh>().text = "< > LOAD SELECTED LEVEL";
                    pfButtons[2].GetComponentInChildren<SuperTextMesh>().text = "< > EXIT";
                    break;
                case 1:
                    pfButtons[1].GetComponentInChildren<SuperTextMesh>().text = "<*> LOAD SELECTED LEVEL";
                    pfButtons[2].GetComponentInChildren<SuperTextMesh>().text = "< > EXIT";
                    break;
                case 2:
                    pfButtons[1].GetComponentInChildren<SuperTextMesh>().text = "< > LOAD SELECTED LEVEL";
                    pfButtons[2].GetComponentInChildren<SuperTextMesh>().text = "<*> EXIT";
                    break;
            }
            //Set the lock icon
            if (stages.Count == 0)
            {
                pfButtons[1].GetComponentInChildren<SuperTextMesh>().text = "<!> MISSING STAGE DATA!";
            }

            //Stage info
            //Update only when needed
            if (lastStageIndex != selectedStageIndex) 
            {
                GameObject customStageDisplay = gameObject.transform.GetChild(0).gameObject;
                GameObject stageInfoBox = gameObject.transform.GetChild(2).gameObject;
                //Drop out if the custom stage display does not exist.
                if (customStageDisplay == null) return;
                //No custom stages loaded
                if (stages.Count == 0)
                {
                    //Set 'no stages loaded' text
                    pfButtons[0].GetComponentInChildren<SuperTextMesh>().text = "<c=green>CHOOSE STAGE NUM. TO INIT\n< NO STAGE DATA FOUND ></c>";

                    customStageDisplay.transform.GetChild(1).GetComponent<SuperTextMesh>().text = "<c=green>FILE:</c> <c=red>NO DATA</c>";
                    customStageDisplay.transform.GetChild(3).GetComponent<SuperTextMesh>().text = "--'--\"--";
                    customStageDisplay.transform.GetChild(5).GetComponent<SuperTextMesh>().text = "?";

                    stageInfoBox.transform.GetChild(0).GetComponent<SuperTextMesh>().text = "<c=green>DEVELOPER:</c> <c=red>NO DATA</c>";
                    stageInfoBox.transform.GetChild(1).GetComponent<SuperTextMesh>().text = "NO STAGE ROMS FOUND ON SYSTEM BOARD!\nPLEASE INSTALL REQUIRED CHIPS PER ARCADE OPERATOR MANUAL.\nFOR STAGE ROM PURCHASE, CONTACT ZAO ENTERPRISES.\nSALES ASSOCIATE CAN BE CALLED AT <c=yellow>0-800-GAMEBANANA</c>.";

                    //Set so we no longer keep retrying to render the menu.
                    lastStageIndex = selectedStageIndex;
                    return;
                }

                //Show custom stages.
                int stageID = stages[selectedStageIndex].id;
                string stageName, stageTime, stageAuthor, stageDescription;
                int stageRank;
                Sprite stageIcon;
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

                    pfButtons[0].GetComponentInChildren<SuperTextMesh>().text = "<c=green>CHOOSE STAGE NUM. TO INIT\n< STAGE " + (selectedStageIndex + 1) + "/" + (stages.Count) + " ></c>" ;

                    customStageDisplay.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = stageIcon;
                    customStageDisplay.transform.GetChild(1).GetComponent<SuperTextMesh>().text = "<c=green>FILE:</c> " + stageName;
                    customStageDisplay.transform.GetChild(3).GetComponent<SuperTextMesh>().text = stageTime;
                    customStageDisplay.transform.GetChild(5).GetComponent<SuperTextMesh>().text = RankToString(stageRank);

                    stageInfoBox.transform.GetChild(0).GetComponent<SuperTextMesh>().text = "<c=green>DEVELOPER:</c> " + stageAuthor;
                    stageInfoBox.transform.GetChild(1).GetComponent<SuperTextMesh>().text = stageDescription;
                }

                MenuLogSource.LogDebug("Set destination scene to: " + destinationScene);
                selectedStageSceneName = destinationScene;
                lastStageIndex = selectedStageIndex;
            }
        }

        private void State_Intro()
        {
            //Skip title screen with start button;
            if (FPStage.menuInput.pause) genericTimer = 0f;

            if (genericTimer > 0f)
            {
                genericTimer -= FPStage.deltaTime;
            }    
            else
            {
                gameObject.transform.GetChild(7).gameObject.SetActive(false);
                state = new FPObjectState(State_Main);
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
                    genericTimer = 10f;
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
                    FPStage.checkpointEnabled = false;
                    FPStage.checkpointPos = new Vector2(0f, 0f);
                    FPSaveManager.previousCheckpointPos = new Vector2(0f, 0f);
                    //Set to return to this menu instead of world map
                    FPSaveManager.previousStage = SceneManager.GetActiveScene().name;
                    //Important! This is how modded stages actually know what ID they are!
                    FPSaveManager.debugStageID = stages[selectedStageIndex].id;
                    FPSaveManager.debugStageName = stages[selectedStageIndex].name;
                    //Set the destination to be the custom stage.
                    component.sceneToLoad = selectedStageSceneName;
                    //Set the exit to our custom stage select.
                    PatchStageExits.returnToLevelSelect = true;
                    //Save last selected stage
                    previousRunStageIndex = selectedStageIndex;
                }
                // "Exit"
                else if (menuSelection == 2)
                {
                    //Reset the intro and last id
                    introPlayed = false;
                    previousRunStageIndex = -1;
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

        private string RankToString(int rank)
        {
            switch (rank)
            {
                case 0:
                case 1:
                    return "<c=brown>C</c>";
                case 2:
                    return "<c=silver>B</c>";
                case 3:
                    return "<c=orange>A</c>";
                case 4:
                    return "<c=yellow>S</c>";
                case 5:
                    return "<c=rainbow>S</c>";
                default:
                    return "<c=white>?</c>";
            }
        }
    }
}
