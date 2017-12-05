/*
 * Copyright 2017 Rat Cow Software and Matt Emson. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification, are
 * permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, this list of
 *    conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice, this list
 *    of conditions and the following disclaimer in the documentation and/or other materials
 *    provided with the distribution.
 * 3. Neither the name of the Rat Cow Software nor the names of its contributors may be used
 *    to endorse or promote products derived from this software without specific prior written
 *    permission.
 *
 * THIS SOFTWARE IS PROVIDED BY RAT COW SOFTWARE "AS IS" AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 * FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
 * ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * The views and conclusions contained in the software and documentation are those of the
 * authors and should not be interpreted as representing official policies, either expressed
 * or implied, of Rat Cow Software and Matt Emson.
 *
 */

using System;
using System.Threading.Tasks;

namespace Ratcow.DebugViewer
{
    using Debugging.Client;

    public delegate void DebugInterfaceEngineUpdateListViewDelegate(string[] nameData);
    public delegate void DebugInterfaceEngineUpdateTextViewDelegate(string data);

    public class DebugInterfaceEngine : IDisposable
    {
        public event DebugInterfaceEngineUpdateTextViewDelegate RefreshDetailTextView;
        public event DebugInterfaceEngineUpdateListViewDelegate RefreshNameListView;

        DebugInterfaceClient client;

        /// <summary>
        /// The Url we will use for the web service. Defaults to localhost
        /// </summary>
        public string Url { get { return client?.Url; } }

        /// <summary>
        /// Creates the Engine.
        /// </summary>
        /// <param name="url">Set this value if the service has a different Url entry point</param>
        public DebugInterfaceEngine(string url = "http://localhost:9001/DebugInterface")
        {
            client = new DebugInterfaceClient(url);
        }

        /// <summary>
        /// Loads names from the service
        /// </summary>
        async Task<string[]> LoadNames()
        {
            return await client.GetVariableNamesAsync();
        }

        /// <summary>
        /// Called to directly get the detail
        /// 
        /// This can probably be merged with RefreshDetail, but at the moment the
        /// two legacy implementations are both here.
        /// </summary>
        public async Task<string> GetDetail(string name)
        {
            return await client.GetVariableValueAsync(name);
        }

        /// <summary>
        /// Called to update the detail via the event
        /// </summary>
        public async Task RefreshDetail(string name)
        {
            await Task.Run(async () =>
            {
                var result = string.Empty;

                try
                {
                    result = await client.GetVariableValueAsync(name);

                    RefreshDetailTextView?.Invoke(result);
                }
                catch (Exception)
                {

                    throw;
                }

            });
        }

        /// <summary>
        /// Called to directly get the variable names
        /// 
        /// This can probably be merged with RefreshDetail, but at the moment the
        /// two legacy implementations are both here.
        /// </summary>
        public async Task<string[]> GetNames()
        {
            return await Task.Run(async () =>
            {
                var names = await LoadNames();

                return names;
            });
        }

        /// <summary>
        /// Called to update the variable names
        /// </summary>
        public async Task RefreshNames()
        {
            await Task.Run(async () =>
            {
                var names = await LoadNames();

                RefreshNameListView?.Invoke(names);
            });
        }

        #region IDisposable Support
        bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    client = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Engine() {
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
