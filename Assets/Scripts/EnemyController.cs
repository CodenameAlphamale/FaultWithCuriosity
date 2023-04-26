using UnityEngine;
using UnityEngine.AI;

public class EnemyController:MonoBehaviour {
	[SerializeField] private float damageMultiplier = 0.1f;
	[SerializeField] private float damageDistance = 3f;

	private NavMeshAgent enemyAIAgent;
	private GameObject player;
	private float distanceToPlayer;
	private float currentParanoia;

	// Start is called before the first frame update
	void Start() {
		player = GameObject.Find("Player Aaron");
		enemyAIAgent = gameObject.GetComponent<NavMeshAgent>();
	}

	// Update is called once per frame
	void Update() {
		currentParanoia = Flashlight.currentParanoia;

		//Get the current paranoia 10s for distance
		int paranoiaDistance = 105 - (int)currentParanoia * 10 / 10;

		distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

		//If the player is further than the paranoiaDistance, move the AI closer.
		if(distanceToPlayer > paranoiaDistance) {
			enemyAIAgent.destination = player.transform.position;
			enemyAIAgent.stoppingDistance = paranoiaDistance;
		}

		DealDamage();
	}

	/// <summary>
	/// Increase the paranoia level of the <see cref="player"/>.
	/// </summary>
	private void DealDamage() {
		if(distanceToPlayer < damageDistance && currentParanoia < 100) {
			Flashlight.currentParanoia += (damageDistance - distanceToPlayer) * damageMultiplier;
		}
	}
}
