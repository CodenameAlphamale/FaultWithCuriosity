using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PhotoCapture:NetworkBehaviour {
	[Header("Camera Item Parameters")]
	[SerializeField] public static float charges = 3;
	public static bool canUseCamera = true;

	[Header("Flash Effect")]
	[SerializeField] private GameObject cameraFlash;
	[SerializeField] private float flashTime;

	private Texture2D screenCapture;
	public static bool viewingPhoto = false;

	// All text GUI needs to be in this parent object
	[Header("GUI")]
	[SerializeField] private GameObject GUI;

	[Header("Enemy Object")]
	[SerializeField] private GameObject objectEnemy;

	[Header("Polaroid GameObject")]
	[SerializeField] private GameObject itemPolaroid;

	// Code needed for finding specific polaroid
	private RaycastHit hitObject;
	private bool itemObject = false;
	private GameObject currentItemPolaroid;
	private GameObject currentPhotoFrame;

	// Gamble polaroid
	[Header("Gamble Parameters")]
	[SerializeField] private float gambleAffect = 10f;
	[SerializeField] private float maxGambleParanoia = 95f;
	[SerializeField] private float sphereRadius = 2f;
	private bool raycastEnemy = false;
	private RaycastHit enemyHit;
	private bool gamble;
	// default layer
	public LayerMask enemyLayer;

	// set ItemPolaroid Bool child to false - guarantee that the bool is false in start
	public override void OnNetworkSpawn() {
		if(!IsOwner)
			return;
		GUI = GameObject.Find("ItemUI");
		base.OnNetworkSpawn();
		itemPolaroid.transform.GetChild(2).gameObject.SetActive(false);
	}

	void Update() {
		if(!IsOwner || PauseMenu.paused || PauseMenu.pausedClient) {
			return;
		}

		if(Input.GetKeyDown(FirstPersonController.useCameraButton) && canUseCamera && !viewingPhoto && charges > 0) {
			// Raycast to see if player is looking at a Enemy - enemy must have collider
			raycastEnemy = Physics.SphereCast(transform.position, sphereRadius, transform.forward, out enemyHit, EnemyController.scareDistance, enemyLayer);

			cameraFlash.SetActive(true);
			if(raycastEnemy && enemyHit.collider.gameObject.CompareTag("Enemy")) {
				GambleRandom();
				if(!gamble) {
					EnemyController.ScareTeleport(transform.position);
				}
			}
			StartCoroutine(CapturePhoto());
			UseCamera();
		}
		// Raycast to see if player is looking at a Polaroid
		itemObject = Physics.Raycast(transform.position, transform.forward, out hitObject, 6f);
		if(!viewingPhoto && Input.GetKeyDown(CameraMovement.interactKey) && itemObject && hitObject.collider.gameObject.CompareTag("Polaroid")) {
			currentItemPolaroid = hitObject.collider.gameObject.transform.parent.gameObject;
			ShowPhoto();
		}

		// Close Photo
		if(viewingPhoto && Input.GetKeyDown(FirstPersonController.useCameraButton)) {
			RemovePhoto();
		}

	}
	/// <summary>
	/// Increases light intensity on object - CameraFlash
	/// </summary>
	/// <returns></returns>

	/// <summary>
	/// Captures the photo.
	/// </summary>
	/// <returns></returns>
	private IEnumerator CapturePhoto() {
		// Game UI set to false so that it does not show in screenshot
		GUI.SetActive(false);
		// Wait for end of frame so that the UI is not captured in the screenshot
		yield return new WaitForEndOfFrame();

		// Takes a screenshot of the screen
		Rect regionToRead = new((Screen.width - Screen.height) / 2, 0, Screen.height, Screen.height);
		Texture2D screenCapture = new(1024, 1024, TextureFormat.RGB24, false);
		screenCapture.ReadPixels(regionToRead, 0, 0, false);
		screenCapture.Apply();

		// Makes a sprite of the screenshot
		Sprite photoSprite = Sprite.Create(screenCapture, new Rect(0, 0, screenCapture.width, screenCapture.height), new Vector2(0, 0), 50);
		// Encodes the sprite to a byte array to send to server.
		byte[] pictureBytes = photoSprite.texture.EncodeToJPG();

		// Sets PhotoFrameBG (Blank canvas) in ItemPolaroidObject to true
		// the coroutine below needs to be located AFTER the picture is being taken!
		StartCoroutine(CameraFlashEffect());
		GUI.SetActive(true);

		//Spawn the polaroid with the picture
		SpawnItemPolaroid(pictureBytes);

		//Scare the enemy away
		EnemyController.ScareTeleport(transform.position);

		//Scare the enemy away
		if(gamble) {
			EnemyController.ScareTeleport(transform.position);
		}
	}

	/// <summary>
	/// Shows the photo.
	/// </summary>
	public void ShowPhoto() {
		// Set viewingPhoto to true so that player cannot move
		viewingPhoto = true;
		GUI.SetActive(false);

		// Object is hardcoded and only The ItemPolaroid within the ItemPolaroid should be Tagged "Polaroid"
		// Sets PhotoFrameBG (Blank canvas) in ItemPolaroidObject to true
		currentPhotoFrame = currentItemPolaroid.transform.GetChild(1).GetChild(0).gameObject;
		currentPhotoFrame.SetActive(true);

		// Destroy the visable body of the ItemPolaroid
		GameObject currentItemPolaroidBody = currentItemPolaroid.transform.GetChild(0).gameObject;
		Destroy(currentItemPolaroidBody);

		// Polaroid Gamble affect
		gamble = itemPolaroid.transform.GetChild(2).gameObject.activeSelf;

		// Apply Polaroid Gamble affect
		if(gamble) {
			if(Flashlight.currentParanoia >= maxGambleParanoia - gambleAffect && Flashlight.currentParanoia < maxGambleParanoia) {
				Flashlight.currentParanoia = maxGambleParanoia;
			} else if(Flashlight.currentParanoia < maxGambleParanoia) {
				Flashlight.currentParanoia += gambleAffect;
			}
		} else {
			if(Flashlight.currentParanoia <= gambleAffect) {
				Flashlight.currentParanoia = 0f;
			} else {
				Flashlight.currentParanoia -= gambleAffect;
			}
		}
	}

	/// <summary>
	/// Removes the photo.
	/// </summary>
	private void RemovePhoto() {
		// Set viewingPhoto to false so that player can move
		viewingPhoto = false;
		currentPhotoFrame.SetActive(false);
		NetworkObjectReference polaroid = currentItemPolaroid.GetComponent<NetworkObject>();
		DeletePolaroidServerRpc(polaroid);
		GUI.SetActive(true);
	}

	/// <summary>
	/// Spawns the item polaroid in a random location around infront of the player.
	/// </summary>
	private void SpawnItemPolaroid(byte[] pictureBytes) {
		// Random position and rotation
		Vector3 randomPosition = new(
			Random.Range(FirstPersonController.characterController.transform.position.x, FirstPersonController.characterController.transform.position.x + 0.5f),
			Random.Range(FirstPersonController.characterController.transform.position.y + 0.5f, FirstPersonController.characterController.transform.position.y + 1.5f),
			Random.Range(FirstPersonController.characterController.transform.position.z, FirstPersonController.characterController.transform.position.z + 0.5f)
			);

		Quaternion randomRotation = Random.rotation;
		// Spawn Polaroid
		SpawnPolaroidServerRpc(randomPosition, randomRotation, pictureBytes);
	}

	/// <summary>
	/// Method for using camera if the button is pressed and player canUseCamera is true,
	/// charges exceed 0
	/// </summary>
	private void UseCamera() {
		charges--;
	}

	/// <summary>
	/// Method for randomizing the polaroid haunted affect, 1 is true and 0 is false
	/// </summary>
	private void GambleRandom() {
		gamble = Random.Range(0, 2) == 1 ? true : false;
		Debug.Log(gamble);
		itemPolaroid.transform.GetChild(2).gameObject.SetActive(gamble);
	}

	/// Increases light intensity on object - CameraFlash
	/// </summary>
	/// <returns></returns>
	private IEnumerator CameraFlashEffect() {
		yield return new WaitForSeconds(flashTime);
		cameraFlash.SetActive(false);
	}

	/// <summary>
	/// SpawnPolaroid is spawning the polaroid picture on the server with the correct screenshot.
	/// </summary>
	/// <param name="pos">pos is a position vector close to the player where the polaroid spawns</param>
	/// <param name="rot">rot is a random rotation</param>
	/// <param name="pictureBytes">pictureBytes is the screenshot encrypted as a jpg byte array</param>
	[ServerRpc]
	private void SpawnPolaroidServerRpc(Vector3 pos, Quaternion rot, byte[] pictureBytes) {
		GameObject newPolaroid = Instantiate(itemPolaroid, pos, rot);
		newPolaroid.GetComponent<NetworkObject>().Spawn();
		NetworkObjectReference newPol = newPolaroid;
		ShowPictureClientRpc(newPol, pictureBytes);
	}

	/// <summary>
	/// DeletePolaroid is despawning a polaroid from the server.
	/// </summary>
	/// <param name="polaroid">polaroid is a reference to the current polaroid that is supposed to be despawned</param>
	[ServerRpc]
	private void DeletePolaroidServerRpc(NetworkObjectReference polaroid) {
		NetworkObject polaroid1 = polaroid;
		polaroid1.Despawn();
	}

	/// <summary>
	/// ShowPicture updates the picture of the polaroid for all clients on the server
	/// </summary>
	/// <param name="polaroid">polaroid refers to the newly spawned polaroid item</param>
	/// <param name="pictureBytes">pictureBytes is the screenshot encrypted as a jpg byte array</param>
	[ClientRpc]
	private void ShowPictureClientRpc(NetworkObjectReference polaroid, byte[] pictureBytes) {
		Texture2D pictureTexture = new(2, 2);
		pictureTexture.LoadImage(pictureBytes);
		Sprite pictureSprite = Sprite.Create(pictureTexture, new Rect(0, 0, pictureTexture.width, pictureTexture.height), new Vector2(0, 0), 50);
		NetworkObject newPolaroid = polaroid;
		newPolaroid.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(1).gameObject.GetComponent<Image>().sprite = pictureSprite;
	}
}
