﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DirectOutput;

namespace DirectOutput_Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DirectOutput.Cab.Cabinet C = new DirectOutput.Cab.Cabinet();
            C.AutoConfig();
            DirectOutput.FrontEnd.CabinetInfo CI = new DirectOutput.FrontEnd.CabinetInfo(C);
            CI.Show();
        }
    }
}
