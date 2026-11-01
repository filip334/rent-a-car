using System;using System.IO;using System.Reflection;using System.Data.OleDb;using System.Runtime.InteropServices;
class CreateDemoDatabase {
 [STAThread] static int Main(string[] args){try{string file=Path.GetFullPath(args.Length==0?"RentACarDB.accdb":args[0]);if(File.Exists(file))throw new Exception("Target already exists; no files overwritten.");Directory.CreateDirectory(Path.GetDirectoryName(file));string connection="Provider=Microsoft.ACE.OLEDB.12.0;Data Source="+file;Type type=Type.GetTypeFromProgID("ADOX.Catalog",true);object catalog=Activator.CreateInstance(type);try{type.InvokeMember("Create",BindingFlags.InvokeMethod,null,catalog,new object[]{connection});}finally{Marshal.FinalReleaseComObject(catalog);}using(var db=new OleDbConnection(connection)){db.Open();string[] sql={"CREATE TABLE Kategorija (id_kategorije COUNTER PRIMARY KEY, naziv TEXT(255), opis MEMO)",
"CREATE TABLE Klijent (id_klijenta COUNTER PRIMARY KEY, ime TEXT(255), prezime TEXT(255), adresa TEXT(255), telefon TEXT(255), vozacka_kategorija TEXT(255))",
"CREATE TABLE Vozilo (id_vozila COUNTER PRIMARY KEY, id_kategorije INTEGER, naziv TEXT(255), marka TEXT(255), model TEXT(255), godina_proizvodnje INTEGER, cena_po_satu INTEGER)",
"CREATE TABLE Rezervacija (id_rezervacija COUNTER PRIMARY KEY, id_vozila INTEGER, id_klijenta INTEGER, datumVreme_pocetka DATETIME, datumVreme_kraja DATETIME, cena INTEGER)",
"ALTER TABLE Vozilo ADD CONSTRAINT FK_Vozilo_Kategorija FOREIGN KEY (id_kategorije) REFERENCES Kategorija (id_kategorije)",
"ALTER TABLE Rezervacija ADD CONSTRAINT FK_Rez_Vozilo FOREIGN KEY (id_vozila) REFERENCES Vozilo (id_vozila)",
"ALTER TABLE Rezervacija ADD CONSTRAINT FK_Rez_Klijent FOREIGN KEY (id_klijenta) REFERENCES Klijent (id_klijenta)",
"INSERT INTO Kategorija (id_kategorije,naziv,opis) VALUES (1,'Demo gradski','Izmisljeni primer')",
"INSERT INTO Klijent (id_klijenta,ime,prezime,adresa,telefon,vozacka_kategorija) VALUES (1,'Demo','Korisnik','Test adresa','0600000000','B')",
"INSERT INTO Vozilo (id_vozila,id_kategorije,naziv,marka,model,godina_proizvodnje,cena_po_satu) VALUES (1,1,'Demo vozilo 1','Primer','Model A',2020,100)",
"INSERT INTO Vozilo (id_vozila,id_kategorije,naziv,marka,model,godina_proizvodnje,cena_po_satu) VALUES (2,1,'Demo vozilo 2','Primer','Model B',2022,150)"};foreach(string statement in sql){using(var cmd=new OleDbCommand(statement,db))cmd.ExecuteNonQuery();}}Console.WriteLine("Created fresh synthetic demo database.");return 0;}catch(Exception e){Console.Error.WriteLine(e.Message);return 1;}}
}