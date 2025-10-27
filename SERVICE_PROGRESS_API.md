# Service Progress API Documentation

This document describes the backend API endpoints for the Service Progress functionality, which allows employees to track time and manage service appointments with timer functionality.

## Overview

The Service Progress system provides:

- Timer management (Start, Pause, Stop)
- Service status updates
- Time tracking and logging
- Real-time progress monitoring

## API Endpoints

### Base URL

```
http://localhost:5001/api/ServiceProgress
```

### 1. Get Employee Service Progress

**GET** `/employee/{employeeId}`

Retrieves all service progress items for a specific employee.

**Parameters:**

- `employeeId` (int): The ID of the employee

**Response:**

```json
[
  {
    "appointmentId": 1,
    "serviceTitle": "Service - Toyota Camry",
    "customerName": "John Doe",
    "status": "InProgress",
    "serviceType": "Service",
    "appointmentDateTime": "2025-10-27T10:00:00Z",
    "isTimerActive": true,
    "currentTimerStartTime": "2025-10-27T10:30:00Z",
    "totalTimeLogged": 2.5,
    "timeLogs": [
      {
        "logId": 1,
        "startDateTime": "2025-10-27T09:00:00Z",
        "endDateTime": "2025-10-27T10:30:00Z",
        "hoursLogged": 1.5,
        "isActive": false
      },
      {
        "logId": 2,
        "startDateTime": "2025-10-27T10:30:00Z",
        "endDateTime": null,
        "hoursLogged": 0,
        "isActive": true
      }
    ]
  }
]
```

### 2. Get Service Progress by Appointment ID

**GET** `/appointment/{appointmentId}`

Retrieves service progress details for a specific appointment.

**Parameters:**

- `appointmentId` (int): The ID of the appointment

**Response:** Same as above but for a single appointment

### 3. Start Timer

**POST** `/timer/start`

Starts a timer for a service appointment.

**Request Body:**

```json
{
  "appointmentId": 1,
  "userId": 123
}
```

**Response:**

```json
{
  "success": true,
  "message": "Timer started successfully.",
  "activeTimeLog": {
    "logId": 5,
    "startDateTime": "2025-10-27T11:00:00Z",
    "endDateTime": null,
    "hoursLogged": 0,
    "isActive": true
  },
  "totalTimeLogged": 3.5
}
```

### 4. Pause Timer

**POST** `/timer/pause`

Pauses the active timer for a service appointment.

**Request Body:**

```json
{
  "appointmentId": 1,
  "userId": 123
}
```

**Response:**

```json
{
  "success": true,
  "message": "Timer paused successfully.",
  "activeTimeLog": {
    "logId": 5,
    "startDateTime": "2025-10-27T11:00:00Z",
    "endDateTime": "2025-10-27T12:30:00Z",
    "hoursLogged": 1.5,
    "isActive": false
  },
  "totalTimeLogged": 5.0
}
```

### 5. Stop Timer

**POST** `/timer/stop`

Stops the timer and marks the service as completed.

**Request Body:**

```json
{
  "appointmentId": 1,
  "userId": 123
}
```

**Response:**

```json
{
  "success": true,
  "message": "Timer stopped and service completed successfully.",
  "activeTimeLog": {
    "logId": 5,
    "startDateTime": "2025-10-27T11:00:00Z",
    "endDateTime": "2025-10-27T13:00:00Z",
    "hoursLogged": 2.0,
    "isActive": false
  },
  "totalTimeLogged": 7.0
}
```

### 6. Update Service Status

**PUT** `/status`

Updates the status of a service appointment.

**Request Body:**

```json
{
  "appointmentId": 1,
  "newStatus": "InProgress",
  "userId": 123,
  "notes": "Started working on the vehicle"
}
```

**Response:**

```json
{
  "message": "Service status updated successfully"
}
```

**Available Status Values:**

- `Pending`
- `InProgress`
- `Completed`
- `Cancelled`
- `Rejected`

### 7. Get Active Timer

**GET** `/timer/active/{appointmentId}/{userId}`

Retrieves the active timer for a specific appointment and user.

**Parameters:**

- `appointmentId` (int): The ID of the appointment
- `userId` (int): The ID of the user

**Response:**

```json
{
  "logId": 5,
  "startDateTime": "2025-10-27T11:00:00Z",
  "endDateTime": null,
  "hoursLogged": 0,
  "isActive": true
}
```

### 8. Get Total Logged Time

**GET** `/time-logged/{appointmentId}`

Retrieves the total logged time for an appointment.

**Parameters:**

- `appointmentId` (int): The ID of the appointment

**Response:**

```json
{
  "appointmentId": 1,
  "totalTimeLogged": 7.5
}
```

## Error Responses

All endpoints return appropriate HTTP status codes:

- `200 OK`: Successful operation
- `400 Bad Request`: Invalid request data or timer already running
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

**Error Response Format:**

```json
{
  "message": "Error description",
  "error": "Detailed error information"
}
```

## Usage Flow

### Typical Employee Workflow:

1. **View Assignments**: GET `/employee/{employeeId}` to see all assigned services
2. **Start Work**: POST `/timer/start` when beginning work on a service
3. **Pause if Needed**: POST `/timer/pause` for breaks or interruptions
4. **Resume Work**: POST `/timer/start` to continue (creates a new time log)
5. **Complete Service**: POST `/timer/stop` to finish and mark as completed
6. **Update Status**: PUT `/status` to manually update service status if needed

### Frontend Integration Notes:

- Use `isTimerActive` to show/hide timer controls
- Display `totalTimeLogged` for progress tracking
- Use `currentTimerStartTime` to show elapsed time in real-time
- The system automatically handles time calculations
- Stopping a timer automatically sets the appointment status to "Completed"

## Database Changes

The following fields were added to the `TimeLog` entity:

- `EndDateTime` - Made nullable for active timers
- `IsActive` - Boolean flag for running timers
- `CreatedAt` - Timestamp for when the log was created
- `Notes` - Optional notes for the time log

## Security Considerations

- Validate user permissions before allowing timer operations
- Ensure users can only operate timers for their assigned appointments
- Consider implementing role-based access control for status updates
