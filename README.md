<div align="center">

# ⚡ FILE SURGE

<img width="1578" height="933" alt="Screenshot 2026-09-30 080711" src="https://github.com/user-attachments/assets/8383d976-d328-4ed8-a7a9-cf41f573299c" />


### PUMP • PROCESS • CONTROL

**A cyber-terminal style Windows utility for file size manipulation, analysis, and batch processing.**

[![Version](https://img.shields.io/badge/version-1.0.0-00FF9C?style=flat-square)](https://github.com/MiSFiT-SecuRiTY/FileSurge/releases)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-00BFA6?style=flat-square)](#-requirements)
[![.NET](https://img.shields.io/badge/.NET-10.0-00FF9C?style=flat-square)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-00BFA6?style=flat-square)](LICENSE)

**[⬇️ Download Latest Release](https://github.com/MiSFiT-SecuRiTY/FileSurge/releases/latest)**

</div>

---

## 📖 Table of Contents

- [What Is File Surge?](#-what-is-file-surge)
- [Why Do People Use File Surge?](#-why-do-people-use-file-surge)
- [Why Hackers and Security Researchers Use It](#-why-hackers-and-security-researchers-use-it)
- [⚠️ About the 144 MB Size](#️-about-the-144-mb-size)
- [⚠️ About Windows SmartScreen Warnings](#️-about-windows-smartscreen-warnings)
- [Features](#-features)
- [How It Works](#-how-it-works)
- [Download](#-download)
- [Usage Guide](#-usage-guide)
  - [Pump Tab](#-pump-tab)
  - [Batch Tab](#-batch-tab)
  - [Queue Tab](#-queue-tab)
  - [Analysis Tab](#-analysis-tab)
  - [Hex View Tab](#-hex-view-tab)
  - [Tools Tab](#-tools-tab)
  - [Settings Tab](#-settings-tab)
- [Padding Methods Explained](#-padding-methods-explained)
- [Target Size Modes Explained](#-target-size-modes-explained)
- [Integrity Verification](#-integrity-verification)
- [Safety and Ethics](#-safety-and-ethics)
- [Requirements](#-requirements)
- [Building From Source](#-building-from-source)
- [Project Structure](#-project-structure)
- [Technology Stack](#-technology-stack)
- [FAQ](#-faq)
- [License](#-license)

---

## 🎯 What Is File Surge?

**File Surge** is a Windows desktop utility that increases the **physical size of files** by appending configurable padding data to them — while **preserving every original byte unchanged**.

Think of it as a file "pump" — you feed a file in, specify how big you want it to become, choose what kind of filler bytes to use, and the tool streams out a new file that is exactly that size. The original data sits untouched at the front of the new file. The rest is padding you control.

It also doubles as a lightweight **file analysis workbench** — hashes, hex viewer, entropy estimation, batch processing, disk monitoring, and more — all wrapped in a dark cyber-terminal interface built for people who live in command lines.

Everything runs **locally**. No cloud. No telemetry. No internet required after download.

---

## 🤔 Why Do People Use File Surge?

File padding has real, legitimate uses across many fields:

| Use Case | Why Padding Helps |
|---|---|
| **Testing storage systems** | Verify how a NAS, backup tool, or file server handles files of specific sizes |
| **Testing upload limits** | Simulate large files against APIs, email gateways, or form uploads |
| **Benchmarking transfer speeds** | Generate consistent test files of known sizes |
| **Simulating disk usage** | Fill drives to specific thresholds for testing |
| **Masking real file sizes (privacy)** | Make a small file indistinguishable from a large one at the filesystem level |
| **Feeding test data to pipelines** | File processing pipelines that require minimum sizes |
| **CTF challenges** | File size manipulation is a common CTF technique |
| **Malware analysis sandbox prep** | Generate files with known padding for testing EDR rules |
| **Forensic tool testing** | Verify tools correctly handle padding vs. real data |
| **Antivirus evasion research** | Understanding how padding affects scanning (defensive research only) |
| **Steganography coursework** | Academic exercises in concealing data within padding |
| **Fuzzing and corpus building** | Create size-varied seed files for fuzzers |

**No legitimate use of File Surge requires it to execute, modify, or inject code into the file.** It only appends bytes to the end. The original content is byte-for-byte intact.

---

## 🕵️ Why Hackers and Security Researchers Use It

Let's be clear about the two very different groups:

### 🛡️ Security Researchers and Penetration Testers (Legitimate)

- **Payload size testing** — understanding how payload size affects delivery mechanisms during **authorized** penetration tests
- **Signature evasion research** — studying whether AV products hash only the header, the whole file, or a fixed size — **defensively**
- **Red team simulations** — building test files that mimic real-world attack artifacts so blue teams can tune detection
- **Incident response training** — creating known-bad sample files with padding so analysts learn to spot anomalies
- **Forensic tool validation** — proving that tools like `binwalk`, `foremost`, or custom parsers still correctly identify embedded content when padding is present
- **CTF competitions** — challenge authors use it to create specific file-size puzzles
- **Malware sandbox setup** — isolating how sandboxed environments respond to different file sizes
- **Reverse engineering coursework** — showing students that not all file bytes are meaningful

### ❌ Malicious Actors (What File Surge Is NOT For)

File Surge **cannot** and **will not**:

- Bypass antivirus or EDR
- Evade file scanners
- Inject code into executables
- Modify PE headers or section tables
- Hide malicious payloads
- Establish persistence
- Execute any file it touches

The tool has **no capability** to do any of these things. It only appends bytes. Nothing else. Anyone claiming otherwise has misunderstood how it works.

### The Honest Reality

Padding a file **can** change its hash and its size — that's a fact of computing, not a feature unique to File Surge. Every text editor, every compression tool, every archival utility can change a file's hash. File Surge just does it deliberately and transparently.

**We do not encourage or condone using this tool for malicious purposes.** If you are a security researcher, use it within your authorized scope. If you're not, use it for the legitimate purposes listed above.

---

## ⚠️ About the 144 MB Size

The first thing most people notice about File Surge is that its download is **~144 MB** — much larger than a typical small utility.

**This is intentional, and here's exactly why:**

### Self-Contained Deployment

File Surge is published as a **self-contained single-file executable**. That means:

- The **entire .NET 10 runtime** is bundled inside `FileSurge.exe`
- The **entire WPF framework** is bundled inside
- **All required base class libraries** are bundled inside
- The **C# code and resources** are bundled inside

**Nothing else is required to run it.**

### Why This Is the Right Choice

| Approach | Size | User Experience |
|---|---|---|
| **Self-contained (File Surge)** | ~144 MB | Download → double-click → runs. Done. |
| Framework-dependent | ~3 MB | Download → run → "**This app requires .NET 10. Click here to install it.**" → download 70 MB runtime → install → restart → run |
| Framework-dependent + bundled installer | ~5 MB + auto-download | Download → run → installer phones home → downloads 70 MB runtime → installs → runs (requires internet) |

**The 144 MB is a deliberate tradeoff for a zero-friction experience.** You download one file and it works — forever — on any Windows 10/11 machine, online or offline, with or without .NET installed.

### What You Get for the Size

- ✅ No .NET installation required
- ✅ No runtime downloads
- ✅ No internet needed after download
- ✅ Works on locked-down corporate PCs where .NET installs are blocked
- ✅ Works on machines you don't have admin rights on
- ✅ No version conflicts with any .NET already installed
- ✅ No registry pollution — it's fully portable

### Can It Be Smaller?

Yes, but each option has a cost:

| Method | Result | Tradeoff |
|---|---|---|
| Framework-dependent publish | ~3 MB | User must install .NET 10 manually |
| Trim unused code | ~30% smaller | **WPF crashes at runtime** when trimmed — not supported |
| Native AOT | ~20 MB | **WPF does not support AOT** — not possible |
| Rewrite in WinForms / Avalonia | ~15 MB | Complete rewrite of the entire app |

**144 MB is the correct, standard choice** for a self-contained WPF application. Most WPF apps on GitHub ship the same way (e.g., popular WPF tools ship at 60–150 MB).

### What About the Installed Size?

There is **no installation**. `FileSurge.exe` is a single portable file. Your disk usage goes up by exactly 144 MB, and deleting the file removes every trace.

---

## ⚠️ About Windows SmartScreen Warnings

When you first launch `FileSurge.exe`, Windows may show one or more of these:

### Warning 1 — "Windows protected your PC"

Windows protected your PC
Microsoft Defender SmartScreen prevented an unrecognized app from starting.


**This is normal** and happens because File Surge is **not code-signed**.

### Why Isn't It Code-Signed?

Code-signing certificates cost **$200–400 per year** and require hardware tokens or cloud-based signing services (e.g., Azure Trusted Signing at ~$10/month). This is a free hobby project — paying for a certificate isn't justified.

**Every unsigned Windows app shows this warning.** Notepad++ (early versions), Rufus (older versions), CPU-Z, and thousands of other well-known free utilities have the same warning. It is **not** a sign that the app is malicious. It's a sign that Microsoft hasn't seen the file 3 million times before.

### How to Run It Anyway

1. Click **More info** (a link in the dialog)
2. Click **Run anyway**

Windows remembers your choice for this file.

### Warning 2 — "Do you want to allow this app to make changes?"

This is a **UAC prompt** for admin rights. **File Surge does not need admin rights to run.**

- If you see this warning, it's likely because you saved the `.exe` to a folder Windows restricts (e.g., `C:\Program Files\`)
- Move `FileSurge.exe` to `C:\Users\<You>\Desktop\` or `C:\Tools\` — then no UAC prompt appears
- Or just click **Yes** — File Surge doesn't touch the system either way

### Warning 3 — Antivirus False Positive

Some antivirus products (rarely, but occasionally) flag unsigned single-file .NET executables as suspicious because they **self-extract** at runtime. This is a known behavior of .NET single-file publishing, not a File Surge issue.

**If this happens:**
- Verify the SHA-256 hash against the release page
- Add an exclusion in your AV
- Or build from source yourself (see [Building From Source](#-building-from-source))

### How to Verify the Download Is Genuine

Every release lists the **SHA-256** hash. To verify:

1. Open **PowerShell**
2. Run: Get-FileHash "C:\path\to\FileSurge.exe" -Algorithm SHA256
3. Compare the output to the hash on the release page

If they match, you have the exact file that was uploaded.

### Building Trust

You don't have to trust a random `.exe` on the internet. You can:

- **Read the source code** — it's all here, MIT licensed
- **Build it yourself** — see [Building From Source](#-building-from-source)
- **Run it in a sandbox** — Windows Sandbox, a VM, or a test machine
- **Check the network** — File Surge makes **zero network calls**. Run it with Wireshark open and you'll see no traffic.

---

## ✨ Features

### ⚡ Core Pumping Engine
- Stream-based file processing — **never loads whole files into RAM**
- Handles files larger than available memory (tested to 100+ GB)
- Preserves original bytes with byte-for-byte accuracy
- Optional SHA-256 integrity verification of the original region post-write
- Cancellable mid-operation with automatic cleanup of partial output
- Pre-checks disk space before starting
- Refuses to overwrite unless explicitly enabled

### 🎯 Flexible Target Sizing
- **Exact size** — make the final file a specific byte count
- **Add to current** — grow the file by a fixed amount
- **Minimum size** — only enlarge if smaller than a threshold
- Byte-accurate 64-bit arithmetic with overflow protection
- Units: Bytes, KB, MB, GB, TB (binary — 1024-based)

### 🎲 Five Padding Methods
1. **Zero Padding** — max compatibility, fastest write, compresses to nothing
2. **Random Bytes** — pseudo-random, fast, non-cryptographic
3. **Cryptographically Secure Random** — uses `RandomNumberGenerator`, indistinguishable from real data
4. **Repeating Pattern** — cyclic byte pattern like `AA BB CC DD`
5. **Custom Pattern** — arbitrary hex byte sequence

### 📦 Batch Processing
- Add multiple files at once
- Add whole folders recursively
- Common target size for the whole batch
- Per-file status indicators
- Failure isolation — one bad file doesn't stop the batch

### ☷ Queue Manager
- Ordered processing list
- Reorder via ▲ / ▼ buttons
- Remove individual items
- Start all / cancel

### 🔍 File Analysis
- Filename, extension, type detection
- Size, creation, modification timestamps
- SHA-256 hash
- SHA-512 hash
- Shannon entropy estimate (bits/byte, sampled from first 1 MiB)
- Read / write permission check
- Full Windows file attributes

### ▤ Hex Viewer
- 16 bytes per line with offset column
- ASCII representation on the right
- Page-through navigation (32 lines per page)
- Go-to-offset by hex value
- Hex pattern search
- Only loads the visible window — safe on huge files

### 🛠️ Tools
- **Size calculator** — current size → target size → padding bytes
- **Hash calculator** — MD5, SHA-1, SHA-256, SHA-384, SHA-512
- **Disk space monitor** — free / total for every mounted drive

### ⚙️ Settings
- Dark / light theme toggle (stored)
- Animations on / off
- Compact mode
- I/O buffer size (default 1 MiB)
- Verify after processing
- Default padding method
- Enable logs + log level
- Confirm overwrite / confirm large files
- Keep originals by default
- Show startup animation

### 🎨 Cyber-Terminal UI
- Dark near-black background (`#030708`)
- Neon green primary accent (`#00FF9C`)
- Cyan-teal secondary accent (`#00BFA6`)
- Red destructive actions (`#FF3344`)
- Monospace technical typography (Cascadia / Consolas)
- Fixed 220 px sidebar with green left-bar active indicator
- Live status bar with disk + RAM readouts
- Console/log panel with color-coded `[INFO] [WARNING] [ERROR] [SUCCESS]` levels

---

## ⚙️ How It Works

## ⚙️ How It Works

File Surge uses a **stream-based pipeline**. Nothing is buffered entirely in RAM.

```text
SOURCE FILE
    │
    │  FileStream
    │  Async Read
    ▼
┌─────────────────────┐
│  READ 1 MiB CHUNK   │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│   OUTPUT WRITER     │
│     FileStream      │
└──────────┬──────────┘
           │
           ├──────────────► Copy original bytes exactly as-is
           │
           │
           └──────────────► Append padding bytes
                            │
                            │ PaddingGenerator
                            ▼
                    ┌─────────────────┐
                    │ WRITE 1 MiB     │
                    │ CHUNKS          │
                    └────────┬────────┘
                             │
                             ▼
                       DESTINATION FILE
```

### Processing Flow

```text
Input File
    │
    ▼
File Analyzer
    │
    ├── File Size
    ├── File Type
    └── File Metadata
    │
    ▼
Size Calculator
    │
    ▼
Calculate Required Padding
    │
    ▼
Pump Engine
    │
    ├── Read Original Data
    │
    ├── Write Original Data
    │
    └── Generate + Write Padding
    │
    ▼
Integrity Verification
    │
    ▼
Completed File
```


### Step-by-Step Process

1. **Validate** — confirm source exists and is readable
2. **Measure** — read source size
3. **Compute** — padding = target − current (never negative)
4. **Check** — verify destination drive has enough free space
5. **Hash source** (optional) — SHA-256 of the source file
6. **Open streams** — source for read, destination for create
7. **Copy** — read source in 1 MiB chunks, write to destination
8. **Pad** — generate padding bytes, write to destination
9. **Flush** — force OS buffers to disk
10. **Verify** (optional) — hash first N bytes of output, compare to source hash
11. **Report** — log success, size, duration, verification status

### Progress Reporting
- Sampled every 512 KiB of progress
- Reports bytes processed, current speed, average speed, ETA
- Delivered to UI via `IProgress<T>` — never blocks the UI thread

### Cancellation
- Uses `CancellationToken` throughout
- Cancellation is cooperative — checked between chunks
- On cancellation, partial output is deleted

### Memory Profile
- Constant memory regardless of file size (~2 MiB working set for buffers)
- Tested on 100+ GB files without issue

---

## 📥 Download

**[⬇️ Download FileSurge.exe](https://github.com/MiSFiT-SecuRiTY/FileSurge/releases/latest)** — ~144 MB

- Portable single file — no install
- Works on Windows 10 / 11 (64-bit)
- Self-contained — no .NET install required

### First Launch

Read the sections above:
- [About the 144 MB Size](#️-about-the-144-mb-size)
- [About Windows SmartScreen Warnings](#️-about-windows-smartscreen-warnings)

**Short version:** click **More info → Run anyway**.

---

## 🚀 Usage Guide

### ⚡ PUMP Tab

The main screen for single-file operations.

**Steps:**

1. Drag a file onto the drop zone, or click **BROWSE**
2. Choose **Target Size** mode:
   - **Set exact size** — final file will be exactly this size
   - **Add to current size** — final = current + this amount
   - **Make it larger than** — only grows if current is smaller
3. Enter the size value and select the unit (B / KB / MB / GB / TB)
4. Choose **Padding Method** (see below)
5. Configure **Output Options**:
   - **Keep original file** — source stays on disk after pumping
   - **Overwrite if exists** — allow replacing an existing `_pumped` file
   - **Add suffix** — append a custom suffix before the extension
6. Click **⚡ START SURGE**
7. Watch progress bar, byte counter, speed, and ETA
8. Log panel shows every step

**Output file location:** Same folder as source, named `<original><suffix>.<ext>`.

**Cancel:** Click **■ CANCEL** at any time. Partial output is deleted.

---

### 📦 BATCH Tab

Process many files at once.

**Steps:**

1. Click **ADD FILES** to select multiple files
2. Or click **ADD FOLDER** to add all files in a folder (recursive)
3. All files inherit the current **Target Size**
4. Review the list — status icons show progress
5. Click **▶ START ALL** to begin
6. Each item shows: waiting → processing → complete / failed

**Failure isolation:** If one file fails (locked, permission denied, etc.), the batch continues to the next file.

---

### ☷ QUEUE Tab

Ordered processing with reordering.

**Steps:**

1. **ADD** files to the queue
2. Use **▲ / ▼** to reorder
3. **✕** removes an item
4. **▶ START ALL** processes from top to bottom

**Difference from Batch:** Queue gives you full manual ordering control.

---

### ⌕ ANALYSIS Tab

Inspect a file's properties.

**Steps:**

1. Click **BROWSE** → select a file
2. Tool automatically computes:
   - Name, type, size
   - Created / modified timestamps
   - SHA-256 and SHA-512 hashes
   - Shannon entropy (bits per byte)
   - Read / write permissions
   - Windows file attributes

**Entropy interpretation:**
- **0.0 – 3.0** — highly structured (text, source code)
- **3.0 – 6.0** — mixed (executables, documents)
- **6.0 – 7.5** — compressed (ZIP, PNG, MP3)
- **7.5 – 8.0** — encrypted / random (ciphertext)

**Use case:** Check entropy of a padded file to confirm the padding method did what you expected.

---

### ▤ HEX VIEW Tab

Read-only hex viewer.

**Steps:**

1. Click **OPEN** → select any file
2. View bytes in classic format: 00000000 4D 5A 90 00 03 00 00 00 MZ......
00000008 04 00 00 00 FF FF 00 00 ........
3. **NEXT ▶ / ◀ PREV** — page forward / backward
4. **GOTO** — jump to offset (hex)
5. **FIND** — search for a hex byte sequence (e.g. `4D5A`)

**Performance:** Only loads 512 bytes at a time. Safe on multi-GB files.

---

### 🛠️ TOOLS Tab

Three mini-tools.

**Size Calculator** — enter current size and target size with units; get required padding.

**Hash Calculator** — pick MD5 / SHA-1 / SHA-256 / SHA-384 / SHA-512, browse a file, click COMPUTE.

**Disk Space** — lists every mounted drive with free / total bytes. Click REFRESH to re-scan.

---

### ⚙️ SETTINGS Tab

Persisted to `%AppData%\FileSurge\settings.json`.

**Appearance**
- Theme — `Dark` (default) or `Light` (future)
- Animations enabled — toggle UI animations
- Compact mode — tighter spacing

**Processing**
- I/O Buffer Size — bytes per chunk (default `1048576` = 1 MiB)
- Verify after processing — recompute SHA-256 of original region and compare

**Logging**
- Enable logs — turn the console panel on/off
- Log level — `Info`, `Warning`, `Error`

**Safety**
- Confirm overwrite — ask before replacing existing output
- Confirm large-file operations — warning above a size threshold
- Keep originals by default — the Keep Original checkbox defaults to checked

**Startup**
- Show startup animation — enable / disable the boot screen

---

## 🎲 Padding Methods Explained

### 1. Zero Padding
Every padding byte is `0x00`.

**Pros:** Fastest, compresses to nothing (ZIP of a padded file stays tiny), most compatible.
**Cons:** Instantly obvious that it's padding.
**Best for:** Testing, disk-filling, archive-friendly output.

### 2. Random Bytes
Uses .NET's non-cryptographic `Random`.

**Pros:** Fast, looks unstructured.
**Cons:** Not cryptographically secure — predictable if seed is known.
**Best for:** Non-security uses where speed matters.

### 3. Cryptographically Secure Random
Uses `System.Security.Cryptography.RandomNumberGenerator`.

**Pros:** Indistinguishable from real data, cryptographically strong.
**Cons:** Slower than `Random`.
**Best for:** Testing encrypted-file behavior, entropy analysis, security research.

### 4. Repeating Pattern
Cycle a hex pattern like `AA BB CC DD AA BB CC DD ...`.

**Pros:** Predictable, easy to identify in analysis, small CPU cost.
**Cons:** Trivially detectable.
**Best for:** Benchmarking, ensuring the padding is identifiable.

### 5. Custom Pattern
Same as Repeating but you supply any hex sequence.

**Pros:** Full control.
**Cons:** Must be valid hex.
**Best for:** Specific test scenarios where padding must contain a marker.

---

## 🎯 Target Size Modes Explained

### Exact Size
Final file size = the value you enter.

Example: source = 2 MB, target = 10 MB → padding = 8 MB.

### Add To Current
Final = current + value.

Example: source = 2 MB, add = 5 MB → final = 7 MB.

### Minimum Size
Final = max(current, value).

Example: source = 2 MB, min = 10 MB → final = 10 MB.
Example: source = 20 MB, min = 10 MB → final = 20 MB (unchanged).

### Unit Accuracy
All units are **binary**, not decimal:

| Unit | Bytes |
|---|---|
| 1 KB | 1,024 |
| 1 MB | 1,048,576 |
| 1 GB | 1,073,741,824 |
| 1 TB | 1,099,511,627,776 |

Sizes use **64-bit signed integers** — supports up to ~9.2 exabytes.

---

## 🔐 Integrity Verification

When **Verify after processing** is enabled (default):

1. **Before pumping:** SHA-256 of the entire source file is computed
2. **After writing:** SHA-256 of the **first N bytes** of the output (where N = source size) is computed
3. **Compare:** If hashes match → `INTEGRITY: VERIFIED`
4. **Fail loud:** If they don't match → operation is marked failed, partial output deleted

This guarantees that **every original byte survived unchanged**.

Note: hashing the source adds time to the operation. For a 10 GB source, hashing adds ~30 seconds on a typical SSD. Disable verification for speed if you're confident in the source.

---

## 🛡️ Safety and Ethics

### What File Surge Does
- Reads the source file
- Writes a **new** file with the same leading bytes
- Appends padding

### What File Surge Does NOT Do
- ❌ Execute selected files
- ❌ Inject code
- ❌ Modify PE headers or section tables
- ❌ Alter executable instructions
- ❌ Bypass antivirus or EDR
- ❌ Disable security software
- ❌ Establish persistence
- ❌ Phone home
- ❌ Collect telemetry
- ❌ Write to any location outside the chosen output folder (except `%AppData%\FileSurge\` for settings)

### Ethical Use Statement

File Surge is a **data manipulation tool**, not a hacking tool. Padding a file is a mathematical operation. Any implication that it can be used to "evade" security products is false — modern scanners inspect file contents, not just sizes.

Use this tool:
- ✅ Within your authorized scope
- ✅ For testing, benchmarking, research, or education
- ✅ On files you own or have permission to modify

Do not use this tool:
- ❌ To hide malware
- ❌ To evade security controls you don't own
- ❌ On files you don't have permission to process

**The authors take no responsibility for misuse.** This is a legitimate utility. Use it as one.

---

## 💻 Requirements

| | Minimum | Recommended |
|---|---|---|
| **OS** | Windows 10 (build 19041) | Windows 11 |
| **Architecture** | x64 | x64 |
| **RAM** | 200 MB free | 500 MB free |
| **Disk** | Enough for source + target | SSD for speed |
| **.NET** | Not required (self-contained) | — |

---

## 🏗️ Building From Source

### Prerequisites

- **Visual Studio 2022** version 17.14 or newer
- **.NET 10 SDK**
- **Windows 10/11 SDK** (installed with VS workload **.NET desktop development**)

### Build

1. Clone: git clone https://github.com/MiSFiT-SecuRiTY/FileSurge.git && cd FileSurge
2. Open `FileSurge.sln` in Visual Studio
3. Press **F5** to build and run

### Publish (single-file, self-contained)

1. Right-click **FileSurge** project → **Publish**
2. Target: **Folder**
3. Configuration: `Release | net10.0-windows | win-x64`
4. Deployment mode: **Self-contained**
5. ✅ Produce single file
6. ✅ Enable ReadyToRun
7. ⬜ Trim unused code (WPF doesn't support trimming)
8. Click **Publish**

Output: `bin/Release/net10.0-windows/win-x64/publish/FileSurge.exe`

### Run Tests

- **Test → Run All Tests** (Ctrl+R, A)
- 9 unit tests cover `SizeCalculator`

---

## 📁 Project Structure

## 📁 Project Structure

```text
FileSurge/
│
├── FileSurge.sln
├── README.md
├── LICENSE
├── .gitignore
│
├── FileSurge/
│   │
│   ├── FileSurge.csproj
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   │
│   ├── Infrastructure/
│   │   ├── AppSettings.cs
│   │   └── SettingsService.cs
│   │
│   ├── Services/
│   │   ├── SizeCalculator.cs
│   │   ├── PaddingGenerator.cs
│   │   ├── HashService.cs
│   │   ├── PumpOptions.cs
│   │   ├── PumpEngine.cs
│   │   ├── FileAnalyzer.cs
│   │   └── HexReader.cs
│   │
│   ├── ViewModels/
│   │   ├── ViewModelBase.cs
│   │   ├── RelayCommand.cs
│   │   ├── AsyncRelayCommand.cs
│   │   ├── LogEntry.cs
│   │   ├── MainViewModel.cs
│   │   ├── PumpViewModel.cs
│   │   ├── BatchItem.cs
│   │   ├── BatchViewModel.cs
│   │   ├── QueueViewModel.cs
│   │   ├── AnalysisViewModel.cs
│   │   ├── HexViewModel.cs
│   │   ├── ToolsViewModel.cs
│   │   └── SettingsViewModel.cs
│   │
│   ├── Views/
│   │   ├── PumpView.xaml
│   │   ├── PumpView.xaml.cs
│   │   ├── BatchView.xaml
│   │   ├── BatchView.xaml.cs
│   │   ├── QueueView.xaml
│   │   ├── QueueView.xaml.cs
│   │   ├── AnalysisView.xaml
│   │   ├── AnalysisView.xaml.cs
│   │   ├── HexView.xaml
│   │   ├── HexView.xaml.cs
│   │   ├── ToolsView.xaml
│   │   ├── ToolsView.xaml.cs
│   │   ├── SettingsView.xaml
│   │   └── SettingsView.xaml.cs
│   │
│   ├── Themes/
│   │   ├── Colors.xaml
│   │   └── Controls.xaml
│   │
│   └── Resources/
│       └── app.ico
│
└── FileSurge.Tests/
    └── SizeCalculatorTests.cs
```

---

## 🧰 Technology Stack

| Layer | Technology |
|---|---|
| **Language** | C# 13 |
| **Runtime** | .NET 10 |
| **UI framework** | WPF |
| **UI pattern** | MVVM (manual, no framework) |
| **I/O** | `FileStream`, `BufferedStream`, async read/write |
| **Hashing** | `System.Security.Cryptography` |
| **RNG** | `RandomNumberGenerator` (CSPRNG) |
| **Config** | `System.Text.Json` to `%AppData%` |
| **Tests** | xUnit |
| **Packaging** | Self-contained single-file publish |

### Design Choices

- **MVVM without a framework** — no Prism, no MVVM Light. Just `INotifyPropertyChanged` + custom `RelayCommand`.
- **Streaming-first** — never `File.ReadAllBytes()`. All I/O is chunked and async.
- **Cancellation-aware** — every long operation accepts a `CancellationToken`.
- **Zero runtime dependencies** — the app references only the .NET BCL. No NuGet packages at runtime.
- **Dark by default** — the cyber aesthetic is baked into the theme system, not retrofitted.

---

## ❓ FAQ

**Q: Does this modify my original file?**
A: No. Unless you check "Overwrite if exists" and target the same path, the source is untouched. Output is a new file with `_pumped` in the name.

**Q: Can I pump a 50 GB file?**
A: Yes. The engine streams in 1 MiB chunks — memory stays flat regardless of size.

**Q: Will this break my executable?**
A: The original bytes stay intact. Windows will still run the `.exe` because PE loaders ignore trailing data. **But you should not pump system files or files you didn't create.**

**Q: Does this bypass antivirus?**
A: No. Modern AV scans file contents, not just sizes. This tool changes size and hash — nothing more.

**Q: Why is the download 144 MB?**
A: It's self-contained — the .NET runtime is bundled in. No dependency install needed. See [About the 144 MB Size](#️-about-the-144-mb-size).

**Q: Why does Windows show a warning?**
A: The app is not code-signed. Click **More info → Run anyway**. See [About Windows SmartScreen Warnings](#️-about-windows-smartscreen-warnings).

**Q: Will it work on Windows 7?**
A: No. Minimum is Windows 10 build 19041.

**Q: Can I use it on macOS / Linux?**
A: No. It's a WPF app — Windows-only by design.

**Q: Does it phone home?**
A: No. Zero network calls. Test with Wireshark if you don't believe me.

**Q: Is it safe?**
A: Yes. It only appends bytes. It doesn't execute files, doesn't touch the registry, doesn't phone home.

**Q: How do I uninstall?**
A: It's portable. Delete `FileSurge.exe`. Optionally delete `%AppData%\FileSurge\` to clear settings.

**Q: How do I reset settings?**
A: Delete `%AppData%\FileSurge\settings.json`. Restart the app.

**Q: How do I verify the download is genuine?**
A: Compare the SHA-256 hash from the release page with `Get-FileHash FileSurge.exe -Algorithm SHA256` in PowerShell.

**Q: Can I contribute?**
A: Yes — fork the repo, make changes, submit a pull request.

---

## 📄 License

MIT License — see [LICENSE](LICENSE) for the full text.

You are free to use, modify, and distribute this software, including commercially, as long as you retain the copyright notice and license text.

---

<div align="center">

**⚡ FILE SURGE ⚡**

**PUMP • PROCESS • CONTROL**

Built with C# and WPF on .NET 10.

[Download](https://github.com/MiSFiT-SecuRiTY/FileSurge/releases/latest) • [Report Bug](https://github.com/MiSFiT-SecuRiTY/FileSurge/issues) • [Source](https://github.com/MiSFiT-SecuRiTY/FileSurge)

</div>
