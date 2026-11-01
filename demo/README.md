# Demo baza

RentACarDB.accdb je nov fajl napravljen iz šeme, ne očišćena kopija privatne baze. Sadrži jedan izmišljeni klijent, jednu kategoriju, dva demo vozila i nijednu rezervaciju.

Za ponovno pravljenje u Visual Studio Developer Command Prompt-u:

```bat
csc /target:exe /platform:x64 /reference:System.Data.dll CreateDemoDatabase.cs
CreateDemoDatabase.exe NovaDemoBaza.accdb
```

Postojeći izlazni fajl se ne prepisuje. Potreban je 64-bitni Access ACE provider.
