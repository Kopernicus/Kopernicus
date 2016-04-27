/**
* Kopernicus Planetary System Modifier
* ====================================
* Created by: BryceSchroeder and Teknoman117 (aka. Nathaniel R. Lewis)
* Maintained by: Thomas P., NathanKell and KillAshley
* Additional Content by: Gravitasi, aftokino, KCreator, Padishar, Kragrathea, OvenProofMars, zengei, MrHappyFace
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

using SystemTimer = System.Timers.Timer;
using System;
using System.Collections;
using UnityEngine;

namespace Kopernicus
{
    namespace OnDemand
    {
        /// <summary>
        /// A Timer that suports a reset
        /// </summary>
        public class Timer
        {
            /// <summary>
            /// An action that is invoked when the timer elapses
            /// </summary>
            public Action callback { get; set; }

            /// <summary>
            /// The real timer
            /// </summary>
            public SystemTimer timer { get; set; }

            /// <summary>
            /// The interval of the timer
            /// </summary>
            public Double interval { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Timer"/> class.
            /// </summary>
            /// <param name="interval">The interval for the timer.</param>
            /// <param name="callback">The callback for the timer.</param>
            public Timer(Double interval, Action callback)
            {
                this.interval = interval;
                this.callback = callback;
                timer = new SystemTimer(interval);
                timer.Elapsed += delegate (object sender, System.Timers.ElapsedEventArgs e) { if (callback != null) callback(); };
            }

            /// <summary>
            /// Starts the timer
            /// </summary>
            public void Start()
            {
                timer.Start();
            }

            /// <summary>
            /// Stops the timer
            /// </summary>
            public void Stop()
            {
                timer.Stop();
            }

            public void Reset()
            {
                bool enabled = timer.Enabled;
                timer.Dispose();
                timer = new SystemTimer(interval);
                timer.Elapsed += delegate (object sender, System.Timers.ElapsedEventArgs e) { if (callback != null) callback(); };
                timer.Enabled = enabled;
            }
        }
    }
}
