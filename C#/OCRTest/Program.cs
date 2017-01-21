using System;
using System.Speech;

namespace OCRTest
{
    using System.Drawing;
    using System.Speech.Synthesis;
    using System.Windows.Forms;
    using tessnet2;

    class Program
    {
        // Proba komentar
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
