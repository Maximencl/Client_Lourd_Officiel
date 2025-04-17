using System;
using System.Linq;
using System.Windows;
using System.Windows.Input; // Assurez-vous que ce using est présent
using ReservationSalles.Models; // <-- USING AJOUTÉ
using ReservationSalles.ViewModels;
using ReservationSalles.Services; // Ajout possible si RefreshReservationInList utilise DataService directement

namespace ReservationSalles.Views
{
    public partial class MainWindow : Window
    {
        // Garder une référence privée au ViewModel pour éviter les casts répétés
        private readonly MainViewModel? _viewModel;

        // Constructeur par défaut (peut être appelé par le designer si aucun DataContext n'est défini en XAML)
        public MainWindow()
        {
            InitializeComponent();
            // Potentiellement initialiser un ViewModel ici si nécessaire pour le designer ou cas par défaut
            // _viewModel = new MainViewModel();
            // DataContext = _viewModel;
        }

        // Constructeur principal prenant l'utilisateur connecté
        public MainWindow(User user) : this() // Appelle le constructeur par défaut pour InitializeComponent
        {
            // Définit le DataContext sur une nouvelle instance de MainViewModel,
            // en lui passant l'utilisateur connecté.
            _viewModel = new MainViewModel(user); // Stocker la référence
            DataContext = _viewModel;
        }

        // Gestionnaire de clic pour le bouton Administration
        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            // Utilise la référence privée _viewModel (plus sûr que caster DataContext)
            // Vérifie si le ViewModel existe et si l'utilisateur est Admin (utilise UserRole complet)
            if (_viewModel?.CurrentUser?.Role == UserRole.Admin) // <-- CORRECTION UserRole.Admin
            {
                AdminWindow adminWindow = new AdminWindow();
                adminWindow.ShowDialog(); // Ouvre la fenêtre Admin en mode modal

                // Rafraîchir les données après la fermeture de la fenêtre admin.
                _viewModel?.RefreshCommand.Execute(null);
            }
            else
            {
                MessageBox.Show("Vous devez être administrateur pour accéder à cette section.",
                                "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Gestionnaire de clic pour ouvrir le Planning Unifié
        private void OpenUnifiedPlanning_Click(object sender, RoutedEventArgs e)
        {
            // CORRECTION CS1729: Appelle le constructeur sans argument
            var unifiedWindow = new UnifiedPlanningWindow();
            unifiedWindow.Show(); // Utiliser Show() pour non-modal ou ShowDialog() pour modal

            // Pas besoin de rafraîchir ici car UnifiedPlanningWindow recharge ses propres données maintenant.
            // Si des modifs faites dans UnifiedPlanningWindow doivent impacter MainWindow,
            // il faudrait un mécanisme de communication (ex: événements, service partagé).
            // _viewModel?.RefreshCommand.Execute(null); // Probablement plus nécessaire
        }

        // Gestionnaire de clic pour la déconnexion
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        // Gestionnaire de double-clic sur une ligne du DataGrid des réservations
        private void ReservationsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ReservationsDataGrid.SelectedItem is Reservation selectedReservation && _viewModel != null)
            {
                // Crée une copie pour l'édition. Important si EditReservationWindow modifie directement l'objet passé.
                // Si EditReservationWindow utilise une copie interne (WorkingCopy), passer l'original est ok.
                // Le code original passait l'original, on garde ça.
                // var reservationCopy = new Reservation { /* ... copie des propriétés ... */ };
                EditReservationWindow editWindow = new EditReservationWindow(selectedReservation);

                if (editWindow.ShowDialog() == true)
                {
                    // Si la sauvegarde a réussi (DialogResult=true),
                    // demander au ViewModel de rafraîchir cet élément spécifique dans sa liste.
                    _viewModel.RefreshReservationInList(selectedReservation.Id);
                    // Ou simplement rafraîchir tout :
                    // _viewModel.RefreshCommand.Execute(null);
                }
                // Si DialogResult est false ou null, l'utilisateur a annulé.
            }
        }
    }
}