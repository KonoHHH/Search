using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cactoos;
using Cactoos.Text;
using System.IO;

using static System.Collections.Generic.Create;
using Cactoos.Scalar;
using System.Threading;
using static System.Windows.Forms.ListViewItem;
using System.Diagnostics;

namespace Search
{
    public partial class Form1 : Form
    {
        private string _location;
        private string[] _results;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !new IsBlank(fbd.SelectedPath).Value())
                {
                    _location = fbd.SelectedPath;
                    textBox1.Text = _location;
                }
            }
        }

        internal async void AppDomainException(object sender, UnhandledExceptionEventArgs e)
        {
            await Message($"An unhandled exception happened: {e.ExceptionObject}", TimeSpan.FromMinutes(1));
        }

        internal async void UnhandledExceptionAsync(object sender, ThreadExceptionEventArgs e)
        {
            await Message(e.Exception.Message, TimeSpan.FromMinutes(1));
        }

        private async Task Message(string content, TimeSpan span = default(TimeSpan))
        {
            label1.Text = content;
            label1.BringToFront();  
            await Task.Delay(span);
            label1.Text = string.Empty;
            label1.SendToBack();
        }

        private async void button2_ClickAsync(object sender, EventArgs e)
        {
            string text = textBox2.Text;
            if (!new IsBlank(text).Value() && !new IsBlank(_location).Value())
            {
                IText pattern;
                if (text.Contains('.'))
                {
                    pattern = new Text(text);
                }
                else
                {
                    pattern =
                            new Text(
                                new LowerText(
                                    new TrimmedText(text)
                                ).String() + '*'
                            );
                }
                var items =
                    new ErrorSafeScalar<string[]>(() =>
                        Directory.GetFileSystemEntries(
                            _location,
                            pattern.String(),
                            SearchOption.AllDirectories
                        ),
                        () => array<string>()
                    );
                label1.Text = "the search is being run...";
                var result = items.Value();
                label1.Text = string.Empty;
                if (!items.HasErrors())
                {
                    //populate list box
                    if (result.Any())
                    {
                        string[] source = array(items.Value().Distinct());
                        _results = source;
                        listView1.Items.Clear();

                        ImageList il = new ImageList();
                        foreach (var item in source)
                        {
                            il.Images.Add(
                                new Icon(
                                    Icon.ExtractAssociatedIcon(item),
                                    new Size(60, 60)
                                )
                            );
                        }

                        listView1.LargeImageList = il;

                        for (int i = 0; i < source.Length; i++)
                        {
                            listView1.Items.Add(new ListViewItem()
                            {
                                ImageIndex = i,
                                Text = Path.GetFileName(source[i])
                            });
                        }
                    }
                }
                else
                {
                    await Message(items.Errors().FirstOrDefault()?.Message, TimeSpan.FromSeconds(15)).ConfigureAwait(false);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = Environment.CurrentDirectory;
        }

        private async void listView1_ItemActivate(object sender, EventArgs e)
        {
            var item = listView1.SelectedIndices[0];
            string file = _results.ElementAtOrDefault(item);
            await Message($"Opening the file {file}", TimeSpan.FromSeconds(1));
            Process.Start(file);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
