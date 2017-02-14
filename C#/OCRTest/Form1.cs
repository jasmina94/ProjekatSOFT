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
            string putanja1 = @"../../../tessdata";
            string imagePath = imageDir + "/" + imageCounter.ToString() + ".png";
            Bitmap bitmap = new Bitmap(image);
            procitanTekst = "";

            bitmap.Save(imagePath);
            using (var engine = new TesseractEngine(putanja1, language, EngineMode.Default))
            using (var image = Pix.LoadFromFile(imagePath))
            using (var page = engine.Process(image))
            {
                string text = page.GetText();
                consoleTab1.Text = text;
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
            if (srpski)
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
                synthesizer.SelectVoice("Microsoft Zira Desktop");
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
            string imageDir = @"../../../image.png";

            using (var engine = new TesseractEngine(putanja, "eng", EngineMode.Default))
            using (var image = Pix.LoadFromFile(imageDir))
            using (var page = engine.Process(image))
            {
                string text = page.GetText();
                consoleTab2.Text = text;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // prvo ocisti konzolu i ispisi tekst
            
            String message = "Validation of static text on 2 different fonts and 3 sizes. Arial and TimesNewRoman based on Levenstain distance algortithm.";

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
                }

                String[] words = getWords(result10);
                String[] realWords = getWords(originalArial10);

                consoleTab2.Text = "Duzina skeniranog: " + words.Length + Environment.NewLine;
                consoleTab2.Text += "Duzina originalnog: " + realWords.Length + Environment.NewLine;

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
                                consoleTab2.Text += "Skenirana rec: " + words[i] + " Original rec: " + realWords[j] + Environment.NewLine + "i: " + i + "; j: " + j + "; POM: " + pom.ToString() + Environment.NewLine;

                                resultAverage += pom;

                                break;
                            }
                        }
                    }
                    resultAverage = resultAverage / realWords.Length;

                    consoleTab2.Text += resultAverage.ToString() + Environment.NewLine;
                }
            }else if(triger.Equals("arial14"))
            {

                using (var engine = new TesseractEngine(@"../../../tessdata", "eng", EngineMode.Default))
                using (var image = Pix.LoadFromFile(a14))
                using (var page = engine.Process(image))
                {
                    result14 = page.GetText();
                }

                String[] words = getWords(result14);
                String[] realWords = getWords(originalArial14);

                consoleTab2.Text = "Duzina skeniranog: " + words.Length + Environment.NewLine;
                consoleTab2.Text += "Duzina originalnog: " + realWords.Length + Environment.NewLine;

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
                                consoleTab2.Text += "Skenirana rec: " + words[i] + " Original rec: " + realWords[j] + Environment.NewLine + "i: " + i + "; j: " + j + "; POM: " + pom.ToString() + Environment.NewLine;

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
                }

                String[] words = getWords(result20);
                String[] realWords = getWords(originalArial20);

                consoleTab2.Text = "Duzina skeniranog: " + words.Length + Environment.NewLine;
                consoleTab2.Text += "Duzina originalnog: " + realWords.Length + Environment.NewLine;

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
                                consoleTab2.Text += "Skenirana rec: " + words[i] + " Original rec: " + realWords[j] + Environment.NewLine + "i: " + i + "; j: " + j + "; POM: " + pom.ToString() + Environment.NewLine;

                                resultAverage += pom;

                                break;
                            }
                        }
                    }
                    resultAverage = resultAverage / realWords.Length;

                    consoleTab2.Text += resultAverage.ToString() + Environment.NewLine;
                }
            }else if (triger.Equals("arial10"))
            {

                using (var engine = new TesseractEngine(@"../../../tessdata", "eng", EngineMode.Default))
                using (var image = Pix.LoadFromFile(tnr10))
                using (var page = engine.Process(image))
                {
                    result10 = page.GetText();
                }

                String[] words = getWords(result10);
                String[] realWords = getWords(originalTNR10);

                consoleTab2.Text = "Duzina skeniranog: " + words.Length + Environment.NewLine;
                consoleTab2.Text += "Duzina originalnog: " + realWords.Length + Environment.NewLine;

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
                                consoleTab2.Text += "Skenirana rec: " + words[i] + " Original rec: " + realWords[j] + Environment.NewLine + "i: " + i + "; j: " + j + "; POM: " + pom.ToString() + Environment.NewLine;

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
                using (var image = Pix.LoadFromFile(tnr14))
                using (var page = engine.Process(image))
                {
                    result14 = page.GetText();
                }

                String[] words = getWords(result14);
                String[] realWords = getWords(originalTNR14);

                consoleTab2.Text = "Duzina skeniranog: " + words.Length + Environment.NewLine;
                consoleTab2.Text += "Duzina originalnog: " + realWords.Length + Environment.NewLine;

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
                                consoleTab2.Text += "Skenirana rec: " + words[i] + " Original rec: " + realWords[j] + Environment.NewLine + "i: " + i + "; j: " + j + "; POM: " + pom.ToString() + Environment.NewLine;

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
                }

                String[] words = getWords(result20);
                String[] realWords = getWords(originalTNR20);

                consoleTab2.Text = "Duzina skeniranog: " + words.Length + Environment.NewLine;
                consoleTab2.Text += "Duzina originalnog: " + realWords.Length + Environment.NewLine;

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
                                consoleTab2.Text += "Skenirana rec: " + words[i] + " Original rec: " + realWords[j] + Environment.NewLine + "i: " + i + "; j: " + j + "; POM: " + pom.ToString() + Environment.NewLine;

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
    }
}
