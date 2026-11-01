using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using TVP_Projekat2_2._0._RentACarDB__1_DataSetTableAdapters;
using System.Text.RegularExpressions;

namespace TVP_Projekat2_2._0
{
    public partial class KlijentForm : Form
    {
        DataGridViewRow selektovanKlijent;
        _RentACarDB__1_DataSet _RentACarDB__1; //ds
        _RentACarDB__1_DataSetTableAdapters.KlijentTableAdapter klijentTableAdapter;
        public KlijentForm(DataGridViewRow klijent)
        {
            InitializeComponent();
            _RentACarDB__1 = new _RentACarDB__1_DataSet();
            klijentTableAdapter = new _RentACarDB__1_DataSetTableAdapters.KlijentTableAdapter();
            selektovanKlijent = klijent;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GreskaLabel.Text = "";
            String telefonPattern = @"^06\d{1}[-.\s]?\d{3}[-.\s]?\d{3,4}$";
            String ime, prezime, adresa, vozacka_kategorija, telefon;
            ime = textBox1.Text.Trim();
            prezime = textBox2.Text.Trim();
            adresa = textBox3.Text.Trim();
            vozacka_kategorija = textBox5.Text.Trim();
            telefon = textBox4.Text.Trim();
            if (string.IsNullOrWhiteSpace(ime) || string.IsNullOrWhiteSpace(prezime) || string.IsNullOrWhiteSpace(adresa) || string.IsNullOrWhiteSpace(telefon) || string.IsNullOrWhiteSpace(vozacka_kategorija))
            {
                GreskaLabel.Text = "Sva polja moraju biti popunjena!";
                return;
            }
            
            if (!Regex.IsMatch(telefon, telefonPattern))
            {
                GreskaLabel.Text = "Polje telefon nije validnog formata";
                return;
            }

            int rez;
            if (selektovanKlijent != null)
            {
                rez = klijentTableAdapter.Update(
                    ime, 
                    prezime, 
                    adresa, 
                    telefon, 
                    vozacka_kategorija, 
                    Convert.ToInt32(selektovanKlijent.Cells["id_klijenta"].Value)
                    );

                if (rez > 0)
                {
                    MessageBox.Show("Uspesno ste izmenili klijenta!");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("GRESKA!!!");
                    return;
                }
            }
            else
            {
                rez = klijentTableAdapter.Insert(
                    ime,
                    prezime,
                    adresa,
                    telefon,
                    vozacka_kategorija
                );

                if (rez > 0)
                {
                    MessageBox.Show("Uspesno ste dodali klijenta!");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("GRESKA!!!");
                    return;
                }
            }
        }

        private void KlijentForm_Load(object sender, EventArgs e)
        {
            if (selektovanKlijent != null)
            {
                textBox1.Text = selektovanKlijent.Cells["ime"].Value.ToString();
                textBox2.Text = selektovanKlijent.Cells["prezime"].Value.ToString();
                textBox3.Text = selektovanKlijent.Cells["adresa"].Value.ToString();
                textBox4.Text = selektovanKlijent.Cells["telefon"].Value.ToString();
                textBox5.Text = selektovanKlijent.Cells["vozacka_kategorija"].Value.ToString();
                button1.Text = "Izmeni";
            }
        }
    }
}
