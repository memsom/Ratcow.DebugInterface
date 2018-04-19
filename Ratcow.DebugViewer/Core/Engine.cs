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

using Ratcow.Debugging.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ratcow.DebugViewer.Core
{
    public delegate void UpdateListViewDelegate(NameContainer[] nameData);
    public delegate void UpdateTextViewDelegate(string data);
    public delegate void UpdateNoneDelegate();

    /// <summary>
    /// Engine that is async and pull realtime debug data from the attached service.
    /// </summary>
    public class Engine : IDisposable
    {
        public event UpdateTextViewDelegate RefreshDetailTextView;
        public event UpdateListViewDelegate RefreshNameListView;

        /// <summary>
        /// The Url we will use for the web service. Defaults to localhost
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Creates the Engine.
        /// </summary>
        /// <param name="url">Set this value if the service has a different Url entry point</param>
        public Engine(string url = "http://localhost:9001/DebugInterface")
        {
            Url = url;
        }

        /// <summary>
        /// Loads names from the service
        /// </summary>
        async Task<string[]> LoadNames()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var service = GetService())
                    {
                        return service.GetVariableNames();
                    }
                }
                catch (Exception ex)
                {
                    return new string[0];
                }

            });
        }

        DebugInterfaceClient GetService()
        {
            return new DebugInterfaceClient(Url);
        }

        /// <summary>
        /// Processes names we got from the service
        /// </summary>
        async Task<NameContainer[]> ProcessNames(string[] names, string filter)
        {
            return await Task.Run(() =>
            {
                var filterIsValid = !string.IsNullOrEmpty(filter?.Trim());
                var results = new List<NameContainer>();
                foreach (var name in names)
                {
                    var values = name.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                    if (filterIsValid && !values[0].StartsWith(filter))
                    {
                        continue;
                    }

                    if (values.Length == 1)
                    {
                        results.Add(new NameContainer { Name = values[0], Type = "object", IsArray = false });
                    }
                    else
                    {
                        results.Add(new NameContainer { Name = values[0], Type = values[1], IsArray = values[1].EndsWith("]") });
                    }

                }

                return results.ToArray();
            });
        }

        /// <summary>
        /// Called to directly get the detail
        /// 
        /// This can probably be merged with RefreshDetail, but at the moment the
        /// two legacy implementations are both here.
        /// </summary>
        public async Task<string> GetDetail(NameContainer value)
        {
            return await Task.Run(() =>
            {

                var result = string.Empty;

                try
                {
                    using (var service = GetService())
                    {
                        result = service.GetVariableValue(value.Name);
                    }
                }
                catch (Exception)
                {

                }

                return result;
            });
        }

        public async Task<Boolean> SetDetail(NameContainer name, string value)
        {
            return await Task.Run(() =>
            {

                var result = false;

                try
                {
                    using (var service = GetService())
                    {
                        result = service.SetVariableValue(name.Name, value);
                    }
                }
                catch (Exception)
                {

                }

                return result;
            });
        }

        /// <summary>
        /// Called to update the detail via the event
        /// </summary>
        public async Task RefreshDetail(NameContainer data)
        {
            await Task.Run(() =>
            {
                var result = string.Empty;

                try
                {
                    using (var service = GetService())
                    {
                        result = service.GetVariableValue(data.Name);
                    }

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
        public async Task<NameContainer[]> GetNames(string filter = null)
        {
            return await Task.Run(async () =>
            {
                var names = await LoadNames();

                var nameData = await ProcessNames(names, filter);

                return nameData;
            });
        }

        /// <summary>
        /// Called to update the variable names
        /// </summary>
        public async Task RefreshNames(string filter = null)
        {
            await Task.Run(async () =>
            {
                var names = await LoadNames();

                var nameData = await ProcessNames(names, filter);

                RefreshNameListView?.Invoke(nameData);
            });
        }

        public async Task RefreshNameTree(TreeView treeView, SynchronizationContext c, string filter = null)
        {
            await Task.Run(async () =>
            {
                var names = await LoadNames();

                var nameData = await ProcessNames(names, filter);

                var tree = await MakeTree(nameData);

                await CreateTree(tree, treeView, c);
            });
        }

        async Task<TreeNodeItem> MakeTree(NameContainer[] names)
        {
            return await Task.Run<TreeNodeItem>(async () =>
            {
                var root = new TreeNodeItem { Name = "root" };

                foreach (var name in names)
                {
                    var path = name.Name;
                    await WalkPath(root, path, path, name);
                }


                return root;
            });
        }

        async Task WalkPath(TreeNodeItem node, string path, string fullpath, NameContainer name)
        {
            await Task.Run(async () =>
            {
                var pathArray = path.Split('.');
                if (pathArray == null || pathArray.Length <= 1)
                {
                    if (!node.Nodes.Any(p => p.Path == path))
                    {
                        var next = new TreeNodeItem { Path = path, Name = path, Final = true, NameContainer = name };
                        node.Nodes.Add(next);
                    }
                }
                else
                {
                    var newPath = string.Join(".", pathArray.Skip(1));
                    var curPath = string.Join(".", pathArray.Take(1));

                    TreeNodeItem next = null;
                    if (!node.Nodes.Any(p => p.Path == curPath))
                    {
                        next = new TreeNodeItem { Name = pathArray[0], Path = curPath };
                        node.Nodes.Add(next);
                    }
                    else
                    {
                        next = node.Nodes.First(p => p.Path == curPath);
                    }

                    await WalkPath(next, newPath, fullpath, name);
                }
            });
        }

        public async Task CreateTree(TreeNodeItem node, TreeView control, SynchronizationContext c)
        {
            TreeNode root = null;

            c.Send((s) =>
            {
                control.Nodes.Clear();
                root = control.Nodes.Add("root");
            }, null);

            await Task.Run(async () =>
            {
                foreach (var item in node.Nodes)
                {
                    await WalkTree(control, c, item, root);
                }
            });
        }

        async Task WalkTree(TreeView control, SynchronizationContext c, TreeNodeItem nodeItem, TreeNode node)
        {
            TreeNode newNode = null;

            c.Send((s) =>
            {
                newNode = node.Nodes.Add(nodeItem.Name);
                newNode.Tag = nodeItem;
            }, null);

            await Task.Run(async () =>
            {                
                if (!nodeItem.Final)
                {
                    foreach (var item in nodeItem.Nodes)
                    {
                        await WalkTree(control, c, item, newNode);
                    }
                }
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
                    // TODO: dispose managed state (managed objects).
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

    public class TreeNodeItem
    {
        public TreeNodeItem()
        {
            Nodes = new List<TreeNodeItem>();
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public bool Final { get; set; }
        public List<TreeNodeItem> Nodes { get; }
        public NameContainer NameContainer { get; internal set; }
    }
}
