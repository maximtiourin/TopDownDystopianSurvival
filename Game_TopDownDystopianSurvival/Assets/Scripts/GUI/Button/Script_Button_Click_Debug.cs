using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Script_Button_Click_Debug : MonoBehaviour, ButtonScript {
    public GameObject debugPanel;

	public void onClick() {
        if (debugPanel != null) {
            debugPanel.SetActive(!debugPanel.activeInHierarchy);
        }
    }
}
