using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Serilog;

class UdpListener
{
    private const int ListenPort = 50000;
    private static readonly string LogFilePath = "logs/udp_log.txt";
    private const int BufferSize = 1024 * 1024* 60; // 1MB
    private static bool isRunning = true;

    static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(LogFilePath, outputTemplate:"{Message}")
            .CreateLogger();

        Thread listenerThread = new Thread(StartListener);
        listenerThread.Start();

        Console.WriteLine("Press any key to stop the listener...");
        Console.ReadKey();
        isRunning = false;
        listenerThread.Join();

        Log.CloseAndFlush();
    }

    private static void StartListener()
    {
        using (UdpClient udpClient = new UdpClient(ListenPort))
        {
            udpClient.Client.ReceiveBufferSize = BufferSize;
            Console.WriteLine($"UDP Listener started. Port: {ListenPort}");

            try
            {
                while (isRunning)
                {
                    if (udpClient.Available > 0)
                    {
                        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, ListenPort);
                        byte[] receivedBytes = udpClient.Receive(ref remoteEndPoint);
                        string receivedData = Encoding.UTF8.GetString(receivedBytes);

                        // Log received data
                        Log.Information(receivedData);
                    }
                    else
                    {
                        Thread.Sleep(10); // Slight delay to reduce CPU usage
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred");
            }
        }
    }
}
