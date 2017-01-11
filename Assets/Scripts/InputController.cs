using UnityEngine;
using System.Collections;
using System.Threading;
using System;

public class InputController : MonoBehaviour {

    public static bool gamePaused;
    public static float shipRotationZ;
    public static bool shotFire = false;
    public static Factory fac;

    [SerializeField] 
    private GameObject usersShip;
    [SerializeField]
    private GameObject [] enemyShips;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private int width;
    [SerializeField]
    private int height;
    [SerializeField]
    private float speed;
    private float shotSpeed;

    private float xMax, xMin, yMax, yMin;

    private float userPosX = 0;
    private float userPosY = 0;

    // input flag
    private bool DEBUG_CODE = true;

    // Use this for initialization
    void Start () {
        fac = GetComponent<Factory>();

        enemyShips = new GameObject[ClientController.serverSize];
        for (int i = 0; i < ClientController.serverSize; ++i)
        {
            enemyShips[i] = fac.createEnemy();
            enemyShips[i].SetActive(false);
        }

        xMin = -width * 2.0f / 5;
        xMax = width * 2.0f / 5;
        yMin = -height / 3;
        yMax = height / 3;
        gamePaused = false;
        shotSpeed = 400 * speed;
        Input.gyro.enabled = true;
	}
	
	// Update is called once per frame
	void Update () {
        movePlayer();
        showOthers();
	}

    private void showOthers()
    {
        if (ClientController.listLock.WaitOne(15))
        {
            for (int i = 0; i < ClientController.serverSize; ++i)
            {
                if(ClientController.infoList[i].hold)
                {

                    enemyShips[i].transform.position = new Vector3( ClientController.infoList[i].x,
                                                                    ClientController.infoList[i].y,
                                                                    usersShip.transform.position.z);
                    enemyShips[i].transform.rotation = Quaternion.identity;
                    enemyShips[i].transform.Rotate(Vector3.forward, ClientController.infoList[i].theta);
                    enemyShips[i].SetActive(true);
                    if(ClientController.infoList[i].shotFired)
                    {
                        // Create the shot
                        GameObject shot = fac.createShot();
                        shot.transform.position = new Vector3(enemyShips[i].transform.position.x, enemyShips[i].transform.position.y, enemyShips[i].transform.position.z + 0.05f);
                        shot.transform.rotation = enemyShips[i].transform.rotation;
                        float velShotX = shotSpeed * Time.deltaTime * Mathf.Sin(ClientController.infoList[i].theta * Mathf.Deg2Rad);
                        float velShotY = shotSpeed * Time.deltaTime * Mathf.Cos(ClientController.infoList[i].theta * Mathf.Deg2Rad);
                        shot.tag = "Enemy Shot";
                        shot.GetComponent<Rigidbody>().velocity = new Vector3(velShotX, velShotY, 0);
                    }
                }
                else
                    enemyShips[i].SetActive(false);
            }
            ClientController.listLock.ReleaseMutex();
        }
    }

    void OnApplicationQuit()
    {
        ClientController.programEnded = true;
        Thread.Sleep(500);
        Debug.Log("finished.");
    }

    private void movePlayer()
    {
        if (!gamePaused)
        {
            float moveHorizontal;
            float moveVertical;

            // Shooting
            if (Input.GetMouseButtonUp(0))
            {
                // Create the shot
                GameObject shot = fac.createShot();
                shot.transform.position = new Vector3(usersShip.transform.position.x, usersShip.transform.position.y, usersShip.transform.position.z + 0.05f);
                shot.transform.rotation = usersShip.transform.rotation;
                float velShotX = shotSpeed * Time.deltaTime * Mathf.Sin(shipRotationZ * Mathf.Deg2Rad);
                float velShotY = shotSpeed * Time.deltaTime * Mathf.Cos(shipRotationZ * Mathf.Deg2Rad);
                shot.tag = "Player Shot";
                shot.GetComponent<Rigidbody>().velocity = new Vector3(velShotX, velShotY, 0);
                shotFire = true;
            }

            // Player move //
            
            if (DEBUG_CODE)
            {
                // Editor controls for debug
                moveHorizontal = Input.GetAxis("Horizontal");
                moveVertical = Input.GetAxis("Vertical");

            }
            else
            {
                // Android controls for release
                moveHorizontal = Input.gyro.gravity.x;
                moveVertical = Input.gyro.gravity.y + 1;
            }

            if (moveVertical < 0)
                moveVertical = 0;

            float rotationAngle = -moveHorizontal * Mathf.Rad2Deg * Time.deltaTime * 4;
            usersShip.transform.Rotate(Vector3.forward, rotationAngle);

            shipRotationZ += -rotationAngle;
            if (shipRotationZ > 360)
                shipRotationZ -= 360;
            if (shipRotationZ < -360)
                shipRotationZ += 360;

            float velX = speed * moveVertical * Time.deltaTime * Mathf.Sin(shipRotationZ * Mathf.Deg2Rad);
            float velY = speed * moveVertical * Time.deltaTime * Mathf.Cos(shipRotationZ * Mathf.Deg2Rad);

            userPosX += velX;
            userPosY += velY;

            usersShip.transform.position = new Vector3
            (
                Mathf.Clamp(userPosX, xMin, xMax),
                Mathf.Clamp(userPosY, yMin, yMax),
                usersShip.transform.position.z
            );

            userPosX = usersShip.transform.position.x;
            userPosY = usersShip.transform.position.y;

            // Camera move
            cam.transform.position = new Vector3
            (
                Mathf.Clamp(usersShip.transform.position.x, xMin, xMax),
                Mathf.Clamp(usersShip.transform.position.y, yMin, yMax),
                cam.transform.position.z
            );

            cam.transform.rotation = usersShip.transform.rotation;
        }
    }

}
