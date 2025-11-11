using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Função chamada ao clicar no botão "Jogar"
    public void PlayGame()
    {
        SceneManager.LoadScene(2); // carrega a cena no índice 1
    }
    // Função chamada ao clicar no botão "Sair"
    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }
}
