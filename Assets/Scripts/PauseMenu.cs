using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenu:MonoBehaviour {

	public static bool paused = false;
	[SerializeField] private GameObject PauseMenuCanvas;
	// Start is called before the first frame update
	void Start() {
		Time.timeScale = 1.0f;
	}

	// Update is called once per frame
	void Update() {
		if(Input.GetKeyDown(KeyCode.Escape)) {
			if(paused) {
				Play();
			} else {
				Stop();
			}
		}
	}

	public void Stop() {
		PauseMenuCanvas.SetActive(true);
		Time.timeScale = 0f;
		paused = true;
		Cursor.lockState = CursorLockMode.None;
	}
	public void Play() {
		PauseMenuCanvas.SetActive(false);
		Time.timeScale = 1f;
		paused = false;
		Cursor.lockState = CursorLockMode.Locked;
	}
	public void MainMenuButton() {
		SceneManager.LoadScene("MainMenu");
	}

	public void Quit() {
		Application.Quit();
		Debug.Log("Player Has Quit The Game");
	}
}