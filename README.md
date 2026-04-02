# Liquid Dispenser Automation System

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512bd4?logo=dotnet)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
[![WPF](https://img.shields.io/badge/UI-WPF-blue)](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
[![Blazor](https://img.shields.io/badge/Web-Blazor-512bd4?logo=blazor)](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)

### A Sophisticated Laboratory Automation Simulator

Welcome to the **Liquid Dispenser Automation System**—a high-fidelity "Software-in-the-Loop" simulation of a laboratory instrument. This project showcases a modern, multi-layered architecture designed for high-throughput reagent handling, featuring a **WPF desktop controller** and a **Blazor cloud dashboard**.

![Liquid_Dispenser_Demo](https://github.com/user-attachments/assets/157c0a8d-9e6f-4808-b11b-87e42008694c)

<img width="1907" height="877" alt="image" src="https://github.com/user-attachments/assets/f6c12400-72d9-44bd-903c-817be8d7e7c5" />

---

## Key Features

*   **Precision Robotics Simulation**: Controls an eight-tip pipette head for complex aspiration and dispensing cycles.
*   **Intelligent Tip Alignment**: Hardware-aware logic ensuring tips only dispense when aligned with target wells, minimizing waste and contamination.
- **Flexible Labware Support**: Compatible with industry-standard **96-well** and **384-well** plates, and customizable microfluidic chips.
*   **Dual-Control Interface**:
    *   **Desktop App**: Real-time instrument control and physical simulation playback.
    *   **Cloud Dashboard**: Remote monitoring (telemetry) and asynchronous job submission.
*   **High Performance**: Built on **.NET 10** with optimized asynchronous pipelines for smooth physical simulation.

---

## Technology Stack

| Layer | Technologies |
| :--- | :--- |
| **Desktop Front-end** | **WPF (C#)**, CommunityToolkit.Mvvm, Microsoft.Extensions.Hosting |
| **Web Dashboard** | **ASP.NET Core (Blazor Server)**, Interactive Telemetry Analytics |
| **Backend / API** | **ASP.NET Core Web API**, Entity Framework Core (SQLite) |
| **Core Logic** | .NET 10 Class Libraries, Interface Polymorphism, Domain Models |
| **Testing** | MSTest (Simulator Logic & Math) |

---

## Architecture & Component Design

The solution follows a clean, decoupled architecture:

1.  **LiquidDispenser.Core**: Pure domain entities and abstractions for labware (Plates, Chips, Wells).
2.  **LiquidDispenser.Simulator**: The "Physical" engine. Handles coordinate math, collision/alignment logic, and the instrument state machine.
3.  **LiquidDispenser.App (WPF)**: The primary operator interface. Acts as a thick client connecting to the physical simulator and streaming telemetry to the cloud.
4.  **LiquidDispenser.Cloud (Blazor)**: A modern web interface for remote laboratory management.
    *   **Telemetry Sink**: Receives real-time instrument status via REST API.
    *   **Job Broker**: Stores and serves pending transfer jobs to the field instruments.

---

## Getting Started (Visual Studio 2022)

To run the full "Software-in-the-Loop" experience:

1.  **Clone the Repository**:
    ```bash
    git clone https://github.com/omyint/liquid-dispenser.git
    ```
2.  **Open the Solution**: Open `LiquidDispenser.slnx` in Visual Studio 2026 (v18.0+ recommended for .NET 10 support).
3.  **Set Startup Projects**:
    - Right-click the Solution -> **Configure Startup Projects**.
    - Select **Multiple Startup Projects**.
    - Set both `LiquidDispenser.Cloud` and `LiquidDispenser.App` to **Start**.
4.  **Run**: Press `F5`. The Blazor dashboard will open in your browser, and the WPF simulation will launch simultaneously.
5.  **Enable Cloud Mode**: In the WPF app, toggle **Cloud Mode** to begin streaming telemetry and receiving remote jobs from the web dashboard.

---

## Cloud Management Dashboard

The **LiquidDispenser.Cloud** portal provides a high-level overview of the instrument:

*   **Real-time Telemetry**: See exactly where the eight-tip head is positioned in the lab.
*   **Remote Job Submission**: Create new "Transfer Jobs" (e.g., Aspirate A1 -> Dispense B2) that are queued for the instrument to pick up.

---

## Key Engineering Patterns

*   **MVVM with Source Generators**: Utilizing `CommunityToolkit.Mvvm` for clean, boilerplate-free ViewModels.
*   **Asynchronous Pipelines**: Use of `Task.Run` and `CancellationToken` for responsive physical movement simulation.
*   **Dependency Injection**: Modular service registration using `Microsoft.Extensions.DependencyInjection`.
*   **Software-in-the-Loop**: Designing software that expects a physical response, simulated via virtual hardware.
