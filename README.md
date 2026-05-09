# 🛠️ WinToolkit Pro

![Windows](https://img.shields.io/badge/OS-Windows_10_%7C_11-blue)
![Platform](https://img.shields.io/badge/Platform-.NET_WPF-512BD4)
![License: AGPL v3](https://img.shields.io/badge/License-AGPL_v3-blue.svg)

**WinToolkit Pro** è un'applicazione desktop avanzata scritta in C# (WPF) progettata per semplificare la manutenzione del sistema, il backup e la gestione dei software su Windows. 
Nata come estensione del progetto [Angolo di Windows](https://angolodiwindows.com), offre un'interfaccia grafica moderna e pulita per eseguire operazioni complesse da riga di comando senza dover mai aprire il terminale.

La versione attualmente è BETA, questo per verificare la presenza di bug o malfunzionamenti.

---

## ✨ Funzionalità Principali

* 🧹 **Manutenzione e Ottimizzazione:** Esegui comandi critici come `SFC Scan`, `DISM Restore`, pulizia dei file temporanei, flush del DNS e controllo del disco con un solo clic.
* 💾 **Backup e Sicurezza:** Crea istantaneamente punti di ripristino in background o avvia la creazione di un'immagine di sistema completa.
* 📦 **Installazione Software Silenziosa:** Installa i tuoi programmi preferiti (Browser, Utility, Media) in background utilizzando **Winget** e **Chocolatey**. Nessun fastidioso popup di installazione.
* 🧳 **Software Portabile (Scoop):** Inizializza e gestisci app portatili tramite **Scoop**. I programmi vengono installati nella cartella utente senza richiedere permessi di amministratore o sporcare il registro di sistema.
* 🖥️ **Console Log Integrata:** Controlla l'output dei processi in tempo reale grazie a un terminale a scomparsa integrato direttamente nell'interfaccia.
* 📊 **Analisi del Sistema:** Recupera rapidamente le specifiche di rete (`ipconfig`), i dettagli hardware (`systeminfo`) e la lista degli ultimi aggiornamenti di Windows installati.
* 🌗 **Tema Personalizzabile:** Supporto completo per la modalità Chiara e Scura.

---

![Schermata Home](https://angolodiwindows.com/wp-content/uploads/2026/05/Screenshot-2026-05-09-091819.png)
![Installazione Software](https://angolodiwindows.com/wp-content/uploads/2026/05/Screenshot-2026-05-09-091824.png)
![Stato Sistema e Analisi](https://angolodiwindows.com/wp-content/uploads/2026/05/Screenshot-2026-05-09-091824.png)

## 📂 Script Personalizzati (Modalità Avanzata)

WinToolkit Pro è espandibile! Non sei limitato ai comandi predefiniti.
Puoi aggiungere i tuoi script Batch (`.bat`) o PowerShell (`.ps1`) direttamente nel programma:

1. Vai nella sezione **Manutenzione**.
2. Clicca su **Apri Cartella** (nella sezione Script Personalizzati).
3. Incolla i tuoi file `.bat` o `.ps1` all'interno della cartella `scripts/custom`.
4. Clicca su **Ricarica Script** nell'app: verranno generati automaticamente dei bottoni per avviare i tuoi script con privilegi di Amministratore.

---

## 🚀 Requisiti e Installazione

### Requisiti
* Windows 10 o Windows 11.
* [.NET Desktop Runtime](https://dotnet.microsoft.com/download) installato.
* *(Opzionale ma consigliato)* Connessione internet attiva per il download dei pacchetti software.

### Utilizzo
L'applicazione implementa un sistema di **Auto-Elevazione**. Facendo doppio clic sull'eseguibile, il programma richiederà automaticamente i privilegi di Amministratore (UAC) necessari per le operazioni di manutenzione profonda.

1. Clona il repository o scarica l'ultima release.
2. Avvia `Wintoolkit_.exe`.
3. *(Solo al primo avvio per i software portatili)*: Vai nella scheda **Software Portable** e clicca su **Inizializza Scoop**.

---

## 🛠️ Tecnologie Utilizzate

* **C# / WPF** (Windows Presentation Foundation) per l'interfaccia utente.
* **Gestori di pacchetti integrati:** `winget`, `choco`, `scoop`.
* **PowerShell & CMD** invocati in modalità asincrona (Thread-safe) per evitare il blocco della UI.

---

## ⚠️ Disclaimer

Questo strumento esegue operazioni di sistema a livello amministrativo. Sebbene i comandi inclusi siano standard di Windows, l'autore non si assume alcuna responsabilità per eventuali malfunzionamenti o perdite di dati. Si consiglia di utilizzare la funzione **"Crea Punto Ripristino"** prima di eseguire installazioni massive o riparazioni DISM.

---

### 🌐 Riferimenti

Progetto sviluppato e mantenuto:  **[Angolo di Windows](https://angolodiwindows.com)**.  
Sentiti libero di aprire una *Issue* o inviare una *Pull Request* per suggerire nuove funzionalità!
