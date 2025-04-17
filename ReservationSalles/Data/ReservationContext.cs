using Microsoft.EntityFrameworkCore;
using ReservationSalles.Models;
using System;
using System.IO;

namespace ReservationSalles.Data
{
    public class ReservationContext : DbContext
    {
        // Déclaration des DbSet qui représentent les tables dans la base de données.
        // EF Core les remplit automatiquement. Le "null!" supprime un avertissement de C#.
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;

        /// <summary>
        /// Configure les options pour ce contexte de base de données.
        /// C'est ici qu'on définit principalement le fournisseur de base de données (SQLite)
        /// et la chaîne de connexion (où se trouve le fichier de base de données).
        /// </summary>
        /// <param name="optionsBuilder">Builder pour configurer les options.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // --- OPTION 3 : Dossier Local Application Data (APPROCHE RECOMMANDÉE) ---
            // Stocke la base dans %LOCALAPPDATA%\ReservationSallesApp\reservation.db
            // C'est portable, respecte les conventions Windows et spécifique à l'utilisateur.

            string dbFileName = "reservation.db";
            string appName = "ReservationSallesApp"; // Nom du dossier pour votre application dans LocalAppData

            try
            {
                // Récupère le chemin du dossier %LOCALAPPDATA% (ex: C:\Users\VotreNom\AppData\Local)
                string localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                // Crée le chemin complet du dossier spécifique à votre application
                string appDataFolder = Path.Combine(localAppDataFolder, appName);

                // S'assure que ce dossier existe, le crée si nécessaire
                Directory.CreateDirectory(appDataFolder);

                // Combine le chemin du dossier avec le nom du fichier de base de données
                string fullDbPath = Path.Combine(appDataFolder, dbFileName);

                // Configure EF Core pour utiliser SQLite avec ce chemin
                optionsBuilder.UseSqlite($"Data Source={fullDbPath}");

                // Optionnel: Logguer le chemin utilisé pour le débogage
                // Console.WriteLine($"Database path configured to: {fullDbPath}"); // Ou utiliser votre Logger
            }
            catch (Exception ex)
            {
                // Gérer l'erreur si la création du dossier ou l'accès échoue (permissions, etc.)
                // Logger l'erreur est essentiel ici.
                Console.Error.WriteLine($"FATAL: Impossible de configurer le chemin de la base de données dans LocalAppData : {ex.Message}");
                // En dernier recours, on pourrait tenter un chemin relatif simple, mais
                // cela indique un problème d'environnement plus sérieux.
                // Pour la robustesse, ajoutons un fallback au cas où.
                string fallbackDbPath = "reservation_fallback.db"; // Sera dans bin/Debug...
                Console.Error.WriteLine($"Fallback: Tentative d'utilisation de la base de données locale : {fallbackDbPath}");
                optionsBuilder.UseSqlite($"Data Source={fallbackDbPath}");
                // Vous pourriez aussi choisir de lancer une exception ici pour arrêter l'application
                // car la base de données principale n'est pas accessible.
                // throw new ApplicationException("Impossible de configurer le stockage de la base de données.", ex);
            }


            /* --- OPTIONS PRÉCÉDENTES (CONSERVÉES POUR RÉFÉRENCE, MAIS COMMENTÉES) ---

            // --- OPTION 1 : Chemin Absolu Spécifique (NON RECOMMANDÉ) ---
            // string specificDatabaseDirectory = @"C:\Users\Maison\Desktop\Projet Client Lourd Officiel\ReservationSalles";
            // string specificDatabaseFileName = "reservation.db";
            // string specificFullPath = Path.Combine(specificDatabaseDirectory, specificDatabaseFileName);
            // optionsBuilder.UseSqlite($"Data Source={specificFullPath}");

            // --- OPTION 2 : Chemin Relatif (SIMPLE POUR DÉVELOPPEMENT) ---
            // optionsBuilder.UseSqlite("Data Source=reservation.db");

            */
        }
    }
}