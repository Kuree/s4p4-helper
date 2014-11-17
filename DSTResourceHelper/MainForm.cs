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
 *  s4pi is distributed in the hope that it will be useful,                *
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
            //this.args = new string[] { @"C:\Users\Keyi\Desktop\test.dds" };
            if (this.args == null || this.args.Length != 1)
            {
                MessageBox.Show("Please use it via s4pe");
                this.Close();
                this.Dispose();
                return;
            }
            this.filePath = this.args[0];
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
            
            this.tableLayoutPanel.Controls.Add(ddsPanel);
            this.tableLayoutPanel.SetRow(ddsPanel, 1);
            this.ckbDST.Checked = dst.IsShuffled;

            ddsPanel.DSTLoad(dst.Stream, false);
            ddsPanel.Padding = new System.Windows.Forms.Padding(3);
            ddsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            ddsPanel.Fit = true;
            ddsPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dst == null) return;
            using (SaveFileDialog save = new SaveFileDialog()
            {
                Filter = "DXT Image|*.dds|Portable Network Grapics files|*.png|Grapics Interchange Format files|*.gif|JPEG files|*.jpg|Bitmap files|*.bmp"
            })
            {
                if(save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using(FileStream fs2 = new FileStream(save.FileName, FileMode.Create, FileAccess.Write))
                    {
                        string extension = Path.GetExtension(save.FileName);
                        if (extension.ToLower() == ".dds")
                        {
                            dst.ToDDS().CopyTo(fs2);
                        }
                        else
                        {
                            var ext = Array.IndexOf(new[] { ".png", ".gif", ".jpg", ".bmp", }, extension.ToLower());
                            var fmt = ext >= 0 ? fmts[ext] : System.Drawing.Imaging.ImageFormat.Png;
                            ddsPanel.Image.Save(fs2, fmt);
                        }
                    }
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (dst == null) return;
            using (OpenFileDialog open = new OpenFileDialog()
            {
                Filter = "DXT Image|*.dds|Image files|*.png;*.gif;*.jpg;*.bmp"
            })
            {
                if (open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (FileStream fs2 = new FileStream(open.FileName, FileMode.Open, FileAccess.Read))
                    {
                        string extension = Path.GetExtension(open.FileName);
                        if (extension.ToLower() == ".dds")
                        {
                            this.dst.ImportToDST(fs2);
                            ddsPanel.DSTLoad(dst.Stream, false);
                        }
                        else
                        {
                            ddsPanel.Import(fs2);
                            using(MemoryStream ms = new MemoryStream())
                            {
                                ddsPanel.DDSSave(ms);
                                this.dst.ImportToDST(ms);
                            }
                        }
                        this.ckbDST.Checked = this.dst.IsShuffled;
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!File.Exists(filePath) && fs != null) { MessageBox.Show("File does not exit any more"); return; }
            fs.SetLength(0); // clear old data
            if (this.dst.IsShuffled == ckbDST.Checked)
            {
                dst.Stream.CopyTo(fs);
            }
            else
            {
                if(ckbDST.Checked && (!dst.IsShuffled))
                {
                    using(MemoryStream ms = dst.ToDDS() as MemoryStream)
                    {
                        dst.ImportToDST(ms);
                        dst.Stream.CopyTo(fs);
                    }
                }
                else
                {
                    dst.ToDDS().CopyTo(fs);
                }
            }
            fs.Flush();
            fs.Close();
            this.Close();
            
        }

        static System.Drawing.Imaging.ImageFormat[] fmts = new[] {
                System.Drawing.Imaging.ImageFormat.Png,
                System.Drawing.Imaging.ImageFormat.Gif,
                System.Drawing.Imaging.ImageFormat.Jpeg,
                System.Drawing.Imaging.ImageFormat.Bmp,
            };

       
    }
}
