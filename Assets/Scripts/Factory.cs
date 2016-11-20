using UnityEngine;
using System.Collections;

public class Factory : MonoBehaviour {

    [SerializeField]
    private GameObject shotPrefab;

	public GameObject createShot()
    {
        if (shotPrefab == null)
            return null;
        return (GameObject)Instantiate(shotPrefab, new Vector3(0, 0, -0.5f), Quaternion.identity);
    }

}
