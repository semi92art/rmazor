
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
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonCreateMazeDB = new System.Windows.Forms.Button();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.panel1 = new System.Windows.Forms.Panel();
            this.textBoxMaxHeight = new System.Windows.Forms.TextBox();
            this.textBoxMaxWidth = new System.Windows.Forms.TextBox();
            this.textBoxMinHeight = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxDiameter = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxAmount = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxMinWidth = new System.Windows.Forms.TextBox();
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
            this.buttonCreateMazeDB.Location = new System.Drawing.Point(349, 71);
            this.buttonCreateMazeDB.Name = "buttonCreateMazeDB";
            this.buttonCreateMazeDB.Size = new System.Drawing.Size(75, 23);
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
            this.panel1.Controls.Add(this.textBoxMaxHeight);
            this.panel1.Controls.Add(this.textBoxMaxWidth);
            this.panel1.Controls.Add(this.textBoxMinHeight);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.textBoxDiameter);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.textBoxAmount);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.textBoxMinWidth);
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
            // textBoxMaxHeight
            // 
            this.textBoxMaxHeight.Location = new System.Drawing.Point(349, 46);
            this.textBoxMaxHeight.Name = "textBoxMaxHeight";
            this.textBoxMaxHeight.Size = new System.Drawing.Size(44, 20);
            this.textBoxMaxHeight.TabIndex = 3;
            this.textBoxMaxHeight.Text = "10";
            // 
            // textBoxMaxWidth
            // 
            this.textBoxMaxWidth.Location = new System.Drawing.Point(349, 20);
            this.textBoxMaxWidth.Name = "textBoxMaxWidth";
            this.textBoxMaxWidth.Size = new System.Drawing.Size(44, 20);
            this.textBoxMaxWidth.TabIndex = 3;
            this.textBoxMaxWidth.Text = "10";
            // 
            // textBoxMinHeight
            // 
            this.textBoxMinHeight.Location = new System.Drawing.Point(272, 46);
            this.textBoxMinHeight.Name = "textBoxMinHeight";
            this.textBoxMinHeight.Size = new System.Drawing.Size(44, 20);
            this.textBoxMinHeight.TabIndex = 3;
            this.textBoxMinHeight.Text = "10";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(213, 46);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Height";
            // 
            // textBoxDiameter
            // 
            this.textBoxDiameter.Location = new System.Drawing.Point(72, 73);
            this.textBoxDiameter.Name = "textBoxDiameter";
            this.textBoxDiameter.Size = new System.Drawing.Size(44, 20);
            this.textBoxDiameter.TabIndex = 3;
            this.textBoxDiameter.Text = "20";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 73);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(27, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Size";
            // 
            // textBoxAmount
            // 
            this.textBoxAmount.Location = new System.Drawing.Point(272, 73);
            this.textBoxAmount.Name = "textBoxAmount";
            this.textBoxAmount.Size = new System.Drawing.Size(44, 20);
            this.textBoxAmount.TabIndex = 3;
            this.textBoxAmount.Text = "3";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(213, 73);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Amount";
            // 
            // textBoxMinWidth
            // 
            this.textBoxMinWidth.Location = new System.Drawing.Point(272, 20);
            this.textBoxMinWidth.Name = "textBoxMinWidth";
            this.textBoxMinWidth.Size = new System.Drawing.Size(44, 20);
            this.textBoxMinWidth.TabIndex = 3;
            this.textBoxMinWidth.Text = "10";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(213, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Width";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Style";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Shape";
            // 
            // comboBoxStyle
            // 
            this.comboBoxStyle.FormattingEnabled = true;
            this.comboBoxStyle.Items.AddRange(new object[] {
            "Orthogonal",
            "Sigma",
            "Delta"});
            this.comboBoxStyle.Location = new System.Drawing.Point(72, 46);
            this.comboBoxStyle.Name = "comboBoxStyle";
            this.comboBoxStyle.Size = new System.Drawing.Size(121, 21);
            this.comboBoxStyle.TabIndex = 1;
            // 
            // comboBoxShape
            // 
            this.comboBoxShape.FormattingEnabled = true;
            this.comboBoxShape.Items.AddRange(new object[] {
            "Rectangular",
            "Circular",
            "Triangular",
            "Hexagonal"});
            this.comboBoxShape.Location = new System.Drawing.Point(72, 19);
            this.comboBoxShape.Name = "comboBoxShape";
            this.comboBoxShape.Size = new System.Drawing.Size(121, 21);
            this.comboBoxShape.TabIndex = 1;
            this.comboBoxShape.SelectedIndexChanged += new System.EventHandler(this.comboBoxShape_SelectedIndexChanged);
            // 
            // MazeCreatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 606);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.webBrowser1);
            this.Name = "MazeCreatorForm";
            this.Text = "MazeCreator";
            this.Load += new System.EventHandler(this.MazeCreatorForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCreateMazeDB;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox textBoxMaxWidth;
        private System.Windows.Forms.TextBox textBoxMinWidth;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxStyle;
        private System.Windows.Forms.ComboBox comboBoxShape;
        private System.Windows.Forms.TextBox textBoxMaxHeight;
        private System.Windows.Forms.TextBox textBoxMinHeight;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxAmount;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxDiameter;
        private System.Windows.Forms.Label label6;
    }
}

