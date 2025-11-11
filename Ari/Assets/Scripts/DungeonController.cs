using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DungeonController : MonoBehaviour
{
    [SerializeField] private float waitTime = 7f;
    [SerializeField] private string nextScene = "Level_1_Design";
    [SerializeField] private FadeManager fade; // arraste o FadePanel (com FadeManager)


    private void Start()
    {
        StartCoroutine(RunIntro());
    }

    private IEnumerator RunIntro()
    {
        // Fade-in (de preto -> cena)
        if (fade) yield return fade.FadeIn();

        // tempo de leitura do texto
        yield return new WaitForSeconds(waitTime);

        // Fade-out (cena -> preto)
        if (fade) yield return fade.FadeOut();

        // carrega o jogo
        SceneManager.LoadScene(nextScene);
    }
}
