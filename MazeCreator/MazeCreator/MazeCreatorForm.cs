using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using MazeCreator.Enum;
using Syroot.Windows.IO;
using Timer = System.Windows.Forms.Timer;

namespace MazeCreator
{
    public partial class MazeCreatorForm : Form
    {
        private const string UriString = "http://www.mazegenerator.net";
        private const string MemberClick = "click";
        private const string AttrValue = "value";
        private int m_Amount, m_Size;
        private int m_Width, m_Height;
        private EShape m_Shape;
        private EStage m_Stage;
        private EStyle m_Style;
        
        private readonly Stopwatch m_Stopwatch = new Stopwatch();
        private readonly Timer m_Timer = new Timer();
        private int m_ServerAnswersCountCheck;
        private int m_ServerAnswersCount;

        private int m_CurrentIndex;
        private int CurrentIndex
        {
            get => m_CurrentIndex;
            set
            {
                m_CurrentIndex = value;
                pb_creation.Value = value;
            }
        }

        public MazeCreatorForm()
        {
            Application.ThreadException += (_Sender, _Args) =>
            {
                MessageBox.Show(@"ThreadException");
                DocumentCompleted(null, null);
            };
            InitializeComponent();
            webBrowser1.ScriptErrorsSuppressed = true;
            pb_creation.Minimum = pb_creation.Value = 0;
            btn_setParams.Enabled = false;
            buttonCreateMazeDB.Enabled = false;
            chb_IsReady.Enabled = false;
            m_Timer.Tick += TimerOnTick;
        }
        
        private void TimerOnTick(object _, EventArgs _E)
        {
            if (m_ServerAnswersCount > m_ServerAnswersCountCheck)
                m_Stopwatch.Restart();
            if (m_Stopwatch.Elapsed.TotalSeconds > 10d)
                buttonCreateMazeDB_Click(null, null);
            m_ServerAnswersCountCheck = m_ServerAnswersCount;
        }
        
        private void btn_Init_Click(object _S, EventArgs _E)
        {
            webBrowser1.Navigate(UriString);
            btn_setParams.Enabled = true;
        }

        private void buttonCreateMazeDB_Click(object _Sender, EventArgs _E)
        {
            m_Timer.Start();
            m_Stage = EStage.Ready;
            webBrowser1.Navigate(UriString);
            m_Amount = Convert.ToInt32(textBoxAmount.Text) + 2;
            pb_creation.Maximum = m_Amount;
            CurrentIndex = pb_creation.Value = 0;
        }
        
        private void btn_setParams_Click(object _, EventArgs _E)
        {
            UpdateSettings();
            chb_IsReady.Enabled = true;
        }

        private void chb_IsReady_CheckedChanged(object _, EventArgs _E)
        {
            buttonCreateMazeDB.Enabled = chb_IsReady.Checked;
            if (chb_IsReady.Checked)
                UpdateSettings();
        }
        
        private void DocumentCompleted(object _Sender, WebBrowserDocumentCompletedEventArgs _E)
        {
            if (!chb_IsReady.Checked)
                return;

            if (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
            {
                m_Stage = EStage.Ready;
                MessageBox.Show($@"Document state: {webBrowser1.ReadyState}");
                return;
            }
            m_ServerAnswersCount++;
 
            switch (m_Stage)
            {
                case EStage.Ready:
                    ProcessStage(StageSelectShape);
                    break;
                case EStage.SetShape:
                    ProcessStage(StageSelectMazeSizeAndGenerate);
                    break;
                case EStage.SetSolution:
                    ProcessStage(SetSolutionCheckbox);
                    break;
                case EStage.Generated:
                    ProcessStage(StageGetSvg);
                    break;
                default:
                    throw new Exception("Not all EStage types implemented");
            }
        }
        
        private void StageSelectShape()
        {
            var shapeElement = webBrowser1.Document?.GetElementById("ShapeDropDownList");
            shapeElement?.SetAttribute(AttrValue, ((int) m_Shape + 1).ToString());
            m_Stage = EStage.SetShape;
            CallGenerateNewButton();
        }
        
        private void StageSelectMazeSizeAndGenerate()
        {
            switch (m_Shape)
            {
                case EShape.Rectangular:
                    var styleElement = webBrowser1.Document?.GetElementById("S1TesselationDropDownList");
                    var widthElement = webBrowser1.Document?.GetElementById("S1WidthTextBox");
                    var heightElement = webBrowser1.Document?.GetElementById("S1HeightTextBox");
                    styleElement?.SetAttribute(AttrValue, ((int) m_Style + 1).ToString());
                    widthElement?.SetAttribute(AttrValue, m_Width.ToString());
                    heightElement?.SetAttribute(AttrValue, m_Height.ToString());
                    break;
                case EShape.Circular:
                    var diameterElement = webBrowser1.Document?.GetElementById("S2OuterDiameterTextBox");
                    diameterElement?.SetAttribute(AttrValue, m_Size.ToString());
                    break;
                case EShape.Triangular:
                    var lengthEl = webBrowser1.Document?.GetElementById("S3SideLengthTextBox");
                    lengthEl?.SetAttribute(AttrValue, m_Size.ToString());
                    break;
                case EShape.Hexagonal:
                    var lengthElement = webBrowser1.Document?.GetElementById("S4SideLengthTextBox");
                    lengthElement?.SetAttribute(AttrValue, m_Size.ToString());
                    break;
            }

            m_Stage = EStage.SetSolution;
            CallGenerateNewButton();
        }
        
        private void SetSolutionCheckbox()
        {
            var showSolutionEl = webBrowser1.Document?.GetElementById("ShowSolutionCheckBox");
            if (showSolutionEl == null)
            {
                MessageBox.Show(@"Show solution checkbox is null");
                return;
            }
            
            bool isChecked = showSolutionEl.GetAttribute("checked") != "False";
            if (!isChecked)
            {
                showSolutionEl.InvokeMember(MemberClick);
                //Thread.Sleep(1000);
            }
            m_Stage = EStage.Generated;
            DocumentCompleted(null, null);
        }
        
        private void StageGetSvg()
        {
            var svgLinkElement = webBrowser1.Document?.GetElementById("MazeDisplay");
            if (svgLinkElement == null)
            {
                MessageBox.Show(@"Svg el is null");
                chb_IsReady.Checked = false;
                return;
            }
                    
            string svgUri = svgLinkElement.GetAttribute("src");
            var req = WebRequest.CreateHttp(svgUri);
            req.CookieContainer = CookiesHelper.GetUriCookieContainer(new Uri(UriString));
            using (var response = req.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null)
                {
                    MessageBox.Show(@"Response stream is null");
                    chb_IsReady.Checked = false;
                    return;
                }
                var reader = new StreamReader( responseStream );
                string reqResult = reader.ReadToEnd();
                var xDoc = XDocument.Parse(reqResult);
                xDoc = ClearSvg(xDoc);
                if (!Directory.Exists(MazeDir))
                    Directory.CreateDirectory(MazeDir);
                if (CurrentIndex > 2)
                    xDoc.Save(Path.Combine(MazeDir, $"{GetMazeName()}.svg"));
            }
            m_Stage = EStage.Ready;

            if (++CurrentIndex == m_Amount)
            {
                lb_log.Text = @"Creation completed!";
                m_Timer.Stop(); 
            }
            else
                CallGenerateNewButton();
        }
        
        private XDocument ClearSvg(XDocument _Doc)
        {
            var xDoc = _Doc;
            var nodes = xDoc.Root?.Descendants();
            nodes?.Attributes("desc").Remove();
            xDoc.Root?.Elements("desc".ToXName()).Remove();
            xDoc.Root?.Add(new XElement("type".ToXName(), m_Shape));
            return xDoc;
        }
        
        private void CallGenerateNewButton()
        {
            var buttonGenElement = webBrowser1.Document?.GetElementById("GenerateButton");
            if (buttonGenElement == null)
            {
                MessageBox.Show(@"Generate New Button is null");
                return;
            }
            buttonGenElement.InvokeMember(MemberClick);
        }
        
        private string MazeNameSuffix => m_Shape == EShape.Rectangular ? $"{m_Width}_{m_Height}_{m_Style}" : $"{m_Size}";
        private string MazeDir => Path.Combine(
            new KnownFolder(KnownFolderType.Desktop).Path, "Mazes", $"{m_Shape}_{MazeNameSuffix}");
        
        private string GetMazeName()
        {
            int newIdx = Directory.GetFiles(MazeDir).Length + 1;
            return m_Shape == EShape.Rectangular ? 
                $"Maze_{m_Shape}_{m_Style}_{MazeNameSuffix}_{newIdx}" 
                : $"Maze_{MazeNameSuffix}_{newIdx}";
        } 
        
        private void ProcessStage(Action _StateAction)
        {
            lb_log.Text = $@"Creating maze {CurrentIndex} of {m_Amount}: {m_Stage}";
            _StateAction?.Invoke();
            if (m_Stage != EStage.Generated)
            {
                int.TryParse(tb_delay.Text, out int delay);
                Thread.Sleep(delay);
            }
        }
        
        private void UpdateSettings()
        {
            var shapeElement = webBrowser1.Document?.GetElementById("ShapeDropDownList");
            if (shapeElement != null)
            {
                int.TryParse(shapeElement.GetAttribute(AttrValue), out int val);
                m_Shape = (EShape)(val - 1);
            }
            
            switch (m_Shape)
            {
                case EShape.Rectangular:
                    var styleElement = webBrowser1.Document?.GetElementById("S1TesselationDropDownList");
                    var widthElement = webBrowser1.Document?.GetElementById("S1WidthTextBox");
                    var heightElement = webBrowser1.Document?.GetElementById("S1HeightTextBox");

                    if (styleElement != null)
                    {
                        int.TryParse(styleElement.GetAttribute(AttrValue), out int val);
                        m_Style = (EStyle) (val - 1);
                    }
                    if (widthElement != null)
                        int.TryParse(widthElement.GetAttribute(AttrValue), out m_Width);
                    if (heightElement != null)
                        int.TryParse(heightElement.GetAttribute(AttrValue), out m_Height);
                    break;
                case EShape.Circular:
                    var diameterElement = webBrowser1.Document?.GetElementById("S2OuterDiameterTextBox");
                    if (diameterElement != null)
                        int.TryParse(diameterElement.GetAttribute(AttrValue), out m_Size);
                    break;
                case EShape.Triangular:
                {
                    var lengthEl = webBrowser1.Document?.GetElementById("S3SideLengthTextBox");
                    if (lengthEl != null)
                        int.TryParse(lengthEl.GetAttribute(AttrValue), out m_Size);
                    break;
                }
                case EShape.Hexagonal:
                {
                    var lengthEl = webBrowser1.Document?.GetElementById("S4SideLengthTextBox");
                    if (lengthEl != null)
                        int.TryParse(lengthEl.GetAttribute(AttrValue), out m_Size);
                    break;
                }
            }
        }
    }
}