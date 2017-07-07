﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ratcow.Debugging.Client
{
    /// <summary>
    /// Super simple. Avoids the complexity of WCF clients, by
    /// hosting the service as a classic WebService. This 
    /// restricts the data to HTTP, but this is meant to be
    /// a simple, lightweight interface to the data in another
    /// process, so I'm okay with that.
    /// 
    /// This code is the .Net 4.0 version.
    /// 
    /// This is wrapped up to make it a lot less fussy to use the 
    /// client. Create an instance and call the methods - that's it.
    /// </summary>
    public class DebugInterfaceClient
    {
        /// <summary>
        /// the Url we will connect to
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// just so that we don't needlessly receate this on error
        /// </summary>
        string[] noValuesArray = new string[0];

        /// <summary>
        /// Never set a default Url and this will probably bite us 
        /// on the backside at some point...
        /// </summary>
        public DebugInterfaceClient(string url)
        {
            Url = url;
        }

        /// <summary>
        /// Super simple 
        /// </summary>
        public string[] GetVariableNames()
        {
            try
            {
                using (var client = new Service.DebugInterface())
                {
                    client.Url = Url;
                    return client.GetVariableNames();
                }
            }
            catch (Exception)
            {

                return noValuesArray;
            }
        }

        /// <summary>
        /// Leverages the Microsoft.Bcl.Async package for .Net 4.0
        /// 
        /// Compiler will moan about needing to install the lib above,
        /// but if the Async versions of the methods are not used,
        /// the programmer can get away with not doing that.
        /// </summary>
        public async Task<string[]> GetVariableNamesAsync(string name)
        {
            return await Task.Factory.StartNew<string[]>(() =>
            {
                return GetVariableNames();
            });
        }

        /// <summary>
        /// Super smple
        /// </summary>
        public string GetVariableValue(string name)
        {
            try
            {
                using (var client = new Service.DebugInterface())
                {
                    client.Url = Url;
                    return client.GetVariableValue(name);
                }
            }
            catch (Exception)
            {

                return string.Empty;
            }
        }

        /// <summary>
        /// Leverages the Microsoft.Bcl.Async package for .Net 4.0
        /// 
        /// Compiler will moan about needing to install the lib above,
        /// but if the Async versions of the methods are not used,
        /// the programmer can get away with not doing that.
        /// </summary>
        public async Task<string> GetVariableValueAsync(string name)
        {
            return await Task.Factory.StartNew<string>(() =>
            {
                return GetVariableValue(name);
            });
        }
    }
}
