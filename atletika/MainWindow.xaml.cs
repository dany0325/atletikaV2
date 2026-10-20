using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;

namespace Atletika
{
    public class Versenyzo
    {
        public int Rajtszam { get; set; }
        public string Nev { get; set; }
        public string Versenyszam { get; set; }
        public string Nem { get; set; }
        public int Eletkor { get; set; }
        public bool OR { get; set; }
        public bool VR { get; set; } 
        public bool ER { get; set; }
    }

    public partial class MainWindow : Window
    {
        ObservableCollection<Versenyzo> Versenyzok = new ObservableCollection<Versenyzo>();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = Versenyzok;

            ICollectionView nezet = CollectionViewSource.GetDefaultView(Versenyzok);
            nezet.Filter = new Predicate<object>(SzuroFeltetel);

            FrissitLetszam();
        }


        private void Eletkor_Valtozott(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtEletkor != null)
            {
                int kor = (int)sldEletkor.Value;
                txtEletkor.Text = kor + " év";
            }
        }


        private bool SzuroFeltetel(object elem)
        {
            Versenyzo v = (Versenyzo)elem;
            int valasztas = cmbRekordSzuro.SelectedIndex;

            if (valasztas == 1)
            {
                return v.OR;
            }
            if (valasztas == 2)
            {
                return v.VR;
            }
            if (valasztas == 3)
            {
                return v.ER;
            }
            return true;
        }

        private void Szuro_Valtozott(object sender, SelectionChangedEventArgs e)
        {
            if (txtLetszam == null)
            {
                return;
            }

            ICollectionView nezet = CollectionViewSource.GetDefaultView(Versenyzok);
            nezet.Refresh();
            FrissitLetszam();
        }

        private void FrissitLetszam()
        {
            ICollectionView nezet = CollectionViewSource.GetDefaultView(Versenyzok);

            int lathato = 0;
            foreach (object elem in nezet)
            {
                lathato++;
            }

            txtLetszam.Text = "Látható: " + lathato + " / Összesen: " + Versenyzok.Count;
        }


        private bool RajtszamFoglalt(int szam)
        {
            foreach (Versenyzo v in Versenyzok)
            {
                if (v.Rajtszam == szam)
                {
                    return true;
                }
            }
            return false;
        }

        private void Figyelmeztet(string uzenet)
        {
            MessageBox.Show(uzenet, "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
        }


        private void Regisztral_Click(object sender, RoutedEventArgs e)
        {
            string nev = txtNev.Text.Trim();
            if (nev == "")
            {
                Figyelmeztet("A versenyző nevének megadása kötelező.");
                return;
            }
            if (nev.Contains(";") || nev.Contains("|"))
            {
                Figyelmeztet("A névben pontosvessző és függőleges vonal nem használható.");
                return;
            }

            ComboBoxItem kijelolt = cmbVersenyszam.SelectedItem as ComboBoxItem;
            if (kijelolt == null)
            {
                Figyelmeztet("Válasszon versenyszámot.");
                return;
            }
            string versenyszam = kijelolt.Content.ToString();

            string nem = "";
            if (rbFerfi.IsChecked == true)
            {
                nem = "F";
            }
            else if (rbNo.IsChecked == true)
            {
                nem = "N";
            }
            else
            {
                Figyelmeztet("Adja meg a versenyző nemét.");
                return;
            }

            int rajtszam = 0;
            string kert = txtRajtszam.Text.Trim();

            if (kert != "")
            {
                bool szam = int.TryParse(kert, out rajtszam);
                if (szam == false || rajtszam < 1 || rajtszam > 9999)
                {
                    Figyelmeztet("A rajtszám 1 és 9999 közötti egész szám lehet.");
                    return;
                }
                if (RajtszamFoglalt(rajtszam))
                {
                    Figyelmeztet("A kért rajtszám már foglalt. Adjon meg másikat, vagy hagyja üresen a mezőt.");
                    return;
                }
            }
            else
            {
                rajtszam = 0;
                for (int i = 1; i <= 9999; i++)
                {
                    if (RajtszamFoglalt(i) == false)
                    {
                        rajtszam = i;
                        break;
                    }
                }
                if (rajtszam == 0)
                {
                    MessageBox.Show("Nincs több szabad rajtszám.", "Figyelmeztetés",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            Versenyzo uj = new Versenyzo();
            uj.Rajtszam = rajtszam;
            uj.Nev = nev;
            uj.Versenyszam = versenyszam;
            uj.Nem = nem;
            uj.Eletkor = (int)sldEletkor.Value;
            uj.OR = chkOlimpiai.IsChecked == true;
            uj.VR = chkVB.IsChecked == true;
            uj.ER = chkEB.IsChecked == true;

            Versenyzok.Add(uj);
            FrissitLetszam();

            MessageBox.Show("A regisztráció sikeres. Kiosztott rajtszám: " + rajtszam,
                "Regisztráció", MessageBoxButton.OK, MessageBoxImage.Information);

            txtNev.Clear();
            txtRajtszam.Clear();
        }


        private void Mentes_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "CSV-fájl (*.csv)|*.csv|Szövegfájl (*.txt)|*.txt";
            dlg.FilterIndex = 1;
            dlg.FileName = "atletika";
            dlg.AddExtension = false;

            if (dlg.ShowDialog() != true)
            {
                return;
            }

            string kivantKiterjesztes = ".csv";
            if (dlg.FilterIndex == 2)
            {
                kivantKiterjesztes = ".txt";
            }

            string ut = dlg.FileName;
            string kiterjesztes = Path.GetExtension(ut).ToLower();

            if (kiterjesztes == "")
            {
                ut = ut + kivantKiterjesztes;
            }
            else if (kiterjesztes != kivantKiterjesztes)
            {
                Figyelmeztet("A kiválasztott fájltípus (" + kivantKiterjesztes +
                    ") és a fájlnév kiterjesztése (" + kiterjesztes + ") nem egyezik. A mentés nem történt meg.");
                return;
            }

            try
            {
                List<string> sorok;
                if (kivantKiterjesztes == ".csv")
                {
                    sorok = CsvSorokKeszit();
                }
                else
                {
                    sorok = TxtSorokKeszit();
                }

                File.WriteAllLines(ut, sorok, new UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                MessageBox.Show("A mentés sikertelen: " + ex.Message, "Hiba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<string> CsvSorokKeszit()
        {
            List<string> sorok = new List<string>();
            sorok.Add("Rajtszam;Nev;Versenyszam;Nem;Eletkor;OR;VR;ER");

            foreach (Versenyzo v in Versenyzok)
            {
                string sor = v.Rajtszam + ";" + v.Nev + ";" + v.Versenyszam + ";" +
                             v.Nem + ";" + v.Eletkor + ";" +
                             LogikaiSzam(v.OR) + ";" + LogikaiSzam(v.VR) + ";" + LogikaiSzam(v.ER);
                sorok.Add(sor);
            }
            return sorok;
        }

        private List<string> TxtSorokKeszit()
        {
            List<string> sorok = new List<string>();

            for (int i = 0; i < Versenyzok.Count; i++)
            {
                Versenyzo v = Versenyzok[i];

                if (i > 0)
                {
                    sorok.Add("");
                }

                string nemSzoveg = "női";
                if (v.Nem == "F")
                {
                    nemSzoveg = "férfi";
                }

                sorok.Add(v.Nev);
                sorok.Add("A " + v.Eletkor + " éves " + nemSzoveg + " versenyző a(z) " +
                          v.Versenyszam + " versenyszámban indul, rajtszáma " + v.Rajtszam + ". " +
                          "Olimpiai csúccsal " + RendelkezikSzoveg(v.OR) + ", " +
                          "világcsúccsal " + RendelkezikSzoveg(v.VR) + ", " +
                          "Európa-csúccsal " + RendelkezikSzoveg(v.ER) + ".");
            }
            return sorok;
        }

        private int LogikaiSzam(bool ertek)
        {
            if (ertek)
            {
                return 1;
            }
            return 0;
        }

        private string RendelkezikSzoveg(bool ertek)
        {
            if (ertek)
            {
                return "rendelkezik";
            }
            return "nem rendelkezik";
        }


        private void Betoltes_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "CSV-fájl (*.csv)|*.csv";

            if (dlg.ShowDialog() != true)
            {
                return;
            }

            try
            {
                string[] sorok = File.ReadAllLines(dlg.FileName, Encoding.UTF8);

                List<Versenyzo> ideiglenes = new List<Versenyzo>();

                for (int i = 1; i < sorok.Length; i++)
                {
                    if (sorok[i].Trim() == "")
                    {
                        continue;
                    }

                    string[] mezok = sorok[i].Split(';');

                    Versenyzo v = new Versenyzo();
                    v.Rajtszam = int.Parse(mezok[0].Trim());
                    v.Nev = mezok[1].Trim();
                    v.Versenyszam = mezok[2].Trim();
                    v.Nem = mezok[3].Trim();
                    v.Eletkor = int.Parse(mezok[4].Trim());
                    v.OR = mezok[5].Trim() == "1";
                    v.VR = mezok[6].Trim() == "1";
                    v.ER = mezok[7].Trim() == "1";

                    foreach (Versenyzo korabbi in ideiglenes)
                    {
                        if (korabbi.Rajtszam == v.Rajtszam)
                        {
                            MessageBox.Show("A fájlban ismétlődő rajtszám található: " + v.Rajtszam +
                                ". A betöltés megszakadt, a lista nem változott.", "Hiba",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    ideiglenes.Add(v);
                }

                Versenyzok.Clear();
                foreach (Versenyzo v in ideiglenes)
                {
                    Versenyzok.Add(v);
                }
                FrissitLetszam();
            }
            catch (Exception ex)
            {
                MessageBox.Show("A betöltés sikertelen: " + ex.Message, "Hiba",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
