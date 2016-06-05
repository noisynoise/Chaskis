
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.IO;
using System.Text;
using SethCS.Exceptions;

namespace RegressionTests.TestCore
{
    /// <summary>
    /// Logs events to the console with a time stamp
    /// and a prefix.
    /// 
    /// For example:
    /// 2016-06-05 06:12:56.1234    LoggerName>  Message
    /// </summary>
    public class Logger
    {
        // --------- Fields --------

        /// <summary>
        /// Where to log to.
        /// </summary>
        private TextWriter output;

        // --------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="writer">Where to write the log to.  Default Console.Out</param>
        public Logger( string name, TextWriter writer = null )
        {
            ArgumentChecker.StringIsNotNullOrEmpty( name, nameof( name ) );

            this.Name = name;
            if ( writer == null )
            {
                this.output = Console.Out;
            }
            else
            {
                this.output = writer;
            }
        }

        // -------- Properties --------

        /// <summary>
        /// The name of the logger.
        /// </summary>
        public string Name { get; private set; }

        // -------- Functions --------

        /// <summary>
        /// Logs the given message to this log.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Log( string message )
        {
            StringBuilder messageToSend = new StringBuilder();
            using ( StringReader reader = new StringReader( message ) )
            {
                string line;
                while ( ( line = reader.ReadLine() ) != null )
                {
                    if ( string.IsNullOrEmpty( line ) == false )
                    {
                        DateTime timeStamp = DateTime.Now;
                        messageToSend.Append(
                            string.Format(
                                "{0}    {1}>  {2}",
                                timeStamp.ToString(),
                                this.Name,
                                message
                            )
                        );
                    }
                }
            }

            this.output.WriteLine(
                messageToSend.ToString()
            );
            this.output.Flush();
        }
    }
}
