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
        Pending,
        InProgress,
        Completed,
        Cancelled,
        Rejected,
    }

    public enum ModificationStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled,
        Rejected,
    }

    public enum PaymentMethod
    {
        CreditCard,
        Cash,
        BankTransfer,
        Stripe
    }

    public enum Type
    {
        Modifications,
        Service
    }
}
