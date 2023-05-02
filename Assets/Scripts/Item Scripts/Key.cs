using TMPro;
using UnityEngine;

public class Key:MonoBehaviour, IInteractable {
	[SerializeField] private int id = 0;

	private TextMeshProUGUI itemText;
	private GameObject itemUI;
	public float MaxRange { get { return maxRange; } }
	private const float maxRange = 100f;

	// Start is called before the first frame update
	void Start() {
		itemUI = GameObject.Find("ItemUI");
		itemText = itemUI.GetComponentInChildren<TextMeshProUGUI>();
	}
	public void OnEndHover() {
		itemText.text = "";
	}

	public void OnInteract() {
		Inventory.keyIds.Add(id);
		Destroy(gameObject);
	}

	public void OnStartHover() {
		itemText.text = "Press " + CameraMovement.interactKey + " to pick up key";
	}
}