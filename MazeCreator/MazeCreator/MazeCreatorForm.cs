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
        private delegate void VoidHandler();
        
        private const string UriString = "http://www.mazegenerator.net";
        private const string MemberClick = "click";
        private const string AttrValue = "value";
        private const double TimerDelay = 6D;
        
        private int m_Amount, m_Size;
        private int m_Width, m_Height;
        private EShape m_Shape;
        private EStage m_Stage;
        private EStyle m_Style;
        
        private readonly Random m_Random = new Random();
        private readonly Stopwatch m_Stopwatch = new Stopwatch();
        private readonly Timer m_Timer = new Timer();
        private int m_ServerAnswersCountCheck;
        private int m_ServerAnswersCount;
        private int m_Delay;

        private int m_CurrentIndex;
        private int CurrentIndex
        {
            get => m_CurrentIndex;
            set => m_CurrentIndex = pb_creation.Value = value;
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
            m_Timer.Tick += TimerOnTick;
        }
        
        private void TimerOnTick(object _, EventArgs _E)
        {
            if (m_ServerAnswersCount > m_ServerAnswersCountCheck)
                m_Stopwatch.Restart();
            if (m_Stopwatch.Elapsed.TotalSeconds > TimerDelay)
            {
                m_Stopwatch.Stop();
                StartCreation();
            }
            m_ServerAnswersCountCheck = m_ServerAnswersCount;
        }
        
        private void btn_Init_Click(object _S, EventArgs _E)
        {
            webBrowser1.Navigate(UriString);
        }

        private void buttonCreateMazeDB_Click(object _Sender, EventArgs _E)
        {
            webBrowser1.DocumentCompleted += DocumentCompleted;
            StartCreation();
        }
        
        private void StartCreation()
        {
            UpdateSettings();
            int.TryParse(tb_delay.Text, out m_Delay);
            tb_delay.Enabled = false;
            buttonCreateMazeDB.Enabled = false;
            m_Timer.Start();
            m_Stage = EStage.Ready;
            m_Amount = pb_creation.Maximum = Convert.ToInt32(textBoxAmount.Text);
            CurrentIndex = pb_creation.Value = 0;
            CallGenerateNewButton();
        }

        private void DocumentCompleted(object _Sender, WebBrowserDocumentCompletedEventArgs _E)
        {
            var rState = webBrowser1.ReadyState;
            if (rState != WebBrowserReadyState.Complete && rState != WebBrowserReadyState.Interactive)
            {
                m_Stage = EStage.Ready;
                MessageBox.Show($@"Document state: {webBrowser1.ReadyState}");
                return;
            }
            m_ServerAnswersCount++;
 
            switch (m_Stage)
            {
                case EStage.Ready:
                    pb_creation.Value = pb_creation.Minimum;
                    ProcessStage(StageSelectShape, m_Stage);
                    break;
                case EStage.SetShape:
                    ProcessStage(StageSelectMazeSizeAndGenerate, m_Stage);
                    break;
                case EStage.SetSolution:
                    ProcessStage(SetSolutionCheckbox, m_Stage);
                    break;
                case EStage.Generated:
                    ProcessStage(StageGetSvg, m_Stage);
                    break;
                default:
                    throw new Exception("Not all EStage types implemented");
            }
        }
        
        private void StageSelectShape()
        {
            var shapeElement = webBrowser1.Document?.GetElementById(ElNames.ShapeDropdown);
            shapeElement?.SetAttribute(AttrValue, ((int) m_Shape + 1).ToString());
            m_Stage = EStage.SetShape;
            CallGenerateNewButton();
        }
        
        private void StageSelectMazeSizeAndGenerate()
        {
            var eParamElement = webBrowser1.Document?.GetElementById(ElNames.EParam);
            var rParamElement = webBrowser1.Document?.GetElementById(ElNames.RParam);
            eParamElement?.SetAttribute(AttrValue, m_Random.Next(0, 100).ToString());
            rParamElement?.SetAttribute(AttrValue, m_Random.Next(50, 100).ToString());
            switch (m_Shape)
            {
                case EShape.Rectangular:
                    var styleElement = webBrowser1.Document?.GetElementById(ElNames.StyleDropdown);
                    var widthElement = webBrowser1.Document?.GetElementById(ElNames.Width);
                    var heightElement = webBrowser1.Document?.GetElementById(ElNames.Height);
                    styleElement?.SetAttribute(AttrValue, ((int) m_Style + 1).ToString());
                    widthElement?.SetAttribute(AttrValue, m_Width.ToString());
                    heightElement?.SetAttribute(AttrValue, m_Height.ToString());
                    break;
                case EShape.Circular:
                case EShape.Triangular:
                case EShape.Hexagonal:
                    var sizeElement = webBrowser1.Document?.GetElementById(ElNames.Size(m_Shape));
                    sizeElement?.SetAttribute(AttrValue, m_Size.ToString());
                    break;
            }

            if (m_Shape == EShape.Triangular)
            {
                var innerSideLenEl = webBrowser1.Document?.GetElementById(ElNames.InnerSideLength);
                innerSideLenEl?.SetAttribute(AttrValue, "0");
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
                showSolutionEl.InvokeMember(MemberClick);
            m_Stage = EStage.Generated;
            DocumentCompleted(null, null);
        }
        
        private void StageGetSvg()
        {
            var svgLinkElement = webBrowser1.Document?.GetElementById("MazeDisplay");
            if (svgLinkElement == null)
            {
                MessageBox.Show(@"Svg el is null");
                return;
            }
                    
            var svgUri = svgLinkElement.GetAttribute("src");
            var req = WebRequest.CreateHttp(svgUri);
            req.CookieContainer = CookiesHelper.GetUriCookieContainer(new Uri(UriString));
            using (var response = req.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null)
                {
                    MessageBox.Show(@"Response stream is null");
                    return;
                }
                var reader = new StreamReader( responseStream );
                string reqResult = reader.ReadToEnd();
                var xDoc = XDocument.Parse(reqResult);
                xDoc = ClearSvg(xDoc);
                if (!Directory.Exists(MazeDir))
                    Directory.CreateDirectory(MazeDir);
                if (CurrentIndex > 1)
                    xDoc.Save(Path.Combine(MazeDir, $"{GetMazeName()}.svg"));
                
                CurrentIndex++;
                if (GetMazeDirectoryFilesCount() >= m_Amount)
                {
                    webBrowser1.DocumentCompleted -= DocumentCompleted;
                    tb_delay.Enabled = true;
                    buttonCreateMazeDB.Enabled = true;
                    lb_log.Text = @"Creation completed!";
                    pb_creation.Value = pb_creation.Maximum;
                    m_Timer.Stop(); 
                }
                else
                {
                    m_Stage = EStage.Ready;
                    CallGenerateNewButton();
                }
            }
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
            int newIdx = GetMazeDirectoryFilesCount() + 1;
            return m_Shape == EShape.Rectangular ? 
                $"Maze_{m_Shape}_{m_Style}_{MazeNameSuffix}_{newIdx}" 
                : $"Maze_{MazeNameSuffix}_{newIdx}";
        }

        private int GetMazeDirectoryFilesCount()
        {
            if (!Directory.Exists(MazeDir))
                Directory.CreateDirectory(MazeDir);
            return Directory.GetFiles(MazeDir).Length;
        }

        private void ProcessStage(Action _StateAction, EStage _Stage)
        {
            lb_log.Text = $@"Creating maze {GetMazeDirectoryFilesCount() + 1} of {m_Amount}: {m_Stage}";
            _StateAction?.Invoke();
            pb_creation.Value = Math.Min(pb_creation.Maximum, pb_creation.Value + 33);
            int defaultDelay = 200;
            switch (_Stage)
            {
                case EStage.Ready:
                    Thread.Sleep(defaultDelay + m_Delay);
                    break;
                case EStage.SetShape:
                case EStage.SetSolution:
                    Thread.Sleep(defaultDelay);
                    break;
                case EStage.Generated:
                    break;
            }
        }
        
        private void UpdateSettings()
        {
            var shapeElement = webBrowser1.Document?.GetElementById(ElNames.ShapeDropdown);
            if (shapeElement != null)
            {
                int.TryParse(shapeElement.GetAttribute(AttrValue), out int val);
                m_Shape = (EShape)(val - 1);
            }

            switch (m_Shape)
            {
                case EShape.Rectangular:
                    var styleElement = webBrowser1.Document?.GetElementById(ElNames.StyleDropdown);
                    var widthElement = webBrowser1.Document?.GetElementById(ElNames.Width);
                    var heightElement = webBrowser1.Document?.GetElementById(ElNames.Height);

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
                case EShape.Triangular:
                case EShape.Hexagonal:
                    var sizeElement = webBrowser1.Document?.GetElementById(ElNames.Size(m_Shape));
                    if (sizeElement != null)
                        int.TryParse(sizeElement.GetAttribute(AttrValue), out m_Size);
                    break;
            }
        }
    }

    public static class ElNames
    {
        public const string ShapeDropdown = "ShapeDropDownList";
        public const string StyleDropdown = "S1TesselationDropDownList";
        public const string EParam = "AlgorithmParameter1TextBox";
        public const string RParam = "AlgorithmParameter2TextBox";
        public const string Width = "S1WidthTextBox";
        public const string Height = "S1HeightTextBox";
        public const string InnerSideLength = "S3InnerSideLengthTextBox";

        public static string Size(EShape _Shape)
        {
            switch (_Shape)
            {
                case EShape.Circular:
                    return "S2OuterDiameterTextBox";
                case EShape.Triangular:
                    return "S3SideLengthTextBox";
                case EShape.Hexagonal:
                    return "S4SideLengthTextBox";
                default:
                    return string.Empty;
            }
        }
    }
}