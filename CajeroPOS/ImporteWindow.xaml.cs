using System;
using System.Windows;
using System.Windows.Controls;

namespace CajeroPOS
{
    public partial class ImporteWindow : Window
    {
        // valor final introducido
        public decimal Importe { get; private set; }

        private decimal totalVenta;

        public ImporteWindow(decimal total)
        {
            InitializeComponent();

            totalVenta = total;
            txtTotal.Text = $"Total: {total:C}";
        }

        // =========================
        // ACEPTAR
        // =========================
        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            // validar número
            if (!decimal.TryParse(txtImporte.Text, out decimal valor))
            {
                MessageBox.Show(
                    "Introduce un número válido",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                return;
            }

            // validar importe mínimo
            if (valor < totalVenta)
            {
                MessageBox.Show(
                    "El importe es insuficiente",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            Importe = valor;

            DialogResult = true;
        }

        // =========================
        // CANCELAR
        // =========================
        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        // =========================
        // BOTONES RÁPIDOS
        // =========================
        private void DineroRapido_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string texto = btn.Content.ToString()
                .Replace("€", "")
                .Trim();

            txtImporte.Text = texto;
        }
    }
}