using UnityEngine;
using System.Collections;
using System.Threading;

public class InputController : MonoBehaviour {

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


    private float xMax, xMin, yMax, yMin;
    private Rigidbody rigidbody;

    private float userPosX = 0;
    private float userPosY = 0;

    private Factory fac;

	// Use this for initialization
	void Start () {
        xMin = -width / 2;
        xMax = width / 2;
        yMin = -height / 2;
        yMax = height / 2;
        rigidbody = usersShip.GetComponent<Rigidbody>();
        fac = GetComponent<Factory>();
	}
	
	// Update is called once per frame
	void Update () {
        Input.gyro.enabled = true;
        movePlayer();

	}

    private void movePlayer()
    {
        // Shooting
        if (Input.GetMouseButtonUp(0))
        {
            float mouseScreenX = Input.mousePosition.x;
            float mouseScreenY = Input.mousePosition.y;

            float userScreenX = cam.WorldToScreenPoint(usersShip.transform.position).x;
            float userScreenY = cam.WorldToScreenPoint(usersShip.transform.position).y;

            float rotationAngle = Mathf.Rad2Deg * Mathf.Atan2(userScreenY - mouseScreenY, userScreenX - mouseScreenX);
            //Debug.Log("Angle: " + rotationAngle);
            usersShip.transform.rotation = Quaternion.identity;
            usersShip.transform.Rotate(Vector3.forward, rotationAngle + 90);

            // Create the shot
            GameObject shot = fac.createShot();
            shot.transform.position = usersShip.transform.position;
            shot.transform.Rotate(Vector3.forward, rotationAngle + 90);
            float velX =  -400 * speed * Time.deltaTime * Mathf.Cos(rotationAngle * Mathf.Deg2Rad);
            float velY = -400 * speed * Time.deltaTime * Mathf.Sin(rotationAngle * Mathf.Deg2Rad);
            shot.GetComponent<Rigidbody>().velocity = new Vector3(velX, velY, 0);

        }
        // Player move
        // Editor controls for debug
        if (Input.anyKey)
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            userPosX += moveHorizontal * speed * Time.deltaTime;
            userPosY += moveVertical * speed * Time.deltaTime;

            usersShip.transform.position = new Vector3
            (
                Mathf.Clamp(userPosX, xMin + 0.5f, xMax - 0.5f),
                Mathf.Clamp(userPosY, yMin + 0.5f, yMax - 0.5f),
                usersShip.transform.position.z
            );

            userPosX = usersShip.transform.position.x;
            userPosY = usersShip.transform.position.y;

            // Camera move
            cam.transform.position = new Vector3
            (
                Mathf.Clamp(usersShip.transform.position.x, xMin + 4.5f, xMax - 4.5f),
                Mathf.Clamp(usersShip.transform.position.y, yMin + 3, yMax - 3),
                cam.transform.position.z
            );
        }
        // Android controls for release
        /*
        float moveHorizontal = Input.gyro.gravity.x;
        float moveVertical = Input.gyro.gravity.y;

        userPosX += moveHorizontal * speed * Time.deltaTime;
        userPosY += moveVertical * speed * Time.deltaTime;

        usersShip.transform.position = new Vector3
        (
            Mathf.Clamp(userPosX, xMin + 0.5f, xMax - 0.5f),
            Mathf.Clamp(userPosY, yMin + 0.5f, yMax - 0.5f),
            usersShip.transform.position.z
        );

        userPosX = usersShip.transform.position.x;
        userPosY = usersShip.transform.position.y;

        // Camera move
        cam.transform.position = new Vector3
        (
            Mathf.Clamp(usersShip.transform.position.x, xMin + 4.5f, xMax - 4.5f),
            Mathf.Clamp(usersShip.transform.position.y, yMin + 3, yMax - 3),
            cam.transform.position.z
        );*/


    }


}
