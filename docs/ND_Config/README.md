# Natural Docs Setup

This project uses **Natural Docs** to generate source code documentation from comments written in natural language.

The generated documentation is published as HTML and can be viewed in any web browser.

---

# Installation

Download the latest version of Natural Docs:

https://www.naturaldocs.org/

Extract the archive to a folder, for example:

```text
C:\Tools\NaturalDocs\
```
No installation is required.

Or install it on your computer with the installer:
```text
C:\Program Files (x86)\Natural Docs\
```

---

# App Directory Structure

```text
desktopPet
│
├── src
│
├── docs
│   ├── _old
│   ├── ND_Config
│   └── index.html  
│   └── [folders]
```

## Folders

| Folder | Description |
|----------|----------|
| _old | Old documentation |
| ND_Config | Natural Docs configuration |
| index.html | Generated documentation output |
| [folders] | Generated folders and files |

---

# Generating Documentation

Open a terminal in the repository root and run:

```powershell
NaturalDocs.exe docs\ND_Config 
```

Or, eveven better, add an external tool to VS:
<img width="457" height="454" alt="image" src="https://github.com/user-attachments/assets/1a67bd44-d7dd-4f9e-9923-37b7288c3e81" />

Adding this as arguments:
```text
$(ProjectDir)..\docs\ND_Config
```

After execution, the generated website can be found in:

```text
docs/
```

Open:

```text
docs/html/index.html
```

in a web browser.

---

# Updating Documentation

Whenever source code changes:

```powershell
NaturalDocs.exe docs\ND_Config 
```
Or the new menu in Tools:
<img width="281" height="130" alt="image" src="https://github.com/user-attachments/assets/cda61b34-8476-42af-a3f4-6107bcc6ce61" />


Natural Docs only processes changed files and therefore runs relatively fast.

---

# Comment Style

Even if ND is able to parse natural language, the code is still xml formatted.

---

# Workflow

1. Write code.
2. Add or*update Natural Docs comments.
3.*Run Natural Docs.
4. Open `docs/ht*l/index.html`.
5.*Commit the generated documentation*if desired.

---

- Comments*should be understandable by non-de*elopers whenever possible.
- Use*clear* natural language*descriptions.

````*
