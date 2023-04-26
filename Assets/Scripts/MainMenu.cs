using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu:MonoBehaviour {
	public void Play() {
		SceneManager.LoadScene("Dungeon");
	}
	public void Quit() {
		Application.Quit();
		Debug.Log("Player Has Quit The Game");
	}
}
