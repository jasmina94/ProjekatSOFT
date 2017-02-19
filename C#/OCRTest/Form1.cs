using System;
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
using SpeechLib;
using System.Diagnostics;

namespace OCRTest
{
    public partial class Form1 : Form
    {
        private String procitanTekst = "";
        private SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        private Image image = null;

        private String triger = "";

        String language = "";
        String languageSpeak = "";
        int redniBroj = 0;
        String putanja;
        BlockAlignReductionStream volumeStream;
        WaveOutEvent player = new WaveOutEvent();

        int imageCounter = 0;
        string imageDir = @"../../../images";

        private Boolean srpski = false;

        public Form1()
        {

            InitializeComponent();

            foreach (InstalledVoice voice in synthesizer.GetInstalledVoices())
            {
                VoiceInfo info = voice.VoiceInfo;
                consoleTab1.Text += info.Name + Environment.NewLine;
            }

            consoleTab1.Enabled = true;
            consoleTab1.ReadOnly = true;
            consoleTab1.BackColor = Color.White;

            consoleTab2.Enabled = true;
            consoleTab2.ReadOnly = true;
            consoleTab2.BackColor = Color.White;

            comboBox1.Items.Add("English");
            comboBox1.Items.Add("German");
            comboBox1.Items.Add("Italian");
            comboBox1.Items.Add("Serbian Latin");

            comboBox2.Items.Add("Arial 10point text");
            comboBox2.Items.Add("Arial 14point text");
            comboBox2.Items.Add("Arial 20point text");
            comboBox2.Items.Add("Times New Roman 10point text");
            comboBox2.Items.Add("Times New Roman 14point text");
            comboBox2.Items.Add("Times New Roman 20point text");

            staticOcr.Enabled = false;


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
            txtMeanConf1.Clear();

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
                consoleTab1.Text = "Succes image load!" + Environment.NewLine + Environment.NewLine;
                Bitmap bitmap = new Bitmap(image);
                Image processedImage = bitmap.DrawAsGrayscale();
                Bitmap processedBitmap = new Bitmap(processedImage);
                panel2.BackgroundImage = processedBitmap; // postavi sredjeni
                panel1.BackgroundImage = bitmap;          // postavi odabrani
                consoleTab1.DeselectAll();
            }
            else
            {
                consoleTab1.Text = "Failed to load image!" + Environment.NewLine + Environment.NewLine;
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
            txtMeanConf1.Clear();


            string putanja1 = @"../../../tessdata";
            string imagePath = imageDir + "/" + imageCounter.ToString() + ".png";
            Bitmap bitmap = (Bitmap) panel2.BackgroundImage;

            procitanTekst = "";

            bitmap.Save(imagePath);  // cuvam procesuiranu sliku

            using (var engine = new TesseractEngine(putanja1, language, EngineMode.Default))
            using (var image = Pix.LoadFromFile(imagePath))
            using (var page = engine.Process(image))
            {
                string text = page.GetText();
                consoleTab1.Text = text;
                txtMeanConf1.Text = String.Format("{0:P}", page.GetMeanConfidence());
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
            consoleTab1.DeselectAll();

            //Za izgovor srpski
            if (srpski && procitanTekst.Length <= 200)
            {
                WebClient tts;
                putanja = Environment.CurrentDirectory + @"/mp3/play" + redniBroj + ".mp3";
                redniBroj++;
                Uri uri = new Uri("https://translate.google.rs/translate_tts?client=tw-ob&tl=sr&q=" + procitanTekst);
                using (tts = new WebClient())
                {
                    tts.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 9.0; Windows;)");
                    tts.DownloadFile(uri, putanja);
                }
            }
            else if (srpski && procitanTekst.Length > 200)
            {
                button3.Enabled = false;
                consoleTab1.Text += Environment.NewLine + Environment.NewLine + "Cant speak Serbian text. " + Environment.NewLine + "Text is too long (>200 characters)!" + Environment.NewLine;
            }


        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!srpski)
            {
                synthesizer.SpeakAsync(procitanTekst);
            }
            else
            {
                WaveStream mainOutputStream = new Mp3FileReader(putanja);
                volumeStream = new BlockAlignReductionStream(mainOutputStream);

                player.Init(volumeStream);

                player.Play();
                player.PlaybackStopped += new EventHandler<StoppedEventArgs>(playStoped);
            }

            button1.Enabled = false;
            getImageToolStripMenuItem.Enabled = false;
            button3.Enabled = false;
            speakToolStripMenuItem.Enabled = false;
            button4.Enabled = true;
            cancelSpeakingToolStripMenuItem.Enabled = true;
            consoleTab1.DeselectAll();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!srpski)
            {
                synthesizer.SpeakAsyncCancelAll();
            }
            else
            {
                player.Stop();
            }

            button1.Enabled = true;
            getImageToolStripMenuItem.Enabled = true;
            button3.Enabled = true;
            speakToolStripMenuItem.Enabled = true;
            button4.Enabled = false;
            cancelSpeakingToolStripMenuItem.Enabled = true;
            consoleTab1.DeselectAll();
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
                //languageSpeak = "en";
                button1.Enabled = true;

                foreach (InstalledVoice voice in synthesizer.GetInstalledVoices())
                {
                    VoiceInfo info = voice.VoiceInfo;
                    if (info.Name.Contains("Anna")){
                        synthesizer.SelectVoice("Microsoft Anna");
                    }else if (info.Name.Contains("Zira"))
                    {
                        synthesizer.SelectVoice("Microsoft Zira Desktop");
                    }
                    
                }
                
                getImageToolStripMenuItem.Enabled = true;
                return;
            }
            else if (comboBox1.Text.Contains("German"))
            {
                language = "deu";
                //languageSpeak = "de";
                button1.Enabled = true;
                synthesizer.SelectVoice("Microsoft Server Speech Text to Speech Voice (de-DE, Hedda)");
                getImageToolStripMenuItem.Enabled = true;
                return;
            }
            else if (comboBox1.Text.Contains("Italian"))
            {
                language = "ita";
                //languageSpeak = "it";
                button1.Enabled = true;
                synthesizer.SelectVoice("Microsoft Server Speech Text to Speech Voice (it-IT, Lucia)");
                getImageToolStripMenuItem.Enabled = true;
                return;
            }
            else if (comboBox1.Text.Contains("Serbian Latin"))
            {
                srpski = true;
                language = "srp_latn";
                //languageSpeak = "sr";
                button1.Enabled = true;
                getImageToolStripMenuItem.Enabled = true;
                return;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string putanja = @"../../../tessdata";
            string imageDir = @"../../../staticimages/Arial10.PNG";

            using (var engine = new TesseractEngine(putanja, "eng", EngineMode.Default))
            using (var image = Pix.LoadFromFile(imageDir))
            using (var page = engine.Process(image))
            {
                string text = page.GetText();
                consoleTab2.Text = text + Environment.NewLine;
                consoleTab2.Text += "Mean confidence: " + String.Format("{0:P}", page.GetMeanConfidence());
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            // prvo ocisti konzolu i ispisi tekst
            txtMeanConf2.Clear();
            consoleTab2.Clear();

            String originalArial10 = "This is a lot of Arial 10 point text to test the ocr code and see if it works on all types of file format. The quick brown dog jumped over the lazy fox. The quick brown dog jumped over the lazy fox. The quick brown dog jumped over the lazy fox. ";
            String originalArial14 = "This is a lot of Arial 14 point text to test the ocr code and see if it works on all types of file format. The quick brown dog jumped over the lazy fox. The quick brown dog jumped over the lazy fox. The quick brown dog jumped over the lazy fox. ";
            String originalArial20 = "This is a lot of Arial 20 point text to test the ocr code and see if it works on all types of file format. The quick brown dog jumped over the lazy fox. The quick brown dog jumped over the lazy fox. The quick brown dog jumped over the lazy fox. ";
            String originalTNR10 = "This is a lot of Times New Roman 10 point text to test the ocr code and see if it works on all types of file format. The quick brown dog jumped over the lazy fox. The quick brown dog jumped over the lazy fox. The quick brown dog jumped over the lazy fox. ";
            String originalTNR14 = "This is a lot of Times New Roman 14 point text to test the ocr code and see if it works on all types of file format. The quick brown dog jumped over the lazy fox. The quick brown dog jumped over the lazy fox. The quick brown dog jumped over the lazy fox. ";
            String originalTNR20 = "This is a lot of Times New Roman 20 point text to test the ocr code and see if it works on all types of file format. The quick brown dog jumped over the lazy fox. The quick brown dog jumped over the lazy fox. The quick brown dog jumped over the lazy fox. ";

            //console.Text += message + Environment.NewLine + val2 + Environment.NewLine;


            //console.Text += "Validation for different sizes same font(Arial).";

            string a10 = @"../../../staticimages/Arial10.PNG";
            string a14 = @"../../../staticimages/Arial14.PNG";
            string a20 = @"../../../staticimages/Arial20.PNG";
            string tnr10 = @"../../../staticimages/TimesNewRoman10.PNG";
            string tnr14 = @"../../../staticimages/TimesNewRoman14.PNG";
            string tnr20 = @"../../../staticimages/TimesNewRoman20.PNG";

            string result10 = "";
            string result14 = "";
            string result20 = "";
            double resultAverage = 0.0;

            if (triger.Equals("arial10"))
            {

                using (var engine = new TesseractEngine(@"../../../tessdata", "eng", EngineMode.Default))
                using (var image = Pix.LoadFromFile(a10))
                using (var page = engine.Process(image))
                {
                    result10 = page.GetText();
                    txtMeanConf2.Text = String.Format("{0:P}", page.GetMeanConfidence());
                }

                String[] words = getWords(result10);
                String[] realWords = getWords(originalArial10);

                consoleTab2.Text = "Scanned text length: " + words.Length + Environment.NewLine;
                consoleTab2.Text += "Original text length: " + realWords.Length + Environment.NewLine;

                //Posto je duzina skenirampg teksta uvek manja ili jednaka duzini originalnog teksta
                //prolazak kroz taj kraci tekst (skeniran) i poredjenje sa recima iz originalnog (duzeg) teksta.

                //Zadovoljava sve nase slucajeve. Da li treba i za slucaj realWords.Length <= words.Length ?
                if (words.Length <= realWords.Length)
                {

                    for (int i = 0; i < words.Length; i++)
                    {
                        for (int j = i; j < realWords.Length; j++)
                        {
                            if (words[i].Contains(realWords[j]) || words[i].Equals(realWords[j]))
                            {
                                LevenstainStaticDataValidator validator = new LevenstainStaticDataValidator();
                                int pom = validator.ComputeLevensteinDistance(words[i], realWords[j]);
                                consoleTab2.Text += "Scanned word: " + words[i] + " Original word: " + realWords[j] + Environment.NewLine + "i: " + i + "; j: " + j + "; POM: " + pom.ToString() + Environment.NewLine;

                                resultAverage += pom;

                                break;
                            }
                        }
                    }
                    resultAverage = resultAverage / realWords.Length;

                    consoleTab2.Text += resultAverage.ToString() + Environment.NewLine;
                }
            }
            else if (triger.Equals("arial14"))
            {

                using (var engine = new TesseractEngine(@"../../../tessdata", "eng", EngineMode.Default))
                using (var image = Pix.LoadFromFile(a14))
                using (var page = engine.Process(image))
                {
                    result14 = page.GetText();
                    txtMeanConf2.Text = String.Format("{0:P}", page.GetMeanConfidence());
                }

                String[] words = getWords(result14);
                String[] realWords = getWords(originalArial14);

                consoleTab2.Text = "Scanned text length: " + words.Length + Environment.NewLine;
                consoleTab2.Text += "Original text length: " + realWords.Length + Environment.NewLine;

                //Posto je duzina skenirampg teksta uvek manja ili jednaka duzini originalnog teksta
                //prolazak kroz taj kraci tekst (skeniran) i poredjenje sa recima iz originalnog (duzeg) teksta.

                //Zadovoljava sve nase slucajeve. Da li treba i za slucaj realWords.Length <= words.Length ?
                if (words.Length <= realWords.Length)
                {

                    for (int i = 0; i < words.Length; i++)
                    {
                        for (int j = i; j < realWords.Length; j++)
                        {
                            if (words[i].Contains(realWords[j]) || words[i].Equals(realWords[j]))
                            {
                                LevenstainStaticDataValidator validator = new LevenstainStaticDataValidator();
                                int pom = validator.ComputeLevensteinDistance(words[i], realWords[j]);
                                consoleTab2.Text += "Scanned word: " + words[i] + " Original word: " + realWords[j] + Environment.NewLine + "i: " + i + "; j: " + j + "; POM: " + pom.ToString() + Environment.NewLine;

                                resultAverage += pom;

                                break;
                            }
                        }
                    }
                    resultAverage = resultAverage / realWords.Length;

                    consoleTab2.Text += resultAverage.ToString() + Environment.NewLine;
                }
            }
            else if (triger.Equals("arial20"))
            {

                using (var engine = new TesseractEngine(@"../../../tessdata", "eng", EngineMode.Default))
                using (var image = Pix.LoadFromFile(a20))
                using (var page = engine.Process(image))
                {
                    result20 = page.GetText();
                    txtMeanConf2.Text = String.Format("{0:P}", page.GetMeanConfidence());
                }

                String[] words = getWords(result20);
                String[] realWords = getWords(originalArial20);

                consoleTab2.Text = "Scanned text length: " + words.Length + Environment.NewLine;
                consoleTab2.Text += "Original text length: " + realWords.Length + Environment.NewLine;

                //Posto je duzina skenirampg teksta uvek manja ili jednaka duzini originalnog teksta
                //prolazak kroz taj kraci tekst (skeniran) i poredjenje sa recima iz originalnog (duzeg) teksta.

                //Zadovoljava sve nase slucajeve. Da li treba i za slucaj realWords.Length <= words.Length ?
                if (words.Length <= realWords.Length)
                {

                    for (int i = 0; i < words.Length; i++)
                    {
                        for (int j = i; j < realWords.Length; j++)
                        {
                            if (words[i].Contains(realWords[j]) || words[i].Equals(realWords[j]))
                            {
                                LevenstainStaticDataValidator validator = new LevenstainStaticDataValidator();
                                int pom = validator.ComputeLevensteinDistance(words[i], realWords[j]);
                                consoleTab2.Text += "Scanned word: " + words[i] + " Original word: " + realWords[j] + Environment.NewLine + "i: " + i + "; j: " + j + "; POM: " + pom.ToString() + Environment.NewLine;

                                resultAverage += pom;

                                break;
                            }
                        }
                    }
                    resultAverage = resultAverage / realWords.Length;

                    consoleTab2.Text += resultAverage.ToString() + Environment.NewLine;
                }
            }
            else if (triger.Equals("tnr10"))
            {

                using (var engine = new TesseractEngine(@"../../../tessdata", "eng", EngineMode.Default))
                using (var image = Pix.LoadFromFile(tnr10))
                using (var page = engine.Process(image))
                {
                    result10 = page.GetText();
                    txtMeanConf2.Text = String.Format("{0:P}", page.GetMeanConfidence());
                }

                String[] words = getWords(result10);
                String[] realWords = getWords(originalTNR10);

                consoleTab2.Text = "Scanned text length: " + words.Length + Environment.NewLine;
                consoleTab2.Text += "Original text length: " + realWords.Length + Environment.NewLine;

                //Posto je duzina skenirampg teksta uvek manja ili jednaka duzini originalnog teksta
                //prolazak kroz taj kraci tekst (skeniran) i poredjenje sa recima iz originalnog (duzeg) teksta.

                //Zadovoljava sve nase slucajeve. Da li treba i za slucaj realWords.Length <= words.Length ?
                if (words.Length <= realWords.Length)
                {

                    for (int i = 0; i < words.Length; i++)
                    {
                        for (int j = i; j < realWords.Length; j++)
                        {
                            if (words[i].Contains(realWords[j]) || words[i].Equals(realWords[j]))
                            {
                                LevenstainStaticDataValidator validator = new LevenstainStaticDataValidator();
                                int pom = validator.ComputeLevensteinDistance(words[i], realWords[j]);
                                consoleTab2.Text += "Scanned word: " + words[i] + " Original word: " + realWords[j] + Environment.NewLine + "i: " + i + "; j: " + j + "; POM: " + pom.ToString() + Environment.NewLine;

                                resultAverage += pom;

                                break;
                            }
                        }
                    }
                    resultAverage = resultAverage / realWords.Length;

                    consoleTab2.Text += resultAverage.ToString() + Environment.NewLine;
                }
            }
            else if (triger.Equals("tnr14"))
            {

                using (var engine = new TesseractEngine(@"../../../tessdata", "eng", EngineMode.Default))
                using (var image = Pix.LoadFromFile(tnr14))
                using (var page = engine.Process(image))
                {
                    result14 = page.GetText();
                    txtMeanConf2.Text = String.Format("{0:P}", page.GetMeanConfidence());
                }

                String[] words = getWords(result14);
                String[] realWords = getWords(originalTNR14);

                consoleTab2.Text = "Scanned text length: " + words.Length + Environment.NewLine;
                consoleTab2.Text += "Original text length: " + realWords.Length + Environment.NewLine;

                //Posto je duzina skenirampg teksta uvek manja ili jednaka duzini originalnog teksta
                //prolazak kroz taj kraci tekst (skeniran) i poredjenje sa recima iz originalnog (duzeg) teksta.

                //Zadovoljava sve nase slucajeve. Da li treba i za slucaj realWords.Length <= words.Length ?
                if (words.Length <= realWords.Length)
                {

                    for (int i = 0; i < words.Length; i++)
                    {
                        for (int j = i; j < realWords.Length; j++)
                        {
                            if (words[i].Contains(realWords[j]) || words[i].Equals(realWords[j]))
                            {
                                LevenstainStaticDataValidator validator = new LevenstainStaticDataValidator();
                                int pom = validator.ComputeLevensteinDistance(words[i], realWords[j]);
                                consoleTab2.Text += "Scanned word: " + words[i] + " Original word: " + realWords[j] + Environment.NewLine + "i: " + i + "; j: " + j + "; POM: " + pom.ToString() + Environment.NewLine;

                                resultAverage += pom;

                                break;
                            }
                        }
                    }
                    resultAverage = resultAverage / realWords.Length;

                    consoleTab2.Text += resultAverage.ToString() + Environment.NewLine;
                }
            }
            else if (triger.Equals("tnr20"))
            {

                using (var engine = new TesseractEngine(@"../../../tessdata", "eng", EngineMode.Default))
                using (var image = Pix.LoadFromFile(tnr20))
                using (var page = engine.Process(image))
                {
                    result20 = page.GetText();
                    txtMeanConf2.Text = String.Format("{0:P}", page.GetMeanConfidence());
                }

                String[] words = getWords(result20);
                String[] realWords = getWords(originalTNR20);

                consoleTab2.Text = "Scanned text length: " + words.Length + Environment.NewLine;
                consoleTab2.Text += "Original text length: " + realWords.Length + Environment.NewLine;

                //Posto je duzina skenirampg teksta uvek manja ili jednaka duzini originalnog teksta
                //prolazak kroz taj kraci tekst (skeniran) i poredjenje sa recima iz originalnog (duzeg) teksta.

                //Zadovoljava sve nase slucajeve. Da li treba i za slucaj realWords.Length <= words.Length ?
                if (words.Length <= realWords.Length)
                {

                    for (int i = 0; i < words.Length; i++)
                    {
                        for (int j = i; j < realWords.Length; j++)
                        {
                            if (words[i].Contains(realWords[j]) || words[i].Equals(realWords[j]))
                            {
                                LevenstainStaticDataValidator validator = new LevenstainStaticDataValidator();
                                int pom = validator.ComputeLevensteinDistance(words[i], realWords[j]);
                                consoleTab2.Text += "Scanned word: " + words[i] + " Original word: " + realWords[j] + Environment.NewLine + "i: " + i + "; j: " + j + "; POM: " + pom.ToString() + Environment.NewLine;

                                resultAverage += pom;

                                break;
                            }
                        }
                    }
                    resultAverage = resultAverage / realWords.Length;

                    consoleTab2.Text += resultAverage.ToString() + Environment.NewLine;
                }
            }
        }

        public String[] getWords(string result)
        {
            String[] ret = new String[1000];

            ret = result.Split(' ');

            return ret;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (comboBox2.Text.Contains("Arial 10point"))
            {
                triger = "arial10";
                staticOcr.Enabled = true;

                return;
            }
            else if (comboBox2.Text.Contains("Arial 14point"))
            {
                triger = "arial14";
                staticOcr.Enabled = true;

                return;
            }
            else if (comboBox2.Text.Contains("Arial 20point"))
            {
                triger = "arial20";
                staticOcr.Enabled = true;

                return;
            }
            else if (comboBox2.Text.Contains("Times New Roman 10point"))
            {
                triger = "tnr10";
                staticOcr.Enabled = true;

                return;
            }
            else if (comboBox2.Text.Contains("Times New Roman 14point"))
            {
                triger = "tnr14";
                staticOcr.Enabled = true;

                return;
            }
            else if (comboBox2.Text.Contains("Times New Roman 20point"))
            {
                triger = "tnr20";
                staticOcr.Enabled = true;

                return;
            }
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBox2.Text.Contains("Arial 10point"))
            {
                triger = "arial10";
                staticOcr.Enabled = true;

                return;
            }
            else if (comboBox2.Text.Contains("Arial 14point"))
            {
                triger = "arial14";
                staticOcr.Enabled = true;

                return;
            }
            else if (comboBox2.Text.Contains("Arial 20point"))
            {
                triger = "arial20";
                staticOcr.Enabled = true;

                return;
            }
            else if (comboBox2.Text.Contains("Times New Roman 10point"))
            {
                triger = "tnr10";
                staticOcr.Enabled = true;

                return;
            }
            else if (comboBox2.Text.Contains("Times New Roman 14point"))
            {
                triger = "tnr14";
                staticOcr.Enabled = true;

                return;
            }
            else if (comboBox2.Text.Contains("Times New Roman 20point"))
            {
                triger = "tnr20";
                staticOcr.Enabled = true;

                return;
            }
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            //opcija za iteriranje rec po rec
            consoleTab2.Clear();
            string putanja = @"../../../tessdata";
            string imageDir = @"../../../staticimages/Arial10.PNG";

            try
            {
                using (var engine = new TesseractEngine(putanja, "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(imageDir))
                    {
                        using (var page = engine.Process(img))
                        {
                            var text = page.GetText();
                            consoleTab2.Text += "Mean confidence: " + String.Format("{0:P}", page.GetMeanConfidence()) + Environment.NewLine;

                            consoleTab2.Text += "Text (GetText): \r\n{0}" + text + Environment.NewLine;
                            consoleTab2.Text += "Text (iterator):" + Environment.NewLine;
                            using (var iter = page.GetIterator())
                            {
                                iter.Begin();

                                do
                                {
                                    do
                                    {
                                        do
                                        {
                                            do
                                            {
                                                if (iter.IsAtBeginningOf(PageIteratorLevel.Block))
                                                {
                                                    consoleTab2.Text += "<BLOCK>" + Environment.NewLine;
                                                }

                                                consoleTab2.Text += iter.GetText(PageIteratorLevel.Word);
                                                consoleTab2.Text += " " + Environment.NewLine;
                                                var prop = iter.GetProperties();
                                                float ugao = prop.DeskewAngle;
                                                WritingDirection dir = prop.WritingDirection;
                                                Tesseract.Orientation orieant = prop.Orientation;

                                                consoleTab2.Text += ugao + " " + dir + " " + orieant + Environment.NewLine;

                                                if (iter.IsAtFinalOf(PageIteratorLevel.TextLine, PageIteratorLevel.Word))
                                                {
                                                    consoleTab2.Text += Environment.NewLine;
                                                }
                                            } while (iter.Next(PageIteratorLevel.TextLine, PageIteratorLevel.Word));

                                            if (iter.IsAtFinalOf(PageIteratorLevel.Para, PageIteratorLevel.TextLine))
                                            {
                                                consoleTab2.Text += Environment.NewLine;
                                            }
                                        } while (iter.Next(PageIteratorLevel.Para, PageIteratorLevel.TextLine));
                                    } while (iter.Next(PageIteratorLevel.Block, PageIteratorLevel.Para));
                                } while (iter.Next(PageIteratorLevel.Block));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                Console.WriteLine("Unexpected Error: " + ex.Message);
                Console.WriteLine("Details: ");
                Console.WriteLine(ex.ToString());
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //string path = @"../../../tessdata";
            //string imageDir = @"../../../staticimages/hello.png";
            //using (TesseractEngine engine = new TesseractEngine(path, "eng", EngineMode.Default))
            //{
            //   using (Pix img = Pix.LoadFromFile(imageDir))
            //    {
            //        using (Page page = engine.Process(img, PageSegMode.OsdOnly))
            //        {
            //            Tesseract.Orientation orientation;
            //            float confidence;
            //            page.DetectBestOrientation(out orientation, out confidence);         
            //            string text = page.GetText().Replace("\n", "\r\n");
            //            consoleTab2.Text += text;
            //        }
            //    }
            //}
            string path = @"../../../tessdata";
            string imageDir = @"../../../staticimages/hello.png";
            Bitmap bitmap = new Bitmap(imageDir);

            DocumentInspector inspector = new DocumentInspector(path, "eng");
            DocumentInspector.DocumentInspectorPageOrientation orientation = inspector.DetectPageOrientation(bitmap);
            consoleTab2.Text += orientation;
        }
    }
}
