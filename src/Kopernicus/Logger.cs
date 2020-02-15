/**
 * Kopernicus Planetary System Modifier
 * ------------------------------------------------------------- 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301  USA
 * 
 * This library is intended to be used as a plugin for Kerbal Space Program
 * which is copyright of TakeTwo Interactive. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.IO;
using UnityEngine;
using Object = System.Object;

namespace Kopernicus
{
    // A generic message logging class to replace Debug.Log
    public class Logger
    {
        // Is the logger initialized?
        private static readonly Boolean IsInitialized;

        // Logger output path
        private static String LogDirectory
        {
            get { return KSPUtil.ApplicationRootPath + "Logs/" + typeof (Logger).Assembly.GetName().Name + "/"; }
        }

        // ==> Implement own version
        private static String Version
        {
            get { return Constants.Version.VersionId; }
        }

        // Default logger
        private static Logger _defaultLogger;

        public static Logger Default
        {
            get
            {
                if (_defaultLogger != null)
                {
                    return _defaultLogger;
                }
                
                _defaultLogger = new Logger(typeof(Logger).Assembly.GetName().Name);
                Debug.Log("[Kopernicus] Default logger initialized as " + typeof(Logger).Assembly.GetName().Name);
                return _defaultLogger;
            }
        }

        // Currently active logger
        private static Logger _activeLogger;

        public static Logger Active
        {
            get { return _activeLogger._loggerStream == null ? _defaultLogger : _activeLogger; }
            private set { _activeLogger = value; }
        }

        // The complete path of this log
        private TextWriter _loggerStream;

        // Whether to flush the stream after every write
        private readonly Boolean _autoFlush;

        // Write text to the log
        public void Log(Object o)
        {
            if (_loggerStream == null)
            {
                return;
            }

            _loggerStream.WriteLine("[LOG " + DateTime.Now.ToString("HH:mm:ss") + "]: " + o);
            if (_autoFlush)
            {
                _loggerStream.Flush();
            }
        }

        // Write text to the log
        public void LogException(Exception e)
        {
            if (_loggerStream == null)
            {
                return;
            }

            _loggerStream.WriteLine("[LOG " + DateTime.Now.ToString("HH:mm:ss") + "]: Exception Was Recorded: " +
                                   e.Message + "\n" + e.StackTrace);

            if (e.InnerException != null)
            {
                _loggerStream.WriteLine("[LOG " + DateTime.Now.ToString("HH:mm:ss") +
                                       "]: Inner Exception Was Recorded: " +
                                       e.InnerException.Message + "\n" + e.InnerException.StackTrace);
            }
            _loggerStream.Flush();
        }

        // Set logger as the active logger
        public void SetAsActive()
        {
            Active = this;
        }

        public void Flush()
        {
            _loggerStream?.Flush();
        }

        // Close the logger
        public void Close()
        {
            if (_loggerStream == null)
            {
                return;
            }

            _loggerStream.Flush();
            _loggerStream.Close();
            _loggerStream = null;
        }

        // Create a logger
        public Logger(String logFileName = null, Boolean autoFlush = false)
        {
            _autoFlush = autoFlush;
            SetFilename(logFileName);
        }

        // Set/Change the filename we log to
        public void SetFilename(String logFileName)
        {
            Close();

            if (!IsInitialized)
            {
                return;
            }

            if (String.IsNullOrEmpty(logFileName))
            {
                return; // effectively makes this logger a black hole
            }

            try
            {
                // Open the log file (overwrite existing logs)
                String logFile = LogDirectory + logFileName + ".log";
                Directory.CreateDirectory(Path.GetDirectoryName(logFile));
                _loggerStream = new StreamWriter(logFile);

                // Write an opening message
                String logVersion = "//=====  " + Version + "  =====//";
                String logHeader = new String('=', logVersion.Length - 4);
                logHeader = "//" + logHeader + "//";

                _loggerStream.WriteLine(logHeader + "\n" + logVersion + "\n" + logHeader); // Don't use Log() because we don't want a date time in front of the version.
                Log("Logger \"" + logFileName + "\" was created");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        // Cleanup the logger
        ~Logger()
        {
            Close();
        }

        // Initialize the Logger (i.e. delete old logs) 
        static Logger()
        {
            // Attempt to clean the log directory
            try
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }

                // Clear out the old log files
                Directory.Delete(LogDirectory, true);
            }
            catch (Exception e) 
            {
                Debug.LogException(e);
                return;
            }

            IsInitialized = true;
        }
    }
}

