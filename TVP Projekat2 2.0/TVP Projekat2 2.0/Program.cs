using System;
using System.Data.OleDb;
using System.IO;
using System.Windows.Forms;
namespace TVP_Projekat2_2._0
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain.CurrentDomain.SetData("DataDirectory", Application.StartupPath);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) => ShowError(e.Exception);
            try
            {
                if (!File.Exists(Path.Combine(Application.StartupPath, "RentACarDB.accdb")))
                    throw new FileNotFoundException("Nedostaje RentACarDB.accdb u folderu aplikacije.");
                using (var connection = new OleDbConnection(Properties.Settings.Default.RentACarDB__1_ConnectionString))
                    connection.Open();
                Application.Run(new Form1());
            }
            catch (Exception ex) { ShowError(ex); }
        }
        private static void ShowError(Exception ex)
        {
            MessageBox.Show("Operacija nije zavrsena. Proverite bazu, dozvolu za upis i Access drajver.\n\n" + ex.Message,
                "Rent-a-car - greska", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
