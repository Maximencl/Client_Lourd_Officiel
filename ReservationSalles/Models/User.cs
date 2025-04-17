namespace ReservationSalles.Models // Assurez-vous que le namespace est correct
{
    // --> CET ENUM EST ESSENTIEL <--
    public enum UserRole
    {
        Admin,
        User
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Stocke le hash
        public UserRole Role { get; set; } // Utilise l'enum
    }
}