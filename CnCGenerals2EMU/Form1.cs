using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlazeLibWV;

namespace CnCGenerals2EMU
{
    public partial class Form1 : Form
    {
        public string loc = Path.GetDirectoryName(Application.ExecutablePath) + "\\";
        public readonly object _sync = new object();
        public List<Blaze.Packet> packets = new List<Blaze.Packet>();
        public int packetCount = 0;
        public List<Blaze.Tdf> inlist;
        public int inlistcount;
        public int clientcount;
        public bool makePackets;

        private List<string> wordList = new List<string> {};
        private ToolTip toolTip = new ToolTip();

        public Form1()
        {
            InitializeComponent();
            AttachAutoCompleteHandler(rtb5);
            AttachAutoCompleteHandler(rtb6);
        }

        private void AttachAutoCompleteHandler(RichTextBox richTextBox)
        {
            richTextBox.KeyUp += (sender, e) => HandleAutoCompleteWithTooltips(richTextBox, e, wordList, toolTip);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (makePackets)
            {
                RefreshPackets();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            toolStripStatusLabel2.Text = version;

            BackendLog.Clear();
            Config.Init();
            GenFiles.CreatePackets();
            Logger.box = rtb1;
            BlazeServer.box = rtb1;
            Webserver.box = rtb3;
            RedirectorServer.box = rtb4;

            Logger.Log("BlazeServer for Command&Conquer Generals 2 ALPHA - by Eisbaer");
            Logger.Log("");

            Config.InitialConfig();
            readConfig();
            RefreshProfiles();

            if (Config.MakePacket.ToLower() == "true")
            {
                logPacketsToolStripMenuItem.Checked = true;
                makePackets = true;
                Config.MakePacket = "true";
            }
            else
            {
                logPacketsToolStripMenuItem.Checked = false;
                makePackets = false;
                Config.MakePacket = "false";
            }

            if (Config.RefreshPacket == "true")
            {
                packetsAutoRefreshToolStripMenuItem.Checked = true;
                Config.RefreshPacket = "true";
                timer1.Enabled = true;
            }
            else
            {
                packetsAutoRefreshToolStripMenuItem.Checked = false;
                Config.RefreshPacket = "false";
                timer1.Enabled = false;
            }

            onlyHighToolStripMenuItem.Checked = false;
            highAndMediumToolStripMenuItem.Checked = false;
            allToolStripMenuItem.Checked = false;

            switch (Logger.LogLevel)
            {
                case LogPriority.high:
                    onlyHighToolStripMenuItem.Checked = true;
                    break;
                case LogPriority.medium:
                    highAndMediumToolStripMenuItem.Checked = true;
                    break;
                case LogPriority.low:
                    allToolStripMenuItem.Checked = true;
                    break;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Webserver.Stop();
            BlazeServer.Stop();
            RedirectorServer.Stop();
            Application.Exit();
        }

        private void readConfig()
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
                    Logger.Log("Error: " + ex.Message);
                }
            }

        }

        private void richTextBox_KeyUp(object sender, RichTextBox richTextBox, KeyEventArgs e)
        {
            HandleAutoCompleteWithTooltips(richTextBox, e, wordList, toolTip);
        }

        private void HandleAutoCompleteWithTooltips(RichTextBox richTextBox, KeyEventArgs e, List<string> wordList, ToolTip toolTip)
        {
            string currentWord = "";

            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                toolTip.Hide(richTextBox);
                return;
            }

            var caretPosition = richTextBox.SelectionStart;
            var text = richTextBox.Text.Substring(0, caretPosition);
            var lastSpaceIndex = text.LastIndexOf(' ');

            if (lastSpaceIndex != -1)
            {
                currentWord = text.Substring(lastSpaceIndex + 1);
            }
            else
            {
                currentWord = text;
            }

            var matchingWords = wordList.Where(w => w.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase)).ToList();

            if (matchingWords.Count == 1 && !string.Equals(currentWord, matchingWords[0], StringComparison.OrdinalIgnoreCase))
            {
                var remainder = matchingWords[0].Substring(currentWord.Length);
                richTextBox.Text = richTextBox.Text.Insert(caretPosition, remainder);
                richTextBox.SelectionStart = caretPosition;
                richTextBox.SelectionLength = remainder.Length;
                richTextBox.SelectionColor = Color.Black;
                richTextBox.SelectionStart = caretPosition + remainder.Length;
                richTextBox.SelectionLength = 0;
                toolTip.Hide(richTextBox);
            }
            else if (matchingWords.Count > 0)
            {
                var wordSuggestions = string.Join(", ", matchingWords);
                var cursorPosition = richTextBox.GetPositionFromCharIndex(caretPosition);
                toolTip.Show(wordSuggestions, richTextBox, cursorPosition.X, cursorPosition.Y + 20);

                int wordStartIndex = caretPosition - currentWord.Length;
                richTextBox.SelectionStart = wordStartIndex;
                richTextBox.SelectionLength = currentWord.Length;
                richTextBox.SelectionColor = Color.Red;
                richTextBox.SelectionStart = caretPosition;
                richTextBox.SelectionLength = 0;
                richTextBox.SelectionColor = Color.Black;
            }
            else
            {
                toolTip.Hide(richTextBox);
                int wordStartIndex = caretPosition - currentWord.Length;
                richTextBox.SelectionStart = wordStartIndex;
                richTextBox.SelectionLength = currentWord.Length;
                richTextBox.SelectionColor = Color.Red;
                richTextBox.SelectionStart = caretPosition;
                richTextBox.SelectionLength = 0;
                richTextBox.SelectionColor = Color.Black;
            }
        }


        private void startBlazeServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BlazeServer.Start();
        }

        private void startWebserverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Webserver.Start();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Webserver.Stop();
            BlazeServer.Stop();
            RedirectorServer.Stop();
            Application.Exit();
        }

        private void onlyHighToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allToolStripMenuItem.Checked =
            highAndMediumToolStripMenuItem.Checked = false;
            onlyHighToolStripMenuItem.Checked = true;
            Logger.LogLevel = LogPriority.high;
        }

        private void highAndMediumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            onlyHighToolStripMenuItem.Checked =
            allToolStripMenuItem.Checked = false;
            highAndMediumToolStripMenuItem.Checked = true;
            Logger.LogLevel = LogPriority.medium;
        }

        private void allToolStripMenuItem_Click(object sender, EventArgs e)
        {
            onlyHighToolStripMenuItem.Checked =
            highAndMediumToolStripMenuItem.Checked = false;
            allToolStripMenuItem.Checked = true;
            Logger.LogLevel = LogPriority.low;
        }

        private void editConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Helper.RunShell("notepad.exe", Config.ConfigFile);
        }

        private void logPacketsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (logPacketsToolStripMenuItem.Checked == true)
            {
                logPacketsToolStripMenuItem.Checked = false;
                makePackets = false;
                Config.MakePacket = "false";
            }
            else
            {
                logPacketsToolStripMenuItem.Checked = true;
                makePackets = true;
                Config.MakePacket = "true";
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SearchInLog();
        }

        private void SearchInLog()
        {
            int pos = rtb1.SelectionStart + 1;
            if (pos < 0 || pos >= rtb1.Text.Length)
                pos = 0;
            int next = rtb1.Text.IndexOf(toolStripTextBox1.Text, pos);
            if (next != -1)
            {
                rtb1.SelectionStart = next;
                rtb1.SelectionLength = toolStripTextBox1.Text.Length;
                rtb1.ScrollToCaret();
            }
        }

        private void toolStripTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                rtb1.Focus();
                SearchInLog();
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            RefreshPackets();
        }

        public void RefreshPackets()
        {
            listBox1.Items.Clear();
            string[] files = Directory.GetFiles("logs\\packets\\", "*.bin");
            foreach (string file in files)
            {
                listBox1.Items.Add(Path.GetFileName(file));
                AddPacket(FileToByteArray(file));
            }
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            DeletePackets();
        }

        public void DeletePackets()
        {
            string[] files = Directory.GetFiles("logs\\packets\\", "*.bin");
            foreach (string file in files)
            {
                File.Delete(file);
            }

        }

        private void RefreshProfiles()
        {
            Profiles.Refresh();
            toolStripComboBox1.Items.Clear();
            foreach (Profile p in Profiles.profiles)
                toolStripComboBox1.Items.Add(p.id + ": " + p.name);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            Blaze.Packet p;
            lock (_sync)
            {
                p = packets[n];
            }
            tv1.Nodes.Clear();
            inlistcount = 0;
            inlist = new List<Blaze.Tdf>();
            List<Blaze.Tdf> Fields = Blaze.ReadPacketContent(p);
            foreach (Blaze.Tdf tdf in Fields)
                tv1.Nodes.Add(TdfToTree(tdf));
        }

        private void tv1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode t = e.Node;
            if (t != null && t.Name != "")
            {
                int n = Convert.ToInt32(t.Name);
                Blaze.Tdf tdf = inlist[n];
                string s;
                switch (tdf.Type)
                {
                    case 0:
                        Blaze.TdfInteger ti = (Blaze.TdfInteger)tdf;
                        rtb2.Text = "0x" + ti.Value.ToString("X");
                        if (ti.Label == "IP  ")
                        {
                            rtb2.Text += Environment.NewLine + "(" + Blaze.GetStringFromIP(ti.Value) + ")";
                        }
                        break;
                    case 1:
                        rtb2.Text = ((Blaze.TdfString)tdf).Value.ToString();
                        break;
                    case 2:
                        rtb2.Text = "Length: " + ((Blaze.TdfBlob)tdf).Data.Length.ToString();
                        rtb2.Text += Environment.NewLine + Blaze.HexDump(((Blaze.TdfBlob)tdf).Data);
                        break;
                    case 4:
                        Blaze.TdfList l = (Blaze.TdfList)tdf;
                        s = "";
                        for (int i = 0; i < l.Count; i++)
                        {
                            switch (l.SubType)
                            {
                                case 0:
                                    s += "{" + ((List<long>)l.List)[i] + "} ";
                                    break;
                                case 1:
                                    s += "{" + ((List<string>)l.List)[i] + "} ";
                                    break;
                                case 9:
                                    Blaze.TrippleVal t2 = ((List<Blaze.TrippleVal>)l.List)[i];
                                    s += "{" + t2.v1.ToString("X") + "; " + t2.v2.ToString("X") + "; " + t2.v3.ToString("X") + "} ";
                                    break;
                            }
                        }
                        rtb2.Text = s;
                        break;
                    case 5:
                        s = "";
                        Blaze.TdfDoubleList ll = (Blaze.TdfDoubleList)tdf;
                        for (int i = 0; i < ll.Count; i++)
                        {
                            s += "{";
                            switch (ll.SubType1)
                            {
                                case 0:
                                    List<long> l1 = (List<long>)ll.List1;
                                    s += l1[i].ToString("X");
                                    break;
                                case 1:
                                    List<string> l2 = (List<string>)ll.List1;
                                    s += l2[i];
                                    break;
                                case 0xA:
                                    List<float> lf1 = (List<float>)ll.List1;
                                    s += lf1[i].ToString();
                                    break;
                                default:
                                    s += "(see List1[" + i + "])";
                                    break;
                            }
                            s += " ; ";
                            switch (ll.SubType2)
                            {
                                case 0:
                                    List<long> l1 = (List<long>)ll.List2;
                                    s += l1[i].ToString("X");
                                    break;
                                case 1:
                                    List<string> l2 = (List<string>)ll.List2;
                                    s += l2[i];
                                    break;
                                case 0xA:
                                    List<float> lf1 = (List<float>)ll.List2;
                                    s += lf1[i].ToString();
                                    break;
                                default:
                                    s += "(see List2[" + i + "])";
                                    break;
                            }
                            s += "}\n";
                        }
                        rtb2.Text = s;
                        break;
                    case 6:
                        rtb2.Text = "Type: 0x" + ((Blaze.TdfUnion)tdf).UnionType.ToString("X2");
                        break;
                    case 7:
                        Blaze.TdfIntegerList til = (Blaze.TdfIntegerList)tdf;
                        s = "";
                        for (int i = 0; i < til.Count; i++)
                        {
                            s += til.List[i].ToString("X");
                            if (i < til.Count - 1)
                                s += "; ";
                        }
                        rtb2.Text = s;
                        break;
                    case 8:
                        Blaze.TdfDoubleVal dval = (Blaze.TdfDoubleVal)tdf;
                        rtb2.Text = "0x" + dval.Value.v1.ToString("X") + " 0x" + dval.Value.v2.ToString("X");
                        break;
                    case 9:
                        Blaze.TdfTrippleVal tval = (Blaze.TdfTrippleVal)tdf;
                        rtb2.Text = "0x" + tval.Value.v1.ToString("X") + " 0x" + tval.Value.v2.ToString("X") + " 0x" + tval.Value.v3.ToString("X");
                        break;
                    default:
                        rtb2.Text = "";
                        break;
                }
            }
        }

        public byte[] FileToByteArray(string fileName)
        {
            byte[] fileData = null;

            using (FileStream fs = File.OpenRead(fileName))
            {
                using (BinaryReader binaryReader = new BinaryReader(fs))
                {
                    fileData = binaryReader.ReadBytes((int)fs.Length);
                }
            }
            return fileData;
        }

        public void AddPacket(byte[] data)
        {
            MemoryStream m = new MemoryStream(data);
            m.Seek(0, 0);
            List<Blaze.Packet> result = Blaze.FetchAllBlazePackets(m);
            lock (_sync)
            {
                packets.AddRange(result);
            }
        }

        private TreeNode TdfToTree(Blaze.Tdf tdf)
        {
            TreeNode t, t2, t3;
            switch (tdf.Type)
            {
                case 3:
                    t = tdf.ToTree();
                    Blaze.TdfStruct str = (Blaze.TdfStruct)tdf;
                    if (str.startswith2)
                        t.Text += " (Starts with 2)";
                    foreach (Blaze.Tdf td in str.Values)
                        t.Nodes.Add(TdfToTree(td));
                    t.Name = (inlistcount++).ToString();
                    inlist.Add(tdf);
                    return t;
                case 4:
                    t = tdf.ToTree();
                    Blaze.TdfList l = (Blaze.TdfList)tdf;
                    if (l.SubType == 3)
                    {
                        List<Blaze.TdfStruct> l2 = (List<Blaze.TdfStruct>)l.List;
                        for (int i = 0; i < l2.Count; i++)
                        {
                            t2 = new TreeNode("Entry #" + i);
                            if (l2[i].startswith2)
                                t2.Text += " (Starts with 2)";
                            List<Blaze.Tdf> l3 = l2[i].Values;
                            for (int j = 0; j < l3.Count; j++)
                                t2.Nodes.Add(TdfToTree(l3[j]));
                            t.Nodes.Add(t2);
                        }
                    }
                    t.Name = (inlistcount++).ToString();
                    inlist.Add(tdf);
                    return t;
                case 5:
                    t = tdf.ToTree();
                    Blaze.TdfDoubleList ll = (Blaze.TdfDoubleList)tdf;
                    t2 = new TreeNode("List 1");
                    if (ll.SubType1 == 3)
                    {
                        List<Blaze.TdfStruct> l2 = (List<Blaze.TdfStruct>)ll.List1;
                        for (int i = 0; i < l2.Count; i++)
                        {
                            t3 = new TreeNode("Entry #" + i);
                            if (l2[i].startswith2)
                                t2.Text += " (Starts with 2)";
                            List<Blaze.Tdf> l3 = l2[i].Values;
                            for (int j = 0; j < l3.Count; j++)
                                t3.Nodes.Add(TdfToTree(l3[j]));
                            t2.Nodes.Add(t3);
                        }
                        t.Nodes.Add(t2);
                    }
                    t2 = new TreeNode("List 2");
                    if (ll.SubType2 == 3)
                    {
                        List<Blaze.TdfStruct> l2 = (List<Blaze.TdfStruct>)ll.List2;
                        for (int i = 0; i < l2.Count; i++)
                        {
                            t3 = new TreeNode("Entry #" + i);
                            if (l2[i].startswith2)
                                t2.Text += " (Starts with 2)";
                            List<Blaze.Tdf> l3 = l2[i].Values;
                            for (int j = 0; j < l3.Count; j++)
                                t3.Nodes.Add(TdfToTree(l3[j]));
                            t2.Nodes.Add(t3);
                        }
                        t.Nodes.Add(t2);
                    }
                    t.Name = (inlistcount++).ToString();
                    inlist.Add(tdf);
                    return t;
                case 6:
                    t = tdf.ToTree();
                    Blaze.TdfUnion tu = (Blaze.TdfUnion)tdf;
                    if (tu.UnionType != 0x7F)
                    {
                        t.Nodes.Add(TdfToTree(tu.UnionContent));
                    }
                    t.Name = (inlistcount++).ToString();
                    inlist.Add(tdf);
                    return t;
                default:
                    t = tdf.ToTree();
                    t.Name = (inlistcount++).ToString();
                    inlist.Add(tdf);
                    return t;
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog d = new FolderBrowserDialog();
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = Directory.GetFiles(d.SelectedPath, "*.bin");
                foreach (string file in files)
                    File.Copy(file, "logs\\packets\\" + Path.GetFileName(file), true);
                RefreshPackets();
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            try
            {
                string text = toolStripTextBox2.Text;
                byte[] buff = Helper.LabelToBytes(text);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in buff)
                    sb.Append(b.ToString("X2"));
                toolStripTextBox3.Text = sb.ToString();
            }
            catch (Exception)
            {
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            try
            {
                string text = toolStripTextBox3.Text.Replace(" ", "").ToUpper();
                MemoryStream m = new MemoryStream();
                for (int i = 0; i < text.Length / 2; i++)
                    m.WriteByte(Convert.ToByte(text.Substring(i * 2, 2), 16));
                toolStripTextBox2.Text = Helper.BytesToLabel(m.ToArray());
            }
            catch (Exception)
            {
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            try
            {
                string text = toolStripTextBox4.Text.Replace(" ", "").ToUpper();
                long l = Convert.ToInt64(text, 16);
                MemoryStream m = new MemoryStream();
                Helper.WriteCompressedInteger(m, l);
                toolStripTextBox5.Text = Helper.ByteArrayToHexString(m.ToArray());
            }
            catch (Exception)
            {
            }
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            try
            {
                string text = toolStripTextBox5.Text.Replace(" ", "").ToUpper();
                MemoryStream m = new MemoryStream(Helper.HexStringToByteArray(text));
                m.Seek(0, 0);
                toolStripTextBox4.Text = Helper.ReadCompressedInteger(m).ToString("X");
            }
            catch (Exception)
            {
            }
        }

        private void playerProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 f = new Form2();
            f.ShowDialog();
        }

        private void packetsAutoRefreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (packetsAutoRefreshToolStripMenuItem.Checked == true)
            {
                packetsAutoRefreshToolStripMenuItem.Checked = false;
                timer1.Enabled = false;
                Config.RefreshPacket = "false";
            }
            else
            {
                packetsAutoRefreshToolStripMenuItem.Checked = true;
                timer1.Enabled = true;
                Config.RefreshPacket = "true";
            }
        }

        private void startRedirectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string backend = toolStripTextBox6.Text;
            string backPort = toolStripTextBox7.Text;
            string targethost = toolStripTextBox8.Text;
            string targetport = toolStripTextBox9.Text;

            RedirectorServer.backend = backend;
            RedirectorServer.backPort = Convert.ToInt32(backPort);
            RedirectorServer.targethost = targethost;
            RedirectorServer.targetPort = Convert.ToInt32(targetport);

            RedirectorServer.Start();
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            int n = toolStripComboBox1.SelectedIndex;
            if (n == -1)
            {
                return;
            }
            toolStripComboBox1.Enabled = false;
            Helper.userProfil = n;

            string args = rtb6.Text.Replace("\r", "").Replace("\n", " ");
            while (args.Contains("  "))
                args = args.Replace("  ", " ");
            Helper.RunShell(Config.Exe, args);
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            string args = rtb5.Text.Replace("\r", "").Replace("\n", " ");
            while (args.Contains("  "))
                args = args.Replace("  ", " ");
            Helper.RunShell(Config.Exe, args);
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            Form2 f = new Form2();
            f.ShowDialog();
            RefreshProfiles();
        }
    }
}
