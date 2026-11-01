using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TVP_Projekat2_2._0._RentACarDB__1_DataSetTableAdapters;

namespace TVP_Projekat2_2._0
{
    public partial class RezervacijaForm : Form
    {
        DataGridViewRow selektovanoVozilo;
        DataGridViewRow selektovanKlijent;
        DataGridViewRow selektovanaRezervacija;

        _RentACarDB__1_DataSet _RentACarDB__1; //ds
        _RentACarDB__1_DataSetTableAdapters.KlijentTableAdapter klijentTableAdapter;
        _RentACarDB__1_DataSetTableAdapters.RezervacijaTableAdapter rezervacijaTableAdapter;

        DataTable rezervacije;

        DataTable sviPodaciRezervacija;
        public RezervacijaForm(DataGridViewRow vozilo)
        {
            InitializeComponent();
            _RentACarDB__1 = new _RentACarDB__1_DataSet();
            klijentTableAdapter = new _RentACarDB__1_DataSetTableAdapters.KlijentTableAdapter();
            rezervacijaTableAdapter = new _RentACarDB__1_DataSetTableAdapters.RezervacijaTableAdapter();
            selektovanoVozilo = vozilo;
            rezervacije = new DataTable();
            sviPodaciRezervacija = new DataTable();
        }

        private void Rezervacija_Load(object sender, EventArgs e)
        {
            UcitajPodatke();
            DohvatiRezervacije();
            PodesiDatume();

            //Prikaz izabranog vozila
            {
                DataGridViewTextBoxColumn idKategorije = new DataGridViewTextBoxColumn();
                idKategorije.Name = "id_kategorije";
                idKategorije.HeaderText = "id_kategorije";
                dataGridView1.Columns.Add(idKategorije);
                DataGridViewTextBoxColumn nazivKolona = new DataGridViewTextBoxColumn();
                nazivKolona.Name = "naziv";
                nazivKolona.HeaderText = "naziv";
                dataGridView1.Columns.Add(nazivKolona);
                DataGridViewTextBoxColumn godinaProizvodnjeKolona = new DataGridViewTextBoxColumn();
                godinaProizvodnjeKolona.Name = "godina_proizvodnje";
                godinaProizvodnjeKolona.HeaderText = "godina_proizvodnje";
                dataGridView1.Columns.Add(godinaProizvodnjeKolona);
                DataGridViewTextBoxColumn cenaPoSatuKolona = new DataGridViewTextBoxColumn();
                cenaPoSatuKolona.Name = "cena_po_satu";
                cenaPoSatuKolona.HeaderText = "cena_po_satu";
                dataGridView1.Columns.Add(cenaPoSatuKolona);

                dataGridView1.Rows.Add(selektovanoVozilo.Cells["id_kategorije"].Value, selektovanoVozilo.Cells["naziv"].Value.ToString(), selektovanoVozilo.Cells["godina_proizvodnje"].Value, selektovanoVozilo.Cells["cena_po_satu"].Value);
                dataGridView1.ClearSelection();
            }
            //label1.Text = selektovanoVozilo.Cells[2].Value.ToString();
        }

        private void UcitajPodatke()
        {
            klijentTableAdapter.Fill(_RentACarDB__1.Klijent);
            rezervacijaTableAdapter.Fill(_RentACarDB__1.Rezervacija);
            dataGridView2.DataSource = _RentACarDB__1.Klijent;
            DohvatiRezervacije();
            dataGridView2.ClearSelection();
            dataGridView3.ClearSelection();
            selektovanKlijent = null;
            selektovanaRezervacija = null;
            ObrisiRezButton.Enabled = false;
            RezervacijaButton.Enabled = false;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void PodesiDatume()
        {
            dateTimePicker2.Value = DateTime.Now.AddHours(1);
            dateTimePicker1.Value = DateTime.Now;
            //dateTimePicker1.MinDate = DateTime.Now;
            //dateTimePicker2.MinDate = DateTime.Now.AddHours(1);
        }
        public void UporediDatume()
        {
            if ((dateTimePicker2.Value - dateTimePicker1.Value).TotalHours <= 1)
            {
                MessageBox.Show("Datum do mora biti veci od datuma od", "GRESKA");
                dateTimePicker2.Value = dateTimePicker1.Value.AddHours(1);
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if ((dateTimePicker2.Value - dateTimePicker1.Value).TotalHours < 1)
            {
                dateTimePicker2.Value = dateTimePicker1.Value.AddHours(1);
            }
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            if ((dateTimePicker2.Value - dateTimePicker1.Value).TotalHours < 1)
            {
                dateTimePicker1.Value = dateTimePicker2.Value.AddHours(-1);
            }
        }

        private void RezervacijaButton_Click(object sender, EventArgs e)
        {
            DateTime datumOd, datumDo;

            if (selektovanKlijent == null || selektovanoVozilo == null) return;
            datumOd = dateTimePicker1.Value;
            datumDo = dateTimePicker2.Value;
            if ((datumDo - datumOd).TotalHours < 1)
            {
                MessageBox.Show("Najam mora trajati najmanje jedan sat.");
                return;
            }
            // Refresh availability immediately before saving.
            rezervacijaTableAdapter.Fill(_RentACarDB__1.Rezervacija);
            DohvatiRezervacije();
            foreach (DataRow row in rezervacije.Rows)
            {
                DateTime rezDatumOd = Convert.ToDateTime(row["datumVreme_pocetka"]);
                DateTime rezDatumDo = Convert.ToDateTime(row["datumVreme_kraja"]);


                if (ProveriPreklapanje(datumOd, datumDo, rezDatumOd, rezDatumDo)) {
                    MessageBox.Show("Vozilo je vec rezervisano u ovom intervalu");
                    return;
                }
            }            //MessageBox.Show(selektovanoVozilo.Cells["cena_po_satu"].Value.ToString());
            int cena = BookingRules.CalculatePrice(Convert.ToInt32(selektovanoVozilo.Cells["cena_po_satu"].Value), datumOd, datumDo);
            if (MessageBox.Show("Cena: " + cena + ". Naplacuje se svaki zapoceti sat. Potvrditi rezervaciju?",
                "Potvrda rezervacije", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            int rez = rezervacijaTableAdapter.Insert(Convert.ToInt32(selektovanoVozilo.Cells["id_vozila"].Value), Convert.ToInt32(selektovanKlijent.Cells["id_klijenta"].Value), datumOd, datumDo, cena);
            if (rez > 0)
            {
                MessageBox.Show("Uspesno ste dodali rezervaciju!");
                UcitajPodatke();
            }
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                //NACIN 2.
                selektovanKlijent = dataGridView2.SelectedRows[0];
                //NACIN 1.
                //var vozilo = _RentACarDB__1.Vozilo.Where(v => v.id_vozila == Convert.ToInt32(cell.Value)).FirstOrDefault();
                RezervacijaButton.Enabled = true;
                //MessageBox.Show(selektovanKlijent.Cells[2].Value.ToString());
                dataGridView3.ClearSelection();
                ObrisiRezButton.Enabled = false;
                //dataGridView1.DataSource = filtrirano;
            }
        }



        public bool ProveriPreklapanje(DateTime pocetak1, DateTime kraj1, DateTime pocetak2, DateTime kraj2)
        {
            return BookingRules.Overlaps(pocetak1, kraj1, pocetak2, kraj2);
        }

        private void DohvatiRezervacije()
        {
            var redovi = _RentACarDB__1.Rezervacija.Where(v => v.id_vozila == Convert.ToInt32(selektovanoVozilo.Cells["id_vozila"].Value));
            rezervacije = _RentACarDB__1.Rezervacija.Copy();
            rezervacije.Clear();
            foreach (var red in redovi)
            {
                DataRow novi = rezervacije.NewRow();
                for (int i = 0; i < _RentACarDB__1.Rezervacija.Columns.Count; i++)
                    novi[i] = red[i];

                //novi["datumVreme_pocetka"] = red.datumVreme_pocetka;
                //novi["datumVreme_kraja"] = red.datumVreme_kraja;
                rezervacije.Rows.Add(novi);
            }

            dataGridView3.DataSource = rezervacije;
            dataGridView3.ClearSelection();
        }

        private void Rezervacija_Click(object sender, EventArgs e)
        {
            dataGridView2.ClearSelection();
            RezervacijaButton.Enabled = false;
        }

        private void DodajKlijentaButton_Click(object sender, EventArgs e)
        {
            KlijentForm kf = new KlijentForm(selektovanKlijent);
            kf.ShowDialog();
            UcitajPodatke();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (selektovanaRezervacija == null) { ObrisiRezButton.Enabled = false; return; }
            if (MessageBox.Show("Obrisati izabranu rezervaciju?", "Potvrda brisanja",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            int id_rezervacije = Convert.ToInt32(selektovanaRezervacija.Cells["id_rezervacija"].Value);
            int id_vozila = Convert.ToInt32(selektovanaRezervacija.Cells["id_vozila"].Value);
            int id_klijenta = Convert.ToInt32(selektovanaRezervacija.Cells["id_klijenta"].Value);
            int cena = Convert.ToInt32(selektovanaRezervacija.Cells["cena"].Value);
            DateTime pocetak = Convert.ToDateTime(selektovanaRezervacija.Cells["datumVreme_pocetka"].Value);
            DateTime kraj = Convert.ToDateTime(selektovanaRezervacija.Cells["datumVreme_kraja"].Value);

            int rez = rezervacijaTableAdapter.Delete(id_rezervacije,id_vozila,id_klijenta,pocetak,kraj,cena);
            UcitajPodatke();
            if (rez == 0) MessageBox.Show("Rezervacija je vec izmenjena ili obrisana. Podaci su osvezeni.");
            if(rez > 0)
            {
                MessageBox.Show("Uspesno ste obrisali rezervaciju!");
                UcitajPodatke();
            }
        }

        private void dataGridView3_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                selektovanaRezervacija = dataGridView3 .SelectedRows[0];
                ObrisiRezButton.Enabled = true;
                dataGridView2.ClearSelection();
                RezervacijaButton.Enabled = false;
            }
        }
    }
}
