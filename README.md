# TrainDataManagement - Real-Time Locomotive Telemetry System

> **Note:** This project was developed during my Software Engineering Internship at **TÜBİTAK RUTE (Rail Transport Technologies Institute)**. 

## About The Project
TrainDataManagement is a full-stack, real-time telemetry simulation and Geographic Information System (GIS). It is designed to continuously generate, process, and visualize locomotive GPS, speed, and status data. The system handles complex real-world geographic challenges such as signal loss (blind zones) and route smoothing by applying advanced mapping algorithms directly on the server-side.

## Architecture & Modules
To ensure high scalability and adherence to the "Separation of Concerns" principle, the system is designed in an N-Tier architecture consisting of three main components:

### 1. Telemetry.Server (Backend API)
The core engine of the system, handling business logic, data persistence, and mathematical optimizations.
* **Map Matching & Gap Detection:** Analyzes incoming timestamp intervals; if a signal gap exceeds 2000 ms, the server automatically patches the missing route using PostgreSQL GeoJSON railway data.
* **Catmull-Rom Spline Interpolation:** A custom `RouteSmootherService` in C# smooths out sharp corners and zigzags from raw telemetry data, ensuring realistic, curved train movements before sending the data to the client.
* **Tech Stack:** C#, ASP.NET Core, Entity Framework Core (Code-First), PostgreSQL, Docker Compose, DTO Pattern.

### 2. Telemetry.Simulator (Hardware Mocking)
A multi-threaded console application designed to mimic physical locomotive hardware in the field.
* **Multi-Threading:** Uses `Task.WhenAll` to asynchronously simulate 10 separate locomotives simultaneously.
* **TCP Socket Communication:** Utilizes low-level TCP Sockets (`TcpClient` / `NetworkStream`) instead of HTTP to stream high-frequency telemetry payload in real-time.
* **Blind Zone Simulation:** Mathematically calculates geographic blind zones and cuts off the TCP signal to simulate realistic communication loss.

### 3. Telemetry.UI (Thin Client Frontend)
A lightweight, dependency-free presentation layer focused strictly on rendering data.
* **Visualizing Data:** Utilizes Vanilla JavaScript, HTML5, and **Leaflet.js** over OpenStreetMap.
* **Dynamic Rendering:** Draws active routes in blue, signal-loss patches in dashed gray, and station stops as red markers. Includes a dynamic animation mechanism to trace historical routes second-by-second based on timestamps.

## Getting Started
Follow these steps to run the simulation locally:

1. Clone the repository:
   ```bash
   git clone [https://github.com/ahmetsefaayaz/TrainDataManagement.git](https://github.com/ahmetsefaayaz/TrainDataManagement.git)
   ```

2. Start the database environment using Docker:
   ```bash
   docker-compose up -d
   ```

3. Run the **Telemetry.Server** to apply EF Core migrations and initialize the endpoints.
4. Run the **Telemetry.Simulator** to start generating real-time TCP telemetry data.
5. Open `index.html` from the **Telemetry.UI** project in your browser to watch the real-time GIS map.
