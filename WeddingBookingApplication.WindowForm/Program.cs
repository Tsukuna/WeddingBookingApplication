using WeddingBookingApplication.WindowForm.Forms;

namespace WeddingBookingApplication.WindowForm
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainShellForm());
        }
    }
}