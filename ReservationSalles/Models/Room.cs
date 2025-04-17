using System.Collections.Generic; // Nécessaire

namespace ReservationSalles.Models // Assurez-vous que le namespace est correct
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }

        // --> CETTE LIGNE EST RECOMMANDÉE <--
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}