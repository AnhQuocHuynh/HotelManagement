namespace HotelManager.Models.Enums
{
    public enum RoomType
    {
        Standard = 0,
        Deluxe = 1,
        Suite = 2
    }

    public enum PaymentMethod
    {
        Cash = 0,
        CreditCard = 1,
        BankTransfer = 2,
        MobilePayment = 3
    }

    public enum EmployeePosition
    {
        Receptionist = 0,
        Cleaner = 1,
        Technician = 2,
        Manager = 3,
        Admin = 4,
    }

    public enum UserRole
    {
        Staff = 0,
        Manager = 1,
        Customer = 2,
        Admin = 3
    }

    public enum CustomerType
    {
        Single = 0,
        Family = 1,
        Group = 2,
        TourGroup = 3,
        Business = 4,
        VIP = 5
    }

    public enum BookingStatus
    {
        Pending = 0,
        Confirmed = 1,
        CheckedIn = 2,
        CheckedOut = 3,
        Completed = 4,
        Cancelled = 5,
        NoShow = 6
    }

    public enum RoomStatus
    {
        Available = 0,
        Occupied = 1,
        Pending = 2,
        UnderMaintenance = 3,
        Reserved = 4,
        OutOfService = 5
    }

    public enum AssignmentType
    {
        Cleaning = 0,
        Maintenance = 1,
        Inspection = 2
    }

    public enum AssignmentStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }

    public static class EnumExtensions
    {
        public static string ToDisplay(this RoomType type) => type.ToString();
        public static string ToDisplay(this PaymentMethod method) => method.ToString();
        public static string ToDisplay(this EmployeePosition position) => position.ToString();
        public static string ToDisplay(this UserRole role) => role.ToString();
        public static string ToDisplay(this CustomerType type) => type.ToString();
        public static string ToDisplay(this BookingStatus status) => status.ToString();
        public static string ToDisplay(this RoomStatus status) => status.ToString();
        public static string ToDisplay(this AssignmentType type) => type.ToString();
        public static string ToDisplay(this AssignmentStatus status) => status.ToString();
    }
}
