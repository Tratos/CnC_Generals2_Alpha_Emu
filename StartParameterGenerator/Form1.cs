using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StartParameterGenerator
{
    public partial class Form1 : Form
    {
        public string loc = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
        private List<string> wordList = new List<string> { };
        private List<int> highlightedIndices = new List<int>();
        public string selection;
        public string filename;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripButton1.Enabled = false;
            ReadConfig();
            GenListView();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ReadConfig()
        {
            string filePath = loc + "conf\\cnc_commands.txt";
            if (File.Exists(filePath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(filePath);
                    wordList.AddRange(lines);
                }
                catch (Exception ex)
                {

                }
            }

        }

        private void GenListView()
        {
            foreach (var items in wordList)
            {
                listBox1.Items.Add(items);
            }
            listBox1.DrawMode = DrawMode.OwnerDrawFixed;
            listBox1.DrawItem += ListBox_DrawItem;
        }

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {
            string searchTerm = toolStripTextBox1.Text.ToLower();
            highlightedIndices.Clear();

            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                string item = listBox1.Items[i].ToString().ToLower();

                if (item.Contains(searchTerm))
                {
                    highlightedIndices.Add(i); 
                }
            }

            if (highlightedIndices.Count == 0)
            {
                MessageBox.Show("not found.");
            }
            else
            {
                toolStripStatusLabel2.Text = highlightedIndices.Count.ToString();
            }

            listBox1.Invalidate();

        }

        private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            Brush textBrush = Brushes.Black;

            if (highlightedIndices.Contains(e.Index))
            {
                e.Graphics.FillRectangle(Brushes.Yellow, e.Bounds);
                textBrush = Brushes.Red;
            }
            e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), e.Font, textBrush, e.Bounds);
            e.DrawFocusRectangle();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0) 
            {
                toolStripStatusLabel5.Text = listBox1.SelectedItem.ToString();
                selection = listBox1.SelectedItem.ToString();
                toolStripButton1.Enabled = true;
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if(selection != null)
            {
                rtb1.SelectedText = "-" + selection + " " + toolStripTextBox2.Text  + " ";
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.txt|*.txt";
            d.InitialDirectory = loc;
            if (d.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader reader = new StreamReader(d.FileName, Encoding.UTF8))
                {
                    rtb1.Text = reader.ReadToEnd();
                }
                filename = d.FileName;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.txt|*.txt";
            d.InitialDirectory = loc;
            d.FileName = filename;
            if (d.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(d.FileName, false, Encoding.UTF8))
                {
                    writer.Write(rtb1.Text);
                }
            }

        }
    }
}
