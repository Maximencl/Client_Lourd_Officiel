using System.Windows;
using System.Windows.Input;
using ReservationSalles.ViewModels;

namespace ReservationSalles.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void PwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = PwdBox.Password;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}