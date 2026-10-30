# FoodSprint · CulinaryOS: Kitchen & Dining Hub

**CulinaryOS (FoodSprint)** is a commercial-grade restaurant back-of-house operations platform and menu engineering suite built with **ASP.NET Core Blazor (Interactive Server / .NET 8)**.

The system bridges front-of-house dining service, culinary station expediting, real-time recipe costing, and freshness-driven pantry inventory management into a single, cohesive interface.

---

## Table of Contents

- [Overview & Purpose](#overview--purpose)
- [Key Features](#key-features)
  - [1. Global Topbar & Command Palette](#1-global-topbar--command-palette)
  - [2. Recipe Costing & Menu Engineering](#2-recipe-costing--menu-engineering)
  - [3. Live Kitchen Display Rail (KDS)](#3-live-kitchen-display-rail-kds)
  - [4. Fresh Pantry & Yield Tracker](#4-fresh-pantry--yield-tracker)
  - [5. Shift & Station Configuration](#5-shift--station-configuration)
- [Architecture & Tech Stack](#architecture--tech-stack)
- [Project Directory Structure](#project-directory-structure)
- [Prerequisites](#prerequisites)
- [Installation & Setup](#installation--setup)
- [Running the Application](#running-the-application)
- [Data Storage & Persistence](#data-storage--persistence)
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

### 1. Global Topbar & Command Palette
- **Quick Search Command Palette (<kbd>⌘K</kbd> / <kbd>Ctrl+K</kbd>)**:
  - Global keyboard listener (`⌘K` on macOS, `Ctrl+K` on Windows/Linux) and Escape to dismiss.
  - Searches dynamically across live dishes, kitchen tickets, and pantry ingredients.
  - Interactive arrow-key navigation (<kbd>&uarr;</kbd> / <kbd>&darr;</kbd>) with <kbd>Enter</kbd> selection.
  - Deep-linking with parameter selection:
    - Selecting a dish opens `/?dish=...`, highlights the row, and loads its recipe into the cost estimator.
    - Selecting a table ticket opens `/kitchen?table=...&ticket=...` and highlights the ticket.
    - Selecting an inventory item opens `/pantry?sku=...`, clears restrictive filters, and highlights the stock row.
- **Dynamic Notifications Center**:
  - Automatically derived from live operational conditions (active critical allergens, items expiring within 24h, stock below par, shift standby mode).
  - Unread badge and count reflect actual unread alerts.
  - Persistent read states: marking an alert read (or "Mark all read") saves to disk and persists across restarts.
  - Clicking a notification marks it read and navigates directly to the target record.
- **Service Pacing & Pacing Indicators**:
  - Formatted average ticket time calculated dynamically from active tickets (`Avg Ticket: 8m 45s`).
  - Dynamic status pills:
    - `Service: On Pace` (sage, average elapsed &le; 9 min).
    - `Service: Near Par` (amber, average elapsed 9–12 min).
    - `Service: Exceeding Par` (coral, average elapsed &ge; 12 min).
    - `Service: Standby / Prep` (muted, when shift is inactive).

### 2. Recipe Costing & Menu Engineering (`/`)
- **Interactive Cost Estimator**:
  - Live target food cost slider (15% to 45%) with industry target markers (28–32%).
  - Dynamic menu price suggestion and target margin dial rendered via SVG / CSS conic gradient.
  - Sub-recipe ingredient breakdown with editable costs and live addition/removal of ingredients.
  - Real-time gross profit margin (%) and dollar contribution calculation per dish.
  - Dynamic category comparison: calculates exact margin percentage above or below the category average.
- **Menu Profitability Matrix**:
  - Dish catalog organized by courses (*Starters*, *Mains*, *Desserts*).
  - Dynamic BCG matrix classification (*Star*, *Plowhorse*, *Puzzle*, *Dog*) evaluated against category margin and volume averages.
  - Interactive item selection: clicking any dish loads its recipe and cost structure directly into the estimator.
  - Availability toggles (Active vs 86'd) with immediate state synchronization.
  - Filters by category (*Starters*, *Mains*, *Desserts*) and classification (*Stars*, *Plowhorses*, *Puzzles*, *Dogs*).
  - Complete recipe lifecycle: create new recipes, update existing ones, or delete obsolete dishes.

### 3. Live Kitchen Display Rail (KDS) (`/kitchen`)
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
  - **My Station Quick-Switch**: One-click button to filter the rail directly to the user's configured station.
  - **Fire New Ticket Modal**: Line cooks and expo leads can create new table orders directly from the UI, specifying table number, guests, server, allergens, rush status, and dishes.
  - **Station Analytics Modal**: Live performance metrics displaying ticket distribution and turnaround pace.
  - **Dynamic Service Insight**: 100% computed operational summary identifying the fastest and slowest kitchen stations.

### 4. Fresh Pantry & Yield Tracker (`/pantry`)
- **Dynamic KPI Dashboard**:
  - **Total Stock Par Value**: Computed real-time valuation of current on-hand inventory.
  - **Stock Par Health Difference**: Dynamic comparison vs total target par (e.g. `+4% vs target par`).
  - **Spoilage Risk (48h)**: Aggregate dollar value of stock expiring within 48 hours.
  - **Items Below Par**: Real-time counter of ingredients requiring immediate reordering.
  - **Turnover Velocity**: Operational inventory velocity metric calculated from par ratios (`3.8x / mo`), paired with health status (*Healthy inventory velocity*).
- **Stock Par & Freshness Management**:
  - Visual par progress bars with target par thresholds.
  - Expiry countdown pills with color-coded freshness states (`good`, `warn`, `danger`).
  - Storage location tracking across *Walk-in Cooler*, *Dry Larder*, and *Freezer*.
- **Ordering & Batch Purchase Orders (PO)**:
  - Single-click **+ Order** button to restock individual ingredients.
  - Multi-item row selection with floating batch action pill to generate combined purchase orders.
  - **Add Ingredient / PO Modal**: Form to add new ingredients into inventory with SKU, storage zone, par thresholds, unit costs, lot numbers, and supplier details.
  - Ingredient deletion to prune deprecated stock items.

### 5. Shift & Station Configuration
- **Operational Shift Toggle**:
  - **Dinner Service Active**: Table load reflects seated dining tables (`18 / 24`, `75%`), tickets flow normally, active pace tracking.
  - **Service Standby / Prep**: Table load drops to `0 / 24` (`0%`), KDS rail switches to prep mode, topbar and notifications reflect standby state.
- **Station & Profile Configuration Modal**:
  - Allows assigning stations (*Garde Manger*, *Expo & Pass*, *Hot Line & Grill*, *Sauté & Pasta*, *Pastry Bar*) and roles (*Expo Lead*, *Executive Chef*, *Sous Chef*, *Line Cook*, *Pastry Chef*).
  - Profile avatar initials, sidebar label, and topbar update dynamically.
  - KDS rail pre-selects the user's station for tickets and adds a "My Station" quick filter.

---

## Architecture & Tech Stack

- **Framework**: [.NET 8.0](https://dotnet.microsoft.com/) / ASP.NET Core Blazor Server.
- **Rendering Mode**: Blazor `InteractiveServer` for zero-latency, server-synchronized real-time UI updates.
- **State Management**: Centralized singleton service (`CulinaryService`) managing state synchronization across all connected clients.
- **Data Persistence**: File-backed JSON data store (`Data/culinary_store.json`) with atomic file writes, ensuring data persists across server restarts and page refreshes.
- **Security / Keys**: ASP.NET Core Data Protection configured with isolated local workspace storage (`Data/Keys/`).
- **Styling**: Bespoke Culinary Design System (`DM Sans`, `DM Serif Display`, `JetBrains Mono`) with custom SVG icons (`Icon.razor`).
- **Testing**: xUnit test suite (`FoodSprint.Tests`) with 21 passing automated tests covering domain models, calculations, workflows, search, and persistence.

---

## Project Directory Structure

```text
Sprint_Midterms/
├── Components/
│   ├── App.razor                     # Root HTML host document, fonts, and keyboard listeners
│   ├── Routes.razor                  # Blazor router and route table
│   ├── _Imports.razor                # Global component namespace imports
│   ├── Common/
│   │   └── Icon.razor                # Reusable vector SVG icons (20+ culinary icons)
│   ├── Layout/
│   │   ├── MainLayout.razor          # Topbar, breadcrumbs, search modal, notifications, pace
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
│   └── FoodSprint.Tests/             # Automated xUnit test suite (21 tests)
│       ├── MenuEngineeringTests.cs  # Unit tests for margin, pricing, and BCG matrix
│       ├── KitchenTicketTests.cs     # Unit tests for tickets, allergens, pacing, and expediting
│       ├── PantryInventoryTests.cs   # Unit tests for par valuations, risk, and POs
│       └── PersistenceTests.cs       # Unit tests for JSON data store, read states, and search
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

## Data Storage & Persistence

### Persistence Mechanism
`CulinaryService` automatically loads from and persists to `Data/culinary_store.json`.
- If the file does not exist on initial startup, the service initializes itself with 12 seed dishes, 3 active table tickets, and 8 pantry inventory lots, then serializes the data to disk.
- All subsequent additions, updates, deletions, notification read states, and shift toggles are automatically persisted to disk.

---

## Automated Testing

The solution includes an automated xUnit test suite (`tests/FoodSprint.Tests`) with **21 automated tests** covering all business logic, margin formulas, matrix classifications, kitchen workflows, stock calculations, notifications, and data persistence.

To execute the test suite:

```powershell
dotnet test tests/FoodSprint.Tests/FoodSprint.Tests.csproj
```

### Test Suite Summary (21 Passed / 0 Failed):
- **`MenuEngineeringTests`** (6 tests): Profit margin math, contribution margin, dish creation, updates, deletions, availability toggles, and dynamic BCG matrix classification (Star, Puzzle, Plowhorse, Dog).
- **`KitchenTicketTests`** (6 tests): Ticket creation, course rendering, critical allergen identification, rush toggles, bump workflows, dynamic average ticket time calculations, and service insight generation.
- **`PantryInventoryTests`** (5 tests): Dynamic stock valuation, 48-hour spoilage risk calculations, below-par detection, individual restocking, batch PO generation, turnover velocity, and par health calculations.
- **`PersistenceTests`** (4 tests): Multi-instance JSON data store reload, persistent notification read states across restarts, shift toggle operational state changes, and unified search matching with deep links.

---

## Known Limitations

- **Single Tenant / Workspace Storage**: The application persists state to a local JSON file (`Data/culinary_store.json`). In a large-scale multi-unit chain enterprise, this would be connected to a relational database (e.g. PostgreSQL / SQL Server) using Entity Framework Core.
- **Thermal Printer Emulation**: Physical kitchen POS receipt printing is emulated via digital KDS cards rather than hardware ESC/POS ESC-sequence printers.

---

## Future Enhancements

- **Real-Time SignalR WebSockets**: Multi-terminal sync across physically separated kitchen stations with audio bell chimes when expo calls fire.
- **Direct Barcode / QR Scanner Support**: USB/Bluetooth barcode scanner integration on the pantry page for instant lot-number stock intake.
- **Multi-Location Menu Sync**: Support for multi-branch price adjustments and localized ingredient purveyors.
