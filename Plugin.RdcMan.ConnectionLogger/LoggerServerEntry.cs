using System;
using System.Windows.Forms;

namespace RdcPlgTest
{
    public partial class LoggerServerEntry : Form
    {
        public LoggerServerEntry()
        {
            InitializeComponent();
        }

        public string Value
        {
            get => textBox1.Text;
            set => textBox1.Text = value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
