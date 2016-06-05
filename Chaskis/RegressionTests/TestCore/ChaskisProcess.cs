using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegressionTests.TestCore
{
    /// <summary>
    /// Launches the Chaskis.exe process.
    /// </summary>
    public class ChaskisProcess : IDisposable
    {
        // -------- Fields --------

        /// <summary>
        /// The chaskis process.
        /// </summary>
        private Process chaskisProcess;

        /// <summary>
        /// The chaskis process start info.
        /// </summary>
        private ProcessStartInfo startInfo;

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        public ChaskisProcess()
        {
            this.startInfo = new ProcessStartInfo();
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
        }

        // -------- Functions --------

        /// <summary>
        /// Starts the chaskis process
        /// </summary>
        public void StartProcess()
        {
        }

        /// <summary>
        /// Stops the process and gets the exit code.
        /// </summary>
        /// <returns></returns>
        public int StopProcess()
        {
        }

        /// <summary>
        /// Closes the process and tears down this class.
        /// </summary>
        public void Dispose()
        {
            if ( this.chaskisProcess != null )
            {
                this.chaskisProcess.StandardInput.Write( ' ' );
                this.chaskisProcess.WaitForExit( 10000 );
            }
        }
    }
}
