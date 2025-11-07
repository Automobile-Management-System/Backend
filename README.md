# AutoServe 360 - Automobile Management System Backend

Welcome to the backend repository for **AutoServe 360**, a comprehensive, enterprise-level platform for automobile service management. This robust backend is built on .NET 9 and provides a secure, scalable, and feature-rich RESTful API to power the entire ecosystem.

## Table of Contents

- [Core Features](#core-features)
- [System Architecture](#system-architecture)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [1. Clone the Repository](#1-clone-the-repository)
  - [2. Configure Environment](#2-configure-environment)
  - [3. Set Up the Database](#3-set-up-the-database)
  - [4. Run the Application](#4-run-the-application)
- [API Documentation](#api-documentation)
- [Real-time Notifications](#real-time-notifications)
- [Docker Support](#docker-support)
  - [Build the Docker Image](#build-the-docker-image)
  - [Run the Docker Container](#run-the-docker-container)

## Core Features

- **User Authentication & Authorization**: Secure JWT-based authentication with roles (Admin, Employee, Customer) and external provider support (Google).
- **Multi-faceted Dashboards**:
    - **Admin Dashboard**: Analytics on revenue, user activity, and service performance.
    - **Employee Dashboard**: View assigned tasks, log work hours, and manage service progress.
    - **Customer Dashboard**: Track service history, manage vehicles, and view appointments.
- **Complete Service Lifecycle**:
    - **Appointment Scheduling**: Customers can book service appointments through available slots.
    - **Service Progress Tracking**: Real-time updates on service status (e.g., Pending, In-Progress, Completed).
    - **Modification Requests**: Customers can request changes to ongoing services, which admins can approve or deny.
- **Financial Management**:
    - **Stripe Integration**: Secure payment processing for all services.
    - **Dynamic PDF Invoicing**: Automated generation of detailed invoices using QuestPDF.
    - **Payment Analytics**: Admins can track revenue trends and payment details.
- **Vehicle & User Management**:
    - **Customer Vehicle Management**: Customers can register and manage their vehicles.
    - **Admin User Management**: Admins can view, manage, and monitor all users.
- **Integrated Communication**:
    - **AI Chatbot**: A Gemini-powered chatbot for instant customer support.
    - **Real-time Notifications**: SignalR pushes live notifications to the admin dashboard for critical events.
    - **Email Notifications**: SMTP integration for sending emails for appointments, payments, and other events.
- **Cloud Storage**: Firebase integration for storing and managing files like vehicle images or service documents.

## System Architecture

The backend follows a clean, layered architecture to ensure separation of concerns, maintainability, and scalability.

- **Controllers**: Handle incoming HTTP requests, validate input, and return responses. They orchestrate the business logic by calling services.
- **Services**: Contain the core business logic of the application. They interact with repositories to access and manipulate data.
- **Repositories**: Abstract the data access layer. They are responsible for all database operations, making it easy to switch data sources if needed.
- **Data (Entities & DbContext)**: Defines the database models and the Entity Framework Core context for data persistence.
- **Interfaces**: Used extensively for dependency injection, promoting loose coupling between layers.

## Technology Stack

- **Framework**: .NET 9 (ASP.NET Core)
- **Database**: Entity Framework Core 9 with SQL Server
- **Authentication**: JWT (JSON Web Tokens) & Google Authentication
- **API Documentation**: Swagger (OpenAPI)
- **Payment Gateway**: Stripe
- **PDF Generation**: QuestPDF
- **Real-time Communication**: SignalR
- **AI Integration**: Google Gemini
- **Cloud Storage**: Firebase Cloud Storage
- **Email**: SMTP
- **Containerization**: Docker

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (e.g., Express Edition)
- [Docker](https://www.docker.com/products/docker-desktop) (Optional)
- A code editor like [Visual Studio](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

## Getting Started

### 1. Clone the Repository
```sh
git clone https://github.com/Automobile-Management-System/Backend.git
cd Backend
```

### 2. Configure Environment

1.  **Update `appsettings.json`**: Open the `appsettings.json` file and fill in the required credentials for your local environment. **Do not commit secrets to version control.**

    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=AutoServeDB;Trusted_Connection=true;TrustServerCertificate=true;"
      },
      "Jwt": { "Key": "...", "Issuer": "...", "Audience": "..." },
      "Google": { "ClientId": "...", "ClientSecret": "..." },
      "Stripe": { "SecretKey": "sk_test_...", "WebhookSecret": "whsec_..." },
      "Gemini": { "ApiKey": "..." },
      "Firebase": { "BucketName": "...", "ServiceAccountKeyPath": "firebase-service-account.json" },
      "Smtp": { "Username": "...", "Password": "..." }
    }
    ```

2.  **Firebase Service Account**: Place your `firebase-service-account.json` file in the root of the project.

### 3. Set Up the Database

Run the Entity Framework Core migration command to create the database and its schema.
```sh
dotnet ef database update
```

### 4. Run the Application
```sh
dotnet run
```
The API will start and be accessible at `https://localhost:7125` or `http://localhost:5125`.

## API Documentation

Once the application is running, you can explore and test all the API endpoints using the interactive Swagger UI, available at:
**`https://localhost:7125/swagger`**

## Real-time Notifications

The application uses a SignalR hub for real-time admin notifications. The hub endpoint is:
`/hubs/admin-notify`

## Docker Support

### Build the Docker Image
```sh
docker build -t autoserve-backend .
```

### Run the Docker Container
You must provide the configuration as environment variables.
```sh
docker run -d -p 8080:8080 \
  -e ConnectionStrings:DefaultConnection="..." \
  -e Jwt:Key="..." \
  -e Jwt:Issuer="..." \
  -e Jwt:Audience="..." \
  -e Google:ClientId="..." \
  -e Google:ClientSecret="..." \
  -e Stripe:SecretKey="..." \
  -e Gemini:ApiKey="..." \
  --name autoserve-app autoserve-backend
```
The application will be accessible at `http://localhost:8080`.
