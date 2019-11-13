/* 
 * Find template images in source image. Uses EmguCV (OpenCV .Net wrapper) 4.1.1, 64bit dlls.
 * Şamil Korkmaz, November 2019
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using TemplateMatching.Properties;

namespace TemplateMatching
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var birdTemplateFileNames = new List<string>
                {
                    "bird_up1",
                    "bird_up2",
                    "bird_level",
                    "bird_down1",
                    "bird_down2",
                    "bird_down3",
                    "bird_down4"
                };
                var resourceManager = Resources.ResourceManager;
                var pipeGapTemplate = new Image<Bgr, byte>((Bitmap)resourceManager.GetObject("pipeGap"));
                const int nbOfSourceFiles = 500;
                for (var iSource = 0; iSource < nbOfSourceFiles; iSource++)
                {
                    var sourceFileName = "C:/temp/flappy/ScreenCapture/bin/Debug/test" + iSource + ".bmp";
                    textBox1.Text = sourceFileName;
                    textBox1.Refresh();
                    var source = new Image<Bgr, byte>(sourceFileName);

                    var pipeGapTask = Task.Factory.StartNew(() => GetMatch(source, pipeGapTemplate));
                    var pipeGapMatchData = pipeGapTask.Result;

                    var taskArray = new Task<MatchData>[birdTemplateFileNames.Count];
                    for (var i = 0; i < birdTemplateFileNames.Count; i++)
                    {
                        var birdTemplate = new Image<Bgr, byte>((Bitmap) resourceManager.GetObject(birdTemplateFileNames[i]));
                        taskArray[i] = Task.Factory.StartNew(() => GetMatch(source, birdTemplate));
                    }
                    var maxBirdMatchQuality = 0.0;
                    var matchBirdRect = new Rectangle(new Point(0, 0), new Size(0, 0));
                    for (var i = 0; i < birdTemplateFileNames.Count; i++)
                    {
                        var matchBirdData = taskArray[i].Result;
                        if (matchBirdData.Quality < maxBirdMatchQuality) continue;
                        //Console.WriteLine("Task " + i + " maxVal = " + matchData.Quality);
                        maxBirdMatchQuality = matchBirdData.Quality;
                        matchBirdRect = new Rectangle(matchBirdData.RectLocation, matchBirdData.Template.Size);
                        pictureBox2.Image = matchBirdData.Template.ToBitmap();

                    }
                    lbInfo.Text = "maxPipeGap = " + pipeGapMatchData.Quality.ToString("F1") + ", maxBird=" + maxBirdMatchQuality.ToString("F1") + 
                        ", birdCX = " + (matchBirdRect.X + matchBirdRect.Width / 2).ToString("F1") + ", birdCY = " + ((matchBirdRect.Y + matchBirdRect.Height / 2) - 14).ToString("F1");
                    lbInfo.Refresh();
                    var imageToShow = source.Copy();
                    imageToShow.Draw(matchBirdRect, new Bgr(Color.Red), 3);
                    if (pipeGapMatchData.Quality > 0.5)
                    {
                        var pipeGapRect = new Rectangle(pipeGapMatchData.RectLocation, pipeGapMatchData.Template.Size);
                        imageToShow.Draw(pipeGapRect, new Bgr(Color.Red), 3);
                    }
                    pictureBox1.Image = imageToShow.ToBitmap();
                    pictureBox1.Refresh();
                    pictureBox2.Refresh();
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private class MatchData
        {
            public Image<Bgr, byte> Template { get; }
            public Point RectLocation { get; }
            public double Quality { get; }
            public MatchData(Point rectLocation, double quality, Image<Bgr, byte> template)
            {
                RectLocation = rectLocation;
                Quality = quality;
                Template = template;
            }
        }

        private MatchData GetMatch(Image<Bgr, byte> source, Image<Bgr, byte> template)
        {
            using (var result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
            {
                double[] currentMinValues, currentMaxValues;
                Point[] currentMinLocations, currentMaxLocations;
                result.MinMax(out currentMinValues, out currentMaxValues, out currentMinLocations, out currentMaxLocations);
                return new MatchData(currentMaxLocations[0], currentMaxValues[0], template);
            }
        }
    }
}

