/*
 * Copyright 2010 Rat Cow Software and Matt Emson. All rights reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Ratcow.DebugViewer.Core.Helpers
{
    public class ListViewHelper<Data> : IDisposable
    {
        ListView fview = null;
        List<Data> fdata = default(List<Data>);

        public ListViewHelper(ListView view)
        {
            fview = view;
            fview.VirtualMode = true;
            fview.MultiSelect = false; //select one and only one
            Updating = false;
        }

        public bool Updating { get; internal set; }

        public List<Data> ListData { get { return fdata; } }

        public void BeginUpdate()
        {
            fview.BeginUpdate();
            fview.VirtualListSize = 0;
            Updating = true;
        }

        public void EndUpdate()
        {
            if (fdata != null)
            {
                fview.VirtualListSize = fdata.Count;
            }
            Updating = false;
            fview.EndUpdate();
        }

        public void SetData(IEnumerable<Data> data)
        {
            if (!Updating)
                BeginUpdate();
            try
            {
                fdata = new List<Data>(data);
            }
            finally
            {
                if (Updating)
                    EndUpdate();
            }
        }

        /// <summary>
        /// Virtual mode notoriously screws this up badly
        /// </summary>
        public int GetSelectedIndex()
        {
            int result = -1;
            for (int i = 0; i < fview.Items.Count; i++)
            {
                if (fview.Items[i].Selected)
                {
                    result = i; //could just return here, but that's messy
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Experimental
        /// </summary>
        public bool SetSelectedIndex(int index)
        {
            try
            {
                fview.Items[index].Selected = true; //this should work
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.Message);
                System.Diagnostics.Debug.Write(ex.StackTrace);
                return false;
            }

        }

        /// <summary>
        /// This is a "nice to have" rather than anything really useful.
        /// </summary>
        /// <returns></returns>
        public Data GetSelectedItemOrDefault()
        {
            try
            {
                int itemIndex = GetSelectedIndex();

                if (itemIndex > -1 && itemIndex < fdata.Count)
                {
                    return fdata[itemIndex];
                }
            }
            catch (Exception)
            {

            }

            return default(Data);
        }

        #region fdata access

        //added to make the helper allow us to access the internal data without a reference to original instance

        public Data this[int index] { get { return (fdata == null ? default(Data) : fdata[index]); } }

        public int Count { get { return (fdata == null ? 0 : fdata.Count); } }

        #endregion fdata access

        #region IDisposable Members

        public void Dispose()
        {
            fdata = null;
            fview = null;
        }

        #endregion IDisposable Members
    }
}
