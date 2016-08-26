using UnityEngine;
using System.Collections;

public class Script_Button_Click_Debug : MonoBehaviour, ButtonScript {
	public void onClick() {
        Debug.Log("Clicked");
    }
}
