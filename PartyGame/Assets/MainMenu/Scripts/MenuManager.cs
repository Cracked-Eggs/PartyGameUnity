using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Luakite
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private Animator fader;

        private bool switchingScene = false;

        public void QuitGame()
        {
            Application.Quit();
        }

        public void FadeIn()
        {
            fader.SetTrigger("FadeIn");
        }

        public void FadeOut()
        {
            fader.SetTrigger("FadeOut");
        }

        public void SwitchScene(string newLvlName)
        {
            SceneManager.LoadScene(newLvlName);
        }

        public void SwitchSceneWFade(string newLvlName)
        {
            if (switchingScene)
                return;

            StartCoroutine(LoadLvlWFade(newLvlName));
            switchingScene = true;
        }

        public IEnumerator LoadLvlWFade(string newLvl)
        {
            FadeIn();

            yield return new WaitForSeconds(1.75f);

            SwitchScene(newLvl);
        }
    }
}
