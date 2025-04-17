using System.Linq;
using System.Windows;
using System.Windows.Input;
using ReservationSalles.Commands;
using ReservationSalles.Models; // <-- USING AJOUTÉ (bonne pratique)
using ReservationSalles.Services;
using ReservationSalles.Views;


namespace ReservationSalles.ViewModels
{
    public class LoginViewModel : BaseViewModel // Assurez-vous qu'il hérite de BaseViewModel si OnPropertyChanged vient de là
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;

        public string Username
        {
            get => _username;
            // Note: Votre code original utilise l'assignation directe + OnPropertyChanged().
            // Si BaseViewModel fournit SetProperty, il est préférable de l'utiliser pour la cohérence.
            // Sinon, votre méthode est correcte. Je la laisse telle quelle pour l'instant.
            set { _username = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); } // Ajout CommandManager pour CanExecuteLogin
        }

        public string Password
        {
            get => _password;
            // Idem que pour Username
            set { _password = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); } // Ajout CommandManager pour CanExecuteLogin
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); } // Pas besoin de SetProperty si BaseViewModel ne l'a pas
        }

        public ICommand LoginCommand { get; }

        // Pour l'œil
        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set { _isPasswordVisible = value; OnPropertyChanged(); } // Idem
        }
        public ICommand TogglePasswordVisibilityCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            TogglePasswordVisibilityCommand = new RelayCommand(ExecuteTogglePwdVisibility);
        }

        private bool CanExecuteLogin(object? parameter)
        {
            // Le bouton Login est actif si les deux champs sont remplis
            return !string.IsNullOrWhiteSpace(Username)
                && !string.IsNullOrWhiteSpace(Password);
        }

        private void ExecuteLogin(object? parameter)
        {
            ErrorMessage = ""; // Effacer l'ancien message d'erreur

            try
            {
                // --- CORRECTION CS1501 ---
                // 1. Récupérer l'utilisateur par son nom seulement
                User? user = DataService.GetUser(Username);

                // 2. Vérifier si l'utilisateur existe ET si le mot de passe est correct
                //    Utilisation d'une méthode hypothétique VerifyPassword dans SecurityHelper (plus propre)
                //    Si vous n'avez pas VerifyPassword, il faut hasher le mot de passe entré:
                //    bool isPasswordCorrect = (user != null && user.Password == SecurityHelper.ComputeSha256Hash(Password));

                if (user != null && SecurityHelper.VerifyPassword(Password, user.Password)) // Adaptez si vous n'avez pas VerifyPassword
                // --- FIN CORRECTION ---
                {
                    // Connexion réussie
                    Logger.Log($"User '{Username}' logged in successfully.");
                    MainWindow main = new MainWindow(user); // Passer l'utilisateur connecté à MainWindow
                    main.Show();

                    // Fermer la fenêtre de login actuelle
                    var loginWindow = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
                    loginWindow?.Close();
                }
                else
                {
                    // Échec de la connexion
                    Logger.Log($"Login failed for user '{Username}'.");
                    ErrorMessage = "Identifiant ou mot de passe incorrect.";
                }
            }
            catch (Exception ex)
            {
                // Gérer les erreurs inattendues (ex: problème de base de données)
                Logger.Log($"Error during login for user '{Username}': {ex.Message}");
                ErrorMessage = "Une erreur s'est produite lors de la connexion.";
                MessageBox.Show($"Erreur de connexion: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteTogglePwdVisibility(object? param)
        {
            IsPasswordVisible = !IsPasswordVisible;
        }
    }
}