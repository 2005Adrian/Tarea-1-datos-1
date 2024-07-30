using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Client
{
    static void Main(string[] args)
    {
        try
        {
            string server = "127.0.0.1";
            int port = 8000;

            TcpClient client = new TcpClient(server, port);
            NetworkStream stream = client.GetStream();

            Console.Write("Introduce tu nombre de usuario: ");
            string userName = Console.ReadLine();
            byte[] userNameData = Encoding.ASCII.GetBytes(userName);
            stream.Write(userNameData, 0, userNameData.Length);

            Console.WriteLine("Conectado al servidor. ");

            Thread readThread = new Thread(() =>
            {
                byte[] buffer = new byte[1024];
                int bytesRead;

                try
                {
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Console.WriteLine(response);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error leyendo del servidor: " + ex.Message);
                }
            });
            readThread.Start();

            Thread writeThread = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        string message = Console.ReadLine();
                        if (message.ToLower() == "exit")
                        {
                            break;
                        }

                        byte[] data = Encoding.ASCII.GetBytes(message);
                        stream.Write(data, 0, data.Length);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error enviando al servidor: " + ex.Message);
                }
            });
            writeThread.Start();

            writeThread.Join();
            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
