using System;
using System.ComponentModel.DataAnnotations.Schema; // Pour ForeignKey si vous l'utilisez explicitement

namespace ReservationSalles.Models
{
    /// <summary>
    /// Représente une réservation d'une salle pour un créneau horaire donné.
    /// </summary>
    public class Reservation
    {
        /// <summary>
        /// Identifiant unique de la réservation.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// La salle réservée.
        /// Propriété de navigation vers l'entité Room.
        /// null! indique à l'analyseur de nullabilité que cette propriété sera initialisée par EF Core.
        /// </summary>
        // Si vous utilisez une clé étrangère explicite, décommentez ForeignKey et ajoutez la propriété RoomId.
        // public int RoomId { get; set; }
        // [ForeignKey("RoomId")]
        public Room Room { get; set; } = null!;

        /// <summary>
        /// Date et heure de début de la réservation.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Date et heure de fin de la réservation.
        /// </summary>
        public DateTime EndTime { get; set; }

        // Ancienne propriété (remplacée par AttendeeFirstName, AttendeeLastName, MeetingSubject)
        // public string BookedBy { get; set; } = string.Empty;

        /// <summary>
        /// Prénom de la personne responsable de la réservation ou du participant principal.
        /// </summary>
        public string AttendeeFirstName { get; set; } = string.Empty;

        /// <summary>
        /// Nom de la personne responsable de la réservation ou du participant principal.
        /// </summary>
        public string AttendeeLastName { get; set; } = string.Empty;

        /// <summary>
        /// Objet ou motif de la réunion/réservation.
        /// </summary>
        public string MeetingSubject { get; set; } = string.Empty;

    }
}