
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SethCS.Exceptions;

namespace RegressionTests.TestCore
{
    /// <summary>
    /// This class is a fake IRC server that our bot can talk to.
    /// </summary>
    public class IrcServer : IDisposable
    {
        // -------- Fields --------

        /// <summary>
        /// The TCP server we create.
        /// </summary>
        private TcpListener server;

        /// <summary>
        /// The thread that does the reading.
        /// </summary>
        private Thread readerThread;

        /// <summary>
        /// Event when a client connects to this server.
        /// </summary>
        private AutoResetEvent connectionEvent;

        /// <summary>
        /// The stream that sends data.
        /// </summary>
        private StreamWriter tcpWriter;

        /// <summary>
        /// Logger for reading from the client.
        /// </summary>
        private Logger rxLogger;

        /// <summary>
        /// Logger for writing to the client/
        /// </summary>
        private Logger txLogger;

        /// <summary>
        /// Event that is called when something is read from the rx thread.
        /// </summary>
        private Action<string> readEvent;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        /// <param name="readEvent">
        /// Action that is performed when something is read from the rx thread.
        /// </param>
        public IrcServer( short port, Action<string> readEvent )
        {
            ArgumentChecker.IsNotNull( readEvent, nameof( readEvent ) );

            this.server = new TcpListener( IPAddress.Parse( "127.0.0.1" ), port );
            this.readerThread = new Thread( this.ReaderThread );
            this.connectionEvent = new AutoResetEvent( false );

            this.rxLogger = new Logger( "Server_RX" );
            this.txLogger = new Logger( "Server_TX" );
        }

        // -------- Functions ---------

        /// <summary>
        /// Opens the TCP connection.
        /// </summary>
        public void Start()
        {
            this.server.Start();
            this.readerThread.Start();
        }

        /// <summary>
        /// Waits for a TCP connection to be made
        /// </summary>
        /// <param name="timeout">How long to wait before timing out in MS.</param>
        /// <returns>True if a connection was made, false if timeout.</returns>
        public bool WaitForConnection( int timeout )
        {
            bool success = this.connectionEvent.WaitOne( timeout );
            if ( success )
            {
                this.txLogger.Log( "Connection made" );
            }
            else
            {
                this.txLogger.Log( "Could not made connection after " + timeout + "ms" );
            }

            return success;
        }

        /// <summary>
        /// Sends the given message to the client.
        /// </summary>
        /// <param name="s"></param>
        public void SendMessage(string s)
        {
            if ( this.tcpWriter == null )
            {
                throw new InvalidOperationException(
                    "TCP writer not open, ensure the connection is open."
                );
            }
            this.txLogger.Log( s );
            this.tcpWriter.WriteLine( s );
            this.tcpWriter.Flush();
        }

        /// <summary>
        /// Closes the TCP connection.
        /// </summary>
        public void Stop()
        {
            if ( this.readerThread.IsAlive )
            {
                this.readerThread.Abort();
                this.readerThread.Join();
            }
            this.server.Stop();
        }

        /// <summary>
        /// Stops the TCP connection.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// The thread that reads from the TCP socket.
        /// </summary>
        private void ReaderThread()
        {
            while ( true )
            {
                try
                {
                    using ( TcpClient client = server.AcceptTcpClient() )
                    {
                        this.tcpWriter = new StreamWriter( client.GetStream() );
                        StreamReader reader = new StreamReader( client.GetStream() );
                        this.connectionEvent.Set();

                        // ReadLine blocks.
                        string s = reader.ReadLine();
                        if ( ( string.IsNullOrWhiteSpace( s ) == false ) && ( string.IsNullOrEmpty( s ) == false ) )
                        {
                            this.rxLogger.Log( s );
                            readEvent( s );
                        }
                    }
                }
                catch ( ThreadAbortException )
                {
                    // Thread aborting.  Return.
                    this.rxLogger.Log( "Caught thread abort exception in server reader thread.  Aborting." );
                    return;
                }
                catch ( Exception err )
                {
                    this.rxLogger.Log( "CAUGHT EXCEPTION IN RX THREAD:" );
                    this.rxLogger.Log( err.ToString() );
                }
                finally
                {
                    this.tcpWriter = null;
                }
            }
        }
    }
}
