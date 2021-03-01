using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly List<string> m_Violations = new List<string>();
        private int m_Amount, m_Size;
        private int m_Width, m_Height;
        private int m_CurrentIndex;
        private int m_ServerAnswersCount;
        private EShape m_Shape;
        private EStage m_Stage;
        private EStyle m_Style;
        private SemaphoreSlim m_Signal;
        private Timer m_Timer;
        private bool m_CheckStageExecution;
        private readonly Stopwatch m_Stopwatch = new Stopwatch();
        private int m_ServerAnswersCountCheck;
        private bool m_CheckStageExecutionPrev;

        public MazeCreatorForm()
        {
            InitializeComponent();
            webBrowser1.Visible = false;
            pb_creation.Minimum = pb_creation.Value = 0;
            m_Timer = new Timer {Enabled = true};
            m_Timer.Tick += TimerOnTick;
            m_Timer.Start();
        }

        

        private void MazeCreatorForm_Load(object _Sender, EventArgs _E)
        {
            comboBoxShape.SelectedIndex = comboBoxStyle.SelectedIndex = 0;
            CheckComboBoxShape();
        }

        private void GetDataFromUi()
        {
            m_Shape = (EShape)comboBoxShape.SelectedIndex;
            m_Style = (EStyle)comboBoxStyle.SelectedIndex;
            m_Width = Convert.ToInt32(tb_Width.Text);
            m_Height = Convert.ToInt32(tb_Height.Text);
            m_Size = Convert.ToInt32(textBoxDiameter.Text);
            m_Amount = Convert.ToInt32(textBoxAmount.Text);
        }

        private async void buttonCreateMazeDB_Click(object _Sender, EventArgs _E)
        {
            GetDataFromUi();
            pb_creation.Maximum = m_Amount;
            m_CurrentIndex = pb_creation.Value = 0;
            m_Violations.Clear();
            for (var i = 0; i < m_Amount; i++)
            {
                m_CurrentIndex++;
                SetLogCreatingMessage();
                m_Signal = new SemaphoreSlim(0, 1);
                Generate();
                await m_Signal.WaitAsync();
                pb_creation.Value = m_CurrentIndex;
            }
            lb_log.Text = GetCreationCompletedMessage();
            ShowViolationsIfExist();
        }

        private void Generate()
        {
            webBrowser1.Navigate(UriString);
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

        private async Task WaitForDocumentReallyComplete()
        {
            await Task.Run(() =>
            {
                var d = new ReadyStateHandler(GetBrowserState);
                WebBrowserReadyState state = (WebBrowserReadyState) webBrowser1.Invoke(d);
                int tries = 0;
                while (state != WebBrowserReadyState.Complete || tries < 3)
                {
                    state = (WebBrowserReadyState) webBrowser1.Invoke(d);
                    Thread.Sleep(500);
                }
            });
        }

        private delegate WebBrowserReadyState ReadyStateHandler();
        
        private WebBrowserReadyState GetBrowserState()
        {
            return webBrowser1.ReadyState;
        }
        
        private async void DocumentComplete(object _Sender, WebBrowserDocumentCompletedEventArgs _E)
        {
            m_ServerAnswersCount++;

            if (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
                await WaitForDocumentReallyComplete();
            
            if (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
            {
                m_Violations.Add($"webBrowser was not ready to generate {GetMazeName()}");
                m_Stage = EStage.Ready;
                m_Signal.Release();
                return;
            }
            if (m_Stage == EStage.Ready && m_Shape == EShape.Rectangular)
                m_Stage = EStage.SelectedShape;
            switch (m_Stage)
            {
                case EStage.Ready:
                    ProcessStage(StageSelectShape);
                    break;
                case EStage.SelectedShape:
                    ProcessStage(StageSelectMazeSizeAndGenerate);
                    break;
                case EStage.Generate:
                    ProcessStage(StageGenerateWithSolution);
                    break;
                case EStage.GeneratedWithSolution:
                    ProcessStage(StageGetSvg);
                    break;
                default:
                    throw new Exception("Not all EStage types implemented");
            }
        }

        private void StageSelectShape()
        {
            webBrowser1.Navigate(UriString);
            var shapeElement = webBrowser1.Document?.GetElementById("ShapeDropDownList");
            shapeElement?.SetAttribute(AttrValue, ((int) m_Shape + 1).ToString());
            var buttonGenElement = webBrowser1.Document?.GetElementById("GenerateButton");
            buttonGenElement?.InvokeMember(MemberClick);
            m_Stage = EStage.SelectedShape;
        }

        private void StageSelectMazeSizeAndGenerate()
        {
            var buttonGenerateElement = webBrowser1.Document?.GetElementById("GenerateButton");
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
            
            buttonGenerateElement?.InvokeMember(MemberClick);
            m_Stage = EStage.Generate;
        }

        private void StageGenerateWithSolution()
        {
            var showSolutionEl = webBrowser1.Document?.GetElementById("ShowSolutionCheckBox");
            string checkAttr = showSolutionEl?.GetAttribute("checked");
            if (checkAttr == "False")
                showSolutionEl.InvokeMember(MemberClick);
            var buttonGenerateElement = webBrowser1.Document?.GetElementById("GenerateButton");
            buttonGenerateElement?.InvokeMember(MemberClick);
            m_Stage = EStage.GeneratedWithSolution;
        }

        private void StageGetSvg()
        {
            var svgLinkElement = webBrowser1.Document?.GetElementById("MazeDisplay");
            if (svgLinkElement == null)
            {
                m_Violations.Add($"Svg Link does not exist for {GetMazeName()}");
                m_Stage = EStage.Ready;
                m_Signal.Release();
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
                    m_Violations.Add($"Response is empty for {GetMazeName()}");
                    m_Stage = EStage.Ready;        
                    m_Signal.Release();
                    return;
                }
                var reader = new StreamReader( responseStream );
                string reqResult = reader.ReadToEnd();
                var xDoc = XDocument.Parse(reqResult);
                xDoc = ClearSvg(xDoc);
                if (!Directory.Exists(MazeDir))
                    Directory.CreateDirectory(MazeDir);
                xDoc.Save(Path.Combine(MazeDir, $"{GetMazeName()}.svg"));
            }
            m_Stage = EStage.Ready;
            m_Signal.Release();
        }
        
        private void comboBoxShape_SelectedIndexChanged(object _Sender, EventArgs _E) => CheckComboBoxShape();

        private void CheckComboBoxShape()
        {
            bool isRectShape = comboBoxShape.SelectedIndex == 0;
            tb_Width.Enabled = tb_Height.Enabled = isRectShape;
            comboBoxStyle.Enabled = isRectShape;
            textBoxDiameter.Enabled = !isRectShape;
        }

        private void ShowViolationsIfExist()
        {
            if (!m_Violations.Any())
                return;
            File.WriteAllLines("log.txt", m_Violations);
            Process.Start("log.txt");
        }

        private void SetLogCreatingMessage()
        {
            lb_log.Text = $@"Creating maze {m_CurrentIndex} of {m_Amount}: {m_Stage}";
        }

        private string GetCreationCompletedMessage() => 
            $"Creation completed {(m_Violations.Any() ? "with" : "without")} errors!";

        private string GetMazeName()
        {
            int newIdx = Directory.GetFiles(MazeDir).Length + 1;
            return m_Shape == EShape.Rectangular ? 
                $"Maze_{m_Shape}_{m_Style}_{MazeNameSuffix}_{newIdx}" 
                : $"Maze_{MazeNameSuffix}_{newIdx}";
        } 
        
        private string MazeNameSuffix => m_Shape == EShape.Rectangular ? $"{m_Width}_{m_Height}_{m_Style}" : $"{m_Size}";
        private string MazeDir => Path.Combine(
            new KnownFolder(KnownFolderType.Desktop).Path, "Mazes", $"{m_Shape}_{MazeNameSuffix}");

        private void ProcessStage(Action _StateAction)
        {
            Thread.Sleep(500);
            SetLogCreatingMessage();
            _StateAction?.Invoke();
            m_CheckStageExecution = true;
            m_ServerAnswersCountCheck = m_ServerAnswersCount;
        }
        
        private void TimerOnTick(object _Sender, EventArgs _E)
        {
            if (m_CheckStageExecution)
            {
                if (!m_CheckStageExecutionPrev)
                    m_Stopwatch.Restart();

                if (m_Stopwatch.Elapsed.TotalSeconds > 4d)
                {
                    if (m_ServerAnswersCount > m_ServerAnswersCountCheck)
                        return;
                    if (m_Stage == EStage.Ready)
                        return;
                    Generate();
                    m_Stage = EStage.Ready;
                    DocumentComplete(null, null);
                    m_CheckStageExecution = false;
                }
            }

            m_CheckStageExecutionPrev = m_CheckStageExecution;
        }
    }
}