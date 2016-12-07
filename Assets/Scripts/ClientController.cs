using System;
using System.Collections;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;

public class ClientController : MonoBehaviour {
    
    static public bool programEnded = false;
    static protected bool connectedToServer = false;
    public GameObject connectionErrorUI;
    public GameObject usersShip;
    private Vector3 usersPosition;
    private Client user;
    private Thread thSendPosition;

    // Use this for initialization
    void Start () {
        user = new Client();
        user.connect();
        thSendPosition = new Thread(sendPosition);
    }

    void sendPosition()
    {
        while (!programEnded)
        {
            if (connectedToServer)
                user.sendData("x: " + usersPosition.x + " y: " + usersPosition.y);
        }
        
        // user closes the application
        user.informServer();
        user.informServer();

    }
	
	// Update is called once per frame
	void FixedUpdate () {
        usersPosition = usersShip.transform.position;
        if(!programEnded && connectedToServer)
        {
            removeConnectionUI();
            thSendPosition.Start();
            InputController.gamePaused = false;
        }
        else if(!connectedToServer)
        {
            displayConnectionUI();
            InputController.gamePaused = true;
        }
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
                returndata = returndata.Substring(0, returndata.IndexOf("<EOF>"));
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


