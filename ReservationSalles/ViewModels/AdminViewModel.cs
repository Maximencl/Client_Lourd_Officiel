using ReservationSalles.Commands;
using ReservationSalles.Models; // <-- USING AJOUTÉ
using ReservationSalles.Services;
using ReservationSalles.Views; // Pour EditReservationWindow
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input; // <-- USING AJOUTÉ


namespace ReservationSalles.ViewModels
{
    public class AdminViewModel : BaseViewModel // Hérite de BaseViewModel pour INotifyPropertyChanged
    {
        #region Properties for User Management
        // Collection pour stocker les utilisateurs à afficher dans le DataGrid
        private ObservableCollection<User> _users = new ObservableCollection<User>();
        public ObservableCollection<User> Users
        {
            get => _users;
            set => SetProperty(ref _users, value); // Utilise SetProperty de BaseViewModel
        }

        // Utilisateur sélectionné dans le DataGrid
        private User _selectedUser = null!; // null! car initialisé par Load ou sélection UI
        public User SelectedUser
        {
            get => _selectedUser;
            // set => SetProperty(ref _selectedUser, value); // Met à jour via SetProperty // Ancienne version
            set
            {
                // Utilise SetProperty et CommandManager pour notifier les commandes dépendantes
                if (SetProperty(ref _selectedUser, value))
                {
                    CommandManager.InvalidateRequerySuggested(); // Notifie les commandes (RemoveUser, ResetPassword)
                }
            }
        }

        // Propriétés liées aux champs pour ajouter un nouvel utilisateur
        private string _newUsername = string.Empty;
        public string NewUsername
        {
            get => _newUsername;
            set => SetProperty(ref _newUsername, value);
        }

        private string _newPassword = string.Empty;
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        // Utilise l'enum UserRole pour le rôle (maintenant accessible avec le using)
        private UserRole _newRole = UserRole.User; // Valeur par défaut
        public UserRole NewRole
        {
            get => _newRole;
            set => SetProperty(ref _newRole, value);
        }

        // Message de feedback pour les opérations sur les utilisateurs
        private string _userManagementMessage = string.Empty;
        public string UserManagementMessage
        {
            get => _userManagementMessage;
            set => SetProperty(ref _userManagementMessage, value);
        }
        #endregion

        #region Properties for Room Management
        // Collection pour les salles
        private ObservableCollection<Room> _rooms = new ObservableCollection<Room>();
        public ObservableCollection<Room> Rooms
        {
            get => _rooms;
            set => SetProperty(ref _rooms, value);
        }

        // Salle sélectionnée
        private Room _selectedRoom = null!;
        public Room SelectedRoom
        {
            get => _selectedRoom;
            // set => SetProperty(ref _selectedRoom, value); // Ancienne version
            set
            {
                // Utilise SetProperty et CommandManager pour notifier les commandes dépendantes
                if (SetProperty(ref _selectedRoom, value))
                {
                    CommandManager.InvalidateRequerySuggested(); // Notifie RemoveRoomCommand
                }
            }
        }

        // Propriétés pour ajouter une nouvelle salle
        private string _newRoomName = string.Empty;
        public string NewRoomName
        {
            get => _newRoomName;
            set => SetProperty(ref _newRoomName, value);
        }

        // La capacité est entrée comme string, puis convertie
        private string _newRoomCapacityString = string.Empty;
        public string NewRoomCapacityString
        {
            get => _newRoomCapacityString;
            set => SetProperty(ref _newRoomCapacityString, value);
        }


        // Message de feedback pour les opérations sur les salles
        private string _roomManagementMessage = string.Empty;
        public string RoomManagementMessage
        {
            get => _roomManagementMessage;
            set => SetProperty(ref _roomManagementMessage, value);
        }
        #endregion

        #region Properties for Reservation Management
        // Collection pour les réservations
        private ObservableCollection<Reservation> _reservations = new ObservableCollection<Reservation>();
        public ObservableCollection<Reservation> Reservations
        {
            get => _reservations;
            set => SetProperty(ref _reservations, value);
        }

        // Réservation sélectionnée
        private Reservation _selectedReservation = null!;
        public Reservation SelectedReservation
        {
            get => _selectedReservation;
            // set => SetProperty(ref _selectedReservation, value); // Ancienne version
            set
            {
                // Utilise SetProperty et CommandManager pour notifier les commandes dépendantes
                if (SetProperty(ref _selectedReservation, value))
                {
                    CommandManager.InvalidateRequerySuggested(); // Notifie ModifyReservation, RemoveReservationAdmin
                }
            }
        }

        // Message de feedback pour les opérations sur les réservations
        private string _reservationManagementMessage = string.Empty;
        public string ReservationManagementMessage
        {
            get => _reservationManagementMessage;
            set => SetProperty(ref _reservationManagementMessage, value);
        }
        #endregion

        #region Commands
        // Commandes pour les actions de l'administrateur
        public ICommand AddUserCommand { get; }
        public ICommand RemoveUserCommand { get; }
        public ICommand ResetPasswordCommand { get; } // <-- COMMANDE AJOUTÉE
        public ICommand AddRoomCommand { get; }
        public ICommand RemoveRoomCommand { get; }
        public ICommand ModifyReservationCommand { get; }
        public ICommand RemoveReservationCommandAdmin { get; } // Nom différent pour éviter conflit si MainViewModel a une commande similaire
        public ICommand RefreshAllCommand { get; }
        #endregion

        #region Constructor
        public AdminViewModel()
        {
            // Initialisation des commandes avec les méthodes Execute et CanExecute associées
            AddUserCommand = new RelayCommand(ExecuteAddUser, CanExecuteAddUser);
            RemoveUserCommand = new RelayCommand(ExecuteRemoveUser, CanExecuteRemoveUser);
            ResetPasswordCommand = new RelayCommand(ExecuteResetPassword, CanExecuteResetPassword); // <-- INITIALISATION AJOUTÉE
            AddRoomCommand = new RelayCommand(ExecuteAddRoom, CanExecuteAddRoom);
            RemoveRoomCommand = new RelayCommand(ExecuteRemoveRoom, CanExecuteRemoveRoom);
            ModifyReservationCommand = new RelayCommand(ExecuteModifyReservation, CanExecuteModifyReservation);
            RemoveReservationCommandAdmin = new RelayCommand(ExecuteRemoveReservationAdmin, CanExecuteRemoveReservationAdmin);
            RefreshAllCommand = new RelayCommand(ExecuteRefreshAll); // Commande pour tout rafraîchir

            // Charger toutes les données nécessaires au démarrage du ViewModel
            LoadAllData();
        }
        #endregion

        #region Data Loading
        // Charge toutes les données (utilisateurs, salles, réservations)
        private void LoadAllData()
        {
            LoadUsers();
            LoadRooms();
            LoadReservations();
        }

        // Charge la liste des utilisateurs depuis le DataService
        private void LoadUsers()
        {
            try { Users = new ObservableCollection<User>(DataService.GetAllUsers()); }
            catch (Exception ex)
            {
                Logger.Log($"Error loading users: {ex.Message}");
                MessageBox.Show($"Erreur lors du chargement des utilisateurs : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Charge la liste des salles
        private void LoadRooms()
        {
            try { Rooms = new ObservableCollection<Room>(DataService.GetAllRooms()); }
            catch (Exception ex)
            {
                Logger.Log($"Error loading rooms: {ex.Message}");
                MessageBox.Show($"Erreur lors du chargement des salles : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Charge la liste des réservations
        private void LoadReservations()
        {
            try { Reservations = new ObservableCollection<Reservation>(DataService.GetAllReservations()); }
            catch (Exception ex)
            {
                Logger.Log($"Error loading reservations: {ex.Message}");
                MessageBox.Show($"Erreur lors du chargement des réservations : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region User Management Logic
        // Logique pour ajouter un utilisateur
        // CORRECTION CS8622 : object? parameter
        private void ExecuteAddUser(object? parameter)
        {
            if (!SecurityHelper.IsPasswordValid(NewPassword)) { UserManagementMessage = "Le mot de passe ne respecte pas les critères de complexité."; MessageBox.Show("Le mot de passe doit contenir au moins 8 caractères, une majuscule, une minuscule, un chiffre et un caractère spécial.", "Mot de passe invalide", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            var newUser = new User { Username = NewUsername, Password = NewPassword, Role = NewRole }; // Utilise UserRole
            try
            {
                var addedUser = DataService.AddUser(newUser);
                if (addedUser != null) { Users.Add(addedUser); UserManagementMessage = $"Utilisateur '{NewUsername}' ajouté avec succès."; Logger.Log($"User '{NewUsername}' added successfully."); NewUsername = string.Empty; NewPassword = string.Empty; NewRole = UserRole.User; } // Utilise UserRole
                else { UserManagementMessage = $"Erreur : L'utilisateur '{NewUsername}' existe déjà ou une erreur s'est produite."; MessageBox.Show($"Impossible d'ajouter l'utilisateur '{NewUsername}'. Il existe peut-être déjà.", "Erreur d'ajout", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
            catch (Exception ex) { Logger.Log($"Error adding user {NewUsername}: {ex.Message}"); MessageBox.Show($"Une erreur est survenue lors de l'ajout de l'utilisateur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error); UserManagementMessage = "Erreur lors de l'ajout de l'utilisateur."; }
        }

        // CORRECTION CS8622 : object? parameter
        private bool CanExecuteAddUser(object? parameter) => !string.IsNullOrWhiteSpace(NewUsername) && !string.IsNullOrWhiteSpace(NewPassword);

        // Logique pour supprimer un utilisateur
        // CORRECTION CS8622 : object? parameter
        private void ExecuteRemoveUser(object? parameter)
        {
            if (SelectedUser == null) return;
            if (SelectedUser.Username.Equals("admin", StringComparison.OrdinalIgnoreCase)) { MessageBox.Show("Le compte administrateur principal ne peut pas être supprimé.", "Suppression impossible", MessageBoxButton.OK, MessageBoxImage.Warning); UserManagementMessage = "Suppression du compte admin impossible."; return; }
            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer l'utilisateur '{SelectedUser.Username}' ?", "Confirmer la suppression", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var removedUserName = SelectedUser.Username; // Sauvegarder avant suppression potentielle de la sélection
                    bool success = DataService.RemoveUser(SelectedUser.Id);
                    if (success) { Users.Remove(SelectedUser); UserManagementMessage = $"Utilisateur '{removedUserName}' supprimé."; Logger.Log($"User '{removedUserName}' removed successfully."); /* SelectedUser devient null par la suppression de la liste */ }
                    else { UserManagementMessage = $"Erreur lors de la suppression de l'utilisateur '{removedUserName}'."; MessageBox.Show($"Impossible de supprimer l'utilisateur '{removedUserName}'.", "Erreur de suppression", MessageBoxButton.OK, MessageBoxImage.Error); }
                }
                catch (Exception ex) { Logger.Log($"Error removing user {SelectedUser?.Username}: {ex.Message}"); MessageBox.Show($"Une erreur est survenue lors de la suppression : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error); UserManagementMessage = "Erreur lors de la suppression."; }
            }
            else { UserManagementMessage = "Suppression annulée."; }
        }

        // CORRECTION CS8622 : object? parameter
        private bool CanExecuteRemoveUser(object? parameter) => SelectedUser != null && !SelectedUser.Username.Equals("admin", StringComparison.OrdinalIgnoreCase);

        // --- LOGIQUE RESET PASSWORD AJOUTÉE ---
        // CORRECTION CS8622 : object? parameter
        private void ExecuteResetPassword(object? parameter)
        {
            if (SelectedUser == null) return;
            string temporaryPassword = "Password123!"; // !! Exemple !! A adapter/sécuriser
            var confirmResult = MessageBox.Show($"Voulez-vous vraiment réinitialiser le mot de passe pour '{SelectedUser.Username}' ?\nLe nouveau mot de passe temporaire sera: {temporaryPassword}", "Confirmer la réinitialisation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmResult == MessageBoxResult.No) { UserManagementMessage = "Réinitialisation annulée."; return; }

            try
            {
                bool success = DataService.ResetUserPassword(SelectedUser.Id, temporaryPassword);
                if (success)
                {
                    Logger.Log($"Password reset initiated for user '{SelectedUser.Username}'.");
                    MessageBox.Show($"Le mot de passe pour l'utilisateur '{SelectedUser.Username}' a été réinitialisé à :\n\n{temporaryPassword}\n\nL'utilisateur devra le changer à sa prochaine connexion (fonctionnalité à implémenter si nécessaire).", "Mot de passe réinitialisé", MessageBoxButton.OK, MessageBoxImage.Information);
                    UserManagementMessage = $"Mot de passe pour '{SelectedUser.Username}' réinitialisé.";
                }
                else
                {
                    UserManagementMessage = $"Échec de la réinitialisation du mot de passe pour '{SelectedUser.Username}'. Vérifiez les logs.";
                    MessageBox.Show($"Impossible de réinitialiser le mot de passe pour '{SelectedUser.Username}'. Le mot de passe temporaire ne respecte peut-être pas les règles de complexité ou l'utilisateur n'existe pas.", "Échec", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex) { Logger.Log($"Error resetting password for user {SelectedUser.Username}: {ex.Message}"); MessageBox.Show($"Une erreur est survenue : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error); UserManagementMessage = "Erreur lors de la réinitialisation."; }
        }

        // CORRECTION CS8622 : object? parameter
        private bool CanExecuteResetPassword(object? parameter) => SelectedUser != null; // Actif si un user est sélectionné
        // --- FIN LOGIQUE RESET PASSWORD ---

        #endregion

        #region Room Management Logic
        // CORRECTION CS8622 : object? parameter
        private void ExecuteAddRoom(object? parameter)
        {
            if (!int.TryParse(NewRoomCapacityString, out int capacity) || capacity <= 0) { RoomManagementMessage = "La capacité doit être un nombre entier positif."; MessageBox.Show("Veuillez entrer une capacité valide (nombre entier positif).", "Capacité invalide", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            var newRoom = new Room { Name = NewRoomName, Capacity = capacity };
            try
            {
                var addedRoom = DataService.AddRoom(newRoom);
                if (addedRoom != null) { Rooms.Add(addedRoom); RoomManagementMessage = $"Salle '{NewRoomName}' ajoutée avec succès."; Logger.Log($"Room '{NewRoomName}' added successfully."); NewRoomName = string.Empty; NewRoomCapacityString = string.Empty; }
                else { RoomManagementMessage = $"Erreur : La salle '{NewRoomName}' existe déjà ou une erreur s'est produite."; MessageBox.Show($"Impossible d'ajouter la salle '{NewRoomName}'. Elle existe peut-être déjà.", "Erreur d'ajout", MessageBoxButton.OK, MessageBoxImage.Warning); }
            }
            catch (Exception ex) { Logger.Log($"Error adding room {NewRoomName}: {ex.Message}"); MessageBox.Show($"Une erreur est survenue lors de l'ajout de la salle : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error); RoomManagementMessage = "Erreur lors de l'ajout de la salle."; }
        }

        // CORRECTION CS8622 : object? parameter
        private bool CanExecuteAddRoom(object? parameter) => !string.IsNullOrWhiteSpace(NewRoomName) && int.TryParse(NewRoomCapacityString, out int capacity) && capacity > 0;

        // CORRECTION CS8622 : object? parameter
        private void ExecuteRemoveRoom(object? parameter)
        {
            if (SelectedRoom == null) return;
            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer la salle '{SelectedRoom.Name}' ?\nToutes les réservations associées seront également supprimées.", "Confirmer la suppression", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var removedRoomName = SelectedRoom.Name; // Sauvegarder avant suppression
                    bool success = DataService.RemoveRoom(SelectedRoom.Id);
                    if (success) { Rooms.Remove(SelectedRoom); RoomManagementMessage = $"Salle '{removedRoomName}' et ses réservations supprimées."; Logger.Log($"Room '{removedRoomName}' and associated reservations removed successfully."); LoadReservations(); /* Rafraîchir réservations */ }
                    else { RoomManagementMessage = $"Erreur lors de la suppression de la salle '{removedRoomName}'."; MessageBox.Show($"Impossible de supprimer la salle '{removedRoomName}'.", "Erreur de suppression", MessageBoxButton.OK, MessageBoxImage.Error); }
                }
                catch (Exception ex) { Logger.Log($"Error removing room {SelectedRoom?.Name}: {ex.Message}"); MessageBox.Show($"Une erreur est survenue lors de la suppression : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error); RoomManagementMessage = "Erreur lors de la suppression."; }
            }
            else { RoomManagementMessage = "Suppression annulée."; }
        }

        // CORRECTION CS8622 : object? parameter
        private bool CanExecuteRemoveRoom(object? parameter) => SelectedRoom != null;
        #endregion

        #region Reservation Management Logic (Admin View)
        // CORRECTION CS8622 : object? parameter
        private void ExecuteModifyReservation(object? parameter)
        {
            if (SelectedReservation == null) return;
            var reservationToEditCopy = new Reservation { Id = SelectedReservation.Id, Room = SelectedReservation.Room, /* RoomId = SelectedReservation.RoomId, // Si RoomId existe */ StartTime = SelectedReservation.StartTime, EndTime = SelectedReservation.EndTime, AttendeeFirstName = SelectedReservation.AttendeeFirstName, AttendeeLastName = SelectedReservation.AttendeeLastName, MeetingSubject = SelectedReservation.MeetingSubject };
            var editWindow = new EditReservationWindow(reservationToEditCopy);
            var result = editWindow.ShowDialog();
            if (result == true)
            {
                try
                {
                    bool success = DataService.UpdateReservation(editWindow.WorkingCopy);
                    if (success)
                    {
                        int index = Reservations.IndexOf(SelectedReservation);
                        if (index != -1) { var updatedReservation = DataService.GetReservationById(SelectedReservation.Id); if (updatedReservation != null) Reservations[index] = updatedReservation; else LoadReservations(); }
                        else LoadReservations();
                        ReservationManagementMessage = "Réservation modifiée avec succès."; Logger.Log($"Reservation ID {editWindow.WorkingCopy.Id} updated successfully by admin.");
                    }
                    else { ReservationManagementMessage = "La modification a échoué (conflit d'horaire ou erreur)."; MessageBox.Show("Impossible de modifier la réservation. Le créneau horaire est peut-être devenu indisponible ou une autre erreur s'est produite.", "Échec de la modification", MessageBoxButton.OK, MessageBoxImage.Warning); }
                }
                catch (Exception ex) { Logger.Log($"Error updating reservation ID {editWindow.WorkingCopy.Id} from admin view: {ex.Message}"); MessageBox.Show($"Une erreur est survenue lors de la modification : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error); ReservationManagementMessage = "Erreur lors de la modification."; }
            }
            else { ReservationManagementMessage = "Modification annulée."; }
        }

        // CORRECTION CS8622 : object? parameter
        private bool CanExecuteModifyReservation(object? parameter) => SelectedReservation != null;

        // CORRECTION CS8622 : object? parameter
        private void ExecuteRemoveReservationAdmin(object? parameter)
        {
            if (SelectedReservation == null) return;
            var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer la réservation pour '{SelectedReservation.MeetingSubject}'\ndu {SelectedReservation.StartTime:dd/MM HH:mm} au {SelectedReservation.EndTime:dd/MM HH:mm} ?", "Confirmer la suppression", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var idBackup = SelectedReservation.Id; // Backup ID
                    var subjectBackup = SelectedReservation.MeetingSubject;
                    bool success = DataService.RemoveReservation(idBackup);
                    if (success) { Reservations.Remove(SelectedReservation); ReservationManagementMessage = $"Réservation pour '{subjectBackup}' supprimée."; Logger.Log($"Reservation ID {idBackup} removed successfully by admin."); /* SelectedReservation devient null */}
                    else { ReservationManagementMessage = "Erreur lors de la suppression de la réservation."; MessageBox.Show("Impossible de supprimer la réservation.", "Erreur de suppression", MessageBoxButton.OK, MessageBoxImage.Error); }
                }
                catch (Exception ex) { Logger.Log($"Error removing reservation ID {SelectedReservation?.Id} by admin: {ex.Message}"); MessageBox.Show($"Une erreur est survenue lors de la suppression : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error); ReservationManagementMessage = "Erreur lors de la suppression."; }
            }
            else { ReservationManagementMessage = "Suppression annulée."; }
        }

        // CORRECTION CS8622 : object? parameter
        private bool CanExecuteRemoveReservationAdmin(object? parameter) => SelectedReservation != null;
        #endregion

        #region Refresh Logic
        // CORRECTION CS8622 : object? parameter
        private void ExecuteRefreshAll(object? parameter)
        {
            UserManagementMessage = RoomManagementMessage = ReservationManagementMessage = "";
            LoadAllData();
            UserManagementMessage = "Données utilisateurs rafraîchies."; RoomManagementMessage = "Données salles rafraîchies."; ReservationManagementMessage = "Données réservations rafraîchies.";
            Logger.Log("All data refreshed in Admin panel by user action.");
        }
        #endregion
    }
}