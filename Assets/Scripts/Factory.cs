using UnityEngine;
using System.Collections;

public class Factory : MonoBehaviour {

    [SerializeField]
    private GameObject shotPrefab;
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private GameObject explosionEnemyPrefab;
    [SerializeField]
    private GameObject explosionPlayerPrefab;

    public GameObject createShot()
    {
        if (shotPrefab == null)
            return null;
        return Instantiate(shotPrefab);
    }

    public GameObject createEnemy()
    {
        if (enemyPrefab == null)
            return null;
        else
            return Instantiate(enemyPrefab);
    }

    public GameObject createEnemyExplosion(Vector3 pos)
    {
        if (explosionEnemyPrefab == null)
            return null;
        return Instantiate(explosionEnemyPrefab, pos, Quaternion.identity);
    }

    public GameObject createPlayerExplosion(Vector3 pos)
    {
        if (explosionPlayerPrefab == null)
            return null;
        return Instantiate(explosionPlayerPrefab, pos, Quaternion.identity);
    }
}
