using Ratcow.Debugging.Client;
using Ratcow.DebugViewer.Core;
using System;
using System.Collections.Generic;
/*
 * Copyright 2018 Rat Cow Software and Matt Emson. All rights reserved.
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

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ratcow.DebugViewer
{
    /// <summary>
    /// Super simple and lazy form to allow editing values
    /// on the fly. This helps with fixing broken state without
    /// having to restart everything. Okay - you need to understand
    /// JSON, but that's not the end of the world, amirite?
    /// </summary>
    public partial class EditValueForm : Form
    {
        readonly NameContainer NameValue;
        readonly Engine Service;

        public EditValueForm(Engine service, NameContainer name)
        {
            NameValue = name;
            Service = service;

            InitializeComponent();

            Text = $"Edit :: {NameValue.DisplayValue}";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            //save the value 
            var success = await Service.SetDetail(NameValue, fastColoredTextBox1.Text);

            //give some fuzzy warm feedback
            if (success)
            {
                stateLabel.Text = "Saved";
            }
            else
            {
                stateLabel.Text = "Failed to save";
            }
        }

        private async void EditValueForm_Load(object sender, EventArgs e)
        {
            //load the detail
            var data = await Service.GetDetail(NameValue);
            UpdateTextView(data);
        }

        private void fastColoredTextBox1_TextChanged(object sender, EventArgs e)
        {
            //make sure we tell the users when the data is dirty
            //it's down to them to remember to press "save" as
            //this is just a quick and dirty form for now.
            stateLabel.Text = "Unsaved";
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            var data = await Service.GetDetail(NameValue);
            UpdateTextView(data);
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
    }
}
