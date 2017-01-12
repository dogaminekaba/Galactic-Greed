using UnityEngine;
using System.Collections;

public class ShotController : MonoBehaviour {

    private GameObject enemyExplosion = null;
    private GameObject playerExplosion = null;

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
            GameController.score++;
            enemyExplosion = InputController.fac.createEnemyExplosion(other.transform.position);
        }
        if (this.tag == "Enemy Shot" && other.tag == "Player")
        {
            if (GameController.score > 0)
                GameController.score--;
            InputController.fac.createPlayerExplosion(other.transform.position);
        }
    }

    private void OnDestroy()
    {
        if (playerExplosion != null)
            Destroy(playerExplosion);
        if (enemyExplosion != null)
            Destroy(enemyExplosion);
    } 
}
