using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IpPortInputScript:NetworkBehaviour {
    [SerializeField] private TMP_InputField ipAndPort;
    [SerializeField] private Button joinButton;
    string inputText;

    public ushort portNumber;
    public string ipAddress;

    void Start() {
        joinButton.onClick.AddListener(OnSubmitInfo);
    }

    private void OnSubmitInfo() {
        inputText = ipAndPort.text;
        string[] stringParts = inputText.Split(":");
        if(stringParts.Length == 2) {
            ipAddress = stringParts[0].Trim();
            ushort.TryParse(stringParts[1].Trim(), out portNumber);
            if(portNumber > 65535) {
                portNumber = 7777;
            } else if(portNumber < 0) {
                portNumber = 7777;
            }
            NetworkManager.Singleton.GetComponentInChildren<UnityTransport>().SetConnectionData(ipAddress, portNumber);
            NetworkManager.Singleton.StartClient();
            SceneManager.LoadScene("Dungeon");
        }
    }
}
