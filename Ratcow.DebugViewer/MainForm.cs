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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ratcow.DebugViewer
{
    using Core;
    using Core.Helpers;

    public partial class MainForm : Form
    {
        ListViewHelper<NameContainer> lvHelper;

        Engine engine;

        public MainForm()
        {
            InitializeComponent();

            lvHelper = new ListViewHelper<NameContainer>(listView1);

            SetEngine();
        }

        void SetEngine(string url = "http://localhost:9001/DebugInterface")
        {
            if (engine != null)
            {
                engine.RefreshNameListView -= UpdateListView;
                engine.RefreshDetailTextView -= UpdateTextView;
                engine.Dispose();
                engine = null;
            }

            engine = new Engine(url);
            engine.RefreshNameListView += UpdateListView;
            engine.RefreshDetailTextView += UpdateTextView;
        }

        async void MainForm_Load(object sender, EventArgs e)
        {
            var filter = filterTextBox.Text.Trim();

            await RefreshAll(); //engine.RefreshNames(filter);
        }

        void UpdateListView(NameContainer[] nameData)
        {
            if (listView1.InvokeRequired)
            {
                this.Invoke(new UpdateListViewDelegate(UpdateListView), new object[] { nameData });
            }
            else
            {
                lvHelper.BeginUpdate();
                try
                {
                    lvHelper.SetData(nameData);
                }
                finally
                {

                    lvHelper.EndUpdate();
                }
            }
        }

        void UpdateTextView(string data)
        {
            if (fastColoredTextBox1.InvokeRequired)
            {
                this.Invoke(new UpdateTextViewDelegate(UpdateTextView), new object[] { data });
            }
            else
            {
                fastColoredTextBox1.Text = data;
            }
        }

        async void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = lvHelper.GetSelectedItemOrDefault();
            if (selected != null)
            {
                await engine.RefreshDetail(selected);
            }
        }

        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.Item == null)
            {
                e.Item = new ListViewItem();
            }
            else
            {
                e.Item.SubItems.Clear();
            }

            var data = lvHelper.ListData[e.ItemIndex];

            e.Item.Text = data.Name;
            e.Item.SubItems.Add(data.Type);

        }

        async Task RefreshAll()
        {
            var c = WindowsFormsSynchronizationContext.Current;

            await Task.Run(async () =>
            {
                var filter = filterTextBox.Text?.Trim();

                await engine.RefreshNames(filter);
               
                await engine.RefreshNameTree(treeView1, c, filter);

            });
        }

        async void button1_Click(object sender, EventArgs e)
        {
            await RefreshAll();
        }

        async void button2_Click(object sender, EventArgs e)
        {
            var selected = lvHelper.GetSelectedItemOrDefault();
            if (selected != null)
            {
                await engine.RefreshDetail(selected);
            }

        }

        async void button3_Click(object sender, EventArgs e)
        {
            if (!engine.Url.StartsWith(addressTextBox.Text))
            {
                SetEngine(addressTextBox.Text);
            }

            await RefreshAll();
        }

        void UpdateAddress(string data)
        {
            if (addressTextBox.InvokeRequired)
            {
                this.Invoke(new UpdateTextViewDelegate(UpdateTextView), new object[] { data });
            }
            else
            {
                addressTextBox.Text = data;
            }
        }

        void OnSelectedUrl(string newUrl)
        {
            UpdateAddress(newUrl);
        }

        void button4_Click(object sender, EventArgs e)
        {
            var linksForm = new LinksForm();
            linksForm.NewUrl = addressTextBox.Text;
            linksForm.SelectedUrl += OnSelectedUrl;
            linksForm.ShowDialog();
            linksForm.SelectedUrl -= OnSelectedUrl;

            linksForm = null;
        }

        async void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var selected = e.Node;
            if (selected != null && selected.Tag != null)
            {
                var item = (TreeNodeItem)selected.Tag;
                if (item.Final)
                {
                    await engine.RefreshDetail(item.NameContainer);
                }
            }
        }

        async void filterTextBox_TextChanged(object sender, EventArgs e)
        {
            await RefreshAll();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            var selected = lvHelper.GetSelectedItemOrDefault();
            if(selected!= null)
            {
                filterTextBox.Text = selected.Name;
            }
        }

        private async void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var selected = lvHelper.GetSelectedItemOrDefault();
            if (selected != null)
            {
                var dialog = new EditValueForm(engine, selected);
                dialog.ShowDialog();

                await RefreshAll();
            }
        }
    }
}
