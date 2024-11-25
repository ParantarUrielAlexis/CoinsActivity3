using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;


namespace CoinsActivity3 {
    public partial class Form1 : Form {

        Bitmap loaded;

        const double VALUE_5_CENTS = 0.05;
        const double VALUE_10_CENTS = 0.1;
        const double VALUE_25_CENTS = 0.25;
        const double VALUE_1_PESO = 1.0;
        const double VALUE_5_PESOS = 5.0;

        public Form1() {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e) {
            loaded = new Bitmap(openFileDialog1.FileName);
            pictureBox1.Image = loaded;
        }

        private void button1_Click(object sender, EventArgs e) {
            if (pictureBox1.Image == null) {
                label1.Text = "Coins: 0\nValue: $0.00";
                return;
            }

            Bitmap bitmap = new Bitmap(pictureBox1.Image);
            Mat sourceImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);

            Mat grayImage = new Mat();
            Cv2.CvtColor(sourceImage, grayImage, ColorConversionCodes.BGR2GRAY);

            Mat blurredImage = new Mat();
            Cv2.GaussianBlur(grayImage, blurredImage, new OpenCvSharp.Size(15, 15), 0);

            Mat edgeImage = new Mat();
            Cv2.Canny(blurredImage, edgeImage, 100, 200);

            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(edgeImage, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            
            var coinDefinitions = new Dictionary<(double MinArea, double MaxArea), (double Value, string CoinType)> {
                { (2600, 2800), (VALUE_5_CENTS, "5 cents") },
                { (3100, 3300), (VALUE_10_CENTS, "10 cents") },
                { (4300, 4600), (VALUE_25_CENTS, "25 cents") },
                { (6200, 6400), (VALUE_1_PESO, "1 peso") },
                { (7800, 8100), (VALUE_5_PESOS, "5 pesos") }
            };

            int totalCoins = 0;
            double totalValue = 0.0;
            var coinCounters = new Dictionary<string, int>();

            foreach (var contour in contours) {
                double area = Cv2.ContourArea(contour);
                Console.WriteLine(area);

                if (area <= 100) continue;

                totalCoins++;

                foreach (var definition in coinDefinitions) {
                    if (area >= definition.Key.MinArea && area < definition.Key.MaxArea) {
                        totalValue += definition.Value.Value;

                        if (!coinCounters.ContainsKey(definition.Value.CoinType)) {
                            coinCounters[definition.Value.CoinType] = 0;
                        }
                        coinCounters[definition.Value.CoinType]++;
                        break;
                    }
                }
            }

            string coinDetails = string.Join("\n", coinCounters.Select(kvp => $"{kvp.Value} x {kvp.Key}"));
            Console.WriteLine(coinDetails);

            label1.Text = $"Total Coins: {totalCoins}\n and Total Value: {totalValue:C2}\n{coinDetails}";
        }
    }
}

