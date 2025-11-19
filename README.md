# Kanban

A full **Kanban task-management system** built in **C#**, featuring a multi-layer backend architecture and a modern WPF UI.
This project includes:

* **Data Access Layer (DAL)** for persistent storage
* **Business Layer (BL)** for domain logic
* **Service Layer** exposing safe DTOs
* **WPF Frontend** providing an interactive Kanban board UI

The architecture was designed for clarity, separation of concerns, testing, and maintainability.

---

## Features

### Core Features

* Create, update, and delete tasks
* Move tasks between columns
* Manage boards and membership
* User authentication and ownership rules
* Dynamic task limits per column
* Persistent storage via DTO controllers
* Full WPF graphical interface
* Complete system load/save functionality

### Additional Capabilities

* Multiple boards per user
* Join and leave boards
* Transfer board ownership
* Data validation and error propagation
* Response wrapper for safe API calls
* Unit tests using NUnit

---

## System Architecture

This Kanban system follows a **three-layer architecture**:

```
┌──────────────────────────────────────────────┐
│                 WPF Frontend                 │
│       (Views, Models, BackendController)     │
└─────────────────────────▲────────────────────┘
                          │
┌─────────────────────────┴────────────────────┐
│               Service Layer                   │
│ (BoardService, TaskService, UserService, etc) │
└─────────────────────────▲────────────────────┘
                          │
┌─────────────────────────┴────────────────────┐
│               Business Layer                  │
│ (BoardBL, TaskBL, UserBL, Facades, Logic)     │
└─────────────────────────▲────────────────────┘
                          │
┌─────────────────────────┴────────────────────┐
│           Data Access Layer (DAL)             │
│    (DTOs + Controllers writing to storage)    │
└──────────────────────────────────────────────┘
```

### Layer Responsibilities

#### DataAccessLayer

Located in:

```
DataAccessLayer/
```

Contains:

* `BoardController.cs`
* `TaskController.cs`
* `UserController.cs`
* DTOs (`BoardDTO`, `TaskDTO`, `UserDTO`)
* Persistence logic (saving/loading)

Special behavior:
**DTO property setters automatically update persistent storage**.

---

#### BusinessLayer

Located in:

```
BusinessLayer/
```

Implements:

* `BoardBL`, `ColumnBL`, `TaskBL`, `UserBL`
* `BoardFacade`, `UserFacade`, `SystemAction`, `AuthAction`

Handles:

* All business rules
* Task movement rules
* Column capacity limits
* Board membership
* Ownership transfer
* Validation
* High-level orchestration

---

#### ServiceLayer

Located in:

```
ServiceLayer/
```

Includes:

* `BoardService.cs`
* `TaskService.cs`
* `UserService.cs`
* `BoardSL.cs`, `TaskSL.cs`, `UserSL.cs`
* `Response<T>` wrapper
* `FactoryService`

Responsible for:

* Exposing safe DTOs to UI
* Preventing BL or DAL classes from leaking to the frontend
* Returning success/error messages consistently

---

## WPF Frontend

Located in:

```
Frontend/  or  Presentation/
```

Includes:

* `KanbanBoardView.xaml`
* `KanbanMyBoards.xaml`
* `BoardModel`, `TaskModel`, `ColumnModel`, `UserModel`
* `BackendController`

Responsibilities:

* UI rendering
* User interactions
* Mapping responses to visual components
* Communicating with the Service Layer through `BackendController`

---

## Running the Project

### Requirements

* .NET 6 / .NET 7 / .NET Framework (depending on your solution)
* Visual Studio 2022 recommended
* Windows OS (for WPF)

### How to Run

1. Clone the repo:

   ```sh
   git clone https://github.com/<your-username>/Kanban.git
   ```
2. Open the solution (`Kanban.sln`) in Visual Studio.
3. Build the solution.
4. Set the WPF project as the startup project.
5. Run the program (F5).

The application will load data from storage (via DAL) or create fresh data if no storage exists.

---

## Usage Guide

### Creating a Board

* Log in or register a new user
* Create a board through the UI
* You automatically become the board owner

### Creating a Task

You can create tasks by:

* Providing title
* Description
* Due date

Tasks appear in the first column.

### Moving Tasks

Tasks can be moved forward/backward across columns while respecting:

* Column limits
* Task validation rules
* Ownership rules (assigned users)

### Editing Tasks

Users may edit:

* Title
* Description
* Due Date
* Assigned user

(All changes are instantly persisted.)

---

## Persistence

The system uses DTO Controllers that write to files or a storage backend.
Every mutation of a DTO property triggers a persistence update.

This ensures:

* No manual save/load required
* Strong consistency
* Automatic recovery on restart

---

## Unit Tests

NUnit tests cover:

* Task creation
* Task editing
* Movement rules
* Board initialization
* Membership rules
* Edge cases in business logic

To run:

```
dotnet test
```

---

## Folder Structure

Example structure:

```
Kanban/
├── BusinessLayer/
├── DataAccessLayer/
├── ServiceLayer/
├── Presentation/ (WPF)
├── Tests/
└── README.md
```

---

## Technologies Used

* C#
* .NET / .NET Framework
* WPF (XAML)
* NUnit
* Object-Oriented Design
* Multi-layer architecture
* Persistent storage via DTO controllers

---

## Future Improvements

* Cloud sync or shared remote board support
* REST API / Web frontend
* Drag-and-drop UI interactions
* Notifications (task assigned, due soon, etc.)
* Real-time collaboration using SignalR

---
