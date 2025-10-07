namespace automobile_backend.Models.Entities
{
    public enum Enums
    {
        Admin,
        Employee,
        Customer
    }
    public enum FuelType
    {
        Petrol,
        Diesel,
        Electric,
        Hybrid
    }

    public enum AppointmentStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Cancelled,
        PendingApproval
    }

    public enum ModificationStatus
    {
        Requested,
        Approved,
        Rejected,
        Completed
    }

    public enum PaymentMethod
    {
        CreditCard,
        DebitCard,
        Cash,
        BankTransfer
    }
}
