using System;
using System.Windows;
using ReservationSalles.Models;
using ReservationSalles.Services;

namespace ReservationSalles.Views
{
    public partial class EditReservationWindow : Window
    {
        // Copie de la réservation d'origine (pour modifications)
        public Reservation WorkingCopy { get; set; }

        // On garde un lien vers la Réservation d'origine pour la MAJ en mémoire (optionnel)
        private Reservation _original;

        public EditReservationWindow(Reservation reservation)
        {
            InitializeComponent();

            // On stocke l'original
            _original = reservation;

            // Créer une copie pour l'édition
            WorkingCopy = new Reservation
            {
                Id = reservation.Id,
                Room = reservation.Room,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                AttendeeLastName = reservation.AttendeeLastName,
                AttendeeFirstName = reservation.AttendeeFirstName,
                MeetingSubject = reservation.MeetingSubject
            };

            DataContext = WorkingCopy;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // On annule
            DialogResult = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // On peut parser la date / heure depuis le TextBox si besoin
                // Ex. WorkingCopy.StartTime = ...
                // Mais si le binding a marché, c'est déjà mis à jour.

                // Mise à jour dans la base
                DataService.UpdateReservation(WorkingCopy);

                // Mettre à jour l'original en mémoire (si on veut que l'affichage se rafraîchisse)
                _original.StartTime = WorkingCopy.StartTime;
                _original.EndTime = WorkingCopy.EndTime;
                _original.AttendeeLastName = WorkingCopy.AttendeeLastName;
                _original.AttendeeFirstName = WorkingCopy.AttendeeFirstName;
                _original.MeetingSubject = WorkingCopy.MeetingSubject;

                // On ferme la fenêtre avec DialogResult = true
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la sauvegarde : {ex.Message}", "Erreur");
            }
        }
    }
}