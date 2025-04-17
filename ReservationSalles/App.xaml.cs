using System.Windows;
using ReservationSalles.Services;

namespace ReservationSalles
{
    public partial class App : Application
    {
        /// <summary>
        /// Point d'entrée de l'application WPF. 
        /// On initialise la base de données avant d'afficher la première fenêtre.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Initialise la base (création ou migration) et insère les données par défaut (Seed).
            DataService.InitializeDatabase();

            base.OnStartup(e);
        }
    }
}