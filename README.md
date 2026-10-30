# FoodSprint · CulinaryOS: Kitchen & Dining Hub

**CulinaryOS (FoodSprint)** is a commercial-grade restaurant back-of-house operations platform and menu engineering suite built with **ASP.NET Core Blazor (Interactive Server / .NET 8)**.

The system bridges front-of-house dining service, culinary station expediting, real-time recipe costing, and freshness-driven pantry inventory management into a single, cohesive interface.

---

## Table of Contents

- [Overview & Purpose](#overview--purpose)
- [Key Features](#key-features)
  - [1. Recipe Costing & Menu Engineering](#1-recipe-costing--menu-engineering)
  - [2. Live Kitchen Display Rail (KDS)](#2-live-kitchen-display-rail-kds)
  - [3. Fresh Pantry & Yield Tracker](#3-fresh-pantry--yield-tracker)
  - [4. Operational Navigation & App Shell](#4-operational-navigation--app-shell)
- [Architecture & Tech Stack](#architecture--tech-stack)
- [Project Directory Structure](#project-directory-structure)
- [Prerequisites](#prerequisites)
- [Installation & Setup](#installation--setup)
- [Running the Application](#running-the-application)
- [Data Storage & Domain Models](#data-storage--domain-models)
- [Automated Testing](#automated-testing)
- [Known Limitations](#known-limitations)
- [Future Enhancements](#future-enhancements)

---

## Overview & Purpose

Commercial kitchens face constant margin pressure, inventory spoilage risks, and kitchen expediting bottlenecks. CulinaryOS was created to solve these core operational challenges:

1. **Menu Intelligence**: Chefs and managers can model menu items down to the sub-recipe and labor level, projecting contribution margins and classifying dishes using the Boston Consulting Group (BCG) menu matrix (*Stars*, *Plowhorses*, *Puzzles*, and *Dogs*).
2. **Kitchen Expediting**: Expeditors and line leads can manage active table courses, enforce critical allergen isolation protocols (e.g., severe peanut anaphylaxis warnings), rush high-priority orders, and bump completed plates.
3. **Pantry Inventory & Yield**: Inventory managers can monitor par levels across storage zones (*Walk-in Cooler*, *Dry Larder*, *Freezer*), mitigate 48-hour spoilage waste, and generate batch purchase orders with a single click.

---

## Key Features

### 1. Recipe Costing & Menu Engineering (`/`)
- **Interactive Cost Estimator**:
  - Live target food cost slider (15% to 45%) with industry target markers (28–32%).
  - Dynamic menu price suggestion and target margin dial rendered via SVG / CSS conic gradient.
  - Sub-recipe ingredient breakdown with editable costs and live addition/removal of ingredients.
  - Real-time gross profit margin (%) and dollar contribution calculation per dish.
- **Menu Profitability Matrix**:
  - Dish catalog organized by courses (*Starters*, *Mains*, *Desserts*).
  - Dynamic BCG matrix classification (*Star*, *Plowhorse*, *Puzzle*, *Dog*) evaluated against category margin and volume averages.
  - Interactive item selection: clicking any dish loads its recipe and cost structure directly into the estimator.
  - Availability toggles (Active vs 86'd) with immediate state synchronization.
  - Filters by category (*Starters*, *Mains*, *Desserts*) and classification (*Stars*, *Plowhorses*, *Puzzles*, *Dogs*).
  - Complete recipe lifecycle: create new recipes, update existing ones, or delete obsolete dishes.

### 2. Live Kitchen Display Rail (KDS) (`/kitchen`)
- **Multi-Station Expediting Rail**:
  - Live tickets dispatched across kitchen stations: *Garde Manger*, *Hot Line*, *Sauté*, *Grill*, and *Pastry*.
  - Full course grouping (*Course 1 (Apps)*, *Course 2 (Mains)*) with course status tracking (*Plated*, *Fire*, *Cooking*).
  - Item modifiers rendered with chef annotations (e.g. *"Medium rare"*, *"Clean pan · fresh gloves"*).
- **Critical Allergen Callouts**:
  - Prominent visual warning banners and individual item allergen chips (e.g. `CRITICAL ALLERGEN: Severe Peanut Anaphylaxis`, `NO PEANUT`, `ALLERGY`).
- **Ticket Status & Expediting Lifecycle**:
  - **Expedite / Rush**: Instant toggle highlighting high-priority orders with visual accents.
  - **Advance / Bump**: Two-stage progression (*Cooking* &rarr; *Plated* &rarr; *Served / Bumped*).
  - **Station & Status Filtering**: Filter tickets by specific stations or view *Active*, *Cooking Only*, or *Plated Only*.
  - **Fire New Ticket Modal**: Line cooks and expo leads can create new table orders directly from the UI, specifying table number, guests, server, allergens, rush status, and dishes.
  - **Station Analytics Modal**: Live performance metrics displaying ticket distribution and turnaround pace.

### 3. Fresh Pantry & Yield Tracker (`/pantry`)
- **Dynamic KPI Dashboard**:
  - **Total Stock Par Value**: Computed real-time valuation of current on-hand inventory.
  - **Spoilage Risk (48h)**: Aggregate dollar value of stock expiring within 48 hours.
  - **Items Below Par**: Real-time counter of ingredients requiring immediate reordering.
  - **Turnover Velocity**: Operational inventory velocity metric.
- **Stock Par & Freshness Management**:
  - Visual par progress bars with target par thresholds.
  - Expiry countdown pills with color-coded freshness states (`good`, `warn`, `danger`).
  - Storage location tracking across *Walk-in Cooler*, *Dry Larder*, and *Freezer*.
- **Ordering & Batch Purchase Orders (PO)**:
  - Single-click **+ Order** button to restock individual ingredients.
  - Multi-item row selection with floating batch action pill to generate combined purchase orders.
  - **Add Ingredient / PO Modal**: Form to add new ingredients into inventory with SKU, storage zone, par thresholds, unit costs, lot numbers, and supplier details.
  - Ingredient deletion to prune deprecated stock items.

### 4. Operational Navigation & App Shell
- **Linen Sidebar Navigation**:
  - Persistent shift card indicating current dinner service status, active table load (`18 / 24`), and capacity progress.
  - Profile footer displaying active station and role with configuration modal (*Expo Lead*, *Executive Chef*, *Sous Chef*, *Line Cook*).
- **Topbar Control Center**:
  - Dynamic breadcrumb navigation.
  - Service pace indicator (Avg ticket duration & pace badge).
  - **Quick Search Modal (<kbd>⌘K</kbd>)**: Real-time unified search across dishes, active tickets, and pantry ingredients with direct navigation.
  - **Notification Center**: Notification badge and dropdown showing unread allergen alerts and spoilage risks.
  - **Shift Toggle**: Front-of-house dinner service active/standby switch.

---

## Architecture & Tech Stack

- **Framework**: [.NET 8.0](https://dotnet.microsoft.com/) / ASP.NET Core Blazor Server.
- **Rendering Mode**: Blazor `InteractiveServer` for zero-latency, server-synchronized real-time UI updates.
- **State Management**: Centralized singleton service (`CulinaryService`) managing state synchronization across all connected clients.
- **Data Persistence**: File-backed JSON data store (`Data/culinary_store.json`) with atomic writes, ensuring data persists across server restarts and page refreshes.
- **Security / Keys**: ASP.NET Core Data Protection configured with isolated local workspace storage (`Data/Keys/`).
- **Styling**: Bespoke Culinary Design System (`DM Sans`, `DM Serif Display`, `JetBrains Mono`) with custom SVG icons (`Icon.razor`).
- **Testing**: xUnit test suite (`FoodSprint.Tests`) with 100% pass rate across domain models, calculations, workflows, and persistence.

---

## Project Directory Structure

```text
Sprint_Midterms/
├── Components/
│   ├── App.razor                     # Root HTML host document and styles
│   ├── Routes.razor                  # Blazor router and route table
│   ├── _Imports.razor                # Global component namespace imports
│   ├── Common/
│   │   └── Icon.razor                # Reusable vector SVG icons (20+ culinary icons)
│   ├── Layout/
│   │   ├── MainLayout.razor          # Topbar, breadcrumbs, search modal, notifications
│   │   └── NavMenu.razor             # Sidebar navigation, shift card, station settings
│   └── Pages/
│       ├── Home.razor                # Recipe Costing & Menu Engineering
│       ├── KitchenDisplay.razor      # Live KDS Rail & Expediting Interface
│       ├── PantryInventory.razor     # Fresh Pantry & Stock Par Level Tracker
│       └── Error.razor               # Error boundary page
├── Data/
│   └── culinary_store.json           # File-backed JSON database store
├── Models/
│   └── CulinaryModels.cs             # Domain models (MenuItem, KitchenTicket, PantryItem, etc.)
├── Properties/
│   └── launchSettings.json           # Local web server launch profiles
├── Services/
│   └── CulinaryService.cs            # In-memory + persistent singleton business logic & CRUD
├── tests/
│   └── FoodSprint.Tests/             # Automated xUnit test suite
│       ├── MenuEngineeringTests.cs  # Unit tests for margin, pricing, and BCG matrix
│       ├── KitchenTicketTests.cs     # Unit tests for tickets, allergens, and expediting
│       ├── PantryInventoryTests.cs   # Unit tests for par valuations, risk, and POs
│       └── PersistenceTests.cs       # Unit tests for JSON data store persistence
├── wwwroot/
│   ├── app.css                       # CulinaryOS design tokens, typography, and UI rules
│   └── favicon.png                   # Application icon
├── FoodSprint.csproj                 # Primary ASP.NET Core Web project configuration
├── FoodSprint.sln                    # Visual Studio solution file
└── Program.cs                        # WebApplication bootstrapper and DI configuration
```

---

## Prerequisites

Before running the application, ensure the following software is installed on your workstation:

- **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** (or later, such as .NET 9 / 10).
- Compatible OS: Windows 10/11, macOS, or Linux.
- Modern web browser (Chrome, Edge, Firefox, Safari).

To verify your installation:
```powershell
dotnet --version
```

---

## Installation & Setup

1. **Clone the repository**:
   ```powershell
   git clone https://github.com/joelconstantineabaincia/Sprint_Midterms.git
   cd Sprint_Midterms
   ```

2. **Restore NuGet packages**:
   ```powershell
   dotnet restore FoodSprint.sln
   ```

3. **Build the solution**:
   ```powershell
   dotnet build FoodSprint.sln
   ```

---

## Running the Application

To start the local development web server:

```powershell
dotnet run --project FoodSprint.csproj
```

The application will launch on:
- **HTTP**: `http://localhost:5028`
- **HTTPS**: `https://localhost:7226`

Open your web browser and navigate to `http://localhost:5028`.

---

## Data Storage & Domain Models

### Persistence Mechanism
`CulinaryService` automatically loads from `Data/culinary_store.json`.
- If the file does not exist on initial startup, the service initializes itself with 12 seed dishes, 3 active table tickets, and 8 pantry inventory lots, then serializes the data to disk.
- All subsequent additions, updates, deletions, and status toggles are automatically persisted to disk.

### Core Domain Entities

1. **`MenuItem`**
   - `Id` (int), `Name` (string), `Category` (Starters / Mains / Desserts)
   - `MenuPrice` (decimal), `FoodCost` (decimal), `PrepLabor` (decimal), `PrepMinutes` (int)
   - `WeeklyVolume` (int), `IsAvailable` (bool), `SubIngredients` (List<SubIngredient>)
   - `ProfitMargin` (computed: `((Price - Cost) / Price) * 100`)
   - `ContributionMargin` (computed: `Price - Cost - Labor`)

2. **`KitchenTicket`**
   - `TicketId` (int), `TableNumber` (int), `Guests` (int), `ServerName` (string), `OrderNumber` (string)
   - `Station` (string), `AllergyAlert` (string), `IsRush` (bool), `Status` (Cooking / Plated / Served)
   - `Courses` (List<TicketCourse>), with `CourseName`, `Status`, and course items with modifiers and allergy tags.

3. **`PantryItem`**
   - `Id` (int), `Name` (string), `Sku` (string), `StorageZone` (Walk-in Cooler / Dry Larder / Freezer)
   - `CurrentStock` (double), `MinThreshold` (double), `Unit` (lb, qt, kg, pack)
   - `DaysUntilExpiry` (int), `FreshnessBadge` (string), `UnitCost` (decimal)
   - `LotNumber` (string), `LocationRack` (string), `Supplier` (string)
   - `IsLowStock` (computed: `CurrentStock <= MinThreshold || ParPercentage < 50`)

---

## Automated Testing

The solution includes an automated xUnit test suite (`tests/FoodSprint.Tests`) covering all business logic, margin formulas, matrix classifications, kitchen workflows, stock calculations, and data persistence.

To execute the test suite:

```powershell
dotnet test tests/FoodSprint.Tests/FoodSprint.Tests.csproj
```

### Verified Test Categories:
- **`MenuEngineeringTests`**: Verifies profit margin and contribution math, dish creation, updates, deletions, availability toggles, and BCG matrix classification (Star, Puzzle, Plowhorse, Dog).
- **`KitchenTicketTests`**: Verifies ticket lifecycle, course rendering, critical allergen identification, and expediting / bump workflows.
- **`PantryInventoryTests`**: Verifies dynamic stock valuation, 48-hour spoilage risk calculations, below-par detection, individual restocking, and batch PO generation.
- **`PersistenceTests`**: Verifies that state modifications persist across independent `CulinaryService` instances.

---

## Known Limitations

- **Single Tenant / Workspace Storage**: The application persists state to a local JSON file (`Data/culinary_store.json`). In a large-scale multi-unit chain enterprise, this would be connected to a relational database (e.g. PostgreSQL / SQL Server) using Entity Framework Core.
- **Thermal Printer Emulation**: Physical kitchen POS receipt printing is emulated via digital KDS cards rather than hardware ESC/POS ESC-sequence printers.

---

## Future Enhancements

- **Real-Time SignalR WebSockets**: Multi-terminal sync across physically separated kitchen stations with audio bell chimes when expo calls fire.
- **Direct Barcode / QR Scanner Support**: USB/Bluetooth barcode scanner integration on the pantry page for instant lot-number stock intake.
- **Multi-Location Menu Sync**: Support for multi-branch price adjustments and localized ingredient purveyors.

