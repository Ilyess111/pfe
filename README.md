# Soroubat — Application de Pilotage de Chantiers

Projet de Fin d'Études (PFE) — Intégration Microsoft Dynamics 365 Business Central & développement d'une application web de gestion de chantiers pour **Soroubat**.

## Contexte du projet

Soroubat utilise **Microsoft Dynamics 365 Business Central** comme ERP central pour la gestion de ses chantiers (projets, achats, stock, matériel, paie...). Ce PFE consiste à construire, autour de cet ERP, une **application web dédiée aux chefs de chantier**, leur permettant de piloter leurs projets au quotidien (avancement des tâches, demandes d'achat, transferts de stock, suivi carburant, pointage du personnel et des engins) depuis une interface moderne, sans avoir à utiliser directement le client Business Central.

Le projet repose sur trois briques complémentaires :

```
Business Central (ERP)
        ↑↓  API REST (extension AL)
   Backend .NET (API intermédiaire)
        ↑↓  API REST (JWT)
   Frontend Angular (PWA)
        ↑
   Chef de chantier
```

## *Réalisation

Ce projet a été réalisé **en binôme** dans le cadre d'un PFE :

- **Backend .NET + Extension AL (Business Central)** : partie API, logique métier, intégration ERP et sécurité — réalisée par l'auteur de ce dépôt.
- **Frontend Angular** : interface utilisateur, expérience chef de chantier (PWA, tableaux de bord, reconnaissance faciale, scan QR) — réalisée par mon collègue de binôme.

## Architecture technique

Le dépôt est organisé en trois modules principaux.

### 1. `AL_Extension/` — Extension Business Central (AL)

Extension AL pour Microsoft Dynamics 365 Business Central 24 (runtime 13.0), qui :
- ajoute des **tables et extensions de tables** propres au métier BTP (chantier, équipe, matériel, budget, localisation GPS...) ;
- expose des **API REST** (`APIPublisher = soroubat`) consommées par le backend .NET, par exemple `siteManagement` (projets/tâches) et `lookups` (données de référence) ;
- définit des **Permission Sets** (dossier `PR`) contrôlant précisément les droits de lecture/création/modification/suppression exposés à l'extérieur de l'ERP ;
- contient la **logique métier** (codeunits) : calcul de coût de chantier, validation de tâches, mise à jour de numéros de tâches, etc. ;
- fournit des **profils utilisateurs** BC personnalisés (ex. Chef de chantier) et des rapports/layouts spécifiques.

### 2. `Backend/Soroubat.Api/` — API intermédiaire (.NET 8)

API ASP.NET Core (C#, .NET 8) qui sert de **couche d'intégration et de sécurité** entre le frontend Angular et Business Central. Elle ne réplique pas les données de l'ERP : elle les interroge et les transforme à la volée via les API AL.

**Modules exposés (contrôleurs) :**
| Contrôleur | Rôle |
|---|---|
| `AuthController` | Authentification et émission de jetons JWT |
| `SiteManagementController` | Projet et tâches du chef de chantier connecté (`my-project`, `my-tasks`, avancement) |
| `PurchaseRequestController` | Demandes d'achat (création, lignes, soumission) |
| `TransferController` | Transferts / mouvements de stock inter-chantiers |
| `StockController` | Consultation du stock du chantier (`my-stock`) |
| `GasoilController` | Suivi de la consommation carburant (bons, lignes, validation) |
| `AttendanceController` | Pointage du personnel, y compris scan de présence (`scan-presence`) |
| `VehiculePointageController` | Pointage des engins et véhicules de chantier |
| `AlertController` | Agrégation des alertes (achats, transferts, stock, véhicules, gasoil, présence) |
| `LookupController` | Données de référence génériques (listes déroulantes) |

**Choix techniques notables :**
- **Entity Framework Core + SQLite** pour le stockage local des comptes utilisateurs (identifiants applicatifs).
- **BCrypt.Net** pour le hachage des mots de passe.
- **JWT Bearer** pour l'authentification des appels API depuis Angular.
- **Authentification Windows intégrée** pour les appels sortants vers Business Central.
- **FaceRecognitionDotNet** pour la vérification de présence par reconnaissance faciale côté serveur.
- **Swagger / OpenAPI** pour la documentation interactive de l'API (environnement de développement).
- **Polly** (`Microsoft.Extensions.Http.Polly`) pour la résilience des appels HTTP vers Business Central.

### 3. `Frontend/Soroubat-App/` — Application web (Angular)

Application **Angular 21** en **Progressive Web App (PWA)**, conçue pour un usage terrain (mobile/tablette) par les chefs de chantier.

**Points clés :**
- **Angular Material** + **Lucide Icons** pour l'interface.
- **Service Worker Angular** (`ngsw-config.json`) pour le fonctionnement hors-ligne et l'installation en PWA.
- **Dexie.js** (IndexedDB) pour la persistance locale des données en cas de connectivité limitée sur chantier.
- **face-api.js** pour la reconnaissance faciale côté client (pointage).
- **html5-qrcode** pour le scan de QR codes (identification de matériel/engins).
- **Chart.js** pour les tableaux de bord (avancement projets, tâches par statut, budget réel vs prévu, alertes).
- **ngx-translate** pour l'internationalisation.
- **Angular SSR** (`@angular/ssr`, `server.ts`) pour le rendu côté serveur.

**Organisation du code (`src/app/`) :**
- `core/` : services transverses, **guards** (`auth-guard`, `role-guard`) et **interceptors** (`auth.interceptor`, `error-interceptor`) ;
- `auth/` : authentification (connexion) ;
- `layout/` : gabarits d'affichage (layout authentifié / layout public) ;
- `models/` : modules fonctionnels — `dashboard`, `projects`, `tasks`, `purchases`, `transfers`, `inventory`, `gasoil`, `equipment`, `attendance`, `alerts`, `calendar`, `settings`, `help` ;
- `shared/` : composants et pipes réutilisables.

## Règles de sécurité

La sécurité du projet repose sur une **défense en profondeur**, avec des contrôles à chaque couche de l'architecture.

### Authentification
- L'authentification est **hybride** : les identifiants (email/mot de passe) sont vérifiés localement (SQLite), tandis que le **statut du compte et le projet assigné** sont revérifiés en temps réel dans Business Central à chaque connexion.
- Les mots de passe ne sont **jamais stockés en clair** : ils sont hachés avec **BCrypt** et vérifiés via comparaison de hash.
- En cas d'échec, l'API distingue volontairement peu d'informations côté client (`INVALID_CREDENTIALS` générique) pour éviter l'énumération de comptes, tout en journalisant le détail côté serveur (`ILogger`).
- Un compte désactivé (`ACCOUNT_INACTIVE`) ou sans projet assigné (`NO_PROJECT_ASSIGNED`) dans Business Central est explicitement bloqué, même si les identifiants sont corrects.

### Jetons JWT
- Chaque connexion réussie génère un **jeton JWT signé** (HMAC-SHA256), contenant l'email, l'identifiant du projet assigné et un identifiant unique (`jti`).
- Le jeton est **limité dans le temps** (expiration à 8h) et validé côté serveur sur l'**émetteur**, l'**audience**, la **signature** et la **durée de vie** (`ValidateIssuer`, `ValidateAudience`, `ValidateIssuerSigningKey`, `ValidateLifetime`).
- Tous les contrôleurs métier (`Alert`, `Attendance`, `Gasoil`, `Lookup`, `PurchaseRequest`, `SiteManagement`, `Stock`, `Transfer`, `VehiculePointage`) sont protégés par l'attribut **`[Authorize]`** : aucun accès aux données de chantier n'est possible sans jeton valide.
- Le frontend attache automatiquement le jeton aux requêtes via un **`auth.interceptor`**, et un **`error-interceptor`** gère la déconnexion/redirection en cas de jeton expiré ou invalide (401/403).
- Des **guards Angular** (`auth-guard`, `role-guard`) empêchent la navigation vers des routes protégées côté client sans session valide ou sans rôle adéquat.

### Communication avec Business Central
- Les appels sortants du backend .NET vers l'API Business Central utilisent l'**authentification Windows intégrée** (`UseDefaultCredentials`), évitant la circulation de secrets applicatifs supplémentaires.
- L'accès aux données ERP est **restreint au périmètre du chef de chantier connecté** : les endpoints comme `my-project`, `my-tasks` ou `my-stock` ne retournent que les données liées au projet assigné à l'utilisateur, jamais l'ensemble du référentiel.
- Côté extension AL, les pages API définissent explicitement les droits **`InsertAllowed` / `ModifyAllowed` / `DeleteAllowed`** par entité (ex. les tâches ne peuvent pas être créées ou supprimées depuis l'application web, seul l'avancement peut être modifié), et des **Permission Sets** dédiés limitent l'exposition des tables aux seuls besoins de l'intégration.

### API et transport
- **CORS** est restreint explicitement aux origines de développement du frontend (`localhost:4200`/`4201`), avec `AllowCredentials`, plutôt qu'ouvert à tout domaine.
- Les entrées utilisateur (ex. formulaire de connexion) sont validées via les annotations de modèle (`ModelState.IsValid`) avant tout traitement.
- La documentation Swagger, incluant le schéma d'authentification Bearer, n'est exposée qu'en **environnement de développement**.

### Bonnes pratiques recommandées pour la mise en production
- Externaliser la clé de signature JWT (`Jwt:Key`) et les chaînes de connexion hors du fichier `appsettings.json` versionné, via des **variables d'environnement**, un **coffre de secrets** (Azure Key Vault, user-secrets) ou un gestionnaire de configuration dédié.
- Restreindre la liste `AllowedHosts` et la politique CORS aux domaines réels de production.
- Activer HTTPS strict et la rotation régulière des secrets (clé JWT, identifiants de service).

## Démarrage rapide

### Backend
```bash
cd Backend/Soroubat.Api
dotnet restore
dotnet run
```
L'API démarre sur `http://localhost:5227` (Swagger sur `/swagger` en développement).

### Frontend
```bash
cd Frontend/Soroubat-App
npm install
npm start
```
L'application est accessible sur `http://localhost:4200`.

### Extension AL
Ouvrir le dossier `AL_Extension/` avec l'extension **AL Language** dans VS Code, télécharger les symboles (`AL: Download Symbols`) puis publier l'extension sur l'environnement Business Central cible.

## Documentation complémentaire

Le dossier [`Markdown-Explain/`](./Markdown-Explain) contient des notes techniques détaillées sur l'architecture (extension AL, API .NET, tableau de bord).
