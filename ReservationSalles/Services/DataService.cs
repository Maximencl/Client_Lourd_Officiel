using Microsoft.EntityFrameworkCore;
using ReservationSalles.Data;
using ReservationSalles.Models; // Assurez-vous que ce using est présent
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows; // Ajout pour MessageBox et Application.Current

namespace ReservationSalles.Services
{
    /// <summary>
    /// Fournit des méthodes pour interagir avec la base de données (Utilisateurs, Salles, Réservations).
    /// Utilise un DbContext statique pour la simplicité dans cet exemple.
    /// Envisager l'injection de dépendances pour des applications plus complexes.
    /// </summary>
    public static class DataService
    {
        // Le contexte de base de données. Instancié une seule fois.
        private static readonly ReservationContext _context = new ReservationContext();

        /// <summary>
        /// Initialise la base de données : applique les migrations et sème les données initiales si nécessaire.
        /// </summary>
        public static void InitializeDatabase()
        {
            try
            {
                // Applique les migrations en attente
                _context.Database.Migrate();
                Logger.Log("Database migration check completed.");

                // Sème les données initiales (admin, salles par défaut) si la base est vide
                Seed();
                Logger.Log("Database seeding check completed.");
            }
            catch (Exception ex)
            {
                // Log l'erreur critique. Dans une application réelle, on pourrait vouloir
                // arrêter l'application ou afficher un message plus clair à l'utilisateur.
                Logger.Log($"FATAL ERROR during database initialization: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                // Il est crucial de gérer cette erreur, potentiellement en arrêtant l'application
                // car elle ne peut pas fonctionner sans base de données valide.
                MessageBox.Show($"Erreur critique lors de l'initialisation de la base de données : {ex.Message}\nL'application va se fermer.", "Erreur Base de Données", MessageBoxButton.OK, MessageBoxImage.Error);
                // Vérifier si l'application a déjà démarré avant de la fermer
                if (Application.Current != null)
                {
                    Application.Current.Shutdown(-1); // Arrête l'application avec un code d'erreur
                }
                else
                {
                    // Si OnStartup n'a pas encore vraiment démarré, relancer peut être une option
                    throw;
                }
            }
        }


        /// <summary>
        /// Remplit la base de données avec des données initiales si elle est vide.
        /// Crée un utilisateur 'admin'/'admin' et 'user'/'user', ainsi que quelques salles par défaut.
        /// </summary>
        private static void Seed()
        {
            bool changesMade = false; // Flag pour savoir si on doit appeler SaveChanges

            // --- Seed Users ---
            try
            {
                // Note : .Any() sans condition est généralement traduisible.
                if (!_context.Users.Any())
                {
                    Logger.Log("Seeding initial users with simple passwords ('admin', 'user')...");
                    string adminPassword = "admin";
                    string userPassword = "user";
                    var adminPasswordHash = SecurityHelper.ComputeSha256Hash(adminPassword);
                    var userPasswordHash = SecurityHelper.ComputeSha256Hash(userPassword);

                    _context.Users.AddRange(
                        new User { Username = "admin", Password = adminPasswordHash, Role = UserRole.Admin },
                        new User { Username = "user", Password = userPasswordHash, Role = UserRole.User }
                    );
                    changesMade = true; // Users added
                    Logger.Log("Initial users prepared for seeding.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during user seeding check/preparation: {ex.Message}");
                // Si la vérification échoue, il est dangereux de continuer le seeding.
                throw; // Relancer l'exception pour qu'InitializeDatabase la capture.
            }

            // --- Seed Rooms ---
            try
            {
                if (!_context.Rooms.Any())
                {
                    Logger.Log("Seeding initial rooms...");
                    _context.Rooms.AddRange(
                        new Room { Name = "Salle A101", Capacity = 10 },
                        new Room { Name = "Salle B205", Capacity = 25 },
                        new Room { Name = "Salle C300 (Conférence)", Capacity = 50 }
                    );
                    changesMade = true; // Rooms added
                    Logger.Log("Initial rooms prepared for seeding.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during room seeding check/preparation: {ex.Message}");
                // Continuer ou pas ? Si les users ont été ajoutés mais pas les salles,
                // on pourrait être dans un état incohérent. Prévenir et potentiellement arrêter.
                throw; // Relancer l'exception
            }

            // --- Save Changes if anything was added ---
            if (changesMade)
            {
                try
                {
                    _context.SaveChanges();
                    Logger.Log("Seeding changes saved successfully.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"ERROR saving seeding changes: {ex.Message}");
                    // État potentiellement incohérent, l'initialisation doit échouer.
                    throw; // Relancer l'exception
                }
            }
            else
            {
                Logger.Log("No seeding required (database already contains data).");
            }
        }

        #region User Operations

        /// <summary>
        /// Récupère un utilisateur par son nom d'utilisateur (insensible à la casse).
        /// Utilise une évaluation côté client en raison des limitations de traduction LINQ.
        /// </summary>
        /// <param name="username">Le nom d'utilisateur.</param>
        /// <returns>L'utilisateur trouvé ou null.</returns>
        public static User? GetUser(string username)
        {
            try
            {
                // **CORRECTION APPLIQUÉE** : Évaluation côté client
                // Attention : peut être inefficace sur de grandes tables Users.
                return _context.Users
                               .AsEnumerable() // Bascule vers l'exécution en mémoire
                               .FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in GetUser searching for '{username}': {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// Récupère tous les utilisateurs.
        /// </summary>
        /// <returns>Une liste de tous les utilisateurs.</returns>
        public static List<User> GetAllUsers()
        {
            try
            {
                return _context.Users.ToList();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in GetAllUsers: {ex.Message}");
                return new List<User>();
            }
        }


        /// <summary>
        /// Ajoute un nouvel utilisateur à la base de données.
        /// Le mot de passe fourni doit être en clair et sera hashé par cette méthode.
        /// La complexité du mot de passe DOIT être validée AVANT d'appeler cette méthode.
        /// Vérifie l'unicité du nom d'utilisateur (insensible à la casse) côté client.
        /// </summary>
        /// <param name="user">L'utilisateur à ajouter (mot de passe en clair).</param>
        /// <returns>L'utilisateur ajouté avec son ID ou null en cas d'échec (ex: doublon, erreur DB).</returns>
        public static User? AddUser(User user)
        {
            // Validation de base
            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
            {
                Logger.Log("Attempted to add user with empty username or password.");
                return null;
            }

            // **CORRECTION APPLIQUÉE** : Vérification d'existence côté client
            bool userExists;
            try
            {
                userExists = _context.Users
                                     .AsEnumerable() // Bascule en mode client avant Any()
                                     .Any(u => string.Equals(u.Username, user.Username, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                // Si la lecture de la table échoue, on ne peut pas vérifier, donc on n'ajoute pas.
                Logger.Log($"Error checking user existence for '{user.Username}': {ex.Message}");
                return null;
            }

            if (userExists)
            {
                Logger.Log($"Attempted to add existing user: {user.Username}");
                return null; // L'utilisateur existe déjà
            }

            // L'utilisateur n'existe pas, on peut continuer l'ajout

            // Hashage du mot de passe
            string hashedPassword;
            try
            {
                hashedPassword = SecurityHelper.ComputeSha256Hash(user.Password);
            }
            catch (ArgumentNullException ex) // Gère si ComputeSha256Hash lève une exception pour null/vide
            {
                Logger.Log($"Error hashing password for user '{user.Username}': {ex.Message}");
                return null;
            }
            // Assignation du mot de passe hashé à l'objet User à ajouter
            user.Password = hashedPassword;


            // Ajout à la base
            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                Logger.Log($"User '{user.Username}' added successfully with ID {user.Id}.");
                return user; // Retourne l'utilisateur ajouté (avec son ID)
            }
            catch (DbUpdateException ex) // Erreur spécifique à la sauvegarde EF
            {
                Logger.Log($"Database error adding user {user.Username}: {ex.InnerException?.Message ?? ex.Message}");
                // Essayer de détacher l'entité pour éviter les problèmes si on réessaie plus tard
                DetachEntity(user);
                return null;
            }
            catch (Exception ex) // Autres erreurs potentielles
            {
                Logger.Log($"Generic error adding user {user.Username}: {ex.Message}");
                DetachEntity(user);
                return null;
            }
        }


        /// <summary>
        /// Supprime un utilisateur par son ID.
        /// Ne permet pas de supprimer l'utilisateur 'admin'.
        /// </summary>
        /// <param name="userId">L'ID de l'utilisateur à supprimer.</param>
        /// <returns>True si la suppression a réussi, sinon False.</returns>
        public static bool RemoveUser(int userId)
        {
            try
            {
                // Find est traduisible et efficace pour chercher par clé primaire.
                var user = _context.Users.Find(userId);
                if (user != null)
                {
                    // La comparaison .Equals se fait en mémoire sur l'objet trouvé, pas de problème de traduction.
                    if (user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Log($"Attempted to remove the main admin user (ID: {userId}). Operation aborted.");
                        return false;
                    }

                    _context.Users.Remove(user);
                    _context.SaveChanges();
                    Logger.Log($"User ID {userId} ('{user.Username}') removed successfully.");
                    return true;
                }
                Logger.Log($"Attempted to remove non-existent user with ID: {userId}.");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error removing user ID {userId}: {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// Réinitialise le mot de passe d'un utilisateur donné.
        /// La complexité du nouveau mot de passe DOIT être validée AVANT d'appeler cette méthode.
        /// </summary>
        /// <param name="userId">L'ID de l'utilisateur.</param>
        /// <param name="newPassword">Le nouveau mot de passe en clair.</param>
        /// <returns>True si la mise à jour a réussi, sinon False.</returns>
        public static bool ResetUserPassword(int userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                Logger.Log($"Password reset attempt failed for user ID {userId}: New password cannot be empty.");
                return false;
            }

            try
            {
                // Find est traduisible
                var user = _context.Users.Find(userId);
                if (user != null)
                {
                    user.Password = SecurityHelper.ComputeSha256Hash(newPassword);
                    _context.SaveChanges();
                    Logger.Log($"Password successfully reset for user ID {userId} ('{user.Username}').");
                    return true;
                }
                else
                {
                    Logger.Log($"Password reset attempt failed: User with ID {userId} not found.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error resetting password for user ID {userId}: {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// Réinitialise le mot de passe d'un utilisateur donné (version asynchrone).
        /// La complexité du nouveau mot de passe DOIT être validée AVANT.
        /// </summary>
        /// <param name="userId">L'ID de l'utilisateur.</param>
        /// <param name="newPassword">Le nouveau mot de passe en clair.</param>
        /// <returns>True si la mise à jour a réussi, sinon False.</returns>
        public static async Task<bool> ResetUserPasswordAsync(int userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                Logger.Log($"Password reset attempt (async) failed for user ID {userId}: New password cannot be empty.");
                return false;
            }

            try
            {
                // FindAsync est traduisible
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.Password = SecurityHelper.ComputeSha256Hash(newPassword);
                    await _context.SaveChangesAsync();
                    Logger.Log($"Password successfully reset (async) for user ID {userId} ('{user.Username}').");
                    return true;
                }
                else
                {
                    Logger.Log($"Password reset attempt (async) failed: User with ID {userId} not found.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error resetting password (async) for user ID {userId}: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Room Operations

        /// <summary>
        /// Récupère toutes les salles, triées par nom.
        /// </summary>
        public static List<Room> GetAllRooms()
        {
            try
            {
                // OrderBy et ToList sont traduisibles.
                return _context.Rooms.OrderBy(r => r.Name).ToList();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in GetAllRooms: {ex.Message}");
                return new List<Room>();
            }
        }

        /// <summary>
        /// Ajoute une nouvelle salle. Vérifie si une salle du même nom existe déjà (insensible à la casse, côté client).
        /// </summary>
        /// <param name="room">La salle à ajouter.</param>
        /// <returns>La salle ajoutée avec son ID, ou null si elle existe déjà ou en cas d'erreur.</returns>
        public static Room? AddRoom(Room room)
        {
            if (string.IsNullOrWhiteSpace(room.Name))
            {
                Logger.Log("Attempted to add room with empty name.");
                return null;
            }
            if (room.Capacity <= 0)
            {
                Logger.Log($"Attempted to add room '{room.Name}' with invalid capacity ({room.Capacity}).");
                return null;
            }

            // **CORRECTION APPLIQUÉE** : Vérification d'existence côté client
            bool roomExists;
            try
            {
                roomExists = _context.Rooms
                                      .AsEnumerable() // Bascule en mode client
                                      .Any(r => string.Equals(r.Name, room.Name, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                Logger.Log($"Error checking room existence for '{room.Name}': {ex.Message}");
                return null; // Ne pas ajouter si on ne peut pas vérifier
            }


            if (roomExists)
            {
                Logger.Log($"Attempted to add existing room: {room.Name}");
                return null;
            }

            try
            {
                _context.Rooms.Add(room);
                _context.SaveChanges();
                Logger.Log($"Room '{room.Name}' added successfully with ID {room.Id}.");
                return room;
            }
            catch (DbUpdateException ex) // Erreur spécifique à la sauvegarde EF
            {
                Logger.Log($"Database error adding room {room.Name}: {ex.InnerException?.Message ?? ex.Message}");
                DetachEntity(room);
                return null;
            }
            catch (Exception ex) // Autres erreurs
            {
                Logger.Log($"Error adding room {room.Name}: {ex.Message}");
                DetachEntity(room);
                return null;
            }
        }

        /// <summary>
        /// Supprime une salle par son ID.
        /// Supprime également toutes les réservations associées à cette salle (grâce à la configuration Cascade d'EF Core).
        /// </summary>
        /// <param name="roomId">L'ID de la salle à supprimer.</param>
        /// <returns>True si la suppression réussit, sinon False.</returns>
        public static bool RemoveRoom(int roomId)
        {
            try
            {
                // Find est traduisible
                var room = _context.Rooms.Find(roomId);

                if (room != null)
                {
                    // La suppression en cascade des réservations est gérée par EF si configuré
                    // (généralement par défaut si la relation est correctement définie).
                    _context.Rooms.Remove(room);
                    _context.SaveChanges();
                    Logger.Log($"Room ID {roomId} ('{room.Name}') and associated reservations removed successfully.");
                    return true;
                }
                Logger.Log($"Attempted to remove non-existent room with ID: {roomId}.");
                return false;
            }
            catch (DbUpdateException ex) // Gérer spécifiquement les erreurs de suppression (ex: contraintes FK si cascade non configurée)
            {
                Logger.Log($"Database error removing room ID {roomId}: {ex.InnerException?.Message ?? ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error removing room ID {roomId}: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Reservation Operations

        /// <summary>
        /// Récupère toutes les réservations, en incluant les informations sur la salle associée.
        /// Triées par date/heure de début.
        /// </summary>
        public static List<Reservation> GetAllReservations()
        {
            try
            {
                // Include, OrderBy, ToList sont traduisibles
                return _context.Reservations
                               .Include(r => r.Room) // Charge les infos de la salle liée
                               .OrderBy(r => r.StartTime)
                               .ToList();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in GetAllReservations: {ex.Message}");
                return new List<Reservation>();
            }
        }


        /// <summary>
        /// Récupère les réservations pour une période et une salle spécifiques.
        /// </summary>
        /// <param name="roomId">ID de la salle.</param>
        /// <param name="startPeriod">Début de la période.</param>
        /// <param name="endPeriod">Fin de la période.</param>
        /// <returns>Liste des réservations correspondantes.</returns>
        public static List<Reservation> GetReservationsForPeriod(int roomId, DateTime startPeriod, DateTime endPeriod)
        {
            try
            {
                // Include, Where, OrderBy, ToList sont traduisibles
                return _context.Reservations
                               .Include(r => r.Room)
                               .Where(r => r.Room.Id == roomId &&
                                           r.StartTime < endPeriod &&
                                           r.EndTime > startPeriod)
                               .OrderBy(r => r.StartTime)
                               .ToList();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in GetReservationsForPeriod (RoomID: {roomId}, Period: {startPeriod}-{endPeriod}): {ex.Message}");
                return new List<Reservation>();
            }
        }


        /// <summary>
        /// Récupère une réservation spécifique par son ID, incluant la salle.
        /// </summary>
        /// <param name="reservationId">ID de la réservation.</param>
        /// <returns>La réservation ou null si non trouvée.</returns>
        public static Reservation? GetReservationById(int reservationId)
        {
            try
            {
                // Include, FirstOrDefault par ID sont traduisibles
                return _context.Reservations
                               .Include(r => r.Room)
                               .FirstOrDefault(r => r.Id == reservationId);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error in GetReservationById (ID: {reservationId}): {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// Ajoute une nouvelle réservation si le créneau est disponible.
        /// </summary>
        /// <param name="reservation">La réservation à ajouter. L'objet Room associé doit être valide mais son état sera marqué comme Unchanged.</param>
        /// <returns>La réservation ajoutée avec son ID et la salle incluse, ou null si le créneau n'est pas disponible ou en cas d'erreur.</returns>
        public static Reservation? AddReservation(Reservation reservation)
        {
            // Validations d'entrée
            if (reservation == null) { Logger.Log("Attempted to add a null reservation."); return null; }
            if (reservation.Room == null || reservation.Room.Id <= 0)
            {
                Logger.Log($"Attempted to add reservation with invalid Room object (ID: {reservation.Room?.Id}).");
                return null;
            }
            if (reservation.EndTime <= reservation.StartTime)
            {
                Logger.Log($"Attempted to add reservation with invalid time slot: EndTime ({reservation.EndTime}) must be after StartTime ({reservation.StartTime}).");
                return null;
            }
            // Vérifier la disponibilité AVANT de toucher au contexte
            if (!IsTimeSlotAvailable(reservation.Room.Id, reservation.StartTime, reservation.EndTime))
            {
                Logger.Log($"Attempted to add reservation for room {reservation.Room.Id} at unavailable time slot: {reservation.StartTime} - {reservation.EndTime}.");
                return null;
            }

            try
            {
                // Gérer l'état de l'entité Room liée pour éviter les erreurs EF
                // Si la Room vient d'être lue du contexte, elle est déjà suivie.
                // Si elle a été créée hors contexte, il faut l'attacher.
                var trackedRoom = _context.ChangeTracker.Entries<Room>().FirstOrDefault(e => e.Entity.Id == reservation.Room.Id);
                if (trackedRoom == null)
                {
                    // La salle n'est pas suivie, on l'attache sans la marquer comme modifiée
                    _context.Attach(reservation.Room);
                    _context.Entry(reservation.Room).State = EntityState.Unchanged;
                    Logger.Log($"Attached Room ID {reservation.Room.Id} to context.");
                }
                else
                {
                    // La salle est déjà suivie, s'assurer qu'elle n'est pas marquée pour ajout/modif
                    if (trackedRoom.State != EntityState.Unchanged && trackedRoom.State != EntityState.Detached)
                    {
                        trackedRoom.State = EntityState.Unchanged;
                        Logger.Log($"Set tracked Room ID {reservation.Room.Id} state to Unchanged.");
                    }
                    // S'assurer que l'instance de la réservation utilise bien l'instance suivie
                    reservation.Room = trackedRoom.Entity;
                }


                _context.Reservations.Add(reservation);
                _context.SaveChanges();
                Logger.Log($"Reservation added successfully for room ID {reservation.Room.Id} from {reservation.StartTime} to {reservation.EndTime}. New ID: {reservation.Id}");

                return reservation; // Retourne la réservation avec son ID
            }
            catch (DbUpdateException ex)
            {
                Logger.Log($"Database error adding reservation: {ex.InnerException?.Message ?? ex.Message}");
                DetachEntity(reservation.Room); // Détacher aussi la salle liée
                DetachEntity(reservation);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error adding reservation: {ex.Message}");
                DetachEntity(reservation.Room);
                DetachEntity(reservation);
                return null;
            }
        }


        /// <summary>
        /// Met à jour une réservation existante. Vérifie la disponibilité du nouveau créneau.
        /// NE PERMET PAS de changer la salle d'une réservation existante via cette méthode.
        /// </summary>
        /// <param name="updatedReservation">La réservation avec les informations mises à jour. updatedReservation.Id doit correspondre à une réservation existante.</param>
        /// <returns>True si la mise à jour a réussi, False si créneau indisponible, réservation non trouvée, changement de salle tenté, ou erreur.</returns>
        public static bool UpdateReservation(Reservation updatedReservation)
        {
            // Validations
            if (updatedReservation == null) { Logger.Log("Attempted to update with a null reservation object."); return false; }
            if (updatedReservation.Room == null || updatedReservation.Room.Id <= 0)
            {
                Logger.Log($"Attempted to update reservation {updatedReservation.Id} with invalid Room object."); return false;
            }
            if (updatedReservation.EndTime <= updatedReservation.StartTime)
            {
                Logger.Log($"Attempted to update reservation {updatedReservation.Id} with invalid time slot: EndTime ({updatedReservation.EndTime}) must be after StartTime ({updatedReservation.StartTime}).");
                return false;
            }
            // Vérifier la disponibilité en excluant la réservation actuelle
            if (!IsTimeSlotAvailable(updatedReservation.Room.Id, updatedReservation.StartTime, updatedReservation.EndTime, updatedReservation.Id))
            {
                Logger.Log($"Attempted to update reservation {updatedReservation.Id} to unavailable time slot: {updatedReservation.StartTime} - {updatedReservation.EndTime}.");
                return false;
            }

            try
            {
                // Récupérer la réservation existante depuis la base, incluant la salle pour vérifier si elle a changé
                var existingReservation = _context.Reservations
                                                  .Include(r => r.Room) // Important pour comparer l'ID de la salle
                                                  .FirstOrDefault(r => r.Id == updatedReservation.Id);

                if (existingReservation == null)
                {
                    Logger.Log($"Attempted to update non-existent reservation with ID: {updatedReservation.Id}.");
                    return false;
                }

                // Vérifier si on essaie de changer de salle (non autorisé par cette méthode)
                if (existingReservation.Room.Id != updatedReservation.Room.Id)
                {
                    Logger.Log($"Attempted to change room ID (from {existingReservation.Room.Id} to {updatedReservation.Room.Id}) during update for reservation {updatedReservation.Id}. Operation aborted.");
                    // Pas besoin de détacher ici, car on n'a rien modifié
                    return false;
                }

                // Mettre à jour les champs modifiables
                existingReservation.StartTime = updatedReservation.StartTime;
                existingReservation.EndTime = updatedReservation.EndTime;
                existingReservation.AttendeeFirstName = updatedReservation.AttendeeFirstName;
                existingReservation.AttendeeLastName = updatedReservation.AttendeeLastName;
                existingReservation.MeetingSubject = updatedReservation.MeetingSubject;
                // Ne pas toucher à existingReservation.Room car elle est déjà liée et suivie par EF

                // Marquer l'entité comme modifiée (souvent automatique, mais peut être explicite si nécessaire)
                // _context.Entry(existingReservation).State = EntityState.Modified;

                _context.SaveChanges();
                Logger.Log($"Reservation ID {updatedReservation.Id} updated successfully.");
                return true;
            }
            catch (DbUpdateException ex)
            {
                Logger.Log($"Database error updating reservation ID {updatedReservation.Id}: {ex.InnerException?.Message ?? ex.Message}");
                return false; // En cas d'erreur DB, la transaction est annulée.
            }
            catch (Exception ex)
            {
                Logger.Log($"Error updating reservation ID {updatedReservation.Id}: {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// Supprime une réservation par son ID.
        /// </summary>
        /// <param name="reservationId">L'ID de la réservation à supprimer.</param>
        /// <returns>True si la suppression a réussi, sinon False.</returns>
        public static bool RemoveReservation(int reservationId)
        {
            try
            {
                // Find est traduisible
                var reservation = _context.Reservations.Find(reservationId);
                if (reservation != null)
                {
                    _context.Reservations.Remove(reservation);
                    _context.SaveChanges();
                    Logger.Log($"Reservation ID {reservationId} removed successfully.");
                    return true;
                }
                Logger.Log($"Attempted to remove non-existent reservation with ID: {reservationId}.");
                return false;
            }
            catch (DbUpdateException ex) // Erreurs spécifiques EF
            {
                Logger.Log($"Database error removing reservation ID {reservationId}: {ex.InnerException?.Message ?? ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error removing reservation ID {reservationId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Vérifie si un créneau horaire est disponible pour une salle donnée,
        /// en excluant éventuellement une réservation spécifique (utile pour les mises à jour).
        /// </summary>
        /// <param name="roomId">L'ID de la salle.</param>
        /// <param name="startTime">L'heure de début souhaitée.</param>
        /// <param name="endTime">L'heure de fin souhaitée.</param>
        /// <param name="excludeReservationId">L'ID de la réservation à exclure de la vérification (0 ou <0 si aucune).</param>
        /// <returns>True si le créneau est disponible, sinon False.</returns>
        public static bool IsTimeSlotAvailable(int roomId, DateTime startTime, DateTime endTime, int excludeReservationId = 0)
        {
            // Validations de base
            if (endTime <= startTime)
            {
                Logger.Log($"Time slot check failed for room {roomId}: EndTime ({endTime}) must be after StartTime ({startTime}).");
                return false;
            }
            if (roomId <= 0)
            {
                Logger.Log($"Time slot check failed: Invalid Room ID ({roomId}).");
                return false;
            }

            try
            {
                // La requête Where avec des comparaisons de date/heure et ID est traduisible.
                // Any() est aussi traduisible.
                var conflictingReservations = _context.Reservations
                       .Where(r => r.Room.Id == roomId &&         // Bonne salle
                                   r.Id != excludeReservationId && // Pas la réservation qu'on met à jour
                                   r.EndTime > startTime &&        // Se termine après le début demandé
                                   r.StartTime < endTime)          // Commence avant la fin demandée
                       .Any(); // Existe-t-il au moins une réservation conflictuelle ?

                // Si Any() retourne true (il y a conflit), le créneau n'est PAS disponible (retourne false).
                // Si Any() retourne false (pas de conflit), le créneau EST disponible (retourne true).
                return !conflictingReservations;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error checking time slot availability for room {roomId} ({startTime} - {endTime}): {ex.Message}");
                // En cas d'erreur de vérification, considérer le créneau comme non disponible par sécurité.
                return false;
            }
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Helper method to detach an entity from the DbContext if it's being tracked.
        /// Useful in catch blocks to prevent issues if operations are retried or if the context is long-lived.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity instance to detach.</param>
        private static void DetachEntity<TEntity>(TEntity? entity) where TEntity : class
        {
            if (entity == null) return;

            var entry = _context.Entry(entity);
            if (entry != null && entry.State != EntityState.Detached)
            {
                entry.State = EntityState.Detached;
                // Logger.Log($"Detached entity of type {typeof(TEntity).Name}."); // Optional logging
            }
        }
        #endregion
    }
}