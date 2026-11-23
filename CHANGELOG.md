# Changelog

All notable changes to Lancy (formerly FCMS Pro) are documented here. Format loosely follows
[Keep a Changelog](https://keepachangelog.com/).

## [2.1.0]

### Added
- New Lancy logo: an "L" whose foot is a lance blade, with refreshed app icons, installer icons and website branding.

### Changed
- Better small-window layout: pages no longer waste 48px on each side, so there is more room on narrow or split-screen windows.
- Client and commission detail pages now wrap their totals and action buttons instead of running off the edge.

### Fixed
- **Automatic updates now work.** The update feed is now signed (the app rejected unsigned feeds, so no update was ever offered), and the downloaded installer keeps its `.exe` name so the silent install actually runs. Versions 2.0.0 and 2.0.1 cannot update themselves: install 2.1.0 manually once from the download page, and later updates are automatic.
- The updater now writes its progress to the app log, which makes "no update appeared" reports diagnosable.

## [2.0.1]

### Changed
- Maintenance release that verifies Lancy's in-app update pipeline end to end (signed release feed, update prompt, silent install). No functional changes from 2.0.0.

## [2.0.0]

FCMS Pro is now **Lancy**. Same local-first app, new name, new look, and new tools for getting paid.

### Added
- **Invoice PDF**: export a client-ready invoice from any commission page, with your business details, the job, payments received and the balance due.
- **CSV export**: save clients, commissions, payments and expenses as spreadsheet files from the Backup page.
- Sidebar version badge with a one-click "Check for updates".
- Your existing FCMS Pro data (database, attachments, backups) is copied into Lancy automatically on first launch. The old install is left untouched.

### Changed
- New brand: Lancy logo, app icons, installers and website.
- Redesigned sidebar with grouped sections and a clearer active-page indicator; softer corners and spacing across the app.
- Updates now come from the new Lancy release feed and are signed with a new key. Install this version manually once; later updates are automatic.

### Fixed
- The active sidebar item was unreadable in the Light theme.
- Saving a backup now offers a JSON file type instead of PDF.

## [1.6.9]

### Fixed
- Fixed opaque dark bounding box on the FCMS Pro wordmark logo in the Welcome / Onboarding window by converting `logo-wordmark.png` to a transparent RGBA asset.
- Fixed Windows taskbar icon binding by explicitly declaring `AppUserModelID` in `Program.cs` and installer shortcuts, and binding `WindowIcon` across window constructors.

## [1.6.8]

### Fixed
- Fixed Windows taskbar icon showing default Avalonia framework icon instead of FCMS Pro brand logo.
- Fixed Inno Setup silent background update flags and added force-close application directive for seamless 1-click in-app updates.
- Fixed window icon asset URI resolution in UpdateService.

## [1.6.7]

### Added
- Added Ko-fi support link on the official website and in app SettingsView.

### Fixed
- Fixed Analytics PDF export and Tax Summary calculations to resolve data discrepancies.

### Changed
- Refreshed brand identity assets (new high-resolution application icons for Windows, macOS, and Linux, updated website logos and favicons).

## [1.6.6]

### Fixed
- Streamlined auto-updater pipeline, release notes markdown formatting, and appcast multi-OS enclosure generation.

## [1.6.5]

### Fixed
- Bumped application version to resolve update loop issues.

## [1.6.4]

### Fixed
- Fixed auto-updater XML generation bug (xmlns_sparkle vs xmlns:sparkle).
- Fixed styling of deleted commission and client labels in lists so they look distinct from regular active items.

## [1.6.3]

### Fixed
- Fixed missing Client Name column in the Commissions table.
- Fixed horizontal column alignment and wrapping issues on the Income table.

## [1.2.4]
- Added complete app-data wipe on uninstall to ensure fresh restarts without manual cleanup.
- Added a new Quick Start guide into the initial onboarding flow.

## [1.2.3]
- Added `VersionInfoCompany` and `VersionInfoCopyright` directives to the Windows setup installer.

## [1.2.2]
- Added Author, Company, and Product metadata to the executable to help build SmartScreen reputation.

## [1.2.1]

### Fixed
- Fixed GitHub Actions release displaying duplicate older artifacts by bumping version to properly trigger a fresh release creation.
- Re-fixed unit tests broken by module removals.

## [1.2.0]

### Removed
- Removed Invoices, Quotes, Receipts, and Templates modules completely to transition the app into a leaner, offline CRM for solo freelancers.

### Fixed
- Fixed dashboard KPI metrics to properly exclude payments associated with deleted clients/commissions.

### Changed
- Added loading animations (ProgressBar) to client, commission, and expense deletion.
- Migrated primary lists to WrapPanel to ensure components wrap elegantly when resizing the window.
- Updated text across Backup and Onboarding pages to remove legacy references and clarify features.
- Added default sample data generation (1 Client, 1 Commission, App Settings) upon creation of a new database.

## [1.1.3]

### Fixed
- Kanban board: the status dropdown and View/Edit buttons on each card did not work at all,
  caused by a binding path that resolved to the wrong parent inside the board's nested column/card
  structure.
- Commission status dropdown (table and kanban) sometimes displayed blank instead of the actual
  current status until manually reselected, caused by a timing gap between when the dropdown's
  option list and its selected value resolved. Fixed by having each row supply its own status
  list directly instead of reaching up to a shared one.
- A commission marked "Completed" with a past deadline was still counted as overdue. Only
  "Delivered" and "Cancelled" were previously excluded; "Completed" now is too, since finished
  work should not be flagged as overdue regardless of which of those statuses it ended in.
- Long commission/client/quote text on the Payments, Receipts, and Quotes pages was cut off
  mid-word. Now wraps onto a second line instead of truncating.
- Loading attachments on a Client, Invoice, or Quote could throw a database error ("SQLite does
  not support expressions of type 'DateTimeOffset' in ORDER BY clauses") instead of showing the
  attachment list.
- The collapsed sidebar rail had a visible gap at the top where its background didn't reach the
  edge, unlike the expanded sidebar, making it look unfinished at narrow window widths.

## [1.1.2]

### Fixed
- Light theme now actually changes the app's appearance. Previously the Settings > Appearance >
  Theme dropdown had no visible effect.
- Search boxes on Clients, Payments, Receipts, Quotes, Invoices, and Commissions no longer
  overflow past the window edge when the window is narrow.

### Changed
- Sharpened the app icon at small sizes (16px and 32px) for better clarity in title bars and
  taskbars.

## [1.1.1]

### Fixed
- Removed a stray developer-name reference from the onboarding terms screen.

## [1.1.0]

### Added
- Recurring expenses — set a monthly/biweekly/weekly schedule on an expense
  (software subscriptions, retainers) and it re-logs itself automatically.
- Global search — press `/` anywhere in the app to search clients,
  commissions, invoices, quotes, and expenses at once.
- Due-soon warnings on the Dashboard — a second banner alongside the
  existing overdue one, for commissions/invoices coming due in the next few
  days rather than only after the deadline has passed.
- File attachments on Clients, Invoices, and Quotes (previously Commissions
  only) — supports images and PDFs, useful for signed contracts and
  reference documents.
- Tax/quarterly summary export — year and quarter breakdown of income,
  expenses, and net, exportable as a PDF from the Analytics page.
- Expanded automated test coverage across services and view models.

## [1.0.2]

### Added
- Soft delete and a Trash page — deleted clients, commissions, payments,
  expenses, quotes, and invoices are recoverable for 30 days instead of
  being removed immediately.
- Kanban board view for Commissions, toggleable alongside the table view.
- Scheduled automatic local backups on app close, with a configurable
  retention count.
- Overdue notifications — a dashboard banner and KPI tiles for overdue
  commissions and invoices.
- Reference image attachments on Commissions.

### Changed
- Activity log moved from the main sidebar into Settings → Advanced.

### Removed
- Unused PNG receipt export path (never implemented, unreachable from the UI).

## [1.0.1]

### Fixed
- Crash when updating a commission or invoice already tracked in the same
  session (Entity Framework change-tracker key collision).
- Client/commission dropdowns silently appearing empty on a load failure.
- Main window and dialog windows opening partially off-screen on smaller
  displays.
- Thermal and A5 receipts missing the client's name and contact info.
- Several numeric input fields crashing when cleared to blank.

### Added
- Thermal (80mm) receipt format alongside the existing A5 layout.
- Optional automatic invoice generation when a commission is marked
  Delivered with an outstanding balance.

### Changed
- Sidebar auto-collapses on narrow windows; minimum window width lowered to
  support split-screen use.

## [1.0.0]

### Fixed
- Currency symbol setting not being applied to any price display in the app.
- Theme/accent color not persisting across restarts.
- Text fields and dropdowns rendering unreadably white-on-white when focused.
- Several numeric fields crashing when cleared to blank.

### Added
- Initial release: Dashboard, Clients, Commissions, Quotes, Payments,
  Receipts, Invoices, Expenses, Goals, Templates, Analytics, Backup, Logs,
  and Settings.
