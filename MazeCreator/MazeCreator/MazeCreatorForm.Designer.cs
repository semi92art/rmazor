
namespace MazeCreator
{
    partial class MazeCreatorForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonCreateMazeDB = new System.Windows.Forms.Button();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.tb_cookie = new System.Windows.Forms.TextBox();
            this.lb_log = new System.Windows.Forms.Label();
            this.pb_creation = new System.Windows.Forms.ProgressBar();
            this.tb_Height = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxDiameter = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxAmount = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tb_Width = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxStyle = new System.Windows.Forms.ComboBox();
            this.comboBoxShape = new System.Windows.Forms.ComboBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCreateMazeDB
            // 
            this.buttonCreateMazeDB.Location = new System.Drawing.Point(363, 7);
            this.buttonCreateMazeDB.Name = "buttonCreateMazeDB";
            this.buttonCreateMazeDB.Size = new System.Drawing.Size(131, 46);
            this.buttonCreateMazeDB.TabIndex = 0;
            this.buttonCreateMazeDB.Text = "Create";
            this.buttonCreateMazeDB.UseVisualStyleBackColor = true;
            this.buttonCreateMazeDB.Click += new System.EventHandler(this.buttonCreateMazeDB_Click);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Location = new System.Drawing.Point(13, 134);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(500, 460);
            this.webBrowser1.TabIndex = 1;
            this.webBrowser1.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.DocumentComplete);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.tb_cookie);
            this.panel1.Controls.Add(this.lb_log);
            this.panel1.Controls.Add(this.pb_creation);
            this.panel1.Controls.Add(this.tb_Height);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.textBoxDiameter);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.textBoxAmount);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.tb_Width);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.comboBoxStyle);
            this.panel1.Controls.Add(this.comboBoxShape);
            this.panel1.Controls.Add(this.buttonCreateMazeDB);
            this.panel1.Location = new System.Drawing.Point(13, 13);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(500, 109);
            this.panel1.TabIndex = 2;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 60);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Cookie";
            // 
            // tb_cookie
            // 
            this.tb_cookie.Location = new System.Drawing.Point(46, 57);
            this.tb_cookie.Name = "tb_cookie";
            this.tb_cookie.Size = new System.Drawing.Size(210, 20);
            this.tb_cookie.TabIndex = 6;
            // 
            // lb_log
            // 
            this.lb_log.AutoSize = true;
            this.lb_log.Location = new System.Drawing.Point(262, 83);
            this.lb_log.Name = "lb_log";
            this.lb_log.Size = new System.Drawing.Size(112, 13);
            this.lb_log.TabIndex = 5;
            this.lb_log.Text = "Press \"Create\" to start";
            // 
            // pb_creation
            // 
            this.pb_creation.Location = new System.Drawing.Point(3, 83);
            this.pb_creation.Name = "pb_creation";
            this.pb_creation.Size = new System.Drawing.Size(253, 15);
            this.pb_creation.TabIndex = 4;
            // 
            // textBoxMinHeight
            // 
            this.tb_Height.Location = new System.Drawing.Point(212, 33);
            this.tb_Height.Name = "tb_Height";
            this.tb_Height.Size = new System.Drawing.Size(44, 20);
            this.tb_Height.TabIndex = 3;
            this.tb_Height.Text = "10";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(173, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Height";
            // 
            // textBoxDiameter
            // 
            this.textBoxDiameter.Location = new System.Drawing.Point(313, 7);
            this.textBoxDiameter.Name = "textBoxDiameter";
            this.textBoxDiameter.Size = new System.Drawing.Size(44, 20);
            this.textBoxDiameter.TabIndex = 3;
            this.textBoxDiameter.Text = "20";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(264, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(27, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Size";
            // 
            // textBoxAmount
            // 
            this.textBoxAmount.Location = new System.Drawing.Point(313, 32);
            this.textBoxAmount.Name = "textBoxAmount";
            this.textBoxAmount.Size = new System.Drawing.Size(44, 20);
            this.textBoxAmount.TabIndex = 3;
            this.textBoxAmount.Text = "3";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(264, 36);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Amount";
            // 
            // textBoxMinWidth
            // 
            this.tb_Width.Location = new System.Drawing.Point(212, 7);
            this.tb_Width.Name = "tb_Width";
            this.tb_Width.Size = new System.Drawing.Size(44, 20);
            this.tb_Width.TabIndex = 3;
            this.tb_Width.Text = "10";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(173, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Width";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Style";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Shape";
            // 
            // comboBoxStyle
            // 
            this.comboBoxStyle.FormattingEnabled = true;
            this.comboBoxStyle.Items.AddRange(new object[] {"Orthogonal", "Sigma", "Delta"});
            this.comboBoxStyle.Location = new System.Drawing.Point(46, 32);
            this.comboBoxStyle.Name = "comboBoxStyle";
            this.comboBoxStyle.Size = new System.Drawing.Size(121, 21);
            this.comboBoxStyle.TabIndex = 1;
            // 
            // comboBoxShape
            // 
            this.comboBoxShape.FormattingEnabled = true;
            this.comboBoxShape.Items.AddRange(new object[] {"Rectangular", "Circular", "Triangular", "Hexagonal"});
            this.comboBoxShape.Location = new System.Drawing.Point(46, 5);
            this.comboBoxShape.Name = "comboBoxShape";
            this.comboBoxShape.Size = new System.Drawing.Size(121, 21);
            this.comboBoxShape.TabIndex = 1;
            this.comboBoxShape.SelectedIndexChanged += new System.EventHandler(this.comboBoxShape_SelectedIndexChanged);
            // 
            // MazeCreatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 134);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.webBrowser1);
            this.Name = "MazeCreatorForm";
            this.Text = "MazeCreator";
            this.Load += new System.EventHandler(this.MazeCreatorForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tb_cookie;

        private System.Windows.Forms.Label lb_log;

        private System.Windows.Forms.ProgressBar pb_creation;

        #endregion

        private System.Windows.Forms.Button buttonCreateMazeDB;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox tb_Width;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxStyle;
        private System.Windows.Forms.ComboBox comboBoxShape;
        private System.Windows.Forms.TextBox tb_Height;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxAmount;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxDiameter;
        private System.Windows.Forms.Label label6;
    }
}

