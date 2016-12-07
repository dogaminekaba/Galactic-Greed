using UnityEngine;
using System.Collections;
using System.Threading;

public class InputController : MonoBehaviour {

    static public bool gamePaused;

    [SerializeField] 
    private GameObject usersShip;
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
    private float shipRotationZ;
    private Rigidbody rigidbody;

    private float userPosX = 0;
    private float userPosY = 0;

    private Factory fac;

    private bool DEBUG_CODE = false;

    // Use this for initialization
    void Start () {
        xMin = -width * 2.0f / 5;
        xMax = width * 2.0f / 5;
        yMin = -height / 3;
        yMax = height / 3;
        rigidbody = usersShip.GetComponent<Rigidbody>();
        fac = GetComponent<Factory>();
        gamePaused = false;
        shotSpeed = 400 * speed;
        Input.gyro.enabled = true;
	}
	
	// Update is called once per frame
	void Update () {
        movePlayer();
	}

    void OnApplicationQuit()
    {
        Debug.Log("finished.");
        ClientController.programEnded = true;
        Thread.Sleep(500);
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
                float mouseScreenX = Input.mousePosition.x;
                float mouseScreenY = Input.mousePosition.y;

                float userScreenX = cam.WorldToScreenPoint(usersShip.transform.position).x;
                float userScreenY = cam.WorldToScreenPoint(usersShip.transform.position).y;

                // Create the shot
                GameObject shot = fac.createShot();
                shot.transform.position = usersShip.transform.position;
                shot.transform.rotation = usersShip.transform.rotation;
                float velShotX = shotSpeed * Time.deltaTime * Mathf.Sin(shipRotationZ * Mathf.Deg2Rad);
                float velShotY = shotSpeed * Time.deltaTime * Mathf.Cos(shipRotationZ * Mathf.Deg2Rad);
                shot.GetComponent<Rigidbody>().velocity = new Vector3(velShotX, velShotY, 0);

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
