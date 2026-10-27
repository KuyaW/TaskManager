# Tandem

A task management app built with ASP.NET Core MVC and SQLite. Warm paper, dark ink, and
a ledger that keeps score — create tasks, set priorities and due dates, filter the list,
and tick things off.

## Run it

```powershell
dotnet run
```

Then open <http://localhost:5273> (or the HTTPS URL printed in the console).

On first run the app creates `Data/tandem.db`, applies the EF Core migration, and adds a
few example tasks so the ledger is not empty. Delete those tasks from the UI whenever you
like.

## Features

- Create, edit, view, complete, and delete tasks
- Title, notes, category, due date, priority (low / medium / high), and status
  (to do / in progress / done)
- Live search across titles, notes, and categories
- Filter by status, priority, and category; sort by due date, priority, recency, or title
- Completion progress, due-today, overdue, and completed counts
- "Clear completed" housekeeping action
- Keyboard shortcuts: `N` for a new task, `/` to focus search
- Server-side validation and anti-forgery protection on all mutations
- Responsive layout with a reduced-motion-friendly interface

The app is single-user: there are no accounts or sign-in. Everything lives in the local
SQLite file, which makes it a good fit for personal use or a small self-hosted setup.

## Stack

- ASP.NET Core MVC (`net10.0`)
- EF Core 10 with the SQLite provider
- Bootstrap for the base layer, with a custom stylesheet in `wwwroot/css/site.css`
- Google Fonts: Fraunces, Karla, and IBM Plex Mono

## Layout

```text
Controllers/TasksController.cs   CRUD, filtering, sorting, toggling, clearing
Data/AppDbContext.cs             EF Core context and indexes
Data/DbSeeder.cs                 First-run example tasks
Data/Migrations/                 InitialCreate migration
Models/TaskItem.cs               Task entity and enums
Models/TaskListViewModel.cs      Ledger view model and summary counts
Views/Tasks/                     Ledger, details, create, edit, delete views
wwwroot/css/site.css             The look and feel
```

## Database

The connection string lives in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=Data/tandem.db"
}
```

To start over, stop the app and delete `Data/tandem.db` (plus any `-wal` / `-shm` files).
The next run recreates the database and seed data.

If you change the model, add a migration:

```powershell
dotnet ef migrations add YourMigrationName --output-dir Data/Migrations
```
