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
        Upcoming,
        InProgress,
        Completed,
        Rejected,
    }

    public enum ModificationStatus
    {
        Pending,
        Upcoming,
        InProgress,
        Completed,
        Rejected,
    }

    public enum PaymentMethod
    {
        CreditCard,
        DebitCard,
        Cash,
        BankTransfer
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }

    public enum SlotsTime
    {
        EightAm,
        TenAm,
        TwelvePm,
        OnePm,
        ThreePm,
        FivePm
    }

    public enum Type
    {
        Modifications,
        Service 
    }
}
