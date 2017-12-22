using System;
using System.Windows.Forms;

namespace ModOrganizerHelper
{
    public class Program
    {
        [STAThread]
        private static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
