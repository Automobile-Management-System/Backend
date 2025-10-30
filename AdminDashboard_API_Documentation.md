# Admin Dashboard API Endpoints

This document provides information about the Admin Dashboard API endpoints that have been created to support your frontend dashboard.

## Base URL
```
http://localhost:5001/api/AdminDashboard
```

## Authentication
All endpoints require Admin role authentication. Include the JWT token in the Authorization header:
```
Authorization: Bearer {your-jwt-token}
```

## Available Endpoints

### 1. Get Complete Dashboard Data
**GET** `/api/AdminDashboard`

Returns all dashboard data including stats, charts, recent users, and alerts.

**Response Example:**
```json
{
  "stats": {
    "totalUsers": 156,
    "totalUsersChangeFromLastMonth": 12,
    "activeBookings": 48,
    "activeBookingsChangeFromLastMonth": 8,
    "monthlyRevenue": 15400,
    "monthlyRevenueChangeFromLastMonth": -300,
    "growthRate": 23.5,
    "growthRateChangeFromLastMonth": 5.3
  },
  "charts": {
    "weeklyRevenue": [
      { "day": "Mon", "revenue": 2500 },
      { "day": "Tue", "revenue": 2800 },
      { "day": "Wed", "revenue": 2200 },
      { "day": "Thu", "revenue": 3100 },
      { "day": "Fri", "revenue": 2900 },
      { "day": "Sat", "revenue": 3400 },
      { "day": "Sun", "revenue": 2700 }
    ],
    "weeklyAppointments": [
      { "day": "Mon", "appointmentCount": 12 },
      { "day": "Tue", "appointmentCount": 15 },
      { "day": "Wed", "appointmentCount": 9 },
      { "day": "Thu", "appointmentCount": 18 },
      { "day": "Fri", "appointmentCount": 14 },
      { "day": "Sat", "appointmentCount": 21 },
      { "day": "Sun", "appointmentCount": 16 }
    ]
  },
  "recentUsers": [
    {
      "userId": 1,
      "firstName": "Emily",
      "lastName": "Johnson",
      "email": "emily@example.com",
      "role": "Customer",
      "timeAgo": "2 hours ago",
      "createdDate": "2024-10-30T10:00:00Z"
    }
  ],
  "systemAlerts": [
    {
      "id": 1,
      "type": "warning",
      "message": "3 pending modification requests need review",
      "actionText": "Take Action →",
      "createdAt": "2024-10-30T08:00:00Z",
      "isRead": false
    }
  ]
}
```

### 2. Get Dashboard Statistics Only
**GET** `/api/AdminDashboard/stats`

Returns only the dashboard statistics (total users, bookings, revenue, growth rate).

### 3. Get Dashboard Charts Data
**GET** `/api/AdminDashboard/charts`

Returns weekly revenue and appointments chart data.

### 4. Get Recent Users
**GET** `/api/AdminDashboard/recent-users?limit=10`

Returns a list of recently registered users.

**Query Parameters:**
- `limit` (optional): Number of users to return (1-50, default: 10)

### 5. Get System Alerts
**GET** `/api/AdminDashboard/alerts`

Returns a list of system alerts and notifications.

### 6. Mark Alert as Read
**PUT** `/api/AdminDashboard/alerts/{alertId}/mark-read`

Marks a specific alert as read.

**Path Parameters:**
- `alertId`: The ID of the alert to mark as read

## Error Responses

All endpoints return consistent error responses:

```json
{
  "message": "Error description",
  "error": "Detailed error information"
}
```

**Common HTTP Status Codes:**
- `200 OK`: Success
- `400 Bad Request`: Invalid request parameters
- `401 Unauthorized`: Authentication required
- `403 Forbidden`: Admin role required
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

## Integration Notes

1. **Data Mapping**: The API responses are structured to match your frontend dashboard components.

2. **Real-time Updates**: For real-time dashboard updates, consider implementing SignalR or periodic polling.

3. **Caching**: Consider implementing caching for dashboard data to improve performance.

4. **Pagination**: The recent users endpoint supports limiting results to prevent large responses.

5. **Time Zones**: All datetime values are returned in UTC. Convert to local time in your frontend as needed.

## Sample Frontend Integration

```javascript
// Fetch complete dashboard data
const fetchDashboardData = async () => {
  try {
    const response = await fetch('/api/AdminDashboard', {
      headers: {
        'Authorization': `Bearer ${authToken}`,
        'Content-Type': 'application/json'
      }
    });
    
    if (response.ok) {
      const dashboardData = await response.json();
      // Update your dashboard components with the data
      updateDashboardComponents(dashboardData);
    }
  } catch (error) {
    console.error('Error fetching dashboard data:', error);
  }
};

// Mark alert as read
const markAlertAsRead = async (alertId) => {
  try {
    const response = await fetch(`/api/AdminDashboard/alerts/${alertId}/mark-read`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${authToken}`,
        'Content-Type': 'application/json'
      }
    });
    
    if (response.ok) {
      // Refresh alerts list
      fetchSystemAlerts();
    }
  } catch (error) {
    console.error('Error marking alert as read:', error);
  }
};
```