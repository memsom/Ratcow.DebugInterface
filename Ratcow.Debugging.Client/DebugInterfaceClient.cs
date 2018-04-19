using System;
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
    public class DebugInterfaceClient: IDisposable
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
        public async Task<string[]> GetVariableNamesAsync()
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

        /// <summary>
        /// Super simple
        /// </summary>
        public bool SetVariableValue(string name, string json)
        {
            try
            {
                using (var client = new Service.DebugInterface())
                {
                    client.Url = Url;

                    bool result, resultSpecified;

                    client.SetVariableValue(name, json, out result, out resultSpecified);

                    return resultSpecified ? result : false;
                }
            }
            catch (Exception)
            {

                return false;
            }
        }

        /// <summary>
        /// Leverages the Microsoft.Bcl.Async package for .Net 4.0
        /// 
        /// Compiler will moan about needing to install the lib above,
        /// but if the Async versions of the methods are not used,
        /// the programmer can get away with not doing that.
        /// </summary>
        public async Task<bool> SetVariableValueAsync(string name, string json)
        {
            return await Task.Factory.StartNew<bool>(() =>
            {
                return SetVariableValue(name, json);
            });
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DebugInterfaceClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
