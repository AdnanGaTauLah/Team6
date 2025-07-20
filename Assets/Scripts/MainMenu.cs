using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject chooseCharacter;
    public void PlayGame()
    {
        //Display Choose Character UI
        chooseCharacter.active = true;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    //Player select Mom
    public void Character(string character)
    {
        SelectCharacter.selectedCharacter = character;
        SceneManager.LoadScene("GameplaySecene");
    }
 
}
