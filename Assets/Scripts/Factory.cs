using UnityEngine;
using System.Collections;

public class Factory : MonoBehaviour {

    [SerializeField]
    private GameObject shotPrefab;

	public GameObject createShot()
    {
        if (shotPrefab == null)
            return null;
        return Instantiate(shotPrefab);
    }

}
