using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TVP_Projekat2_2._0;
class RegressionTests
{
    static int passed;
    static void Check(bool ok, string name) { if (!ok) throw new Exception(name); Console.WriteLine("PASS " + name); passed++; }
    static object Field(object target, string name) { return target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target); }
    static void Call(object target, string name) { target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null); }
    [STAThread] static int Main(string[] args)
    {
        try
        {
            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test-data-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dataDir);
            File.Copy(args[0], Path.Combine(dataDir, "RentACarDB.accdb"));
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);
            var t = new DateTime(2030, 1, 1, 10, 0, 0);
            Check(!BookingRules.Overlaps(t,t.AddHours(1),t.AddHours(1),t.AddHours(2)), "Adjacent bookings allowed");
            Check(BookingRules.Overlaps(t,t.AddHours(2),t.AddHours(1),t.AddHours(3)), "Overlap rejected");
            Check(BookingRules.Overlaps(t,t.AddHours(4),t.AddHours(1),t.AddHours(2)), "Contained interval rejected");
            Check(BookingRules.CalculatePrice(100,t,t.AddHours(1)) == 100, "Exact hour price");
            Check(BookingRules.CalculatePrice(100,t,t.AddMinutes(61)) == 200, "Started hour price");
            bool invalid = false; try { BookingRules.CalculatePrice(100,t,t.AddMinutes(45)); } catch (ArgumentException) { invalid = true; }
            Check(invalid, "Short rental rejected");
            using (var c = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Path.Combine(dataDir,"RentACarDB.accdb")))
            {
                c.Open();
                Check(c.State == ConnectionState.Open, "Access database opens");
                using (var f = new Form1())
                {
                    Call(f,"UcitajPodatke");
                    Check(((DataGridView)Field(f,"dataGridView1")).Rows.Count > 0, "Main form loads vehicles");
                    var vehicles = (DataTable)((DataGridView)Field(f,"dataGridView1")).DataSource;
                    int vehicleId = Convert.ToInt32(vehicles.Rows[0]["id_vozila"]);
                    using (var tx = c.BeginTransaction()) { using (var cmd = new OleDbCommand("DELETE FROM Rezervacija",c,tx)) cmd.ExecuteNonQuery(); tx.Commit(); }
                    System.Threading.Thread.Sleep(2000);
                    Call(f,"UcitajPodatke");
                    Check(((Label)Field(f,"NajRezLabel")).Text == "NEMA REZERVACIJA", "Empty bookings do not crash");
                    using (var cmd = new OleDbCommand("SELECT TOP 1 id_klijenta FROM Klijent",c))
                    {
                        int client = Convert.ToInt32(cmd.ExecuteScalar());
                        using (var insert = new OleDbCommand("INSERT INTO Rezervacija (id_vozila,id_klijenta,datumVreme_pocetka,datumVreme_kraja,cena) VALUES (?,?,?,?,?)", c))
                        {
                            insert.Parameters.AddWithValue("v",vehicleId); insert.Parameters.AddWithValue("k",client);
                            insert.Parameters.AddWithValue("s",t); insert.Parameters.AddWithValue("e",t.AddHours(1)); insert.Parameters.AddWithValue("p",100);
                            Check(insert.ExecuteNonQuery()==1,"Booking insert on disposable database");
                        }
                    }
                    System.Threading.Thread.Sleep(2000);
                    Call(f,"UcitajPodatke");
                    var adjacent = f.FiltrirajVozila(0,int.MaxValue,t.AddHours(1),t.AddHours(2),-1);
                    Check(adjacent.Select("id_vozila = " + vehicleId).Length == 1,"Search includes adjacent availability");
                    var overlap = f.FiltrirajVozila(0,int.MaxValue,t,t.AddHours(1),-1);
                    Check(overlap.Select("id_vozila = " + vehicleId).Length == 0,"Search excludes booked vehicle");
                    var grid = (DataGridView)Field(f,"dataGridView1");
                    using (var reservation = new RezervacijaForm(grid.Rows[0]))
                    {
                        ((Button)Field(reservation,"ObrisiRezButton")).Enabled=true;
                        Call(reservation,"UcitajPodatke");
                        Check(!((Button)Field(reservation,"ObrisiRezButton")).Enabled,"Reload disables delete");
                        reservation.GetType().GetMethod("button1_Click",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(reservation,new object[]{null,EventArgs.Empty});
                        Check(true,"Delete with no selection is safe");
                    }
                }
                using(var stats = new StatistikaForm())
                {
                    Call(stats,"RefreshStatistics"); Call(stats,"RefreshStatistics");
                    Check(((DataGridView)Field(stats,"dataGridView1")).Rows.Count > 0,"Statistics refresh");
                    using (var tx = c.BeginTransaction()) { using (var cmd = new OleDbCommand("DELETE FROM Rezervacija",c,tx)) cmd.ExecuteNonQuery(); tx.Commit(); }
                    System.Threading.Thread.Sleep(2000);
                    Call(stats,"RefreshStatistics");
                    Check(((DataGridView)Field(stats,"dataGridView1")).Rows.Count == 0,"Empty statistics");
                }
            }
            Console.WriteLine("TOTAL PASS: " + passed);
            return 0;
        }
        catch(Exception e) { Console.WriteLine(e.ToString()); return 1; }
    }
}