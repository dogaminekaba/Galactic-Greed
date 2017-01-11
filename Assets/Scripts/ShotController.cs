using UnityEngine;
using System.Collections;

public class ShotController : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        if (this.tag == "Player Shot" && other.tag == "Enemy")
        {
            InputController.fac.createEnemyExplosion(other.transform.position);
        }
        if (this.tag == "Enemy Shot" && other.tag == "Player")
        {
            Debug.Log("shot!");
            InputController.fac.createPlayerExplosion(other.transform.position);
        }
    }
}
