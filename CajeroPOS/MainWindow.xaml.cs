using Microsoft.VisualBasic;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CajeroPOS
{
    public partial class MainWindow : Window
    {
        // =====================================================
        // CAMPOS / VARIABLES
        // =====================================================
        List<Producto> carrito = new List<Producto>();
        List<Pedido> pedidos = new List<Pedido>();
        List<Usuario> usuarios = new List<Usuario>();

        int contadorPedidos = 1;

        PantallaPedidos pantallaPedidos;
        DispatcherTimer reloj;

        Usuario usuarioActual;
        SpeechSynthesizer synth = new SpeechSynthesizer();

        Factura facturaActual;

        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public MainWindow()
        {
            InitializeComponent();

            if (System.IO.File.Exists("usuarios.json"))
            {
                string json = System.IO.File.ReadAllText("usuarios.json");
                usuarios = System.Text.Json.JsonSerializer.Deserialize<List<Usuario>>(json);
            }
            else
            {
                usuarios = new List<Usuario>
                {
                    new Usuario { Nombre = "admin", Password = "1234", Rol = "Administrador" },
                    new Usuario { Nombre = "cajero", Password = "1111", Rol = "Cajero" }
                };

                GuardarUsuarios();
            }

            dgUsuarios.ItemsSource = usuarios;
            dgUsuarios.Items.Refresh();

            LoginWindow loginWindow = new LoginWindow(usuarios);
            bool? resultado = loginWindow.ShowDialog();

            if (resultado != true)
            {
                Application.Current.Shutdown();
                return;
            }

            usuarioActual = loginWindow.UsuarioLogueado;
            lblUsuario.Content = $"Sesión iniciada como: {usuarioActual.Nombre}";

            pantallaPedidos = new PantallaPedidos();
            pantallaPedidos.Show();

            reloj = new DispatcherTimer();
            reloj.Interval = TimeSpan.FromSeconds(1);
            reloj.Tick += Reloj_Tick;
            reloj.Start();
        }

        // =====================================================
        // RELOJ
        // =====================================================
        private void Reloj_Tick(object? sender, EventArgs e)
        {
            lblHora.Content = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        // =====================================================
        // PRODUCTOS
        // =====================================================
        public void BigMac_Click(object sender, RoutedEventArgs e) => AgregarProducto("Big Mac", 5.99m);
        public void Patatas_Click(object sender, RoutedEventArgs e) => AgregarProducto("Patatas", 2.50m);
        public void Bebida_Click(object sender, RoutedEventArgs e) => AgregarProducto("Bebida", 1.99m);

        public void AgregarProducto(string nombre, decimal precio)
        {
            Producto existente = carrito.FirstOrDefault(p => p.Nombre == nombre);

            if (existente != null)
                existente.Cantidad++;
            else
                carrito.Add(new Producto { Nombre = nombre, Precio = precio, Cantidad = 1 });

            ActualizarListaPedido();
            ActualizarTotal();
        }

        public void ActualizarTotal()
        {
            decimal total = carrito.Sum(p => p.Precio * p.Cantidad);
            txtTotal.Text = $"Total: {total:C}";
        }

        private void ActualizarListaPedido()
        {
            lstPedido.Items.Clear();

            foreach (var p in carrito)
            {
                decimal importe = p.Precio * p.Cantidad;
                lstPedido.Items.Add($"{p.Cantidad} x {p.Nombre} - {importe:C}");
            }
        }

        // =====================================================
        // CONFIRMAR PEDIDO / FACTURA
        // =====================================================
        public void Confirmar_Click(object sender, RoutedEventArgs e)
        {
            if (carrito.Count == 0)
            {
                MessageBox.Show("El Pedido no contiene nada");
                return;
            }

            string numero = $"A{contadorPedidos:D3}";
            decimal total = carrito.Sum(p => p.Precio * p.Cantidad);

            List<Producto> productosFactura = carrito.Select(p => new Producto
            {
                Nombre = p.Nombre,
                Precio = p.Precio,
                Cantidad = p.Cantidad
            }).ToList();

            Pedido pedido = new Pedido
            {
                Numero = numero,
                Total = total,
                Estado = "En preparación"
            };

            pedidos.Add(pedido);
            contadorPedidos++;
            ActualizarPantallas();



            decimal importe = 0;
            bool importeValido = false;

            while (!importeValido)
            {
                string valor = Interaction.InputBox("Ingrese el importe recibido:", "Importe Recibido", "");

                if (!decimal.TryParse(valor, out importe))
                {
                    MessageBox.Show("Debe ingresar un número válido.");
                    continue;
                }

                if (importe < total)
                {
                    MessageBox.Show("Importe insuficiente");
                    continue;
                }

                importeValido = true;
            }

            Factura factura = new Factura
            {
                Numero = numero,
                Usuario = usuarioActual.Nombre,
                Fecha = DateTime.Now,
                Productos = productosFactura,
                Total = total,
                Importe = importe
            };

            MostrarTicketTermico(factura);

            MessageBox.Show($"Pedido {numero} confirmado 🧾");

            ImprimirTicketTermico(null, null);

            carrito.Clear();
            lstPedido.Items.Clear();
            ActualizarTotal();
        }

        private void MostrarTicketTermico(Factura factura)
        {
            tkNumero.Text = $"Ticket: {factura.Numero}";
            tkFecha.Text = $"Fecha: {factura.Fecha:dd/MM/yyyy HH:mm}";
            tkUsuario.Text = $"Atendido por: {factura.Usuario}";

            tkProductos.ItemsSource = factura.Productos;

            decimal cambio = factura.Importe - factura.Total;

            tkTotal.Text = factura.Total.ToString("C");
            tkRecibido.Text = factura.Importe.ToString("C");
            tkCambio.Text = cambio.ToString("C");

            ticketTermico.Visibility = Visibility.Visible;
        }

        private void ImprimirTicketTermico(object sender, RoutedEventArgs e)
        {
            ticketTermico.Visibility = Visibility.Visible; // asegurarnos que está visible
            ticketTermico.UpdateLayout(); // fuerza el layout de todos los hijos
            ticketTermico.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            ticketTermico.Arrange(new Rect(ticketTermico.DesiredSize));

            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                DateTime horaLocal = DateTime.Now;
                pd.PrintVisual(ticketTermico, "Ticket " + horaLocal.ToString("dd-MM-yyyy-HH-mm-ss"));
            }
        }

        // =====================================================
        // PEDIDOS (PREPARACIÓN / LISTOS)
        // =====================================================
        public void ActualizarPantallas()
        {
            lstPedidosPreparacion.Items.Clear();
            foreach (var p in pedidos)
                if (p.Estado == "En preparación")
                    lstPedidosPreparacion.Items.Add(p.Numero);

            lstPedidosListos.Items.Clear();
            foreach (var p in pedidos)
                if (p.Estado == "Listo")
                    lstPedidosListos.Items.Add(p.Numero);

            if (pantallaPedidos != null)
                pantallaPedidos.ActualizarPedidos(pedidos);
        }

        public void MarcarListo_Click(object sender, RoutedEventArgs e)
        {
            if (lstPedidosPreparacion.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un pedido");
                return;
            }

            string numero = lstPedidosPreparacion.SelectedItem.ToString();

            foreach (var p in pedidos)
            {
                if (p.Numero == numero)
                {
                    PromptBuilder builder1 = new PromptBuilder();
                    builder1.StartVoice("Microsoft Pablo Desktop");
                    builder1.AppendText("El Pedido número " + p.Numero + " esta Listo");
                    builder1.EndVoice();

                    synth.SpeakAsync(builder1);
                    p.Estado = "Listo";
                    break;
                }
            }

            ActualizarPantallas();
        }

        private void Eliminar_Click(object sender, RoutedEventArgs e)
        {
            if (lstPedidosListos.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un pedido");
                return;
            }

            string numero = lstPedidosListos.SelectedItem.ToString();

            foreach (var p in pedidos)
            {
                if (p.Numero == numero)
                {
                    p.Estado = "Eliminar";
                    break;
                }
            }

            ActualizarPantallas();
        }

        // =====================================================
        // USUARIOS
        // =====================================================
        private void agregarUsuarios(object sender, RoutedEventArgs e)
        {
            if (usuarioActual.Rol != "Administrador")
            {
                MessageBox.Show("Solo un administrador puede agregar usuarios.");
                return;
            }

            Usuario nuevo = new Usuario
            {
                Nombre = txtNombreUsuario.Text,
                Password = txtConstraseña.Text,
                Rol = ((ComboBoxItem)cmbRol.SelectedItem).Content.ToString()
            };

            usuarios.Add(nuevo);
            dgUsuarios.Items.Refresh();
        }

        private void modificarUsuario(object sender, RoutedEventArgs e)
        {
            if (usuarioActual.Rol != "Administrador")
                return;

            Usuario seleccionado = (Usuario)dgUsuarios.SelectedItem;
            seleccionado.Nombre = txtNombreUsuario.Text;
            seleccionado.Password = txtConstraseña.Text;
            seleccionado.Rol = ((ComboBoxItem)cmbRol.SelectedItem).Content.ToString();

            dgUsuarios.Items.Refresh();
            GuardarUsuarios();
        }

        private void eliminarUsuario(object sender, RoutedEventArgs e)
        {
            if (usuarioActual.Rol != "Administrador")
                return;

            Usuario seleccionado = (Usuario)dgUsuarios.SelectedItem;
            usuarios.Remove(seleccionado);
            dgUsuarios.Items.Refresh();
            GuardarUsuarios();
        }

        private void GuardarUsuarios()
        {
            string json = System.Text.Json.JsonSerializer.Serialize(
                usuarios,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            System.IO.File.WriteAllText("usuarios.json", json);
        }

        private void dgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsuarios.SelectedItem == null) return;

            Usuario seleccionado = (Usuario)dgUsuarios.SelectedItem;
            txtNombreUsuario.Text = seleccionado.Nombre;
            txtConstraseña.Text = seleccionado.Password;

            foreach (ComboBoxItem item in cmbRol.Items)
                if ((string)item.Content == seleccionado.Rol)
                    cmbRol.SelectedItem = item;
        }

        // =====================================================
        // CLASES
        // =====================================================
        public class Usuario
        {
            public string Nombre { get; set; }
            public string Password { get; set; }
            public string Rol { get; set; }
            public override string ToString() => Nombre;
        }

        public class Producto
        {
            public string Nombre { get; set; }
            public decimal Precio { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioTotal => Precio * Cantidad;
        }

        public class Pedido
        {
            public string Numero { get; set; }
            public decimal Total { get; set; }
            public string Estado { get; set; }
        }

        public class Factura
        {
            public string Numero { get; set; }
            public string Usuario { get; set; }
            public DateTime Fecha { get; set; }
            public List<Producto> Productos { get; set; }
            public decimal Total { get; set; }
            public decimal Importe { get; set; }
        }
    }
}

