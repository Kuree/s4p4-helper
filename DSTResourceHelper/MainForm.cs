/***************************************************************************
 *  Copyright (C) 2014 by Keyi Zhang                                       *
 *  kz005@bucknell.edu                                                     *
 *                                                                         *
 *  This file is part of the Sims 4 Package Interface (s4pi)               *
 *                                                                         *
 *  s4pi is free software: you can redistribute it and/or modify           *
 *  it under the terms of the GNU General Public License as published by   *
 *  the Free Software Foundation, either version 3 of the License, or      *
 *  (at your option) any later version.                                    *
 *                                                                         *
 *  s3pi is distributed in the hope that it will be useful,                *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with s4pi.  If not, see <http://www.gnu.org/licenses/>.          *
 ***************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using s4pi.Helpers;
using s4pi.ImageResource;
using s4pi.Interfaces;
using System.IO;

namespace DSTResourceHelper
{
    public partial class MainForm : Form
    {
        private string[] args;
        private string filePath;
        private FileStream fs;
        DSTResource dst;
        static bool channel1 = true, channel2 = true, channel3 = true, channel4 = true, invertch4 = false;
        DDSPanel ddsPanel;

        public MainForm(string[] args)
        {
            InitializeComponent();
            this.args = args;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (this.args == null || this.args.Length < 2)
            {
                MessageBox.Show("Please use it via s4pe");
                this.Close();
            }
            this.filePath = this.args[1];
            if (!File.Exists(filePath)) this.Close();
            fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            dst = new s4pi.ImageResource.DSTResource(1, fs);
            ddsPanel = new DDSPanel()
            {
                Fit = true,
                Channel1 = channel1,
                Channel2 = channel2,
                Channel3 = channel3,
                Channel4 = channel4,
                InvertCh4 = invertch4,
                Margin = new Padding(3),
            };
            ddsPanel.Channel1Changed += (sn, e2) => channel1 = ddsPanel.Channel1;
            ddsPanel.Channel2Changed += (sn, e2) => channel2 = ddsPanel.Channel2;
            ddsPanel.Channel3Changed += (sn, e2) => channel3 = ddsPanel.Channel3;
            ddsPanel.Channel4Changed += (sn, e2) => channel4 = ddsPanel.Channel4;
            ddsPanel.InvertCh4Changed += (sn, e2) => invertch4 = ddsPanel.InvertCh4;
            
            this.panel.Controls.Add(ddsPanel);
            ddsPanel.DSTLoad(dst.Stream, false);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dst == null) return;
            using (SaveFileDialog save = new SaveFileDialog()
            {
                Filter = "DXT Image|*.dds"
            })
            {
                if(save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using(FileStream fs2 = new FileStream(save.FileName, FileMode.Create, FileAccess.Write))
                    {
                        dst.ToDDS().CopyTo(fs2);
                    }
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (dst == null) return;
            using (OpenFileDialog open = new OpenFileDialog()
            {
                Filter = "DXT Image|*.dds"
            })
            {
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (FileStream fs2 = new FileStream(open.FileName, FileMode.Open, FileAccess.Read))
                    {
                        this.dst.ImportToDST(fs2);
                        ddsPanel.DSTLoad(dst.Stream, false);
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!File.Exists(filePath) && fs != null) { MessageBox.Show("File does not exit any more"); return; }
            fs.SetLength(0); // clear old data
            dst.Stream.CopyTo(fs);
            fs.Flush();
            fs.Close();
            this.Close();
            
        }

    }
}
