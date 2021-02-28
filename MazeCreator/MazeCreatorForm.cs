using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Linq;

namespace MazeCreator
{
    public partial class MazeCreatorForm : Form
    {
        private EShape Shape;
        private EStyle Style;
        private int MinWidth;
        private int MaxWidth;
        private int MinHeight;
        private int MaxHeight;
        private int Width = 0;
        private int Height = 0;
        private EFileFormat FileFormat;
        private EStage Stage;
        private string FileName;
        private int Amount = 0;
        private SemaphoreSlim signal;
        private int Size = 0;

        public MazeCreatorForm()
        {
            InitializeComponent();
        }

        private void MazeCreatorForm_Load(object sender, EventArgs e)
        {
            comboBoxShape.SelectedIndex = 0;
            comboBoxStyle.SelectedIndex = 0;
            CheckComboBoxShape();
        }

        private void GetDataFromUI()
        {
            Shape = GetShapeFromUI();
            Style = GetStyleFromUI();
            MinWidth = Convert.ToInt32(textBoxMinWidth.Text);
            MaxWidth = Convert.ToInt32(textBoxMaxWidth.Text);
            MinHeight = Convert.ToInt32(textBoxMinHeight.Text);
            MaxHeight = Convert.ToInt32(textBoxMaxHeight.Text);
            Size = Convert.ToInt32(textBoxDiameter.Text);
            Amount = Convert.ToInt32(textBoxAmount.Text);
            FileFormat = EFileFormat.SVG_WithSolution;
        }

        private EShape GetShapeFromUI()
        {
            EShape res;
            switch (comboBoxShape.SelectedIndex)
            {
                case 0:
                    res = EShape.Rectangular;
                    break;
                case 1:
                    res = EShape.Circular;
                    break;
                case 2:
                    res = EShape.Triangular;
                    break;
                case 3:
                    res = EShape.Hexagonal;
                    break;
                default:
                    res = EShape.Rectangular;
                    break;
            }

            return res;
        }

        private EStyle GetStyleFromUI()
        {
            EStyle res;
            switch (comboBoxShape.SelectedIndex)
            {
                case 0:
                    res = EStyle.Orthogonal;
                    break;
                case 1:
                    res = EStyle.Sigma;
                    break;
                case 2:
                    res = EStyle.Delta;
                    break;
                default:
                    res = EStyle.Orthogonal;
                    break;
            }
            return res;
        }

        private async void buttonCreateMazeDB_Click(object sender, EventArgs e)
        {
            GetDataFromUI();

            for (Width = MinWidth; Width <= MaxWidth; Width++)
            {
                for (Height = MinHeight; Height <= MaxHeight; Height++)
                {
                    for (int i = 0; i < Amount; i++)
                    {
                        signal = new SemaphoreSlim(0, 1);
                        if (Shape == EShape.Rectangular)
                        {
                            FileName = $"Maze_{Shape}_{Style}_{Width}_{Height}_{i}";
                        }
                        else
                        {
                            FileName = $"Maze_{Shape}_{Size}_{i}";
                        }
                        Generate();
                        await signal.WaitAsync();
                        ClearSVG(FileName, new KnownFolder(KnownFolderType.Downloads).Path);
                    }
                }
            }
        }

        private void Generate()
        {
            webBrowser1.Navigate("http://www.mazegenerator.net");
        }

        private void ClearSVG(string Filename,string path)
        {
            var xDoc = XDocument.Load(path + "\\" + Filename+".svg");
            var node = xDoc.Descendants("svg");
            node.Attributes("desc").Remove();
            xDoc.Element("{http://www.w3.org/2000/svg}svg").Elements("{http://www.w3.org/2000/svg}desc").Remove();
            //MessageBox.Show(xDoc.ToString());
            xDoc.Save(path + "\\" + Filename + ".svg");
        }

        private void DocumentComplete(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
                return;
           
            if (Stage == EStage.Ready && Shape == EShape.Rectangular)
                Stage = EStage.SelectedShape;

            switch (Stage)
            {
                case EStage.Ready:
                {
                    HtmlElement shapeElement = webBrowser1.Document.GetElementById("ShapeDropDownList");
                    shapeElement.SetAttribute("value", ((int) Shape + 1).ToString());
                    HtmlElement buttonGenerateElement = webBrowser1.Document.GetElementById("GenerateButton");
                    buttonGenerateElement.InvokeMember("click");

                        Stage = EStage.SelectedShape;
                    break;
                }

                case EStage.SelectedShape:
                {
                    //HtmlElement shapeElement = webBrowser1.Document.GetElementById("ShapeDropDownList");
                    //shapeElement.SetAttribute("value", ((int)Shape + 1).ToString());
                        HtmlElement buttonGenerateElement = webBrowser1.Document.GetElementById("GenerateButton");
                    switch (Shape)
                    {
                        case EShape.Rectangular:
                        {
                            HtmlElement styleElement = webBrowser1.Document.GetElementById("S1TesselationDropDownList");
                            HtmlElement widthElement = webBrowser1.Document.GetElementById("S1WidthTextBox");
                            HtmlElement heightElement = webBrowser1.Document.GetElementById("S1HeightTextBox");

                            styleElement.SetAttribute("value", ((int) Style + 1).ToString());
                            widthElement.SetAttribute("value", Width.ToString());
                            heightElement.SetAttribute("value", Height.ToString());
                            break;
                        }
                        case EShape.Circular:
                        {
                            HtmlElement diameterElement = webBrowser1.Document.GetElementById("S2OuterDiameterTextBox");
                            diameterElement.SetAttribute("value", Size.ToString());
                            break;
                        }
                        case EShape.Triangular:
                        {
                            HtmlElement lengthElement = webBrowser1.Document.GetElementById("S3SideLengthTextBox");
                            lengthElement.SetAttribute("value", Size.ToString());
                            break;
                        }
                        case EShape.Hexagonal:
                        {
                            HtmlElement lengthElement = webBrowser1.Document.GetElementById("S4SideLengthTextBox");
                            lengthElement.SetAttribute("value", Size.ToString());
                            break;
                        }
                    }

                    buttonGenerateElement.InvokeMember("click");
                    Stage = EStage.Generated;
                    break;
                }

                case EStage.Generated:
                {
                    HtmlElement fileFormatElement = webBrowser1.Document.GetElementById("FileFormatSelectorList");
                    HtmlElement downloadButtonElement = webBrowser1.Document.GetElementById("DownloadFileButton");

                    fileFormatElement.SetAttribute("value", ((int) FileFormat + 1).ToString());
                    downloadButtonElement.InvokeMember("click");

                    Thread th = new Thread(() =>
                    {
                        System.Threading.Thread.Sleep(1000);
                        SendKeys.SendWait("{Tab}");
                        SendKeys.SendWait("{Tab}");
                        SendKeys.SendWait("{Tab}");
                        SendKeys.SendWait("{Enter}");
                        System.Threading.Thread.Sleep(1000);
                        SendKeys.SendWait(FileName);
                        System.Threading.Thread.Sleep(1000);
                        SendKeys.SendWait("{Tab}");
                        SendKeys.SendWait("{Tab}");
                        SendKeys.SendWait("{Tab}");
                        SendKeys.SendWait("{Enter}");
                        System.Threading.Thread.Sleep(1000);
                        signal.Release();
                    });
                    th.SetApartmentState(ApartmentState.MTA);
                    th.Start();
                    Stage = EStage.Ready;
                    break;
                }
            }
        }

        private void comboBoxShape_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckComboBoxShape();
        }

        private void CheckComboBoxShape()
        {
            switch (comboBoxShape.SelectedIndex)
            {
                case 0:
                {
                    textBoxMinWidth.Enabled = textBoxMaxWidth.Enabled =
                        textBoxMinHeight.Enabled = textBoxMaxHeight.Enabled = true;
                    comboBoxStyle.Enabled = true;
                    textBoxDiameter.Enabled = false;
                    break;
                }
                case 1:
                case 2:
                case 3:
                {
                    textBoxMinWidth.Enabled = textBoxMaxWidth.Enabled =
                        textBoxMinHeight.Enabled = textBoxMaxHeight.Enabled = false;
                    comboBoxStyle.Enabled = false;
                    textBoxDiameter.Enabled = true;
                    break;
                }
            }
        }
    }
}
