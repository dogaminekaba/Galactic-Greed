using System;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;
using System.Globalization;

public class Info
{
    public int clientID = -1;
    public float x;
    public float y;
    public float theta;
    public int color;
    public bool shotFired;
    public int score;
    public bool hold = false;

    public string toString()
    {
        return "id:" + clientID + "x:" + x + "y:" + y + "theta:" + theta + "color:" + color + "shotFired:" + shotFired + "score:" + score;
    }
}

public class ClientController : MonoBehaviour {

    public static Mutex listLock;
    public static bool programEnded = false;
    public static bool connectedToServer = false;
    public static Info[] infoList;
    public static int serverSize = 20;

    static protected bool signedUp = false;
    static protected bool loggedIn = false;
    static protected Thread thSendStatus;
    static protected Info currentInfo;

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

        listLock = new Mutex();

        user = new Client();
        user.connect();

        // TO-DO retrieve info and fill currentInfo

        currentInfo = new Info();
        currentInfo.color = 0;
        currentInfo.hold = true;
        currentInfo.theta = InputController.shipRotationZ;
        currentInfo.shotFired = false;
        currentInfo.x = usersPosition.x;
        currentInfo.y = usersPosition.y;

        infoList = new Info[serverSize];
        for (int i = 0; i < serverSize; ++i)
            infoList[i] = new Info();

        thSendStatus = new Thread(sendStatus);
        thSendStatus.Start();
        signedUp = true;
        loggedIn = true;
    }

	// Update is called once per frame
	void Update () {
        if (usersShip && connectedToServer)
        {
            usersPosition = usersShip.transform.position;

            currentInfo.theta = InputController.shipRotationZ;
            currentInfo.x = usersPosition.x;
            currentInfo.y = usersPosition.y;
            currentInfo.score = GameController.score;

            if (InputController.shotFire)
            {
                currentInfo.shotFired = true;
                InputController.shotFire = false;
            }
            else
                currentInfo.shotFired = false;
        }
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
            GameController.currentState = GameController.GameState.GAME_PLAY;
            
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
            if (connectedToServer && (signedUp || loggedIn))
                user.sendData("*info*" + currentInfo.toString());
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
        //loggedIn = false;
        //signedUp = false;
        connectedToServer = false;
        InputController.gamePaused = true;
        if (GameController.currentState != GameController.GameState.SERVER_CONNECTION_ERROR)
            GameController.prevState = GameController.currentState;
        GameController.currentState = GameController.GameState.SERVER_CONNECTION_ERROR;
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
        //private const string SERVER_IP = "138.68.168.212";
        private const string SERVER_IP = "192.168.1.108";
        private const int transferDataSize = 1024;

        //---create a TCPClient object at the IP and port no.---
        private TcpClient clientSocket;
        private NetworkStream serverStream;

        public void connect()
        {
            try
            {
                clientSocket = new TcpClient();
                clientSocket.Connect(SERVER_IP, PORT_NO);
                ClientController.connected();
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
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

                returndata = returndata.Substring(0, returndata.IndexOf("<EOF>"));

                if (returndata.IndexOf("*signupsucceed*") > -1)
                {
                    signedUp = true;
                    thSendStatus.Start();
                }
                else if (returndata.IndexOf("*loginsucceed*") > -1)
                {
                    loggedIn = true;
                    thSendStatus.Start();
                }
                else if (returndata.IndexOf("*others*") > -1)
                    parseOthersInfos(returndata);

            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                clientSocket.Close();
                ClientController.disconnected();
            }
        }

        private void parseOthersInfos(string othersInfos)
        {
            
            int subStartIndex;
            string xPos;
            string yPos;
            string theta;
            string color;
            string shotFire;
            string score;
            string id;
            string count;

            int userCount = 0;

            if (othersInfos.Length < 1)
                return;

            // connected user count
            subStartIndex = othersInfos.IndexOf("userSize:") + "userSize:".Length;
            count = othersInfos.Substring(subStartIndex);

            if (count.IndexOf("id:") < 0)
            {
                userCount = 0;
                for (int i = 0; i < serverSize; ++i)
                    infoList[i].hold = false;
            }
            else
            {
                count = count.Substring(0, count.IndexOf("id:"));
                userCount = int.Parse(count);
            }

            if (listLock.WaitOne(10))
            {
                for (int i = 0; i < userCount; ++i)
                {
                     if (othersInfos.Length > 10)
                     {
                         infoList[i].hold = true;

                         // id info
                         subStartIndex = othersInfos.IndexOf("id:") + "id:".Length;
                         id = othersInfos.Substring(subStartIndex);
                         id = id.Substring(0, id.IndexOf("x:"));

                         infoList[i].clientID = int.Parse(id);

                         // x info
                         subStartIndex = othersInfos.IndexOf("x:") + "x:".Length;
                         xPos = othersInfos.Substring(subStartIndex);
                         xPos = xPos.Substring(0, xPos.IndexOf("y:"));

                         infoList[i].x = float.Parse(xPos, CultureInfo.InvariantCulture);

                         // y info
                         subStartIndex = othersInfos.IndexOf("y:") + "y:".Length;
                         yPos = othersInfos.Substring(subStartIndex);
                         yPos = yPos.Substring(0, yPos.IndexOf("theta:"));

                         infoList[i].y = float.Parse(yPos, CultureInfo.InvariantCulture);

                         // theta degree info
                         subStartIndex = othersInfos.IndexOf("theta:") + "theta:".Length;
                         theta = othersInfos.Substring(subStartIndex);
                         theta = theta.Substring(0, theta.IndexOf("color:"));

                         infoList[i].theta = float.Parse(theta, CultureInfo.InvariantCulture);

                         // color info
                         subStartIndex = othersInfos.IndexOf("color:") + "color:".Length;
                         color = othersInfos.Substring(subStartIndex);
                         color = color.Substring(0, color.IndexOf("shotFired:"));

                         infoList[i].color = int.Parse(color);

                         // shot fire info
                         subStartIndex = othersInfos.IndexOf("shotFired:") + "shotFired:".Length;
                         shotFire = othersInfos.Substring(subStartIndex);
                         shotFire = shotFire.Substring(0, shotFire.IndexOf("score:"));

                         infoList[i].shotFired = bool.Parse(shotFire);

                         // score info
                         subStartIndex = othersInfos.IndexOf("score:") + "score:".Length;
                         score = othersInfos.Substring(subStartIndex);
                         if(score.IndexOf("id:") > 0)
                            score = score.Substring(0, score.IndexOf("id:"));

                         infoList[i].score = int.Parse(score);

                         othersInfos = othersInfos.Substring(subStartIndex);
                         subStartIndex = othersInfos.IndexOf("id:");

                         if (subStartIndex >= 0)
                             othersInfos = othersInfos.Substring(subStartIndex);
                         else
                             othersInfos = "";

                        //Debug.Log(infoList[i].toString());
                     }
                    // free all remaining empty spaces in list
                    for (int j = userCount;  j < serverSize; ++j)
                        infoList[j].hold = false;

                }
                listLock.ReleaseMutex();
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
            Debug.Log("Data from Server : " + returndata);
            return;
        }

    }

}


