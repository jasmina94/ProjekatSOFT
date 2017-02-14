﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using tessnet2;
using System.Media;
using System.Net;
using NAudio.Wave;
using System.IO;
using Tesseract;

namespace OCRTest
{
    public partial class Form1 : Form
    {
        private String procitanTekst = "";
        private SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        private Image image = null;


        String language = "";
        String languageSpeak = "";
        int redniBroj = 0;
        String putanja;
        BlockAlignReductionStream volumeStream;
        WaveOutEvent player = new WaveOutEvent();

        int imageCounter = 0;
        string imageDir = @"../../../images";

        public Form1()
        {

            InitializeComponent();

            console.Enabled = true;
            console.ReadOnly = true;
            console.BackColor = Color.White;

            comboBox1.Items.Add("English");
            comboBox1.Items.Add("German");
            comboBox1.Items.Add("Italian");
            comboBox1.Items.Add("Serbian Latin");


            synthesizer.Volume = 100;  // 0...100
            synthesizer.Rate = -2;     // -10...10

            synthesizer.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(synth_SpeakCompleted);
            button1.Enabled = false;
            getImageToolStripMenuItem.Enabled = false;
            button2.Enabled = false;
            getTextToolStripMenuItem.Enabled = false;
            button3.Enabled = false;
            speakToolStripMenuItem.Enabled = false;
            button4.Enabled = false;
            cancelSpeakingToolStripMenuItem.Enabled = false;

            DirectoryInfo di2 = new DirectoryInfo(Environment.CurrentDirectory + @"/mp3/");
            if (di2.GetFiles() != null)
            {
                foreach (FileInfo file in di2.GetFiles())
                {
                    file.Delete();
                }
            }

            // Ocisti sve prethodne slike
            DirectoryInfo di = new DirectoryInfo(imageDir);
            if (di.GetFiles() != null)
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }


        }

        private void synth_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            button1.Enabled = true;
            getImageToolStripMenuItem.Enabled = true;
            button2.Enabled = false;
            getTextToolStripMenuItem.Enabled = false;
            button3.Enabled = true;
            speakToolStripMenuItem.Enabled = true;
            button4.Enabled = false;
            cancelSpeakingToolStripMenuItem.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            image = SnippingTool.Snip();
            if (image != null)
            {
                imageCounter++;
                procitanTekst = "";
                button2.Enabled = true;
                getTextToolStripMenuItem.Enabled = true;
                button3.Enabled = false;
                speakToolStripMenuItem.Enabled = false;
                button4.Enabled = false;
                cancelSpeakingToolStripMenuItem.Enabled = false;
                console.Text = "Succes image load!" + Environment.NewLine + Environment.NewLine;
                console.DeselectAll();
            }
            else
            {
                console.Text = "Failed to load image!" + Environment.NewLine + Environment.NewLine;
                button2.Enabled = false;
                getTextToolStripMenuItem.Enabled = false;
                button3.Enabled = false;
                speakToolStripMenuItem.Enabled = false;
                button4.Enabled = false;
                cancelSpeakingToolStripMenuItem.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string putanja1 = @"../../../tessdata";
            string imagePath = imageDir + "/" + imageCounter.ToString() + ".png";
            Bitmap bitmap = new Bitmap(image);
            string procitanTekst = "";

            bitmap.Save(imagePath);
            using (var engine = new TesseractEngine(putanja1, language, EngineMode.Default))
            using (var image = Pix.LoadFromFile(imagePath))
            using (var page = engine.Process(image))
            {
                string text = page.GetText();
                console.Text = text;
                procitanTekst = text;
            }

            procitanTekst.Trim();
            if (!procitanTekst.Contains("~"))
            {
                button2.Enabled = false;
                getTextToolStripMenuItem.Enabled = false;
                button3.Enabled = true;
                speakToolStripMenuItem.Enabled = true;
            }
            console.DeselectAll();

            //Za izgovor
            //WebClient tts;
            //putanja = Environment.CurrentDirectory + @"/mp3/play" + redniBroj + ".mp3";
            //redniBroj++;
            //Uri uri = new Uri("https://translate.google.rs/translate_tts?client=tw-ob&tl=" + languageSpeak + "&q=" + procitanTekst);
            //using (tts = new WebClient())
            //{
            //    tts.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 9.0; Windows;)");
            //    tts.DownloadFile(uri, putanja);
            //}
        }

        private void button3_Click(object sender, EventArgs e)
        {

            WaveStream mainOutputStream = new Mp3FileReader(putanja);
            volumeStream = new BlockAlignReductionStream(mainOutputStream);



            player.Init(volumeStream);

            player.Play();
            player.PlaybackStopped += new EventHandler<StoppedEventArgs>(playStoped);
            //synthesizer.SpeakAsync(text);
            button1.Enabled = false;
            getImageToolStripMenuItem.Enabled = false;
            //button3.Enabled = false;
            //speakToolStripMenuItem.Enabled = false;
            button4.Enabled = true;
            cancelSpeakingToolStripMenuItem.Enabled = true;
            console.DeselectAll();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            player.Stop();
            //synthesizer.SpeakAsyncCancelAll();
            button1.Enabled = true;
            getImageToolStripMenuItem.Enabled = true;
            button3.Enabled = true;
            speakToolStripMenuItem.Enabled = true;
            button4.Enabled = false;
            cancelSpeakingToolStripMenuItem.Enabled = true;
            console.DeselectAll();
        }

        private void playStoped(object sender, StoppedEventArgs e)
        {
            player.Stop();
            player.Dispose();
            volumeStream.Close();
            Console.WriteLine("a");
        }

        private void getImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        private void getTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button2_Click(sender, e);
        }

        private void speakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button3_Click(sender, e);
        }

        private void cancelSpeakingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button4_Click(sender, e);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text.Contains("English"))
            {
                language = "eng";
                languageSpeak = "en";
                button1.Enabled = true;
                getImageToolStripMenuItem.Enabled = true;
                return;
            }
            else if (comboBox1.Text.Contains("German"))
            {
                language = "deu";
                languageSpeak = "de";
                button1.Enabled = true;
                getImageToolStripMenuItem.Enabled = true;
                return;
            }
            else if (comboBox1.Text.Contains("Italian"))
            {
                language = "ita";
                languageSpeak = "it";
                button1.Enabled = true;
                getImageToolStripMenuItem.Enabled = true;
                return;
            }
            else if (comboBox1.Text.Contains("Serbian Latin"))
            {
                language = "srp_latn";
                languageSpeak = "sr";
                button1.Enabled = true;
                getImageToolStripMenuItem.Enabled = true;
                return;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string putanja = @"../../../tessdata";
            string imageDir = @"../../../image.png";

            using (var engine = new TesseractEngine(putanja, "eng", EngineMode.Default))
            using (var image = Pix.LoadFromFile(imageDir))
            using (var page = engine.Process(image))
            {
                string text = page.GetText();
                console.Text = text;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // prvo ocisti konzolu i ispisi tekst
            
            String message = "Validation of static text on 2 different fonts and 3 sizes. Arial and TimesNewRoman based on Levenstain distance algortithm.";

            String val2 = "This is a lot of Arial 20 point text to test the ocr code and see if it works on all types of file format. The quick brown dog jumped over the lazy fox. The quick brown dog jumped over the lazy fox. The quick brown dog jumped over the lazy fox.";

            //console.Text += message + Environment.NewLine + val2 + Environment.NewLine;


            //console.Text += "Validation for different sizes same font(Arial).";

            string a10 = @"../../../staticimages/Arial10.PNG";
            string a14 = @"../../../staticimages/Arial14.PNG";
            string a20 = @"../../../staticimages/Arial20.PNG";

            string result10 = "";
            string result14 = "";
            string result20 = "";
            double resultAverage = 0.0;

            using (var engine = new TesseractEngine(@"../../../tessdata", "eng", EngineMode.Default))
            using (var image = Pix.LoadFromFile(a20))
            using (var page = engine.Process(image))
            {
                result20 = page.GetText();
            }

            String[] words = getWords(result20);
            String[] realWords = getWords(val2);

            console.Text += "Duzina skeniranog: " + words.Length + Environment.NewLine;
            console.Text += "Duzina originalnog: " + realWords.Length + Environment.NewLine;

            

            for(int i = 0; i < words.Length; i++)
            {
                for (int j = i; j < realWords.Length; j++)
                {
                    if (words[i].Contains(realWords[j]) || words[i].Equals(realWords[j]))
                    {
                        LevenstainStaticDataValidator validator = new LevenstainStaticDataValidator();
                        int pom = validator.ComputeLevensteinDistance(words[i], realWords[j]);
                        console.Text +="Skenirana rec: " + words[i] + " Original rec: " + realWords[j] + Environment.NewLine + "i: " + i + "; j: " + j + "; POM: " + pom.ToString() + Environment.NewLine;

                        resultAverage += pom;

                        break;   
                    }
                }
            }
            resultAverage = resultAverage / realWords.Length;

            console.Text += resultAverage.ToString() + Environment.NewLine;

        }

        public String[] getWords(string result)
        {
            String[] ret = new String[1000];

            ret = result.Split(' ');

            return ret;
        }
    }
}
