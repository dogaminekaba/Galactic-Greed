using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Net.Mail;

public class ServerApp
{
    public static bool isRunning = true;
    public static int clientCounter = 0;

    private const int serverSize = 15; // max # of the connected clients 
    private const int PORT_NO = 8080;

    //---listen at the specified IP and port no.---
    static TcpListener serverSocket = new TcpListener(IPAddress.Any, PORT_NO);

    static void Main(string[] args)
    {
        // Handle ctrl-c
        Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            isRunning = false;
            serverSocket.Stop();
        };

        TcpClient clientSocket = default(TcpClient);
        serverSocket.Start();
        Console.WriteLine("Waiting for connection...");

        while (isRunning)
        {
            if (clientCounter < serverSize)
            {
                try
                {
                    clientCounter += 1;
                    //---incoming client connected---
                    clientSocket = serverSocket.AcceptTcpClient();
                    Console.WriteLine("Client No: " + Convert.ToString(clientCounter) + " is connected.");
                    handleClinet client = new handleClinet();
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

//Class to handle each client request separatly
public class handleClinet
{
    const int transferDataSize = 1024;
    TcpClient clientSocket;
    string clientName;
    bool clientConnected = false;
    public void startClient(TcpClient inClientSocket)
    {
        this.clientSocket = inClientSocket;
        clientConnected = true;
        Thread ctThread = new Thread(getInfo);
        ctThread.Start();
    }
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
                }
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
                Console.WriteLine(ex.ToString());
            }
        }
    }

    private bool signUp(string dataFromClient)
    {
        bool userExists = false;
        DirectoryInfo d = new DirectoryInfo(Directory.GetCurrentDirectory());
        FileInfo[] Files = d.GetFiles("*.log"); //Getting log files

        int subStartIndex = dataFromClient.IndexOf("username:") + "username:".Length;
        string username = dataFromClient.Substring(subStartIndex);

        username = username.Substring(0, username.IndexOf("password:"));

        Console.WriteLine("username: " + username);
        foreach (FileInfo file in Files)
        {
            if(file.Name.Equals(username + ".log"))
                userExists = true;
        }

        if(!userExists)
        {
            File.Open(username + ".log" , FileMode.Create);
            return true;
        }

        return false;
    }
} 
