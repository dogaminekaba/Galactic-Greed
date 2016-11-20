using UnityEngine;
using System.Collections;

public class BorderController : MonoBehaviour {

	void OnTriggerExit(Collider other) {
        Destroy(other.gameObject);
    }
}
