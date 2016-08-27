using UnityEngine;
using System.Collections;

public class Script_Toggle_Click_DrawDebug : MonoBehaviour, ButtonScript {
    public GameObject overlay;

    public void onClick() {
        if (overlay != null) {
            overlay.SetActive(!overlay.activeSelf);
        }
    }
}
