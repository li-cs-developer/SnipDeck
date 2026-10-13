[English](README.md) • [Français](README.fr.md)

---

# 📋 SnipDeck

> 💡 **Un gestionnaire de presse-papiers Windows qui enregistre, recherche et restaure votre historique de copie — hors ligne, privé et rapide.**

SnipDeck est une **application de barre d'état** qui conserve un historique consultable de tout ce que vous copiez : texte brut, texte enrichi, images et listes de fichiers. Il s'exécute discrètement en arrière-plan, respecte les exclusions des gestionnaires de mots de passe et **ne se connecte jamais à Internet**. Développé sur **.NET 10 + WPF**, il illustre l'architecture MVVM, une structure en quatre projets, un thème commutable à chaud, une localisation bilingue et une suite de tests xUnit complète.



## 🔗 Liens du projet

| 📖 **Confluence** | [Documentation SnipDeck](https://li-cs-developer.atlassian.net/wiki/x/AwAY) |
|---|---|
| 🎫 **Jira** | [Tableau du projet SNIP](https://li-cs-developer.atlassian.net/jira/software/projects/SNIP/list) |
| 📦 **Versions** | [Versions GitHub](https://github.com/li-cs-developer/SnipDeck/releases) |

---

## ✨ Fonctionnalités

### 📋 Capture
- 📝 **Texte brut** — chaque copie est capturée
- 🎨 **Texte enrichi** — formats RTF et HTML conservés en plus du texte brut
- 🖼️ **Images** — captures d'écran et images copiées
- 📄 **Listes de fichiers** — chemins des fichiers copiés depuis l'Explorateur
- 🛡️ **Exclusions des gestionnaires de mots de passe** — respecte `ExcludeClipboardContentFromMonitorProcessing` et `CanIncludeInClipboardHistory`
- 🚫 **Suppression des doublons** — les copies identiques consécutives ne créent pas d'entrée en double

### 🔍 Rechercher et restaurer
- 🔎 **Recherche floue** — correspondance de sous-séquences (`hlw` correspond à `hello world`), ou sous-chaîne simple — au choix
- ↕️ **Tri** — plus récents, plus anciens ou les plus utilisés
- 🏷️ **Étiquettes** — libellés personnalisés sur chaque élément, persistés
- 👁️ **Volet d'aperçu** — texte complet, image complète ou chemin de fichier complet
- ☑️ **Sélection multiple** — Ctrl+clic, Maj+clic, Ctrl+A
- 📤 **Copie multiple** — texte joint par saut de ligne à partir de plusieurs éléments sélectionnés

### 🖥️ Intégration au bureau
- ⌨️ **Raccourci global** — `Ctrl+Maj+V` ouvre la fenêtre depuis n'importe quelle application
- 🖥️ **Icône de la barre d'état** — menu Afficher / Paramètres / Quitter
- 🔢 **Info-bulle en direct** — affiche le nombre actuel d'éléments
- 🔔 **Notifications facultatives** — petite confirmation lors de la copie (adaptée au thème)
- 🚀 **Démarrage avec Windows** — facultatif
- 🔒 **Instance unique** — une seule copie s'exécute à la fois
- 🙈 **Masqué d'Alt+Tab** — comportement propre de fenêtre outil

### 🎨 Finitions
- 🌗 **Thèmes** — Clair, Sombre ou système ; commutable à chaud
- 🌐 **Localisation** — anglais canadien + français canadien, commutable à chaud
- ♿ **Accessibilité** — étiquettes Narrateur sur chaque élément interactif
- 💾 **Historique persistant** — survit aux redémarrages
- 📊 **Taille d'historique configurable** — 50 / 100 / 200 / 400 / 800 éléments (par paliers)
- 🧹 **Nettoyage automatique des images** — les PNG sont supprimés lorsque leur élément est évincé

---

## 🛠️ Pile technologique

| Couche | Technologie |
|--------|-------------|
| **Interface** | WPF, MVVM, XAML |
| **Runtime** | .NET 10 (`net10.0-windows`) |
| **Icône de barre d'état** | `Hardcodet.NotifyIcon.Wpf` 2.0.1 |
| **Persistance** | Fichiers JSON + PNG dans `%APPDATA%\SnipDeck\` |
| **Localisation** | Ressources `.resx` + extension de balisage `{loc:Loc}` personnalisée |
| **Interop Win32** | P/Invoke pour `RegisterHotKey` et les styles de fenêtre |
| **Tests** | xUnit 2.9.2, 54 tests |
| **Compilation** | MSBuild, CLI `dotnet` |

---

## 🏗️ Architecture

SnipDeck est une **solution à quatre projets** avec un graphe de dépendances strictement unidirectionnel.

```
SnipDeck.slnx
└── src/
    ├── 🎨 SnipDeck.Core/            ← logique métier, aucune dépendance UI
    ├── 🌐 SnipDeck.Localization/    ← dictionnaires resx, aucune dépendance à Core
    ├── 🖥️ SnipDeck.App/              ← exécutable WPF, référence les deux
    └── 🧪 SnipDeck.Tests/            ← xUnit, référence les trois
```

### Graphe de dépendances

```
        SnipDeck.App ────► SnipDeck.Localization
              │                             │
              └──────► SnipDeck.Core ◄──────┘
                             ▲
                             │
                      SnipDeck.Tests
```

La seule façon de rendre cela plus propre serait de supprimer la flèche `App → Localization`, ce qui exigerait d'inverser la dépendance de localisation. Pour une petite application, la forme actuelle est le bon compromis.

### Flux de données — capture du presse-papiers

```
        ┌──────────────────────────────┐
        │  Presse-papiers Windows      │
        └──────────────┬───────────────┘
                       │ tick du DispatcherTimer (500 ms)
                       ▼
        ┌──────────────────────────────┐          ┌───────────────────────────────┐
        │       ClipboardMonitor       │─────────►│   ClipboardExclusionChecker   │
        └──────────────┬───────────────┘          └───────────────────────────────┘
                       │ lit via
                       ▼
        ┌──────────────────────────────┐  (réessai sur COMException)
        │      ClipboardAccessor       │
        └──────────────┬───────────────┘
                       │ produit
                       ▼
        ┌──────────────────────────────────────────────┐
        │         Texte / Enrichi / Image / Fichier    │
        └──────────────────────┬───────────────────────┘
                               │ événement ItemCaptured
                               ▼
        ┌──────────────────────────────┐          ┌──────────────────────────────┐
        │        MainViewModel         │─────────►│     JsonHistoryStore         │
        │        (SnipDeck.App)        │  ajout   │      └─► ImageFileStore      │
        └──────────────┬───────────────┘          └──────────────────────────────┘
                       │ RefreshFilter
                       ▼
        ┌──────────────────────────────┐
        │     ObservableCollection     │
        └──────────────┬───────────────┘
                       │ Liaison de données
                       ▼
        ┌──────────────────────────────┐
        │       ListBox WPF            │
        └──────────────────────────────┘
```

### Flux de données — restauration du presse-papiers

```
        Double-clic sur un élément de la liste
                     │
                     ▼
        🧠 MainViewModel.CopyToClipboard(vm)
                     │
                     ▼
        📤 ClipboardActionService.Copy(model)
                     │
                     ▼
        🔄 ClipboardAccessor.TrySet*
                     │ en cas de succès
                     ▼
        🎉 événement Copied → 💬 ToastWindow.ShowToast()
```

---

## 📁 Structure du projet

```
    SnipDeck/
    ├── assets/                               ← ressources source (logo, etc.)
    ├── build/                                ← scripts de compilation et d'empaquetage
    │   ├── version.props                     ← numéro de version central
    │   ├── publish.ps1                       ← script de publication
    │   ├── installer.iss                     ← script d'installation Inno Setup
    │   └── output/                           ← sortie de publication ignorée par git
    ├── docs/                                 ← documentation détaillée
    ├── src/
    │   ├── SnipDeck.Core/                    ← bibliothèque de classes, sans UI
    │   │   ├── Models/                       ← hiérarchie ClipboardItem
    │   │   ├── Services/                     ← Monitor, Accessor, Stores, CrashLog
    │   │   ├── Search/                       ← implémentations de recherche floue + simple
    │   │   ├── Settings/                     ← AppSettings + JsonSettingsService
    │   │   └── Interop/                      ← NativeMethods, HotkeyManager
    │   │
    │   ├── SnipDeck.Localization/            ← bibliothèque de classes, ressources
    │   │   ├── Strings.resx                  ← chaînes en-CA
    │   │   ├── Strings.fr-CA.resx            ← chaînes fr-CA
    │   │   ├── Loc.cs                        ← API Loc.T(key) + événement LanguageChanged
    │   │   ├── LocSource.cs                  ← indexeur INotifyPropertyChanged pour XAML
    │   │   └── LocExtension.cs               ← extension de balisage {loc:Loc Key}
    │   │
    │   ├── SnipDeck.App/                     ← exécutable WPF
    │   │   ├── Views/                        ← MainWindow, SettingsWindow, ToastWindow
    │   │   ├── ViewModels/                   ← modèles de vue MVVM
    │   │   ├── Controls/                     ← sélecteurs de modèle
    │   │   ├── Converters/                   ← convertisseurs de valeur
    │   │   ├── Themes/                       ← Shared / Light / Dark + ThemeManager
    │   │   ├── Tray/                         ← TrayIconHost
    │   │   ├── Services/                     ← AutoStartHelper, ToastDescription
    │   │   ├── Resources/                    ← app.ico
    │   │   └── app.manifest                  ← conscience DPI, pas d'UAC
    │   │
    │   └── SnipDeck.Tests/                   ← projet de test xUnit
    │       ├── AppSettingsTests.cs
    │       ├── FilterViewModelTests.cs
    │       ├── FuzzySearchServiceTests.cs
    │       ├── ImageFileStoreTests.cs
    │       ├── JsonHistoryStoreTests.cs
    │       └── LocalizationTests.cs
    │
    ├── .editorconfig
    ├── .gitattributes
    ├── .gitignore
    ├── LICENSE
    ├── PRIVACY.md
    ├── README.md
    └── SnipDeck.slnx
```

---

## 🚦 Démarrage

### 📋 Prérequis

- 🪟 **Windows 10 build 19041+** ou **Windows 11**
- 🟣 **SDK .NET 10** — [télécharger](https://dotnet.microsoft.com/download)
- 🧑💻 Un éditeur : **Rider** (recommandé), **Visual Studio 2022** ou **VS Code** + extension C#

### 🐳 Installer depuis une version (recommandé pour les utilisateurs)

1. Téléchargez `SnipDeck-portable.zip` depuis la [page des versions](https://github.com/li-cs-developer/SnipDeck/releases).
2. Extrayez n'importe où.
3. Exécutez `SnipDeck.exe`.
4. Recherchez l'icône dans la barre d'état (vérifiez le chevron `^` si vous ne la voyez pas).

### 💻 Compiler depuis les sources

```powershell
git clone https://github.com/li-cs-developer/SnipDeck
cd SnipDeck
dotnet build -c Release
dotnet run --project src/SnipDeck.App
```

L'application démarre **masquée dans la barre d'état**.

### 🔨 Compiler l'installateur

```powershell
# Publier les builds dépendant du framework et portable
pwsh build\publish.ps1

# Compiler l'installateur (nécessite Inno Setup 6)
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" build\installer.iss
```

---

## 🎮 Utilisation

| Action | Comment |
|---|---|
| **Afficher la fenêtre** | Double-cliquez sur l'icône de la barre d'état ou appuyez sur `Ctrl+Maj+V` |
| **Recopier un élément** | Double-cliquez dessus ou clic droit → **Copier dans le presse-papiers** |
| **Rechercher** | Tapez dans la zone de recherche (délai de 200 ms) |
| **Supprimer un élément** | Sélectionnez, appuyez sur `Suppr` |
| **Supprimer plusieurs** | Ctrl+clic pour sélectionner, appuyez sur `Suppr` |
| **Copier plusieurs** | Ctrl+clic pour sélectionner, clic droit → **Copier dans le presse-papiers** |
| **Ajouter une étiquette** | Sélectionnez un élément, tapez dans la zone d'étiquette, cliquez sur **Ajouter l'étiquette** |
| **Ouvrir les paramètres** | Bouton ⚙ **Paramètres** ou barre d'état → **Paramètres…** |
| **Quitter** | Barre d'état → **Quitter** |

### Raccourcis clavier

| Raccourci | Action |
|---|---|
| `Ctrl+Maj+V` | Global — affiche la fenêtre depuis n'importe quelle application |
| `Ctrl+A` | Sélectionne tous les éléments de la liste |
| `Suppr` | Supprime les éléments sélectionnés |
| `Échap` | Ferme la fenêtre des paramètres |

---

## ⚙️ Référence des paramètres

| Paramètre | Valeur par défaut | Description |
|---|---|---|
| **Nombre maximal d'éléments** | 50 | 50 / 100 / 200 / 400 / 800 — par paliers |
| **Thème** | Système | Clair / Sombre / Système |
| **Afficher les notifications** | Activé | Popup de confirmation lors de la copie |
| **Recherche floue** | Activé | Correspondance de sous-séquences vs sous-chaîne |
| **Démarrer avec Windows** | Désactivé | Clé de registre `Run` |
| **Langue** | English (Canada) | en-CA / fr-CA |

---

## 🧠 Pratiques d'ingénierie

### 🏛️ Architecture propre

La solution applique un graphe de dépendances strictement unidirectionnel. `SnipDeck.Core` n'a **aucune dépendance aux types de présentation WPF** — uniquement des primitives du framework comme `BitmapSource`, `DispatcherTimer` et `Clipboard`. Cela signifie que la logique de surveillance du presse-papiers et de stockage de l'historique pourrait être réutilisée dans une application console ou un autre framework d'interface sans rien réécrire.

| Principe | Où cela apparaît |
|---|---|
| **Responsabilité unique** | Le moniteur lit le presse-papiers ; le stockage possède l'historique ; le modèle de vue orchestre ; la vue affiche. Chaque couche ne connaît que son travail. |
| **Inversion de dépendance** | `IHistoryStore`, `IClipboardMonitor`, `ISearchService`, `ISettingsService` — les quatre abstrayent une préoccupation concrète, permettant le remplacement et les tests. |
| **Ségrégation d'interface** | Interfaces petites et ciblées. `IClipboardMonitor` a trois membres. `ISettingsService` en a trois. |
| **Encapsulation** | La hiérarchie `ClipboardItem` valide dans les constructeurs. `ImageClipboardItem.FullImage` charge paresseusement depuis le disque. |
| **Composition plutôt qu'héritage** | La classe `App` câble les services directement — pas de conteneur DI, pas de réflexion, pas de cérémonie. |
| **Testabilité** | 54 tests xUnit. La suite de tests s'exécute sans interface (pas de répartiteur WPF nécessaire pour les tests de logique pure). |
| **Sûr par défaut** | Démarrage enveloppé dans try/catch avec erreurs visibles. Chaque bloc catch journalise dans `CrashLog` ou reste silencieux avec un commentaire. |

### 🧪 Couverture des tests

54 tests répartis en six classes :

| Classe de test | Couverture |
|---|---|
| `AppSettingsTests` | Comportement de l'échelonnage, valeurs par défaut |
| `FilterViewModelTests` | Correspondance des étiquettes de tri, déclenchement des événements |
| `FuzzySearchServiceTests` | Correspondance de sous-séquences, correspondance d'étiquettes, départage |
| `ImageFileStoreTests` | Cycle de vie des fichiers avec injection de répertoire |
| `JsonHistoryStoreTests` | Ajout / déduplication / élagage / suppression / vidage |
| `LocalizationTests` | Résolution en-CA et fr-CA, repli si clé manquante, arguments de format |

```powershell
dotnet test
```

---

## 🏛️ Décisions d'architecture

### Pourquoi WPF plutôt que WinForms ?
La liaison de données, l'outillage MVVM et la thématisation moderne sont nettement plus faciles en WPF. La bibliothèque d'icônes de la barre d'état est la seule dépendance à `System.Drawing`, et c'est un détail d'implémentation.

### Pourquoi interroger le presse-papiers au lieu d'utiliser `WM_CLIPBOARDUPDATE` ?
L'API `AddClipboardFormatListener` de Win32 est plus efficace, mais l'API de presse-papiers de WPF est plus facile à utiliser et une interrogation toutes les 500 ms est invisible en pratique. Coût : un `Clipboard.ContainsText()` toutes les 500 ms — négligeable.

### Pourquoi le texte enrichi avant les images dans l'ordre de capture ?
La plupart des applications modernes placent un fragment HTML dans le presse-papiers en plus du texte brut lorsque vous copiez du contenu formaté. Si nous vérifiions les images en premier, nous raterions le texte enrichi. Priorité : **Fichiers → Texte enrichi → Image → Texte**.

### Pourquoi stocker les images sous forme de fichiers PNG plutôt qu'en base64 dans le JSON ?
Une seule capture d'écran 1920×1080 représente environ 8 Mo en pixels bruts. En base64 dans le JSON, cela ferait environ 11 Mo, et `history.json` gonflerait à des centaines de mégaoctets en une session. Les fichiers PNG sur disque avec un nom de fichier dans le JSON évitent entièrement cela.

### Pourquoi SnipDeck ne colle-t-il pas directement dans l'application ciblée ?
Le collage direct via `SendInput` a été conçu puis retiré. Il nécessitait une gestion minutieuse du focus des fenêtres pour éviter de coller dans la mauvaise fenêtre. Les utilisateurs collent manuellement avec `Ctrl+V` après avoir sélectionné un élément — plus prévisible, cohérent avec le comportement de l'historique du presse-papiers Windows.

### Pourquoi pas de conteneur DI ?
L'application a moins de 20 services, tous câblés dans `App.OnStartup`. Un conteneur ajouterait de la cérémonie (enregistrement, durées de vie, disposition) sans bénéfice à cette échelle.

### Pourquoi des tailles d'historique par paliers (50/100/200/400/800) ?
Un champ numérique libre invite les utilisateurs à taper `10000` — ce qui livrerait une expérience cassée (gonflement de la mémoire, démarrage lent). Un menu déroulant de cinq choix raisonnables donne le choix dans une contrainte sensée. Les valeurs sont écrêtées au palier le plus proche au chargement, donc un `settings.json` édité à la main ne peut pas casser l'application.

---

## 🔒 Confidentialité

SnipDeck stocke tout localement dans `%APPDATA%\SnipDeck\`. **Aucun accès réseau. Aucune télémétrie. Aucun compte.** Voir [PRIVACY.md](PRIVACY.md) pour les détails.

### Ce qui se trouve où

| Fichier | Contenu |
|---|---|
| `%APPDATA%\SnipDeck\history.json` | Historique du presse-papiers (texte, texte enrichi, chemins de fichiers) |
| `%APPDATA%\SnipDeck\settings.json` | Préférences utilisateur |
| `%APPDATA%\SnipDeck\images\*.png` | Images capturées |
| `%APPDATA%\SnipDeck\crash.log` | Journal d'erreurs (uniquement en cas d'échec inattendu) |

### Tout réinitialiser

```powershell
Remove-Item -Recurse "$env:APPDATA\SnipDeck"
```

---

## 🤖 Développement assisté par IA

Des outils d'IA (Copilot, Gemini, ChatGPT) ont été utilisés tout au long du développement pour accélérer le travail mécanique et soutenir l'apprentissage. Utilisations efficaces :

- **Échafaudage d'architecture** — disposition de la structure en quatre projets, câblage des couches MVVM
- **Brouillons de tests d'abord** — production de tests défaillants initiaux pour `FuzzySearchService`, `ImageFileStore`, et autres
- **Support de refactorisation** — extraction de `ClipboardActionService` depuis `MainViewModel`, élimination des catchs vides, ajout de `CrashLog` centralisé
- **Passe de localisation** — traduction des chaînes en-CA vers fr-CA et structuration de l'extension de balisage `Loc`
- **Documentation** — rédaction du README, PRIVACY et de la documentation Confluence
- **Débogage** — diagnostic d'un bug récurrent de recréation de HWND dans l'enregistrement du raccourci global, et d'une défaillance de recherche de ressource de thème qui faisait planter la fenêtre des paramètres

---

## 🖥️ Compatibilité

| | |
|---|---|
| ✅ **Pris en charge** | Windows 10 (build 19041+) et Windows 11, x64 |
| ⚠️ **Non testé** | Windows 10 ARM64, Windows Server |
| 🟣 **Runtime** | .NET 10 Desktop Runtime (inclus dans le build portable) |

---

## 🐛 Dépannage

### L'icône de la barre d'état n'apparaît pas
Vérifiez le chevron `^` près de l'horloge système — Windows 11 masque les icônes en trop par défaut. Faites glisser SnipDeck sur la barre des tâches visible si vous voulez qu'elle soit toujours affichée.

### Le raccourci global `Ctrl+Maj+V` ne fonctionne pas
Une autre application possède le raccourci. Coupables fréquents : Ditto, CopyQ, Windows Terminal. Modifiez l'association dans `%APPDATA%\SnipDeck\settings.json` :

```json
{
  "HotkeyModifiers": 6,
  "HotkeyKey": 86
}
```

`6` correspond à `Ctrl+Maj`, `86` à `V`. Changez la touche pour une valeur inutilisée, par ex. `0x7A` pour `F11`.

### Les images n'apparaissent pas après le redémarrage
Vérifiez `%APPDATA%\SnipDeck\images\` — les PNG devraient s'y trouver. S'ils manquent, l'image a été capturée avant la mise en œuvre de la persistance des images, ou le fichier a été supprimé manuellement. Le nettoyage des orphelins au démarrage est idempotent — les fichiers qui ne correspondent à aucun élément d'historique en direct sont supprimés.

### Les paramètres plantent à l'ouverture
Vérifiez `%APPDATA%\SnipDeck\crash.log`. Il s'agissait d'un problème connu avec la recherche de ressources de thème ; s'il se reproduit, collez le journal dans le suivi Jira.

### « Tout effacer » ne persiste pas
Quittez via **barre d'état → Quitter**, pas le Gestionnaire des tâches. L'historique et les paramètres sont écrits lors d'un arrêt gracieux. Tuer le processus fait perdre les modifications non enregistrées.

### Comment tout réinitialiser ?
```powershell
Remove-Item -Recurse "$env:APPDATA\SnipDeck"
```

---

## 📄 Licence

MIT — voir [LICENSE](LICENSE).
