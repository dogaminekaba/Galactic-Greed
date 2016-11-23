using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;

public class ClientController : MonoBehaviour {
    public GameObject usersShip;

    private Client user;

	// Use this for initialization
	void Start () {
        user = new Client();
        user.connect();
	}
	
	// Update is called once per frame
	void Update () {
        user.sendData(  "x: " + usersShip.transform.position.x + 
                        " y: " + usersShip.transform.position.y);
	}


}

public class Client
{
    private const int transferDataSize = 1024;
    private const int PORT_NO = 8080;
    private const string SERVER_IP = "127.0.0.1";
    private bool programEnded = false;

    //---create a TCPClient object at the IP and port no.---
    private TcpClient clientSocket = new TcpClient();
    private NetworkStream serverStream;

    public void connect()
    {
        try
        {
            // Handle exit signal to let server know that client is left
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            clientSocket.Connect("127.0.0.1", 8080);
        }
        catch(Exception e)
        {
            displayMessage(e.Message);
        }
    }

    public void sendData(string data)
    {
        byte[] outStream;
        if (programEnded)
        {
            serverStream = clientSocket.GetStream();
            outStream = System.Text.Encoding.ASCII.GetBytes("I died <EOF>");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
            clientSocket.Close();
            return;
        }
        serverStream = clientSocket.GetStream();
        outStream = System.Text.Encoding.ASCII.GetBytes(data + "<EOF>");
        serverStream.Write(outStream, 0, outStream.Length);
        serverStream.Flush();

        byte[] inStream = new byte[transferDataSize];
        serverStream.Read(inStream, 0, transferDataSize);
        string returndata = System.Text.Encoding.ASCII.GetString(inStream);
        returndata = returndata.Substring(0, returndata.IndexOf("<EOF>"));
        displayMessage("Data from Server : " + returndata);
    }

    private void OnProcessExit(object sender, EventArgs e)
    {
        programEnded = true;
    }

    private void displayMessage(string msg)
    {
        Debug.Log(msg);
    } 
}
