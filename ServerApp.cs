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

public class Info
{
    public string userInfo;
    public string clientName; // to avoid multiple log-ins at same time
    public bool dead; 
    public bool hold = false;
}

public class ServerApp
{
    public static bool isRunning = true;
    public static DirectoryInfo d;
    public static FileInfo[] Files;
    public static Mutex fileLock;
    public static Mutex positionLock;
    public static Info[] infoList;

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

        //------------- Initialize client info array -------------//
        infoList = new Info[serverSize];
        for(int i = 0; i < serverSize; ++i)
            infoList[i] = new Info();

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
                        if(!(infoList[i].hold))
                        {
                            availableId = i;
                            infoList[i].hold = true;
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

}

//------------- Handle each client request separatly -------------//
public class handleClinet
{
    const int transferDataSize = 1024;

    public int clientID = -1;

    private TcpClient clientSocket;
    private bool clientConnected = false;
    private string otherPositions = "";
    private string clientName;
    private string password;
    private string email;
    private Info currentInfo;
    private bool newPlayer = true;

    public void startClient(TcpClient inClientSocket)
    {
        currentInfo = new Info();
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

                //------------- Client logged out -------------//
                if (dataFromClient.IndexOf("I died") > -1)
                {
                    Console.WriteLine("client left");
                    clientConnected = false;
                    clientSocket.Close();
                    --ServerApp.clientCounter;

                    // TO-DO save info to log file 

                    ServerApp.positionLock.WaitOne();
                    ServerApp.infoList[clientID].userInfo = "";
                    ServerApp.infoList[clientID].hold = false;
                    ServerApp.positionLock.ReleaseMutex();
                }

                //------------- Sign up request -------------//
                else if (dataFromClient.IndexOf("*signup*") > -1)
                {
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("<EOF>"));

                    Console.WriteLine("Signing up");
                    if (signUp(dataFromClient))
                        serverResponse = "*signupsucceed*id:" + clientID + "<EOF>";
                    else
                        serverResponse = "*signupfailed*<EOF>";

                    byte[] outStream = System.Text.Encoding.ASCII.GetBytes(serverResponse);
                    networkStream.Write(outStream, 0, outStream.Length);
                    networkStream.Flush();
                }

                //------------- Retrieve information from user -------------//
                else if (dataFromClient.IndexOf("*info*") > -1)
                {
                    int subStartIndex;

                    subStartIndex = dataFromClient.IndexOf("x:");
                    dataFromClient = dataFromClient.Substring(subStartIndex);
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
                ServerApp.infoList[clientID].userInfo = "";
                ServerApp.infoList[clientID].hold = false;
                ServerApp.positionLock.ReleaseMutex();
                Console.WriteLine(clientName + " is left.");
            }
        }
    }

    //------------- Update current position -------------//
    private string updatePosition(string dataFromClient)
    {
        string otherUserInfos = "";
        int counter = 0;
        try
        {
            // put new info of the client
            ServerApp.positionLock.WaitOne();
            ServerApp.infoList[clientID].userInfo = "id:" + clientID + dataFromClient;
            ServerApp.positionLock.ReleaseMutex();

            // retrieve others infos
            ServerApp.positionLock.WaitOne();
            for (int i = 0; i < ServerApp.serverSize; ++i)
            {
                if (ServerApp.infoList[i].hold && i != clientID)
                {
                    otherUserInfos += ServerApp.infoList[i].userInfo;
                    ++counter;
                }
            }
            otherUserInfos = "*others*userSize:" + counter + otherUserInfos;
            //Console.WriteLine(otherUserInfos);
            ServerApp.positionLock.ReleaseMutex();
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
        }

        return otherUserInfos;
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


        ServerApp.fileLock.WaitOne();

        // Search for username in the file array

        Console.WriteLine("username: " + username);
        foreach (FileInfo file in ServerApp.Files)
        {
            if(file.Name.Equals(username + ".log"))
                userExists = true;
        }
        if(!userExists)
        {
            try
            {
                System.IO.FileStream usernameopen = File.Open(username + ".log", FileMode.Create);
                System.IO.FileStream mailopen = File.Open(email + ".log", FileMode.Create);
                ServerApp.Files = ServerApp.d.GetFiles("*.log"); //Update log file array

                usernameopen.Close();
                mailopen.Close();

                // put username and password info inside mail-log file
                System.IO.StreamWriter mailfile = new System.IO.StreamWriter(email + ".log");
                mailfile.WriteLine("username: " + username);
                mailfile.WriteLine("password: " + password);
                mailfile.Close();

                System.IO.StreamWriter usernamefile = new System.IO.StreamWriter(username + ".log");
                usernamefile.WriteLine("password: " + password);
                usernamefile.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        ServerApp.fileLock.ReleaseMutex();

        return false;
    }

    private void sendMailToUser(String userMail)
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
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    //------------- Log in -------------//

} 
