using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ratcow.DebugViewer
{
    public delegate void SelectedUrlHandler(string newUrl);
    public delegate void UpdateTextViewHandler();

    public partial class LinksForm : Form
    {
        private Settings settings;

        public LinksForm()
        {
            InitializeComponent();
        }

        public event SelectedUrlHandler SelectedUrl;

        public string NewUrl { get; internal set; }

        void listBox1_DoubleClick(object sender, EventArgs e)
        {
            SelectItem();
        }

        void SelectItem()
        {
            var selected = listBox1.SelectedItem;
            if (selected is DebugTarget dt)
            {
                SelectedUrl?.Invoke(dt.Url);

                DialogResult = DialogResult.OK;
            }
        }

        async void LinksForm_Load(object sender, EventArgs e)
        {
            textBox1.Text = NewUrl;

            await LoadConfig();
        }

        void SetDataSource()
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new UpdateTextViewHandler(SetDataSource));
            }
            else
            {
                listBox1.DataSource = null;

                listBox1.DataSource = settings.Urls;
                listBox1.DisplayMember = "Url";
            }
        }

        async Task LoadConfig()
        {
            await Task.Run(() =>
            {
                if (File.Exists("settings.json"))
                {
                    var file = File.ReadAllText("settings.json");

                    settings = JsonConvert.DeserializeObject<Settings>(file);
                }
                else
                {
                    settings = new Settings();
                }

                SetDataSource();
            });
        }

        void button1_Click(object sender, EventArgs e)
        {
            if (!settings.Urls.Any(u => u.Url == textBox1.Text))
            {
                listBox1.DataSource = null;

                settings.Urls.Add(new DebugTarget { Url = textBox1.Text });

                var file = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText("settings.json", file);

                listBox1.DataSource = settings.Urls;
                listBox1.DisplayMember = "Url";
            }
        }

        void button2_Click(object sender, EventArgs e)
        {
            SelectItem();
        }
    }
}
