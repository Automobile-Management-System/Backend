﻿using System.Threading.Tasks;

namespace automobile_backend.InterFaces.IRepository
{
    public interface IEmployeeDashboardRepository
    {
        Task<int> GetTodayUpcomingAppointmentCountAsync(int employeeId);

        Task<int> GetInProgressAppointmentCountAsync(int employeeId);

        Task<List<object>> GetTodayRecentServicesAsync(int employeeId);

        Task<List<object>> GetTodayRecentModificationsAsync(int employeeId);
        Task<int> GetCompletedServiceCountAsync(int employeeId);
        Task<int> GetCompletedModificationCountAsync(int employeeId);



    }
}
