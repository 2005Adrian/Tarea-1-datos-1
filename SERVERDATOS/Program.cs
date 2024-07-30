using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    private static List<TcpClient> clients = new List<TcpClient>();
    private static object lockObj = new object();

    static void Main(string[] args)
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 8000;

        TcpListener listener = new TcpListener(ipAddress, port);
        listener.Start();

        Console.WriteLine("Servidor iniciado. Esperando conexiones...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            lock (lockObj)
            {
                clients.Add(client);
            }
            Console.WriteLine("Cliente conectado.");

            NetworkStream stream = client.GetStream();
            Thread clientThread = new Thread(() => HandleClient(client, stream));
            clientThread.Start();
        }
    }

    static void HandleClient(TcpClient client, NetworkStream stream)
    {
        byte[] buffer = new byte[1024];
        int bytesRead;
        string clientName = "";

        try
        {
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            clientName = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"{clientName} se ha conectado.");

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"{clientName}: {data}");

                BroadcastMessage($"{clientName}: {data}", client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
        finally
        {
            lock (lockObj)
            {
                clients.Remove(client);
            }
            client.Close();
            Console.WriteLine($"{clientName} se ha desconectado.");
        }
    }

    static void BroadcastMessage(string message, TcpClient sender)
    {
        lock (lockObj)
        {
            foreach (var client in clients)
            {
                if (client != sender)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        byte[] data = Encoding.ASCII.GetBytes(message);
                        stream.Write(data, 0, data.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error enviando mensaje: " + ex.Message);
                    }
                }
            }
        }
    }
}
