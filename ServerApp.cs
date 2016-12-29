using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Net.Mail;
using System.Collections;
using System.Globalization;

public class Point
{
    public float x;
    public float y;
    public bool hold = false;

    public string toString()
    {
        return "x:" + x + "y:" + y;
    }
}

public class ServerApp
{
    public static bool isRunning = true;
    public static DirectoryInfo d;
    public static FileInfo[] Files;
    public static Mutex fileLock;
    public static Mutex positionLock;
    public static Point[] coordList;

    public static int clientCounter = 0;
    public const int serverSize = 20; // max # of the connected clients 

    private const int PORT_NO = 8080;
    private static TcpListener serverSocket = new TcpListener(IPAddress.Any, PORT_NO);

    static void Main(string[] args)
    {
        //------------- Handle ctrl-c -------------//
        Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            isRunning = false;
            serverSocket.Stop();
        };

        //------------- At start gather all file names into an array -------------//
        d = new DirectoryInfo(Directory.GetCurrentDirectory());
        Files = d.GetFiles("*.log"); //Getting log files

        //------------- Initialize client coordinate array -------------//
        coordList = new Point[serverSize];
        for(int i = 0; i < serverSize; ++i)
            coordList[i] = new Point();

        //------------- Initialize mutex locks -------------//
        fileLock = new Mutex();
        positionLock = new Mutex();

        TcpClient clientSocket = default(TcpClient);
        serverSocket.Start();
        Console.WriteLine("Waiting for connection...");

        while (isRunning)
        {
            if (clientCounter < serverSize)
            {
                try
                {
                    if (clientCounter >= serverSize)
                        continue;

                    clientSocket = serverSocket.AcceptTcpClient();
                    //------------- Give user an id to track current coordinates -------------//
                    int availableId = -1;
                    positionLock.WaitOne();
                    for (int i = 0; i < serverSize; ++i )
                    {
                        if(!(coordList[i].hold))
                        {
                            availableId = i;
                            coordList[i].hold = true;
                            clientCounter += 1;
                            break;
                        }
                    }
                    positionLock.ReleaseMutex();
                    Console.WriteLine("Client No: " + Convert.ToString(availableId) + " is connected.");
                    handleClinet client = new handleClinet();
                    client.clientID = availableId;
                    client.startClient(clientSocket);
                }
                catch (SocketException e)
                {
                    if ((e.SocketErrorCode == SocketError.Interrupted))
                        Console.WriteLine("Socket listening is ended.");
                }
            }
        }
        if(clientSocket != null)
            clientSocket.Close();
        serverSocket.Stop();
    }

    static void sendMailToUser(String userMail)
    {
        try
        {
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("galacticgreed@gmail.com", "dmk1995dmk");

            MailMessage mm = new MailMessage("donotreply@domain.com", userMail, "test", "test");
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

}

//------------- Handle each client request separatly -------------//
public class handleClinet
{
    const int transferDataSize = 1024;

    public int clientID = -1;

    private TcpClient clientSocket;
    private bool clientConnected = false;
    private string clientName;
    private Point currentPosition;
    private string otherPositions = "";

    public void startClient(TcpClient inClientSocket)
    {
        currentPosition = new Point();
        this.clientSocket = inClientSocket;
        clientConnected = true;
        Thread ctThread = new Thread(getInfo);
        ctThread.Start();
    }

    //------------- Get information from player -------------//
    private void getInfo()
    {
        byte[] bytesFrom = new byte[transferDataSize];
        string dataFromClient = null;
        string serverResponse = null;

        while (clientConnected)
        {
            try
            {
                NetworkStream networkStream = clientSocket.GetStream();
                networkStream.Read(bytesFrom, 0, transferDataSize);
                dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                
                if (dataFromClient.IndexOf("I died") > -1)
                {
                    Console.WriteLine("client left");
                    clientConnected = false;
                    clientSocket.Close();
                    --ServerApp.clientCounter;
                    ServerApp.positionLock.WaitOne();
                    ServerApp.coordList[clientID].x = 0;
                    ServerApp.coordList[clientID].y = 0;
                    ServerApp.coordList[clientID].hold = false;
                    ServerApp.positionLock.ReleaseMutex();
                }

                //------------- Sign up request -------------//
                else if (dataFromClient.IndexOf("*signup*") > -1)
                {
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("<EOF>"));

                    Console.WriteLine("Signing up");
                    if (signUp(dataFromClient))
                        serverResponse = "*signupsucceed*<EOF>";
                    else
                        serverResponse = "*signupfailed*<EOF>";

                    byte[] outStream = System.Text.Encoding.ASCII.GetBytes(serverResponse);
                    networkStream.Write(outStream, 0, outStream.Length);
                    networkStream.Flush();
                }

                //------------- Retrieve information from user -------------//
                else if (dataFromClient.IndexOf("*info*") > -1)
                {
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("<EOF>"));

                    otherPositions = updatePosition(dataFromClient);
                    serverResponse = otherPositions + "<EOF>";

                    byte[] outStream = System.Text.Encoding.ASCII.GetBytes(serverResponse);
                    networkStream.Write(outStream, 0, outStream.Length);
                    networkStream.Flush();
                }

                else
                {
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("<EOF>"));
                    Console.WriteLine("From client-" + clientName + ": " + dataFromClient);

                    serverResponse = "Server to client(" + clientName + ") " + " <EOF>";
                    byte[] outStream = System.Text.Encoding.ASCII.GetBytes(serverResponse);
                    networkStream.Write(outStream, 0, outStream.Length);
                    networkStream.Flush();
                }
            }
            catch (Exception ex)
            {
                clientConnected = false;
                //Console.WriteLine(ex.ToString());

                ServerApp.positionLock.WaitOne();
                ServerApp.coordList[clientID].x = 0;
                ServerApp.coordList[clientID].y = 0;
                ServerApp.coordList[clientID].hold = false;
                ServerApp.positionLock.ReleaseMutex();
                Console.WriteLine(clientName + " is left.");
            }
        }
    }

    //------------- Update current position -------------//
    private string updatePosition(string dataFromClient)
    {
        int subStartIndex;
        string xPos;
        string yPos;

        string otherPositions = "";

        subStartIndex = dataFromClient.IndexOf("x:") + "x:".Length;
        xPos = dataFromClient.Substring(subStartIndex);
        xPos = xPos.Substring(0, xPos.IndexOf("y:"));

        subStartIndex = dataFromClient.IndexOf("y:") + "y:".Length;
        yPos = dataFromClient.Substring(subStartIndex);

        currentPosition.x = float.Parse(xPos, CultureInfo.InvariantCulture);
        currentPosition.y = float.Parse(yPos, CultureInfo.InvariantCulture);

        ServerApp.positionLock.WaitOne();

        ServerApp.coordList[clientID].x = currentPosition.x;
        ServerApp.coordList[clientID].y = currentPosition.y;

        ServerApp.positionLock.ReleaseMutex();

        ServerApp.positionLock.WaitOne();

        for (int i = 0; i < ServerApp.serverSize; ++i)
        {
            if (ServerApp.coordList[i].hold)
            {
                otherPositions += ServerApp.coordList[i].toString();
                
            }
        }
        Console.WriteLine("others: " + otherPositions);
        ServerApp.positionLock.ReleaseMutex();

        return otherPositions;
    }

    //------------- Sign up -------------//
    private bool signUp(string dataFromClient)
    {
        bool userExists = false;
        int subStartIndex;
        string username;
        string password;
        string email;

        subStartIndex = dataFromClient.IndexOf("email:") + "email:".Length;
        email = dataFromClient.Substring(subStartIndex);
        email = email.Substring(0, email.IndexOf("username:"));

        subStartIndex = dataFromClient.IndexOf("username:") + "username:".Length;
        username = dataFromClient.Substring(subStartIndex);
        username = username.Substring(0, username.IndexOf("password:"));

        subStartIndex = dataFromClient.IndexOf("password:") + "password:".Length;
        password = dataFromClient.Substring(subStartIndex);

        Console.WriteLine( "\n email: " + email + "\n username: " + username + "\n password: " + password + "\n");

        clientName = username;

        /*
        ServerApp.mLock.WaitOne();

        // Search for username in the file array

        Console.WriteLine("username: " + username);
        foreach (FileInfo file in ServerApp.Files)
        {
            if(file.Name.Equals(username + ".log"))
                userExists = true;
        }
        if(!userExists)
        {
            File.Open(username + ".log" , FileMode.Create);
            ServerApp.Files = ServerApp.d.GetFiles("*.log"); //Update log file array
            return true;
        }

        ServerApp.mLock.ReleaseMutex();*/

        return false;
    }

    //------------- Log in -------------//

} 
