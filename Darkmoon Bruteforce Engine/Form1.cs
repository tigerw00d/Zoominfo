namespace Darkmoon_Bruteforce_Engine
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;
    using xNet;

    public class Form1 : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xa1;
        public const int HT_CAPTION = 2;
        private List<string> comboList = new List<string>();
        private List<string> proxyList = new List<string>();
        private List<Thread> ThreadList = new List<Thread>();
        private List<string> userAgentList = new List<string>();
        private int goodCount;
        private int badCount;
        private int remaningCount;
        private int errorCount;
        private static int Timeout = 0x13880;
        private bool stop = true;
        private IContainer components;
        private Panel panel1;
        private Panel panel2;
        private Panel panel3;
        private GroupBox groupBox2;
        private GroupBox groupBox1;
        private Button btnLoadCombo;
        private GroupBox groupBox3;
        private Button btnLoadProxy;
        private CheckBox useProxyChk;
        private Label label1;
        private ComboBox proxyTypeCombo;
        private CheckBox loadProxyLinkCHK;
        private CheckBox autoupdateProxyCHK;
        private CheckBox proxyFromFileChk;
        private NumericUpDown autoupdateProxyNumeric;
        private GroupBox groupBox4;
        private Label label3;
        private Label label2;
        private Label label5;
        private Label label4;
        private Label badCountTXT;
        private Label goodCountTXT;
        private Label remaningCountTXT;
        private Label comboCountTXT;
        private TextBox proxyLinkTXT;
        private GroupBox groupBox5;
        private GroupBox groupBox7;
        private Label label10;
        private NumericUpDown threadCountNumeric;
        private RichTextBox logTXT;
        private Label label13;
        private Label errorCountTXT;
        private Label label11;
        private Label proxyCountTXT;
        private Label closeSoftwareTXT;
        private Label traySoftwareTXT;
        private Label label15;
        private System.Windows.Forms.Timer timer1;
        private GroupBox groupBox8;
        private Button btnStop;
        private Button btnStart;
        private ProgressBar Progress;
        private Label label6;
        private Label captchaCountTXT;
        private TextBox sleepTime;
        private Label label7;
        private Label label9;

        public Form1()
        {
            this.InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            this.userAgentList = Enumerable.ToList<string>(File.ReadAllLines("userAgents.txt"));
        }

        private void autoupdateProxyCHK_CheckedChanged(object sender, EventArgs e)
        {
            if (this.autoupdateProxyCHK.Checked)
            {
                this.autoupdateProxyNumeric.Visible = true;
            }
            else
            {
                this.autoupdateProxyNumeric.Visible = false;
            }
        }

        private void btnLoadCombo_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Filter = "Файл с базой (*.txt)|*.txt"
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.comboList.Clear();
                    foreach (string str in Enumerable.Distinct<string>(File.ReadAllLines(dialog.FileName)))
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            this.comboList.Add(str);
                        }
                    }
                    this.comboCountTXT.Text = this.comboList.Count.ToString();
                    this.remaningCount = this.comboList.Count;
                    this.remaningCountTXT.Text = this.remaningCount.ToString();
                }
            }
            catch
            {
            }
        }

        private void btnLoadProxy_Click(object sender, EventArgs e)
        {
            if (this.proxyFromFileChk.Checked)
            {
                this.proxyList.Clear();
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Filter = "Файл с прокси (*.txt)|*.txt"
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.proxyList = Enumerable.ToList<string>(Enumerable.Distinct<string>(File.ReadAllLines(dialog.FileName)));
                    this.proxyCountTXT.Text = this.proxyList.Count.ToString();
                }
            }
            if (this.loadProxyLinkCHK.Checked)
            {
                this.proxyList.Clear();
                using (HttpRequest request = new HttpRequest())
                {
                    request.Cookies = new CookieDictionary(false);
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";
                    string contents = request.Get(this.proxyLinkTXT.Text, null).ToString();
                    File.Delete("get_proxy.txt");
                    File.AppendAllText("get_proxy.txt", contents);
                    this.proxyList = Enumerable.ToList<string>(File.ReadAllLines("get_proxy.txt"));
                    this.proxyCountTXT.Text = this.proxyList.Count.ToString();
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!Enumerable.Any<string>(this.comboList))
            {
                MessageBox.Show("Вы забыли загрузить базу!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                if (this.comboList.Count < this.threadCountNumeric.Value)
                {
                    this.threadCountNumeric.Value = this.comboList.Count;
                }
                int num1 = this.errorCount = 0;
                this.goodCount = this.badCount = num1;
                this.comboCountTXT.Text = this.comboList.Count.ToString();
                this.proxyCountTXT.Text = this.proxyList.Count.ToString();
                this.remaningCountTXT.Text = this.remaningCount.ToString();
                this.goodCountTXT.Text = this.goodCount.ToString();
                this.badCountTXT.Text = this.badCount.ToString();
                this.errorCountTXT.Text = this.errorCount.ToString();
                this.Progress.Maximum = this.comboList.Count;
                this.Progress.Value = 0;
                this.timer1.Interval = (((int)this.autoupdateProxyNumeric.Value) * 60) * 0x3e8;
                this.timer1.Enabled = this.autoupdateProxyCHK.Checked;
                this.stop = false;
                this.ThreadList.Clear();
                for (int i = 0; i < this.threadCountNumeric.Value; i++)
                {
                    Thread item = new Thread(new ThreadStart(this.StartCheck));
                    this.ThreadList.Add(item);
                    item.Start();
                }
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            this.stop = true;
            using (List<Thread>.Enumerator enumerator = this.ThreadList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Abort();
                }
            }
            this.timer1.Enabled = false;
            if (this.comboList.Count == 0)
            {
                MessageBox.Show("Работа завершена");
            }
            else
            {
                File.AppendAllLines("ostatok_" + this.comboList.Count.ToString() + ".txt", this.comboList);
                MessageBox.Show("Работа завершена");
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (this.loadProxyLinkCHK.Checked && this.proxyFromFileChk.Checked)
            {
                this.proxyFromFileChk.Checked = false;
            }
            if (this.loadProxyLinkCHK.Checked)
            {
                this.proxyLinkTXT.Visible = true;
            }
            else
            {
                this.proxyLinkTXT.Visible = false;
            }
        }

        private string Checker(string userOne, string proxy)
        {
            string str4;
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            try
            {
                using (HttpRequest request = new HttpRequest())
                {
                    request.Cookies = new CookieDictionary(false);
                    request.UserAgent = userAgentList[rnd.Next(userAgentList.Count + 1)];
                    switch (GetProxy_Tools.GT)
                    {
                        case GetProxy_Tools.GetType.HTTP:
                            request.Proxy = HttpProxyClient.Parse(proxy);
                            break;

                        case GetProxy_Tools.GetType.SOCKS4:
                            request.Proxy = Socks4ProxyClient.Parse(proxy);
                            break;

                        case GetProxy_Tools.GetType.SOCKS5:
                            request.Proxy = Socks5ProxyClient.Parse(proxy);
                            break;

                        default:
                            break;
                    }
                    if (GetProxy_Tools.GT != GetProxy_Tools.GetType.NONE)
                    {
                        request.ConnectTimeout = Timeout;
                    }
                    request.AllowAutoRedirect = true;
                    request.MaximumAutomaticRedirections = 15;
                    request.AddHeader("Upgrade-Insecure-Requests", "1");
                    request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    request.AddHeader("Accept-Language", "q=0.9,en-US");
                    string str = request.Get("https://www.google.com/search?q=" + userOne + "+zoominfo", null).ToString();
                    if (!str.Contains("href=\"https://www.zoominfo.com/") && !str.Contains("captcha"))
                    {
                        str4 = "false";
                    }
                    else if (str.Contains("captcha"))
                    {
                        str4 = "error";
                    }
                    else
                    {
                        string str2 = str.Substrings("href=\"https://www.zoominfo.com/", "\"", StringComparison.Ordinal)[0];
                        request.AddHeader("Upgrade-Insecure-Requests", "1");
                        request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                        request.AddHeader("Accept-Language", "q=0.9,en-US");
                        request.Referer = "https://www.google.com/search?q=" + userOne + "+zoominfo";
                        string str3 = request.Get("https://www.zoominfo.com/" + str2, null).ToString();
                        if (!str3.Contains("class=\"icon-text-content content\">"))
                        {
                            str4 = "false";
                        }
                        else
                        {
                            string str5 = string.Empty;
                            string str6 = string.Empty;
                            string str7 = string.Empty;
                            string str8 = string.Empty;
                            string str9 = string.Empty;
                            str9 = !str3.Contains("class=\"content link\" href=\"") ? "Empty" : str3.Substrings("class=\"content link\" href=\"", "\"", StringComparison.Ordinal)[0];
                            if (str3.Contains("class=\"icon-label\">Phone"))
                            {
                                str5 = str3.Substrings("class=\"icon-text-content content\">", "<", StringComparison.Ordinal)[0];
                                str8 = str3.Substrings("class=\"icon-text-content content\">", "<", StringComparison.Ordinal)[1];
                                str6 = str3.Substrings("class=\"icon-text-content content\">", "<", StringComparison.Ordinal)[2];
                                str7 = str3.Substrings("class=\"icon-text-content content\">", "<", StringComparison.Ordinal)[3];
                            }
                            else
                            {
                                str5 = str3.Substrings("class=\"icon-text-content content\">", "<", StringComparison.Ordinal)[0];
                                str6 = str3.Substrings("class=\"icon-text-content content\">", "<", StringComparison.Ordinal)[1];
                                str7 = str3.Substrings("class=\"icon-text-content content\">", "<", StringComparison.Ordinal)[2];
                                str8 = "Empty";
                            }
                            string[] textArray1 = new string[0x11];
                            textArray1[0] = "URL: ";
                            textArray1[1] = userOne;
                            textArray1[2] = Environment.NewLine;
                            textArray1[3] = "Location: ";
                            textArray1[4] = str5;
                            textArray1[5] = Environment.NewLine;
                            textArray1[6] = "Phone: ";
                            textArray1[7] = str8;
                            textArray1[8] = Environment.NewLine;
                            textArray1[9] = "Website: ";
                            textArray1[10] = str9;
                            textArray1[11] = Environment.NewLine;
                            textArray1[12] = "Employees: ";
                            textArray1[13] = str6;
                            textArray1[14] = Environment.NewLine;
                            textArray1[15] = "Revenue: ";
                            textArray1[0x10] = str7;
                            string str10 = string.Concat(textArray1);
                            str4 = "true=" + str10;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                str4 = "error";
            }
            return str4;
        }

        private void closeSoftwareTXT_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label15 = new System.Windows.Forms.Label();
            this.traySoftwareTXT = new System.Windows.Forms.Label();
            this.closeSoftwareTXT = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.captchaCountTXT = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.proxyCountTXT = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.errorCountTXT = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.badCountTXT = new System.Windows.Forms.Label();
            this.goodCountTXT = new System.Windows.Forms.Label();
            this.remaningCountTXT = new System.Windows.Forms.Label();
            this.comboCountTXT = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.proxyLinkTXT = new System.Windows.Forms.TextBox();
            this.autoupdateProxyNumeric = new System.Windows.Forms.NumericUpDown();
            this.proxyFromFileChk = new System.Windows.Forms.CheckBox();
            this.autoupdateProxyCHK = new System.Windows.Forms.CheckBox();
            this.loadProxyLinkCHK = new System.Windows.Forms.CheckBox();
            this.proxyTypeCombo = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.useProxyChk = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnLoadProxy = new System.Windows.Forms.Button();
            this.btnLoadCombo = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.sleepTime = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.threadCountNumeric = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.Progress = new System.Windows.Forms.ProgressBar();
            this.logTXT = new System.Windows.Forms.RichTextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.autoupdateProxyNumeric)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.threadCountNumeric)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Silver;
            this.panel1.Controls.Add(this.label15);
            this.panel1.Controls.Add(this.traySoftwareTXT);
            this.panel1.Controls.Add(this.closeSoftwareTXT);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(908, 31);
            this.panel1.TabIndex = 0;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseMove);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.label15.Location = new System.Drawing.Point(6, 15);
            this.label15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(168, 14);
            this.label15.TabIndex = 1;
            this.label15.Text = "zoominfo.com [URL Checker]";
            // 
            // traySoftwareTXT
            // 
            this.traySoftwareTXT.AutoSize = true;
            this.traySoftwareTXT.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.traySoftwareTXT.Location = new System.Drawing.Point(843, 0);
            this.traySoftwareTXT.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.traySoftwareTXT.Name = "traySoftwareTXT";
            this.traySoftwareTXT.Size = new System.Drawing.Size(32, 24);
            this.traySoftwareTXT.TabIndex = 2;
            this.traySoftwareTXT.Text = "__";
            this.traySoftwareTXT.Click += new System.EventHandler(this.traySoftwareTXT_Click);
            // 
            // closeSoftwareTXT
            // 
            this.closeSoftwareTXT.AutoSize = true;
            this.closeSoftwareTXT.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.closeSoftwareTXT.Location = new System.Drawing.Point(879, 8);
            this.closeSoftwareTXT.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.closeSoftwareTXT.Name = "closeSoftwareTXT";
            this.closeSoftwareTXT.Size = new System.Drawing.Size(21, 20);
            this.closeSoftwareTXT.TabIndex = 1;
            this.closeSoftwareTXT.Text = "X";
            this.closeSoftwareTXT.Click += new System.EventHandler(this.closeSoftwareTXT_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.DimGray;
            this.panel2.Controls.Add(this.groupBox4);
            this.panel2.Controls.Add(this.groupBox2);
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 31);
            this.panel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(235, 430);
            this.panel2.TabIndex = 1;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.captchaCountTXT);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Controls.Add(this.proxyCountTXT);
            this.groupBox4.Controls.Add(this.label13);
            this.groupBox4.Controls.Add(this.errorCountTXT);
            this.groupBox4.Controls.Add(this.label11);
            this.groupBox4.Controls.Add(this.badCountTXT);
            this.groupBox4.Controls.Add(this.goodCountTXT);
            this.groupBox4.Controls.Add(this.remaningCountTXT);
            this.groupBox4.Controls.Add(this.comboCountTXT);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBox4.Location = new System.Drawing.Point(8, 248);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox4.Size = new System.Drawing.Size(214, 180);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Статистика";
            // 
            // captchaCountTXT
            // 
            this.captchaCountTXT.AutoSize = true;
            this.captchaCountTXT.Location = new System.Drawing.Point(171, 125);
            this.captchaCountTXT.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.captchaCountTXT.Name = "captchaCountTXT";
            this.captchaCountTXT.Size = new System.Drawing.Size(13, 14);
            this.captchaCountTXT.TabIndex = 14;
            this.captchaCountTXT.Text = "0";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(4, 125);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(44, 14);
            this.label9.TabIndex = 13;
            this.label9.Text = "Каптча";
            // 
            // proxyCountTXT
            // 
            this.proxyCountTXT.AutoSize = true;
            this.proxyCountTXT.Location = new System.Drawing.Point(171, 108);
            this.proxyCountTXT.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.proxyCountTXT.Name = "proxyCountTXT";
            this.proxyCountTXT.Size = new System.Drawing.Size(13, 14);
            this.proxyCountTXT.TabIndex = 12;
            this.proxyCountTXT.Text = "0";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(4, 108);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(48, 14);
            this.label13.TabIndex = 11;
            this.label13.Text = "Прокси";
            // 
            // errorCountTXT
            // 
            this.errorCountTXT.AutoSize = true;
            this.errorCountTXT.Location = new System.Drawing.Point(171, 89);
            this.errorCountTXT.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.errorCountTXT.Name = "errorCountTXT";
            this.errorCountTXT.Size = new System.Drawing.Size(13, 14);
            this.errorCountTXT.TabIndex = 10;
            this.errorCountTXT.Text = "0";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(4, 89);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(51, 14);
            this.label11.TabIndex = 9;
            this.label11.Text = "Ошибок";
            // 
            // badCountTXT
            // 
            this.badCountTXT.AutoSize = true;
            this.badCountTXT.Location = new System.Drawing.Point(171, 73);
            this.badCountTXT.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.badCountTXT.Name = "badCountTXT";
            this.badCountTXT.Size = new System.Drawing.Size(13, 14);
            this.badCountTXT.TabIndex = 8;
            this.badCountTXT.Text = "0";
            // 
            // goodCountTXT
            // 
            this.goodCountTXT.AutoSize = true;
            this.goodCountTXT.Location = new System.Drawing.Point(171, 54);
            this.goodCountTXT.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.goodCountTXT.Name = "goodCountTXT";
            this.goodCountTXT.Size = new System.Drawing.Size(13, 14);
            this.goodCountTXT.TabIndex = 7;
            this.goodCountTXT.Text = "0";
            // 
            // remaningCountTXT
            // 
            this.remaningCountTXT.AutoSize = true;
            this.remaningCountTXT.Location = new System.Drawing.Point(171, 36);
            this.remaningCountTXT.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.remaningCountTXT.Name = "remaningCountTXT";
            this.remaningCountTXT.Size = new System.Drawing.Size(13, 14);
            this.remaningCountTXT.TabIndex = 6;
            this.remaningCountTXT.Text = "0";
            // 
            // comboCountTXT
            // 
            this.comboCountTXT.AutoSize = true;
            this.comboCountTXT.Location = new System.Drawing.Point(171, 18);
            this.comboCountTXT.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.comboCountTXT.Name = "comboCountTXT";
            this.comboCountTXT.Size = new System.Drawing.Size(13, 14);
            this.comboCountTXT.TabIndex = 5;
            this.comboCountTXT.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(4, 73);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(42, 14);
            this.label5.TabIndex = 4;
            this.label5.Text = "Бэдов";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 54);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 14);
            this.label4.TabIndex = 3;
            this.label4.Text = "Гудов";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 36);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 14);
            this.label3.TabIndex = 2;
            this.label3.Text = "Осталось";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 18);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 14);
            this.label2.TabIndex = 1;
            this.label2.Text = "Всего";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.proxyLinkTXT);
            this.groupBox2.Controls.Add(this.autoupdateProxyNumeric);
            this.groupBox2.Controls.Add(this.proxyFromFileChk);
            this.groupBox2.Controls.Add(this.autoupdateProxyCHK);
            this.groupBox2.Controls.Add(this.loadProxyLinkCHK);
            this.groupBox2.Controls.Add(this.proxyTypeCombo);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.useProxyChk);
            this.groupBox2.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.groupBox2.Location = new System.Drawing.Point(8, 97);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox2.Size = new System.Drawing.Size(214, 147);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Работа с прокси";
            // 
            // proxyLinkTXT
            // 
            this.proxyLinkTXT.Location = new System.Drawing.Point(26, 59);
            this.proxyLinkTXT.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.proxyLinkTXT.Name = "proxyLinkTXT";
            this.proxyLinkTXT.Size = new System.Drawing.Size(176, 20);
            this.proxyLinkTXT.TabIndex = 0;
            this.proxyLinkTXT.Visible = false;
            // 
            // autoupdateProxyNumeric
            // 
            this.autoupdateProxyNumeric.Location = new System.Drawing.Point(167, 99);
            this.autoupdateProxyNumeric.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.autoupdateProxyNumeric.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.autoupdateProxyNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.autoupdateProxyNumeric.Name = "autoupdateProxyNumeric";
            this.autoupdateProxyNumeric.Size = new System.Drawing.Size(45, 20);
            this.autoupdateProxyNumeric.TabIndex = 0;
            this.autoupdateProxyNumeric.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.autoupdateProxyNumeric.Visible = false;
            // 
            // proxyFromFileChk
            // 
            this.proxyFromFileChk.AutoSize = true;
            this.proxyFromFileChk.Enabled = false;
            this.proxyFromFileChk.Location = new System.Drawing.Point(7, 82);
            this.proxyFromFileChk.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.proxyFromFileChk.Name = "proxyFromFileChk";
            this.proxyFromFileChk.Size = new System.Drawing.Size(164, 18);
            this.proxyFromFileChk.TabIndex = 6;
            this.proxyFromFileChk.Text = "Загрузка прокси с файла";
            this.proxyFromFileChk.UseVisualStyleBackColor = true;
            this.proxyFromFileChk.CheckedChanged += new System.EventHandler(this.proxyFromFileChk_CheckedChanged);
            // 
            // autoupdateProxyCHK
            // 
            this.autoupdateProxyCHK.AutoSize = true;
            this.autoupdateProxyCHK.Enabled = false;
            this.autoupdateProxyCHK.Location = new System.Drawing.Point(7, 101);
            this.autoupdateProxyCHK.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.autoupdateProxyCHK.Name = "autoupdateProxyCHK";
            this.autoupdateProxyCHK.Size = new System.Drawing.Size(166, 18);
            this.autoupdateProxyCHK.TabIndex = 5;
            this.autoupdateProxyCHK.Text = "Автообновление прокси";
            this.autoupdateProxyCHK.UseVisualStyleBackColor = true;
            this.autoupdateProxyCHK.CheckedChanged += new System.EventHandler(this.autoupdateProxyCHK_CheckedChanged);
            // 
            // loadProxyLinkCHK
            // 
            this.loadProxyLinkCHK.AutoSize = true;
            this.loadProxyLinkCHK.Enabled = false;
            this.loadProxyLinkCHK.Location = new System.Drawing.Point(7, 44);
            this.loadProxyLinkCHK.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.loadProxyLinkCHK.Name = "loadProxyLinkCHK";
            this.loadProxyLinkCHK.Size = new System.Drawing.Size(178, 18);
            this.loadProxyLinkCHK.TabIndex = 4;
            this.loadProxyLinkCHK.Text = "Загрузка прокси по ссылке";
            this.loadProxyLinkCHK.UseVisualStyleBackColor = true;
            this.loadProxyLinkCHK.CheckedChanged += new System.EventHandler(this.checkBox3_CheckedChanged);
            // 
            // proxyTypeCombo
            // 
            this.proxyTypeCombo.Enabled = false;
            this.proxyTypeCombo.FormattingEnabled = true;
            this.proxyTypeCombo.Items.AddRange(new object[] {
            "HTTP(S)",
            "SOCKS4/SOCKS5"});
            this.proxyTypeCombo.Location = new System.Drawing.Point(83, 16);
            this.proxyTypeCombo.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.proxyTypeCombo.Name = "proxyTypeCombo";
            this.proxyTypeCombo.Size = new System.Drawing.Size(117, 22);
            this.proxyTypeCombo.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 14);
            this.label1.TabIndex = 3;
            this.label1.Text = "Тип прокси : ";
            // 
            // useProxyChk
            // 
            this.useProxyChk.AutoSize = true;
            this.useProxyChk.Location = new System.Drawing.Point(7, 119);
            this.useProxyChk.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.useProxyChk.Name = "useProxyChk";
            this.useProxyChk.Size = new System.Drawing.Size(145, 18);
            this.useProxyChk.TabIndex = 0;
            this.useProxyChk.Text = "Использовать прокси";
            this.useProxyChk.UseVisualStyleBackColor = true;
            this.useProxyChk.CheckedChanged += new System.EventHandler(this.useProxyChk_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnLoadProxy);
            this.groupBox1.Controls.Add(this.btnLoadCombo);
            this.groupBox1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBox1.Location = new System.Drawing.Point(8, 11);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Size = new System.Drawing.Size(214, 75);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Загрузить";
            // 
            // btnLoadProxy
            // 
            this.btnLoadProxy.Enabled = false;
            this.btnLoadProxy.Location = new System.Drawing.Point(4, 41);
            this.btnLoadProxy.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnLoadProxy.Name = "btnLoadProxy";
            this.btnLoadProxy.Size = new System.Drawing.Size(207, 21);
            this.btnLoadProxy.TabIndex = 4;
            this.btnLoadProxy.Text = "Прокси";
            this.btnLoadProxy.UseVisualStyleBackColor = true;
            this.btnLoadProxy.Click += new System.EventHandler(this.btnLoadProxy_Click);
            // 
            // btnLoadCombo
            // 
            this.btnLoadCombo.Location = new System.Drawing.Point(4, 16);
            this.btnLoadCombo.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnLoadCombo.Name = "btnLoadCombo";
            this.btnLoadCombo.Size = new System.Drawing.Size(207, 21);
            this.btnLoadCombo.TabIndex = 3;
            this.btnLoadCombo.Text = "Базу";
            this.btnLoadCombo.UseVisualStyleBackColor = true;
            this.btnLoadCombo.Click += new System.EventHandler(this.btnLoadCombo_Click);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.DimGray;
            this.panel3.Controls.Add(this.groupBox8);
            this.panel3.Controls.Add(this.groupBox7);
            this.panel3.Controls.Add(this.groupBox5);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(706, 31);
            this.panel3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(202, 430);
            this.panel3.TabIndex = 2;
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.btnStop);
            this.groupBox8.Controls.Add(this.btnStart);
            this.groupBox8.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.groupBox8.Location = new System.Drawing.Point(13, 356);
            this.groupBox8.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox8.Size = new System.Drawing.Size(178, 73);
            this.groupBox8.TabIndex = 3;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Управление";
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(4, 44);
            this.btnStop.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(170, 21);
            this.btnStop.TabIndex = 6;
            this.btnStop.Text = "Стоп";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(4, 17);
            this.btnStart.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(170, 21);
            this.btnStart.TabIndex = 5;
            this.btnStart.Text = "Старт";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.label6);
            this.groupBox7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.groupBox7.Location = new System.Drawing.Point(13, 93);
            this.groupBox7.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox7.Size = new System.Drawing.Size(178, 40);
            this.groupBox7.TabIndex = 2;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Информация";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(4, 19);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(165, 14);
            this.label6.TabIndex = 0;
            this.label6.Text = "- Обновления прокси в мин.";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.sleepTime);
            this.groupBox5.Controls.Add(this.label7);
            this.groupBox5.Controls.Add(this.threadCountNumeric);
            this.groupBox5.Controls.Add(this.label10);
            this.groupBox5.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.groupBox5.Location = new System.Drawing.Point(13, 11);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox5.Size = new System.Drawing.Size(178, 75);
            this.groupBox5.TabIndex = 0;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Настройки";
            // 
            // sleepTime
            // 
            this.sleepTime.Location = new System.Drawing.Point(97, 44);
            this.sleepTime.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.sleepTime.Name = "sleepTime";
            this.sleepTime.Size = new System.Drawing.Size(61, 20);
            this.sleepTime.TabIndex = 9;
            this.sleepTime.Text = "15";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(4, 46);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(66, 14);
            this.label7.TabIndex = 8;
            this.label7.Text = "SleepTime";
            // 
            // threadCountNumeric
            // 
            this.threadCountNumeric.Location = new System.Drawing.Point(97, 20);
            this.threadCountNumeric.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.threadCountNumeric.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.threadCountNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.threadCountNumeric.Name = "threadCountNumeric";
            this.threadCountNumeric.Size = new System.Drawing.Size(60, 20);
            this.threadCountNumeric.TabIndex = 7;
            this.threadCountNumeric.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(4, 21);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(54, 14);
            this.label10.TabIndex = 7;
            this.label10.Text = "Потоков";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.Progress);
            this.groupBox3.Controls.Add(this.logTXT);
            this.groupBox3.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.groupBox3.Location = new System.Drawing.Point(238, 34);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox3.Size = new System.Drawing.Size(463, 427);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Лог работы";
            // 
            // Progress
            // 
            this.Progress.Location = new System.Drawing.Point(0, 410);
            this.Progress.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Progress.Name = "Progress";
            this.Progress.Size = new System.Drawing.Size(463, 15);
            this.Progress.TabIndex = 1;
            // 
            // logTXT
            // 
            this.logTXT.BackColor = System.Drawing.Color.DimGray;
            this.logTXT.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logTXT.Location = new System.Drawing.Point(4, 16);
            this.logTXT.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.logTXT.Name = "logTXT";
            this.logTXT.ReadOnly = true;
            this.logTXT.Size = new System.Drawing.Size(454, 390);
            this.logTXT.TabIndex = 0;
            this.logTXT.Text = "";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Gray;
            this.ClientSize = new System.Drawing.Size(908, 461);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.autoupdateProxyNumeric)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.threadCountNumeric)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void Obrabotka(string rez, string account)
        {
            string result = string.Empty;
            bool flag = false;
            bool flag2 = false;
            if (string.IsNullOrEmpty(rez))
            {
                this.errorCount++;
                this.comboList.Add(account);
            }
            if (rez.Contains("true"))
            {
                char[] separator = new char[] { '=' };
                result = rez.Split(separator)[1];
                flag = true;
                this.remaningCount--;
                this.goodCount++;
            }
            if (rez.Contains("false"))
            {
                flag2 = true;
                this.remaningCount--;
                this.badCount++;
            }
            if (rez.Contains("error"))
            {
                this.comboList.Add(account);
                this.errorCount++;
            }
            if (flag)
            {
                File.AppendAllText("Good.txt", result + Environment.NewLine + "===================================" + Environment.NewLine);
                RichTextBox logTXT = this.logTXT;
                string[] textArray1 = new string[] { logTXT.Text, result, Environment.NewLine, "===================================", Environment.NewLine };
                logTXT.Text = string.Concat(textArray1);
            }
            if (flag2)
            {
                File.AppendAllText("Bad.txt", account + "\r\n");
            }
            if (flag | flag2)
            {
                this.errorCountTXT.Text = this.errorCount.ToString();
                this.goodCountTXT.Text = this.goodCount.ToString();
                this.remaningCountTXT.Text = this.remaningCount.ToString();
                this.badCountTXT.Text = this.badCount.ToString();
                this.Progress.Value++;
                if ((this.comboList.Count == 0) && (this.remaningCount == 0))
                {
                    this.btnStop_Click(null, null);
                }
            }
            else
            {
                this.errorCountTXT.Text = this.errorCount.ToString();
            }
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(base.Handle, 0xa1, 2, 0);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void proxyFromFileChk_CheckedChanged(object sender, EventArgs e)
        {
            if (this.proxyFromFileChk.Checked && this.loadProxyLinkCHK.Checked)
            {
                this.loadProxyLinkCHK.Checked = false;
            }
        }

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        private void StartCheck()
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            while (!this.stop && (this.comboList.Count > 0))
            {
                string str = string.Empty;
                Form1 form = this;
                lock (form)
                {
                    if (this.comboList.Count > 0)
                    {
                        str = this.comboList[0];
                        this.comboList.RemoveAt(0);
                    }
                }
                if (string.IsNullOrEmpty(str))
                {
                    Thread.Sleep(500);
                }
                else
                {
                    string proxy = string.Empty;
                    string rez = string.Empty;
                    try
                    {
                        if (!this.useProxyChk.Checked)
                        {
                            GetProxy_Tools.GT = GetProxy_Tools.GetType.NONE;
                        }
                        if (this.proxyTypeCombo.SelectedIndex == 0)
                        {
                            GetProxy_Tools.GT = GetProxy_Tools.GetType.HTTP;
                        }
                        if (this.proxyTypeCombo.SelectedIndex == 1)
                        {
                            GetProxy_Tools.GT = GetProxy_Tools.GetType.SOCKS5;
                        }
                        if (GetProxy_Tools.GT != GetProxy_Tools.GetType.NONE)
                        {
                            int proxyNumber = rnd.Next(this.proxyList.Count);
                            proxy = this.proxyList[proxyNumber];
                        }
                        rez = this.Checker(str, proxy);
                        try
                        {
                            int sleepTime = Convert.ToInt32(this.sleepTime.Text) * 1000;
                            int rndTime = rnd.Next(5000);
                            Thread.Sleep(sleepTime + rndTime);
                        }
                        catch { }
                    }
                    catch
                    {
                    }
                    form = this;
                    lock (form)
                    {
                        this.Obrabotka(rez, str);
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.loadProxyLinkCHK.Checked)
            {
                while (true)
                {
                    try
                    {
                        this.proxyList.Clear();
                        using (HttpRequest request = new HttpRequest())
                        {
                            request.Cookies = new CookieDictionary(false);
                            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";
                            string contents = request.Get(this.proxyLinkTXT.Text, null).ToString();
                            File.Delete("get_proxy.txt");
                            File.AppendAllText("get_proxy.txt", contents);
                            this.proxyList = Enumerable.ToList<string>(File.ReadAllLines("get_proxy.txt"));
                            this.proxyCountTXT.Text = this.proxyList.Count.ToString();
                        }
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(0xea60);
                        continue;
                    }
                    break;
                }
            }
        }

        private void traySoftwareTXT_Click(object sender, EventArgs e)
        {
            base.WindowState = FormWindowState.Minimized;
        }

        private void useProxyChk_CheckedChanged(object sender, EventArgs e)
        {
            if (this.useProxyChk.Checked)
            {
                this.autoupdateProxyCHK.Enabled = true;
                this.proxyFromFileChk.Enabled = true;
                this.loadProxyLinkCHK.Enabled = true;
                this.proxyTypeCombo.Enabled = true;
                this.btnLoadProxy.Enabled = true;
            }
            else
            {
                this.autoupdateProxyCHK.Enabled = false;
                this.proxyFromFileChk.Enabled = false;
                this.loadProxyLinkCHK.Enabled = false;
                this.proxyTypeCombo.Enabled = false;
                this.btnLoadProxy.Enabled = false;
            }
        }
    }
}

