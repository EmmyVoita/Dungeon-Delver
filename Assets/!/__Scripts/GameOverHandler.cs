using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverHandler : MonoBehaviour
{
    public void OnGameOver()
    {
        Debug.Log("Game Over!");
        //HighScoreManager.SaveScore(finalScore);
         SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        //StartCoroutine(RestartAfterDelay());
    }

    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Reloading scene...");
            SceneManager.LoadScene("MainMenuScene");
        }
        */
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(1.0f); // optional fade-out
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        Debug.Log("Reloading scene...");
       
    }
}
