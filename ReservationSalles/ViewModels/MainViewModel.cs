using ReservationSalles.Commands;
using ReservationSalles.Models; // <-- USING AJOUTÉ
using ReservationSalles.Services;
using ReservationSalles.Views; // Pour EditReservationWindow
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input; // <-- USING AJOUTÉ


namespace ReservationSalles.ViewModels
{
    public class MainViewModel : BaseViewModel // Hérite de BaseViewModel pour INotifyPropertyChanged
    {
        #region Properties

        // Utilisateur actuellement connecté
        private User _currentUser = null!; // Sera défini dans le constructeur
        public User CurrentUser
        {
            get => _currentUser;
            private set => _currentUser = value; // Pas de SetProperty si défini une fois
        }

        // Propriété pour vérifier facilement si l'utilisateur est admin (utilise UserRole)
        public bool IsCurrentUserAdmin => CurrentUser?.Role == UserRole.Admin;


        // Liste des salles disponibles
        private ObservableCollection<Room> _rooms = new ObservableCollection<Room>();
        public ObservableCollection<Room> Rooms
        {
            get => _rooms;
            set => SetProperty(ref _rooms, value); // Utilise SetProperty
        }

        // Salle sélectionnée dans la ListBox
        private Room _selectedRoom = null!;
        public Room SelectedRoom
        {
            get => _selectedRoom;
            // set => SetProperty(ref _selectedRoom, value); // Ancienne version
            set
            {
                if (SetProperty(ref _selectedRoom, value))
                {
                    CommandManager.InvalidateRequerySuggested(); // Notifie AddReservationCommand
                }
            }
        }

        // Liste des réservations
        private ObservableCollection<Reservation> _reservations = new ObservableCollection<Reservation>();
        public ObservableCollection<Reservation> Reservations
        {
            get => _reservations;
            set => SetProperty(ref _reservations, value); // Utilise SetProperty
        }

        // Réservation sélectionnée dans le DataGrid
        private Reservation _selectedReservation = null!;
        public Reservation SelectedReservation
        {
            get => _selectedReservation;
            // set => SetProperty(ref _selectedReservation, value); // Ancienne version
            set
            {
                if (SetProperty(ref _selectedReservation, value))
                {
                    CommandManager.InvalidateRequerySuggested(); // Notifie RemoveReservationCommand
                }
            }
        }

        // ---- Propriétés pour le formulaire de nouvelle réservation ----

        private DateTime _reservationDate = DateTime.Today;
        public DateTime ReservationDate
        {
            get => _reservationDate;
            // set => SetProperty(ref _reservationDate, value); // Ancienne version
            set
            {
                if (SetProperty(ref _reservationDate, value))
                {
                    CommandManager.InvalidateRequerySuggested(); // Peut affecter CanExecuteAddReservation
                }
            }
        }

        public ObservableCollection<TimeSpan> StartTimeOptions { get; } = new ObservableCollection<TimeSpan>();
        public ObservableCollection<TimeSpan> EndTimeOptions { get; } = new ObservableCollection<TimeSpan>();

        private TimeSpan? _selectedStartTime;
        public TimeSpan? SelectedStartTime
        {
            get => _selectedStartTime;
            set
            {
                if (SetProperty(ref _selectedStartTime, value))
                {
                    AdjustEndTimeIfNeeded();
                    CommandManager.InvalidateRequerySuggested(); // Notifie AddReservationCommand
                }
            }
        }

        private TimeSpan? _selectedEndTime;
        public TimeSpan? SelectedEndTime
        {
            get => _selectedEndTime;
            // set => SetProperty(ref _selectedEndTime, value); // Ancienne version
            set
            {
                if (SetProperty(ref _selectedEndTime, value))
                {
                    CommandManager.InvalidateRequerySuggested(); // Notifie AddReservationCommand
                }
            }
        }

        private string _attendeeFirstName = string.Empty;
        public string AttendeeFirstName
        {
            get => _attendeeFirstName;
            // set => SetProperty(ref _attendeeFirstName, value); // Ancienne version
            set { if (SetProperty(ref _attendeeFirstName, value)) CommandManager.InvalidateRequerySuggested(); } // Notifie aussi
        }

        private string _attendeeLastName = string.Empty;
        public string AttendeeLastName
        {
            get => _attendeeLastName;
            // set => SetProperty(ref _attendeeLastName, value); // Ancienne version
            set { if (SetProperty(ref _attendeeLastName, value)) CommandManager.InvalidateRequerySuggested(); } // Notifie aussi
        }

        private string _meetingSubject = string.Empty;
        public string MeetingSubject
        {
            get => _meetingSubject;
            // set => SetProperty(ref _meetingSubject, value); // Ancienne version
            set { if (SetProperty(ref _meetingSubject, value)) CommandManager.InvalidateRequerySuggested(); } // Notifie aussi
        }


        private string _message = string.Empty;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value); // Utilise SetProperty
        }

        #endregion

        #region Commands
        public ICommand AddReservationCommand { get; }
        public ICommand RemoveReservationCommand { get; }
        public ICommand RefreshCommand { get; }
        #endregion

        #region Constructor
        public MainViewModel(User currentUser)
        {
            CurrentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            // --- CORRECTION CS8622 APPLIQUÉE ICI ---
            AddReservationCommand = new RelayCommand(ExecuteAddReservation, CanExecuteAddReservation);
            RemoveReservationCommand = new RelayCommand(ExecuteRemoveReservation, CanExecuteRemoveReservation);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            // --- FIN CORRECTION ---
            GenerateTimeOptions();
            LoadRooms();
            LoadReservations();
            Logger.Log($"MainViewModel initialized for user '{CurrentUser.Username}' (Role: {CurrentUser.Role}).");
        }

        // Constructeur pour le designer (utilise UserRole)
        public MainViewModel() : this(new User { Username = "DesignerUser", Role = UserRole.User })
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Rooms.Add(new Room { Name = "Salle Design A", Capacity = 5 });
                Rooms.Add(new Room { Name = "Salle Design B", Capacity = 10 });
                if (Rooms.Any()) SelectedRoom = Rooms[0]; // Check if not empty
                Reservations.Add(new Reservation { Room = SelectedRoom ?? new Room(), StartTime = DateTime.Now, EndTime = DateTime.Now.AddHours(1), AttendeeFirstName = "Jean", AttendeeLastName = "Dupont", MeetingSubject = "Réunion Design" });
                Message = "Mode Design Activé";
            }
        }
        #endregion

        #region Data Loading and Refresh
        private void LoadRooms()
        {
            try
            {
                // Correction : Utiliser GetAllRooms
                var rooms = DataService.GetAllRooms();
                Rooms = new ObservableCollection<Room>(rooms);
                Logger.Log($"Rooms loaded ({Rooms.Count}) for MainViewModel.");
            }
            catch (Exception ex) { Logger.Log($"Error loading rooms in MainViewModel: {ex.Message}"); MessageBox.Show($"Erreur chargement salles : {ex.Message}", "Erreur"); Rooms.Clear(); }
        }

        private void LoadReservations()
        {
            try
            {
                // Correction : Utiliser GetAllReservations
                var reservations = DataService.GetAllReservations();
                Reservations = new ObservableCollection<Reservation>(reservations);
                Logger.Log($"Reservations loaded ({Reservations.Count}) for MainViewModel.");
            }
            catch (Exception ex) { Logger.Log($"Error loading reservations in MainViewModel: {ex.Message}"); MessageBox.Show($"Erreur chargement réservations : {ex.Message}", "Erreur"); Reservations.Clear(); }
        }

        // CORRECTION CS8622 : object? parameter
        private void ExecuteRefresh(object? parameter)
        {
            Message = "Rafraîchissement des données...";
            LoadRooms(); LoadReservations();
            Message = "Données rafraîchies.";
            Logger.Log("Data refreshed in MainViewModel by user action.");
        }
        #endregion

        #region Time Options Generation and Handling
        private void GenerateTimeOptions()
        {
            TimeSpan startTime = TimeSpan.FromHours(8), endTime = TimeSpan.FromHours(18), interval = TimeSpan.FromMinutes(30), currentTime = startTime;
            StartTimeOptions.Clear(); EndTimeOptions.Clear(); // Clear before adding
            while (currentTime < endTime) { StartTimeOptions.Add(currentTime); if (currentTime > startTime) EndTimeOptions.Add(currentTime); currentTime = currentTime.Add(interval); }
            EndTimeOptions.Add(endTime);
        }

        private void AdjustEndTimeIfNeeded()
        {
            if (SelectedStartTime.HasValue && SelectedEndTime.HasValue && SelectedEndTime <= SelectedStartTime.Value)
            {
                var nextValidEndTime = EndTimeOptions.FirstOrDefault(t => t > SelectedStartTime.Value);
                SelectedEndTime = nextValidEndTime != default ? nextValidEndTime : (TimeSpan?)null;
            }
            else if (SelectedStartTime.HasValue && !SelectedEndTime.HasValue)
            {
                var nextValidEndTime = EndTimeOptions.FirstOrDefault(t => t > SelectedStartTime.Value);
                SelectedEndTime = nextValidEndTime != default ? nextValidEndTime : (TimeSpan?)null;
            }
        }
        #endregion

        #region Add Reservation Logic
        // CORRECTION CS8622 : object? parameter
        private void ExecuteAddReservation(object? parameter)
        {
            if (!CanExecuteAddReservation(parameter)) { Message = "Veuillez sélectionner une salle, des horaires valides et remplir tous les champs."; return; }
            DateTime startDateTime = ReservationDate.Date + SelectedStartTime!.Value; DateTime endDateTime = ReservationDate.Date + SelectedEndTime!.Value;

            // Appel à IsTimeSlotAvailable avec SelectedRoom.Id (supposé correct)
            if (!DataService.IsTimeSlotAvailable(SelectedRoom!.Id, startDateTime, endDateTime))
            {
                Message = $"Le créneau {SelectedStartTime:hh\\:mm} - {SelectedEndTime:hh\\:mm} est déjà réservé pour la salle {SelectedRoom.Name}.";
                MessageBox.Show(Message, "Créneau indisponible", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }

            // Création réservation avec Room = SelectedRoom (comme dans votre original)
            var newReservation = new Reservation { Room = SelectedRoom, StartTime = startDateTime, EndTime = endDateTime, AttendeeFirstName = AttendeeFirstName, AttendeeLastName = AttendeeLastName, MeetingSubject = MeetingSubject };
            // Si vous avez ajouté RoomId, préférez:
            // var newReservation = new Reservation { RoomId = SelectedRoom.Id, StartTime = startDateTime, ... };

            try
            {
                var addedReservation = DataService.AddReservation(newReservation);
                if (addedReservation != null)
                {
                    Reservations.Add(addedReservation); Reservations = new ObservableCollection<Reservation>(Reservations.OrderBy(r => r.StartTime));
                    Message = $"Réservation ajoutée pour {AttendeeLastName} {AttendeeFirstName} dans la salle {SelectedRoom.Name}.";
                    Logger.Log($"Reservation added by {CurrentUser.Username}: ID {addedReservation.Id} for room {SelectedRoom.Name}");
                    SelectedStartTime = null; SelectedEndTime = null; AttendeeFirstName = string.Empty; AttendeeLastName = string.Empty; MeetingSubject = string.Empty; SelectedRoom = null!;
                }
                else { Message = "Erreur lors de l'ajout (conflit?)."; MessageBox.Show(Message, "Erreur d'ajout"); }
            }
            catch (Exception ex) { Logger.Log($"Error adding reservation in MainViewModel: {ex.Message}"); Message = "Erreur technique ajout."; MessageBox.Show(Message + $"\n{ex.Message}", "Erreur Technique"); }
        }

        // CORRECTION CS8622 : object? parameter
        private bool CanExecuteAddReservation(object? parameter) => SelectedRoom != null && SelectedStartTime.HasValue && SelectedEndTime.HasValue && SelectedEndTime > SelectedStartTime && !string.IsNullOrWhiteSpace(AttendeeFirstName) && !string.IsNullOrWhiteSpace(AttendeeLastName) && !string.IsNullOrWhiteSpace(MeetingSubject);
        #endregion

        #region Remove Reservation Logic
        // CORRECTION CS8622 : object? parameter
        private void ExecuteRemoveReservation(object? parameter)
        {
            if (SelectedReservation == null) return;
            // CORRECTION : La vérification IsCurrentUserAdmin est correcte, mais le message pourrait être plus précis
            if (!IsCurrentUserAdmin) { MessageBox.Show("Seuls les administrateurs peuvent supprimer des réservations depuis cette vue.", "Action non autorisée", MessageBoxButton.OK, MessageBoxImage.Warning); Message = "Suppression non autorisée."; return; }
            var result = MessageBox.Show($"Supprimer la réservation pour '{SelectedReservation.MeetingSubject}' ?", "Confirmer", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var idBackup = SelectedReservation.Id; // Backup ID before potential null
                    var subjectBackup = SelectedReservation.MeetingSubject;
                    // Appel RemoveReservation avec SelectedReservation.Id (supposé correct)
                    bool success = DataService.RemoveReservation(idBackup);
                    if (success) { Reservations.Remove(SelectedReservation); Message = $"Réservation '{subjectBackup}' supprimée."; Logger.Log($"Reservation ID {idBackup} removed by user {CurrentUser.Username}."); }
                    else { Message = "Erreur lors de la suppression."; MessageBox.Show(Message, "Erreur"); }
                }
                catch (Exception ex) { var idToRemove = SelectedReservation?.Id; Logger.Log($"Error removing reservation ID {idToRemove}: {ex.Message}"); Message = "Erreur technique suppression."; MessageBox.Show(Message + $"\n{ex.Message}", "Erreur Technique"); }
            }
            else { Message = "Suppression annulée."; }
        }

        // CORRECTION CS8622 : object? parameter
        private bool CanExecuteRemoveReservation(object? parameter) => SelectedReservation != null && IsCurrentUserAdmin; // La condition est correcte
        #endregion

        #region Edit Reservation Logic (called from MainWindow code-behind)
        public void RefreshReservationInList(int reservationId)
        {
            var updatedReservation = DataService.GetReservationById(reservationId);
            if (updatedReservation != null) { var existing = Reservations.FirstOrDefault(r => r.Id == reservationId); if (existing != null) { Reservations[Reservations.IndexOf(existing)] = updatedReservation; Message = "Réservation màj."; Logger.Log($"Reservation {reservationId} refreshed."); } else LoadReservations(); }
            else LoadReservations();
        }
        #endregion
    }
}