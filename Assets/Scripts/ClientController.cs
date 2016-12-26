using System;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;

public class ClientController : MonoBehaviour {
    
    static public bool programEnded = false;

    static public bool connectedToServer = false;
    static protected bool signedUp = false;

    public GameObject connectionErrorUI;
    public GameObject logInUI;
    public GameObject usersShip;

    private Vector3 usersPosition;
    private Client user;
    private Thread thSendStatus;

    [SerializeField]
    private InputField userNameInput;
    [SerializeField]
    private InputField passwordInput;

    // Use this for initialization
    void Start () {
        user = new Client();
        user.connect();
        //thSendStatus = new Thread(sendStatus);
    }

	// Update is called once per frame
	void FixedUpdate () {

        if (signedUp)
            logInUI.SetActive(false);

        usersPosition = usersShip.transform.position;
        if(!programEnded && connectedToServer)
        {
            removeConnectionUI();
            if(!logInUI.active)
                InputController.gamePaused = false;
        }
        else if(!connectedToServer)
        {
            displayConnectionUI();
            InputController.gamePaused = true;
        }
	}

    public void signUp()
    {
        String username = userNameInput.text;
        String password = passwordInput.text;

        if (connectedToServer)
        {
            Debug.Log("*signup* username: " + username + " password: " + password);
            user.sendData("*signup*username:" + username + "password:" + password);

        }
        // not connected to server, try to connect again
        else
        {
            displayConnectionUI();
            connectedToServer = false;
            InputController.gamePaused = true;
        }
    }

    void sendStatus()
    {
        while (!programEnded)
        {
            if (connectedToServer)
                user.sendData("*position*x:" + usersPosition.x + "y:" + usersPosition.y);
        }

        // user closes the application
        user.informServer();
        user.informServer();

    }

    public void connectServer()
    {
        user.connect();
    }

    private void OnProcessExit(object sender, EventArgs e)
    {
        programEnded = true;
    }

    private void displayConnectionUI()
    {
        if (!connectionErrorUI.activeInHierarchy)
            connectionErrorUI.SetActive(true);
    }

    private void removeConnectionUI()
    {
        if (connectionErrorUI.activeInHierarchy)
            connectionErrorUI.SetActive(false);
    }

    public class Client
    {
        private const int PORT_NO = 8080;
        private const string SERVER_IP = "192.168.1.108";
        private const int transferDataSize = 1024;

        //---create a TCPClient object at the IP and port no.---
        private TcpClient clientSocket = new TcpClient();
        private NetworkStream serverStream;

        public void connect()
        {
            try
            {
                clientSocket.Connect(SERVER_IP, PORT_NO);
                connectedToServer = true;
            }
            catch (Exception e)
            {
                displayMessage(e.Message);
                connectedToServer = false;
            }
        }

        public void sendData(string data)
        {
            try
            {
                byte[] outStream;
                byte[] inStream;
                string returndata;

                serverStream = clientSocket.GetStream();
                outStream = System.Text.Encoding.ASCII.GetBytes(data + "<EOF>");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();

                inStream = new byte[transferDataSize];
                serverStream.Read(inStream, 0, transferDataSize);
                returndata = System.Text.Encoding.ASCII.GetString(inStream);

                //TO-DO add function here
                returndata = returndata.Substring(0, returndata.IndexOf("<EOF>"));
                
                if(returndata.IndexOf("*signupsucceed*") > -1)
                    signedUp = true;
                
                displayMessage("Data from Server : " + returndata);
            }
            catch (Exception e)
            {
                connectedToServer = false;
                displayMessage(e.Message);
            }
        }

        public void informServer()
        {
            byte[] outStream;
            byte[] inStream;
            string returndata;

            serverStream = clientSocket.GetStream();
            outStream = System.Text.Encoding.ASCII.GetBytes("I died <EOF>");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            inStream = new byte[transferDataSize];
            serverStream.Read(inStream, 0, transferDataSize);
            returndata = System.Text.Encoding.ASCII.GetString(inStream);
            returndata = returndata.Substring(0, returndata.IndexOf("<EOF>"));
            displayMessage("Data from Server : " + returndata);
            return;
        }

        private void displayMessage(string msg)
        {
            Debug.Log(msg);
        }
    }

}


