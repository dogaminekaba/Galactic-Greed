using System;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;

public class ClientController : MonoBehaviour {
    
    static public bool programEnded = false;
    static public bool connectedToServer = false;

    static protected bool signedUp = false;
    static protected Thread thSendStatus;

    public GameObject connectionErrorUI;
    public GameObject logInUI;
    public GameObject usersShip;

    private Vector3 usersPosition;
    private Client user;

    [SerializeField]
    private InputField emailInput;
    [SerializeField]
    private InputField userNameInput;
    [SerializeField]
    private InputField passwordInput;

    // Use this for initialization
    void Start () {
        user = new Client();
        user.connect();
        thSendStatus = new Thread(sendStatus);
        thSendStatus.Start();
    }

	// Update is called once per frame
	void Update () {
        usersPosition = usersShip.transform.position;
        if (!connectedToServer)
            disconnected();
	}

    public void signUp()
    {
        String email = emailInput.text;
        String username = userNameInput.text;
        String password = passwordInput.text;
        bool emailOutOfSize = false;
        bool usernameOutOfSize = false;
        bool passwordOutOfSize = false;
        bool outOfMailFormat = false;

        if (email.Length < 1 || email.Length > 50)
            emailOutOfSize = true;

        if (username.Length < 1 || username.Length > 20)
            usernameOutOfSize = true;

        if (password.Length < 1 || password.Length > 20)
            passwordOutOfSize = true;

        if(!emailOutOfSize && email.IndexOf("@") < 0)
            outOfMailFormat = true;

        if(outOfMailFormat)
        {
            emailInput.text = "";
            emailInput.placeholder.GetComponent<Text>().text = "invalid mail";
        }
        if (emailOutOfSize)
        {
            emailInput.text = "";
            emailInput.placeholder.GetComponent<Text>().text = "mail (1-50)";
        }
        if (usernameOutOfSize)
        {
            userNameInput.text = "";
            userNameInput.placeholder.GetComponent<Text>().text = "username (1-20)";
        }
        if (passwordOutOfSize)
        {
            passwordInput.text = "";
            passwordInput.placeholder.GetComponent<Text>().text = "password (1-20)";
        }

        if (connectedToServer)
        {
            Debug.Log("*signup* email: " + email + " username: " + username + " password: " + password);

            if (!emailOutOfSize && !outOfMailFormat && !usernameOutOfSize && !passwordOutOfSize)
                user.sendData("*signup*email:" + email + "username:" + username + "password:" + password);
        }
        // not connected to server, try to connect again
        else
            disconnected();
    }

    public void sendStatus()
    {
        while (!programEnded)
        {
            if (connectedToServer)
                user.sendData("*info*x:" + usersPosition.x + "y:" + usersPosition.y);
        }

        // user closes the application
        user.informServer();
        user.informServer();

    }

    public void connectServer()
    {
        user.connect();
    }

    static public void disconnected()
    {
        if(GameController.currentState != GameController.GameState.SERVER_CONNECTION_ERROR)
            GameController.prevState = GameController.currentState;
        GameController.currentState = GameController.GameState.SERVER_CONNECTION_ERROR;
        connectedToServer = false;
        InputController.gamePaused = true;
    }

    static public void connected()
    {
        GameController.currentState = GameController.prevState;
        connectedToServer = true;
    }

    private void OnProcessExit(object sender, EventArgs e)
    {
        programEnded = true;
    }

    public class Client
    {
        private const int PORT_NO = 8080;
        private const string SERVER_IP = "172.20.10.3";
        private const int transferDataSize = 1024;

        //---create a TCPClient object at the IP and port no.---
        private TcpClient clientSocket = new TcpClient();
        private NetworkStream serverStream;

        public void connect()
        {
            try
            {
                clientSocket.Connect(SERVER_IP, PORT_NO);
                ClientController.connected();
            }
            catch (Exception e)
            {
                displayMessage(e.Message);
                ClientController.disconnected();
            }
        }

        public void sendData(string data)
        {
            try
            {
                byte[] outStream;
                byte[] inStream;
                string returndata = "";

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
                
                Debug.Log("Data from Server : " + returndata);
            }
            catch (Exception e)
            {
                displayMessage(e.Message);
                ClientController.disconnected();
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


