using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace reyz_Twitch.TV
{
    public partial class Form1 : Form
    {

        CookieContainer cookie = new CookieContainer();

        string path_livestreamer;
        string path_vlc;
        string twtv_username;
        string path_application;
        string path_config;

        string[] s_path_livestreamer;
        string[] s_path_vlc;
        string[] s_twtv_username;

        string channel;
        string quality;

        int update_timer;

        bool show_settings = false;


        public Form1()
        {
            InitializeComponent();
        }



        private void button1_Click(object sender, EventArgs e)
        {


            if(textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "")
            {
                MessageBox.Show("Please set a VLC/Livestreamer-Path and enter a Twitch.TV Username");
                return;
            }
            backgroundWorker1.RunWorkerAsync();
        }




        private void parse_follows()
        {

            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();

            string channel;
            string status;
            string game;


            MainRequest mainCS = new MainRequest();
            string user_follows = mainCS.sendRequest("https://api.twitch.tv/kraken/users/" + s_twtv_username[1] +  "/follows/channels?limit=100&offset=0");

            try
            {
                //{"_total":72,"
                string total_following = Regex.Match(user_follows, "{\"_total\":(.+?),\"").Groups[1].ToString();

                int total = Convert.ToInt32(total_following);

                //status":"slahser dota","display_name":"slahserDota",
                MatchCollection matches = Regex.Matches(user_follows, ",\"status\":(.+?),\"display_name\":(.+?),\"game\":(.+?),");



                int current_progress = 0;
                progressBar1.Maximum = total;

                // Loop over matches.
                foreach (Match m in matches)
                {
                    status = m.Groups[1].ToString().Replace("\"", ""); //replace quotes because of above regex, it's different for some channels
                    channel = m.Groups[2].ToString().Replace("\"", "");
                    game = m.Groups[3].ToString().Replace("\"", "");

                    string isonline = mainCS.requestChannelStatus(channel);

                    if (isonline != "")
                    {
                        Console.WriteLine(channel + " is offline");
                    }
                    else
                    {
                        listBox1.Items.Add(channel);
                        listBox2.Items.Add(status);
                        listBox3.Items.Add(game);
                    }

                    current_progress = current_progress + 1;
                    backgroundWorker1.ReportProgress(current_progress);


                }

            }
            catch (Exception e)
            {
                StreamWriter sw = new StreamWriter(path_application + "/log.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + " - " + e.Message);
                sw.Flush();
                sw.Close();

            }


        }




        private void parse_quality(string current_channel)
        {

            comboBox2.Items.Clear();
            comboBox2.Text = "";

            MainRequest mainCS = new MainRequest();
            string requested_quality = mainCS.requestQuality(current_channel);

            MatchCollection matches = Regex.Matches(requested_quality, ",NAME=\"(.+?)\",");

            // Loop over matches.
            foreach (Match m in matches)
            {
                comboBox2.Items.Add(m.Groups[1].ToString());
            }
        }




        private void Form1_Load(object sender, EventArgs e)
        {
            Form1.CheckForIllegalCrossThreadCalls = false;

            //get path to vlc and livestreamer
            path_application = Path.GetDirectoryName(Application.ExecutablePath);
            path_config = path_application + "/config.txt";

            string[] lines;
            lines = File.ReadAllLines(path_application + "/config.txt");
            path_livestreamer = lines[0];
            path_vlc = lines[1];
            twtv_username = lines[2];

            //split livestreamer-path
            char[] splitchar = { '=' };
            s_path_livestreamer = path_livestreamer.Split(splitchar); //-> s_path_livestreamer[1]

            //split vlc-path
            char[] splitchar_vlc = { '=' };
            s_path_vlc = path_vlc.Split(splitchar_vlc); //-> s_path_vlc[1]

            //split twtv-username
            char[] splitchar_twtv_username = { '=' };
            s_twtv_username = twtv_username.Split(splitchar_twtv_username);

            textBox1.Text = s_path_livestreamer[1];
            textBox2.Text = s_path_vlc[1];
            textBox3.Text = s_twtv_username[1];

            comboBox1.SelectedIndex = 1;

        }




        private void button2_Click(object sender, EventArgs e)
        {
            if(channel == null || quality == null)
            {
                MessageBox.Show("Please select a channel and choose a quality-setting to view with livestreamer first");
                return;
            }

            MainRequest start = new MainRequest();

            //check if channel is still online
            string isonline = start.requestChannelStatus(channel);

            if (isonline != "")
            {
                MessageBox.Show(channel + " is offline. Please refresh your channel list");
            }
            else
            {
                //open livestreamer
                start.startProcess(s_path_livestreamer[1], channel, quality);
            }

        }




        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            button1.Enabled = false;
            parse_follows();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 0;
            button1.Enabled = true;
        }


        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }




        private void button4_Click(object sender, EventArgs e)
        {
            string path_vlc_new;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "VLC.exe (vlc.exe)|vlc.exe";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path_vlc_new = openFileDialog1.FileName;

                string text = File.ReadAllText(path_config);
                text = text.Replace("path_vlc=" + s_path_vlc[1], "path_vlc=" + path_vlc_new);
                File.WriteAllText(path_config, text);

                s_path_vlc[1] = path_vlc_new;
                textBox2.Text = path_vlc_new;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string path_livestreamer_new;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "livestreamer.exe (livestreamer.exe)|livestreamer.exe";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path_livestreamer_new = openFileDialog1.FileName;

                string text = File.ReadAllText(path_config);
                text = text.Replace("path_livestreamer=" + s_path_livestreamer[1], "path_livestreamer=" +path_livestreamer_new);
                File.WriteAllText(path_config, text);

                s_path_livestreamer[1] = path_livestreamer_new;
                textBox1.Text = path_livestreamer_new;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if(listBox1.SelectedIndex < 0)
            {
                return;
            }
                channel = listBox1.SelectedItem.ToString();
                int lb_position = listBox1.SelectedIndex;

                toolTip1.SetToolTip(listBox1, listBox2.Items[lb_position].ToString());

                if (channel != null)
                {
                    comboBox2.Visible = true;
                    label5.Visible = true;
                    parse_quality(listBox1.SelectedItem.ToString());
                }
                else if (channel == null)
                {
                    comboBox2.Visible = false;
                    label5.Visible = false;
                }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            string text = File.ReadAllText(path_config);
            text = text.Replace("twtv_username=" + s_twtv_username[1], "twtv_username=" + textBox3.Text);
            File.WriteAllText(path_config, text);
        }

        private void button5_Click(object sender, EventArgs e)
        {

            string text = File.ReadAllText(path_config);
            text = text.Replace("twtv_username=" + s_twtv_username[1], "twtv_username=" + textBox3.Text);
            File.WriteAllText(path_config, text);

            s_twtv_username[1] = textBox3.Text;
        }

        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                comboBox1.Enabled = false;
                string selected_update_value = comboBox1.SelectedItem.ToString();
               

                switch (selected_update_value)
                {
                    case "30 seconds":
                        update_timer = 30 * 1000;
                        break;
                    case "1 minute":
                        update_timer = 60 * 1000;
                        break;
                    case "2 minutes":
                        update_timer = 120 * 1000;
                        break;
                    case "5 minutes":
                        update_timer = 300 * 1000;
                        break;
                }

                if (textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "")
                {
                    StreamWriter sw = new StreamWriter(path_application + "/log.txt", true);
                    sw.WriteLine(DateTime.Now.ToString() + " - Please set a VLC/Livestreamer-Path and enter a Twitch.TV Username");
                    sw.Flush();
                    sw.Close();
                    return;
                }

                timer1.Interval = update_timer;
                timer1.Start();

            }
            else if(!checkBox1.Checked)
            {
                timer1.Stop();
                comboBox1.Enabled = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            bool isbusy = backgroundWorker1.IsBusy;

            if(!isbusy)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (!show_settings)
            {
                textBox1.Visible = true;
                textBox2.Visible = true;
                button3.Visible = true;
                button4.Visible = true;

                checkBox1.Visible = true;
                comboBox1.Visible = true;

                label3.Visible = true;
                textBox3.Visible = true;
                button5.Visible = true;

                show_settings = true;
            }
            else if(show_settings)
            {
                textBox1.Visible = false;
                textBox2.Visible = false;
                button3.Visible = false;
                button4.Visible = false;

                checkBox1.Visible = false;
                comboBox1.Visible = false;

                label3.Visible = false;
                textBox3.Visible = false;
                button5.Visible = false;

                show_settings = false;
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            quality = comboBox2.SelectedItem.ToString();
        }








        //new function
    }
}
