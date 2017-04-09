using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class TutoSceneManager : MonoBehaviour {

    GameObject gameManager;

	// Use this for initialization
	void Start () {
        gameManager=GameObject.Find("GameManager");
    }
	
	// Update is called once per frame
	void Update () {
        if (gameManager.GetComponent<GameManager>().m_gameOver) {
            if (SceneManager.GetActiveScene().name == "Tuto1")
            {
                SceneManager.LoadScene("Tuto2");
            }
            else if (SceneManager.GetActiveScene().name == "Tuto2")
            {
                SceneManager.LoadScene("Tuto3");
            }
            else if (SceneManager.GetActiveScene().name == "Tuto3")
            {
                SceneManager.LoadScene("Main");
            }
        }
    }
}
