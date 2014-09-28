using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using s4pi.ImageResource;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace ThumbnailResourceHelper
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Helper started");
            if (args.Contains("/import"))
            {
                using (FileStream fs = new FileStream(args[1], FileMode.Open))
                {
                    using (OpenFileDialog open = new OpenFileDialog() { Filter = "PNG File|*.png" })
                    {
                        if (open.ShowDialog() == DialogResult.OK)
                        {
                            using (FileStream fs2 = new FileStream(open.FileName, FileMode.Open))
                            {
                                fs.SetLength(0);
                                fs2.CopyTo(fs);
                                Console.WriteLine("Converting format...");
                                ThumbnailResource t = new ThumbnailResource(1, null);
                                t.Image = new Bitmap(fs2);
                                t.Stream.CopyTo(fs);
                                return;
                            }
                        }
                    }
                }
                
            }
            else if (args.Contains("/export"))
            {
                using (FileStream fs = new FileStream(args[1], FileMode.Open))
                {
                    using (SaveFileDialog save = new SaveFileDialog() { Filter = "PNG File|*.png" })
                    {
                        if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {

                            Console.WriteLine("Converting format...");
                            ThumbnailResource t = new ThumbnailResource(1, fs);
                            t.Image.Save(save.FileName, System.Drawing.Imaging.ImageFormat.Png);
                            return;

                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Some error occurred");
                return;
            }
        }
    }
}
