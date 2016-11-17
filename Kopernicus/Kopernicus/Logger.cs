/**
 * Kopernicus Planetary System Modifier
 * ====================================
 * Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
 * Maintained by: Thomas P., NathanKell and KillAshley
 * Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace, Sigma88
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
 * which is copyright 2011-2015 Squad. Your usage of Kerbal Space Program
 * itself is governed by the terms of its EULA, not the license above.
 * 
 * https://kerbalspaceprogram.com
 */

using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Kopernicus
{
    // A message logging class to replace Debug.Log
    public class Logger
    {
        // Is the logger initialized?
        private static bool isInitialized = false;

        // Logger output path
        public static string LogDirectory
        {
            get { return KSPUtil.ApplicationRootPath + "Logs/" + typeof (Logger).Assembly.GetName().Name + "/"; }
        }

        // ==> Implement own version
        public static string version
        {
            get { return Constants.Version.version; }
        }

        // Default logger
        private static Logger _DefaultLogger = null;

        public static Logger Default
        {
            get
            {
                if (_DefaultLogger == null)
                    _DefaultLogger = new Logger();
                return _DefaultLogger;
            }
        }

        // Currently active logger
        private static Logger _ActiveLogger = null;

        public static Logger Active
        {
            get
            {
                if (_ActiveLogger.loggerStream == null)
                    return _DefaultLogger;
                return _ActiveLogger;
            }
            private set { _ActiveLogger = value; }
        }

        // The complete path of this log
        TextWriter loggerStream = null;

        // Write text to the log
        public void Log(object o)
        {
            if (loggerStream == null)
                return;

            loggerStream.WriteLine("[LOG " + DateTime.Now.ToString("HH:mm:ss") + "]: " + o);
            loggerStream.Flush();
        }

        // Write text to the log
        public void LogException(Exception e)
        {
            if (loggerStream == null)
                return;

            loggerStream.WriteLine("[LOG " + DateTime.Now.ToString("HH:mm:ss") + "]: Exception Was Recorded: " +
                                   e.Message + "\n" + e.StackTrace);

            if (e.InnerException != null)
                loggerStream.WriteLine("[LOG " + DateTime.Now.ToString("HH:mm:ss") + "]: Inner Exception Was Recorded: " +
                                       e.InnerException.Message + "\n" + e.InnerException.StackTrace);
            loggerStream.Flush();
        }

        // Set logger as the active logger
        public void SetAsActive()
        {
            Logger.Active = this;
        }

        public void Flush()
        {
            if (loggerStream == null)
                return;

            loggerStream.Flush();
        }

        // Close the logger
        public void Close()
        {
            if (loggerStream == null)
                return;

            loggerStream.Flush();
            loggerStream.Close();
            loggerStream = null;
        }

        // Create a logger
        public Logger([Optional] string LogFileName)
        {
            if (!isInitialized)
                return;
            if (String.IsNullOrEmpty(LogFileName))
                LogFileName = typeof(Logger).Assembly.GetName().Name;


            try
            {
                // Open the log file (overwrite existing logs)
                LogFileName = LogFileName.Replace("/", "").Replace("\\", "");
                string LogFile = Logger.LogDirectory + LogFileName + ".log";
                loggerStream = new StreamWriter(LogFile);

                // Write an opening message
                string logVersion = "//=====  " + version + "  =====//";

                // Create the header this way, because I'm maybe too stupid to find the "fill" function
                string logHeader = "";
                for (int i = 0; i < (logVersion.Length - 4); i++)
                {
                    logHeader += "=";
                }
                logHeader = "//" + logHeader + "//";

                loggerStream.WriteLine(logHeader + "\n" + logVersion + "\n" + logHeader); // Don't use Log() because we don't want a date time in front of the Versioning.
                Log ("Logger \"" + LogFileName + "\" was created");
            }
            catch (Exception e) 
            {
                Debug.LogException (e);
            }
        }

        // Cleanup the logger
        ~Logger()
        {
            loggerStream.Flush ();
            loggerStream.Close ();
        }

        // Initialize the Logger (i.e. delete old logs) 
        static Logger()
        {
            // Attempt to clean the log directory
            try
            {
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);

                // Clear out the old log files
                foreach(string file in Directory.GetFiles(LogDirectory))
                {
                    File.Delete(file);
                }
            }
            catch (Exception e) 
            {
                Debug.LogException (e);
                return;
            }

            isInitialized = true;
        }
    }
}

