/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        Listener.cs
 * PURPOSE:     Simple port checker.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Communication;
/// <summary>
///     Simple Port Listener
/// </summary>
public sealed class Listener
{
    /// <summary>
    ///     The port
    /// </summary>
    private readonly int _port;
    /// <summary>
    ///     The TCP listener
    /// </summary>
    private readonly TcpListener _tcpListener;
    /// <summary>
    ///     The is running
    /// </summary>
    private bool _isRunning;
    /// <summary>
    ///     Initializes a new instance of the <see cref = "Listener"/> class.
    /// </summary>
    /// <param name = "port">The port.</param>
    public Listener(int port)
    {
        _port = port;
        _tcpListener = new TcpListener(IPAddress.Any, port);
    }

    /// <summary>
    ///     Starts the listening.
    /// </summary>
    /// <param name = "cancellationToken">The cancellation token.</param>
    public void StartListening(CancellationToken cancellationToken)
    {
        _isRunning = true;
        _tcpListener.Start();
        Console.WriteLine(string.Format(ComResource.MessageListening, _port));
        while (_isRunning)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                StopListening();
                return;
            }

            try
            {
                if (_tcpListener.Pending()) // Non-blocking accept
                {
                    var client = _tcpListener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(HandleClient, client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format(ComResource.ErrorAcceptingCommunication, ex.Message));
            }
        }
    }

    /// <summary>
    ///     Stops the listening.
    /// </summary>
    public void StopListening()
    {
        _isRunning = false;
        _tcpListener.Stop();
        Console.WriteLine(ComResource.ServerStatusStop);
    }

    /// <summary>
    ///     Handles the client.
    /// </summary>
    /// <param name = "obj">The object.</param>
    private static void HandleClient(object obj)
    {
        var client = (TcpClient)obj;
        var stream = client.GetStream();
        // Respond with a simple message (acting as the "ping response")
        const string response = ComResource.AnswerMessage;
        var buffer = Encoding.ASCII.GetBytes(response);
        stream.Write(buffer, 0, buffer.Length);
        // Close the connection
        client.Close();
        Console.WriteLine(ComResource.MessageAnswer);
    }
}