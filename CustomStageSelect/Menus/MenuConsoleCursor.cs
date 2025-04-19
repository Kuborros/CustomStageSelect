using UnityEngine;

namespace CustomStageSelect.Menus
{
    public class MenuConsoleCursor : MonoBehaviour
    {

        [HideInInspector]
        public bool optionSelected;
        [HideInInspector]
        public float timer;
        private SuperTextMesh textMesh;

        private void Start()
        {
            textMesh = GetComponent<SuperTextMesh>();
        }

        private void Update()
        {
            if (optionSelected && timer <= 0f)
            {
                timer = 30f;
                textMesh.text = "<c=red>---></c>";
            }
            else if (timer > 0f)
            {
                timer -= FPMenu.deltaTime;
                if (timer <= 0f)
                {
                    optionSelected = false;
                    textMesh.text = "<c=yellow>---></c>";
                }
            }
        }

        public void Refresh(float t)
        {
            optionSelected = true;
            timer = t;
            textMesh.text = "<c=red>---></c>";
        }
    }
}
