using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using CajeroPOS.Data;
using CajeroPOS.Models;

namespace CajeroPOS
{
    public partial class LoginWindow : Window
    {
        private bool loginExitoso = false;

        private AppDbContext db = new AppDbContext();

        public Usuario UsuarioLogueado { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            this.Closing += LoginWindow_Closing;
        }

        private void LoginWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!loginExitoso)
                Application.Current.Shutdown();
        }

        private void BtnIngresar_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text;
            string password = txtPassword.Password;

            if (ValidarUsuario(usuario, password))
            {
                loginExitoso = true;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos");
            }
        }

        private bool ValidarUsuario(string nombre, string password)
        {
            var user = db.Usuarios.FirstOrDefault(u => u.Nombre == nombre);

            if (user == null)
                return false;

            if (MainWindow.VerificarPassword(password, user.PasswordHash, user.Salt))
            {
                UsuarioLogueado = user;
                return true;
            }

            return false;
        }
    }
}