using CajeroPOS.Data;
using CajeroPOS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using MySqlConnector;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Speech.Synthesis;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CajeroPOS
{
    public partial class MainWindow : Window
    {

        // =====================================================
        // CAMPOS
        // =====================================================
        private List<Producto> carrito = new List<Producto>();
        private List<Pedido> pedidos = new List<Pedido>();
        PantallaPedidos pantallaPedidos;


        DispatcherTimer reloj;
        Usuario usuarioActual;
        SpeechSynthesizer synth = new SpeechSynthesizer();
        Factura facturaActual;

        List<Producto> productos;
        List<Categoria> categorias;
        List<Usuario> usuarios;

        string categoriaSeleccionada = "Todas";
        ConfiguracionTicket configTicket = new ConfiguracionTicket();
        private ConfigApp configApp = new ConfigApp();
        string rutaImagen = "";


        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => AjustarColumnas();

            // ================= VOCES  =================
            foreach (var voice in synth.GetInstalledVoices())
            {
                cmbVoces.Items.Add(voice.VoiceInfo.Name);
            }

            if (cmbVoces.Items.Count > 0)
                cmbVoces.SelectedIndex = 0;

            // ================= CARGA BD =================

            try
            {




                using (var db = new AppDbContext())
                {

                    db.Database.CanConnect();
                    usuarios = db.Usuarios.ToList();
                    productos = db.Productos.Include(x => x.Categoria).ToList();
                    categorias = db.Categorias.ToList();



                    // CREAR CATEGORÍA "TODAS" SI NO EXISTEN CATEGORÍAS
                    if (!categorias.Any())
                    {
                        Categoria categoriaTodas = new Categoria
                        {
                            Nombre = "Todas"
                        };

                        db.Categorias.Add(categoriaTodas);
                        db.SaveChanges();

                        categorias = db.Categorias.ToList();
                    }

                }
            }
            catch {
                MessageBox.Show("No se puede conectar a MariaDB. Verifique la instalación. Recuerda añadir tus datos de MariaDB en el archivo App.config del proyecto"); Application.Current.Shutdown();

            }
            CargarConfiguracionApp();

            // ================= LOGIN =================
            LoginWindow loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() != true)
            {
                Application.Current.Shutdown();
                return;
            }

            usuarioActual = loginWindow.UsuarioLogueado;
            lblUsuario.Content = $"Sesión iniciada como: {usuarioActual.Nombre}";

            // ================= PANTALLA PEDIDOS =================
            pantallaPedidos = new PantallaPedidos();
            pantallaPedidos.Show();

            // ================= RELOJ =================
            reloj = new DispatcherTimer();
            reloj.Interval = TimeSpan.FromSeconds(1);
            reloj.Tick += Reloj_Tick;
            reloj.Start();

            // ================= UI =================
            cmbCategorias.ItemsSource = categorias.Select(c => c.Nombre).ToList();
            cmbFiltroCategoria.ItemsSource = categorias;
            cmbFiltroCategoria.DisplayMemberPath = "Nombre";

            RefrescarTodo();
            RenderCategorias();
            RenderProductos();
            CargarUsuariosDesdeBD();
            CargarConfiguracionTicket();

        }




        // =====================================================
        // RELOJ
        // =====================================================
        private void Reloj_Tick(object? sender, EventArgs e)
        {
            lblHora.Content = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }


        // =====================================================
        // CONFIGURACIÓN TICKET
        // =====================================================

        private void GuardarConfiguracionTicket()
        {
            string json = System.Text.Json.JsonSerializer.Serialize(
                configTicket,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText("config_ticket.json", json);
        }

        private void CargarConfiguracionTicket()
        {
            if (File.Exists("config_ticket.json"))
            {
                string json = File.ReadAllText("config_ticket.json");
                configTicket = JsonSerializer.Deserialize<ConfiguracionTicket>(json);
            }

            //  EVITAR NULL
            if (configTicket == null)
            {
                configTicket = new ConfiguracionTicket
                {
                    Empresa = "Tu Empresa",
                    Direccion = "Av. Principal 123",
                    CIF = "A12345678",

                };
            }

            // =========================
            // UI CONFIG
            // =========================
            txtEmpresa.Text = configTicket.Empresa;
            txtDireccion.Text = configTicket.Direccion;
            txtCIF.Text = configTicket.CIF;

            // =========================
            // TICKET UI
            // =========================
            tkEmpresa.Text = configTicket.Empresa;
            tkDireccion.Text = configTicket.Direccion;
            tkCIF.Text = "CIF: " + configTicket.CIF;


        }

        // =====================================================
        // MISCELANEOS(Renderizado y refresco de los productos)
        // =====================================================

        void RenderProductos()
        {
            PanelProductos.Children.Clear();

            var lista = productos.Where(p =>
                (categoriaSeleccionada == "Todas" || p.Categoria?.Nombre == categoriaSeleccionada) &&
                (string.IsNullOrWhiteSpace(txtBuscar.Text) ||
                 p.Nombre.ToLower().Contains(txtBuscar.Text.ToLower()))
            );

            foreach (var p in lista)
            {
                //Estilo del botón(tarjetas visuales)
                Border card = new Border
                {
                    Width = 110,
                    Height = 110,
                    Margin = new Thickness(5),
                    CornerRadius = new CornerRadius(14),
                    Background = (System.Windows.Media.Brush)Application.Current.Resources["PanelBrush"],
                    BorderBrush = (System.Windows.Media.Brush)Application.Current.Resources["AccentBrush"],
                    BorderThickness = new Thickness(1),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    DataContext = p
                };

                var normalBrush = (System.Windows.Media.Brush)Application.Current.Resources["PanelBrush"];
                var hoverBrush = (System.Windows.Media.Brush)Application.Current.Resources["AccentBrush"];


                //Acciones del Boton
                card.MouseEnter += (s, e) =>
                {
                    card.Background = hoverBrush;
                };

                card.MouseLeave += (s, e) =>
                {
                    card.Background = normalBrush;
                };

                StackPanel sp = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                //ubicación de las imágenes
                string ruta = "";

                if (!string.IsNullOrEmpty(p.Imagen))
                {
                    ruta = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "imagenes",
                        p.Imagen
                    );
                }

                if (File.Exists(ruta))
                {
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.UriSource = new Uri(ruta, UriKind.Absolute);
                    img.EndInit();
                    img.Freeze();

                    sp.Children.Add(new Image
                    {
                        Source = img,
                        Width = 60,
                        Height = 60,
                        Margin = new Thickness(0, 5, 0, 5)
                    });
                }

                //Informacion del botón

                sp.Children.Add(new TextBlock
                {
                    Text = p.Nombre,
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    TextAlignment = TextAlignment.Center,
                    Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextBrush"]
                });

                sp.Children.Add(new TextBlock
                {
                    Text = $"{p.Precio:C}",
                    FontSize = 12,
                    Opacity = 0.8,
                    TextAlignment = TextAlignment.Center,
                    Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextBrush"]
                });

                card.Child = sp;

                card.MouseLeftButtonDown += Producto_Click;

                PanelProductos.Children.Add(card);
            }
        }

        //Ajuste de columnas automatico en funcion del tamaño de la ventana

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AjustarColumnas();
        }
        private void AjustarColumnas()
        {
            double anchoDisponible = ((FrameworkElement)PanelProductos.Parent).ActualWidth;

            double anchoBoton = 120;

            int columnas = (int)(anchoDisponible / anchoBoton);

            if (columnas < 1)
                columnas = 1;

            PanelProductos.Columns = columnas;
        }
        void RefrescarTodo()
        {
            //  refrescar lista productos
            lstProductos.ItemsSource = null;
            lstProductos.ItemsSource = productos.ToList();

            //  refrescar lista categorías
            lstCategorias.ItemsSource = null;
            lstCategorias.ItemsSource = categorias

                .OrderBy(c => c.Nombre)
                .ToList();

            lstCategorias.DisplayMemberPath = "Nombre";

            //  refrescar combos
            cmbCategorias.ItemsSource = null;
            cmbCategorias.ItemsSource = categorias
                .Where(c => c.Nombre != "Todas")
                .OrderBy(c => c.Nombre)
                .ToList();

            cmbCategorias.DisplayMemberPath = "Nombre";


            cmbFiltroCategoria.ItemsSource = null;
            cmbFiltroCategoria.ItemsSource = categorias;
            cmbFiltroCategoria.DisplayMemberPath = "Nombre";
            CargarUsuariosDesdeBD();
            CargarConfiguracionTicket();
            //  refrescar botones del cajero
            RenderProductos();
            CargarDashboard();
        }



        void RenderCategorias()
        {
            PanelCategorias.Children.Clear();

            foreach (var c in categorias
                .OrderBy(c => c.Nombre == "Todas" ? 0 : 1)
                .ThenBy(c => c.Nombre))
            {
                PanelCategorias.Children.Add(CrearCategoria(c.Nombre));
            }
        }

        // =====================================================
        //  PRODUCTOS DINÁMICOS 
        // =====================================================

        //Crear y guarda la imagen seleccionada en una carpeta imagenes
        string GuardarImagenEnCarpeta(string rutaOriginal)
        {
            if (string.IsNullOrWhiteSpace(rutaOriginal))
                return null;

            string carpetaDestino = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "imagenes"
            );

            if (!Directory.Exists(carpetaDestino))
                Directory.CreateDirectory(carpetaDestino);

            string extension = Path.GetExtension(rutaOriginal);

            string nombreArchivo = Path.GetFileName(rutaOriginal);

            string destinoFinal = Path.Combine(carpetaDestino, nombreArchivo);

            // Para sobreescribir
            if (File.Exists(destinoFinal))
                return nombreArchivo;

            File.Copy(rutaOriginal, destinoFinal);

            return nombreArchivo;
        }

        //El estilo del boton categoria
        Button CrearCategoria(string nombre)
        {
            Button btn = new Button
            {
                Content = nombre,
                Margin = new Thickness(6, 4, 6, 4),
                Padding = new Thickness(14, 6, 14, 6),
                DataContext = nombre,

                BorderThickness = new Thickness(1),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Height = 32,
                MinWidth = 80,

                Template = (ControlTemplate)Application.Current.Resources["CategoriaButtonTemplate"]
            };

            // recursos dinámicos al cambiar de tema
            btn.SetResourceReference(Button.BackgroundProperty, "PanelBrush");
            btn.SetResourceReference(Button.ForegroundProperty, "TextBrush");
            btn.SetResourceReference(Button.BorderBrushProperty, "AccentBrush");

            btn.Click += Categoria_Click;

            return btn;
        }



        private void Categoria_Click(object sender, RoutedEventArgs e)
        {
            string nombre = (string)((Button)sender).DataContext;

            categoriaSeleccionada = nombre;
            RenderProductos();
        }

        private void GuardarProductos()
        {
            using (var db = new AppDbContext())
            {
                foreach (var p in productos)
                {
                    var existente = db.Productos
                        .FirstOrDefault(x => x.Id == p.Id);

                    if (existente == null)
                    {
                        // INSERT
                        db.Productos.Add(p);
                    }
                    else
                    {
                        // UPDATE
                        existente.Nombre = p.Nombre;
                        existente.Precio = p.Precio;
                        existente.Imagen = p.Imagen;
                        existente.CategoriaId = p.Categoria?.Id;
                    }
                }

                db.SaveChanges();
            }
        }
        private void GuardarCategorias()
        {
            using (var db = new AppDbContext())
            {
                foreach (var c in categorias)
                {
                    var existente = db.Categorias.FirstOrDefault(x => x.Id == c.Id);

                    if (existente == null)
                    {
                        db.Categorias.Add(c);
                    }
                    else
                    {
                        existente.Nombre = c.Nombre;
                    }
                }

                db.SaveChanges();
            }
        }

        private void SeleccionarImagen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Imagen (*.png;*.jpg)|*.png;*.jpg";

            if (dlg.ShowDialog() == true)
                rutaImagen = dlg.FileName;
        }

        private void AgregarProducto_Click(object sender, RoutedEventArgs e)
        {

            // Verifica permisos
            if (usuarioActual.Rol == "Cajero")
            {
                MessageBox.Show(
                    "No posees los derechos de administrador",
                    "Advertencia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation
                );

                return;
            }


            if (string.IsNullOrWhiteSpace(txtNombreProducto.Text))
            {
                MessageBox.Show("Introduce un nombre");
                return;
            }

            if (!decimal.TryParse(txtPrecioProducto.Text, out decimal precio))
            {
                MessageBox.Show("Precio inválido");
                return;
            }

            if (string.IsNullOrEmpty(rutaImagen))
            {
                MessageBox.Show(
                    "No se ha seleccionado ninguna imagen",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            var categoria = categorias
                .FirstOrDefault(c => c.Nombre == cmbCategorias.Text);

            if (categoria == null)
            {
                MessageBox.Show("Categoría no válida");
                return;
            }

            string nombreImagen = GuardarImagenEnCarpeta(rutaImagen);

            Producto p = new Producto
            {
                Nombre = txtNombreProducto.Text,
                Precio = precio,
                CategoriaId = categoria.Id,
                Imagen = nombreImagen
            };

            using (var db = new AppDbContext())
            {
                db.Productos.Add(p);
                db.SaveChanges();
            }

            using (var db = new AppDbContext())
            {
                productos = db.Productos
                    .Include(x => x.Categoria)
                    .ToList();
            }

            RefrescarTodo();

            MessageBox.Show(
                "Producto agregado correctamente",
                "Información",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
        private void ModificarProducto_Click(object sender, RoutedEventArgs e)
        {

            // Verifica permisos
            if (usuarioActual.Rol == "Cajero")
            {
                MessageBox.Show(
                    "No posees los derechos de administrador",
                    "Advertencia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation
                );

                return;
            }


            if (lstProductos.SelectedItem == null) return;

            Producto p = (Producto)lstProductos.SelectedItem;

            if (!decimal.TryParse(txtPrecioProducto.Text, out decimal precio))
                return;

            p.Nombre = txtNombreProducto.Text;
            p.Precio = precio;


            p.Categoria = categorias.FirstOrDefault(c => c.Nombre == cmbCategorias.Text);

            if (!string.IsNullOrEmpty(rutaImagen))
            {
                string nombreImagen = GuardarImagenEnCarpeta(rutaImagen);
                p.Imagen = nombreImagen;
            }

            GuardarProductos();
            RefrescarTodo();

            RenderCategorias();
            RenderProductos();

        }

        private void EliminarProducto_Click(object sender, RoutedEventArgs e)
        {

            // Verifica permisos
            if (usuarioActual.Rol == "Cajero")
            {
                MessageBox.Show(
                    "No posees los derechos de administrador",
                    "Advertencia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation
                );

                return;
            }



            if (lstProductos.SelectedItem == null)
                return;

            if (configApp.ConfirmacionEliminar)
            {
                var result = MessageBox.Show(
                    "Este producto se eliminará permanentemente.\n¿Deseas continuar?",
                    "Confirmar eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.No)
                    return;
            }

            Producto productoSeleccionado = (Producto)lstProductos.SelectedItem;

            using (var db = new AppDbContext())
            {
                var productoBD = db.Productos
                    .FirstOrDefault(p => p.Id == productoSeleccionado.Id);

                if (productoBD != null)
                {
                    db.Productos.Remove(productoBD);
                    db.SaveChanges();
                }
            }

            // eliminar también de la lista local
            productos.Remove(productoSeleccionado);

            RefrescarTodo();
            RenderCategorias();
            RenderProductos();

            MessageBox.Show(
                "Producto eliminado correctamente.",
                "Información",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
        private void AgregarCategoria_Click(object sender, RoutedEventArgs e)
        {
            // =========================
            // CONTROL DE PERMISOS
            // =========================
            // Solo administradores pueden añadir categorías
            if (usuarioActual.Rol == "Cajero")
            {
                MessageBox.Show(
                    "No posees los derechos de administrador",
                    "Advertencia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation
                );

                return;
            }

            // =========================
            // OBTENER TEXTO DE LA UI
            // =========================
            string cat = txtNuevaCategoria.Text;

            // Validación: no permitir vacío o espacios
            if (string.IsNullOrWhiteSpace(cat)) return;

            // =========================
            // VALIDACIÓN DE DUPLICADOS
            // =========================
            // Evita crear categorías con el mismo nombre
            if (categorias.Any(c => c.Nombre == cat)) return;

            // =========================
            // CREACIÓN DE LA CATEGORÍA
            // =========================
            categorias.Add(new Categoria
            {
                Nombre = cat
            });

            // =========================
            // PERSISTENCIA EN BASE DE DATOS
            // =========================
            GuardarCategorias();

            // =========================
            // ACTUALIZACIÓN DE COMBOS
            // =========================
            cmbCategorias.ItemsSource = categorias
                .Where(c => c.Nombre != "Todas")
                .Select(c => c.Nombre)
                .ToList();

            cmbFiltroCategoria.ItemsSource = categorias
                .Select(c => c.Nombre)
                .ToList();

            // =========================
            // REFRESCO DE INTERFAZ
            // =========================
            RefrescarTodo();
            RenderCategorias();
            RenderProductos();
        }
        private void EliminarCategoria_Click(object sender, RoutedEventArgs e)
        {

            // Verifica permisos
            if (usuarioActual.Rol == "Cajero")
            {
                MessageBox.Show(
                    "No posees los derechos de administrador",
                    "Advertencia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation
                );

                return;
            }


            if (lstCategorias.SelectedItem is not Categoria catSeleccionada)
                return;

            using (var db = new AppDbContext())
            {
                // 1. eliminar productos asociados
                var productosRelacionados = db.Productos
                    .Where(p => p.CategoriaId == catSeleccionada.Id)
                    .ToList();

                db.Productos.RemoveRange(productosRelacionados);

                // 2. eliminar categoría
                var categoria = db.Categorias
                    .FirstOrDefault(c => c.Id == catSeleccionada.Id);

                if (categoria != null)
                {
                    db.Categorias.Remove(categoria);
                }

                db.SaveChanges();
            }

            // 3. recargar desde BD
            CargarCategoriasDesdeBD();
            RefrescarTodo();
            RenderCategorias();
            RenderProductos();
        }
        private void CargarCategoriasDesdeBD()
        {
            using (var db = new AppDbContext())
            {
                categorias = db.Categorias.ToList();
            }
        }

        // =====================================================
        //  FILTROS
        // =====================================================

        private void TxtBuscarProducto_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void TxtPrecioMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void TxtPrecioMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();
        }
        private void CmbFiltroCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }
        private void LstProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstProductos.SelectedItem == null) return;

            Producto p = (Producto)lstProductos.SelectedItem;

            txtNombreProducto.Text = p.Nombre;
            txtPrecioProducto.Text = p.Precio.ToString();
            cmbCategorias.SelectedItem = p.Categoria;
            rutaImagen = p.Imagen;
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            RenderProductos();
        }

        private void AplicarFiltros()
        {
            var lista = productos.AsEnumerable();

            //  búsqueda
            if (!string.IsNullOrWhiteSpace(txtBuscarProducto.Text))
            {
                string t = txtBuscarProducto.Text.ToLower();

                lista = lista.Where(p =>
                    p.Nombre.ToLower().Contains(t) ||
                    (p.Categoria != null && p.Categoria.Nombre.ToLower().Contains(t))
                );
            }

            //  categoría
            if (cmbFiltroCategoria.SelectedItem is Categoria cat)
            {
                // SOLO filtra si NO es "Todas"
                if (cat.Nombre != "Todas")
                {
                    lista = lista.Where(p =>
                        p.Categoria != null &&
                        p.Categoria.Id == cat.Id
                    );
                }
            }
            //  precio min
            if (decimal.TryParse(txtPrecioMin.Text, out decimal min))
                lista = lista.Where(p => p.Precio >= min);

            //  precio max
            if (decimal.TryParse(txtPrecioMax.Text, out decimal max))
                lista = lista.Where(p => p.Precio <= max);

            lstProductos.ItemsSource = lista.ToList();
        }


        // =====================================================
        //  CARRITO 
        // =====================================================


        //Agrega el producto al carrito
        private void Producto_Click(object sender, RoutedEventArgs e)
        {
            var prod = (Producto)((Border)sender).DataContext;
            AgregarProducto(prod.Nombre, prod.Precio);
        }
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
        private void LstPedido_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lstPedido.SelectedIndex == -1)
                return;

            var producto = carrito[lstPedido.SelectedIndex];

            // SI HAY MÁS DE 1 → RESTA
            if (producto.Cantidad > 1)
            {
                producto.Cantidad--;
            }
            else
            {
                // SI SOLO HAY 1 → ELIMINA
                carrito.Remove(producto);
            }

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
                lstPedido.Items.Add($"{p.Cantidad} x {p.Nombre} - {p.Precio * p.Cantidad:C}");
        }

        public void Confirmar_Click(object sender, RoutedEventArgs e)
        {
            if (carrito.Count == 0)
            {
                MessageBox.Show(
                        "El pedido no contiene nada",
                        "Información",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                return;
            }

            string numero = GenerarNumeroTicket();
            decimal total = carrito.Sum(p => p.Precio * p.Cantidad);

            // ----------------------------
            // IMPORTE RECIBIDO
            // ----------------------------
            decimal importe = 0;
            ImporteWindow ventana = new ImporteWindow(total);

            if (ventana.ShowDialog() != true)
            {
                return;
            }

            importe = ventana.Importe;

            // ----------------------------
            // FACTURA PARA TICKET
            // ----------------------------
            Factura factura = new Factura
            {
                Numero = numero,
                Usuario = usuarioActual.Nombre,
                Fecha = DateTime.Now,
                Productos = carrito.Select(p => new Producto
                {
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    Cantidad = p.Cantidad
                }).ToList(),
                Total = total,
                Importe = importe
            };

            // ----------------------------
            // BASE DE DATOS
            // ----------------------------
            using (var db = new AppDbContext())
            {
                // 1. CREAR VENTA
                Venta venta = new Venta
                {
                    Numero = numero,
                    Fecha = DateTime.Now,
                    Total = total,
                    UsuarioId = usuarioActual.Id,
                    Importe = importe
                };

                db.Ventas.Add(venta);
                db.SaveChanges(); //  necesario para obtener venta.Id

                // 2. CREAR DETALLES
                foreach (var item in carrito)
                {
                    var productoBD = db.Productos
                        .FirstOrDefault(p => p.Nombre == item.Nombre);

                    if (productoBD != null)
                    {
                        VentasDetalles detalle = new VentasDetalles
                        {
                            VentaId = venta.Id,
                            ProductoId = productoBD.Id,
                            Cantidad = item.Cantidad,
                            PrecioUnitario = item.Precio
                        };

                        db.VentaDetalles.Add(detalle);
                    }
                }

                db.SaveChanges();
                CargarDashboard();

                // ----------------------------
                // AGREGAR PEDIDO A PREPARACIÓN
                // ----------------------------
                pedidos.Add(new Pedido
                {
                    Numero = numero,

                    Estado = "En preparación"
                });

                // refrescar UI
                ActualizarPantallas();
            }

            // ----------------------------
            // UI + TICKET
            // ----------------------------
            MostrarTicketTermico(factura);

            MessageBox.Show(
    $"Pedido {numero} registrado correctamente.\n\n" +
    "El ticket se ha generado y está listo para impresión.",
    "Confirmación de venta",
    MessageBoxButton.OK,
    MessageBoxImage.Information
);

            if (configApp.ImpresionAuto)
            {
                Task.Delay(200).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ImprimirTicketTermico(factura);
                    });
                });
            }


            // ----------------------------
            // LIMPIAR CARRITO
            // ----------------------------
            carrito.Clear();
            lstPedido.Items.Clear();
            ActualizarTotal();

            ActualizarPantallas();



        }

        // =====================================================
        //  PEDIDOS 
        // =====================================================
        public void ActualizarPantallas()
        {
            lstPedidosPreparacion.Items.Clear();
            lstPedidosListos.Items.Clear();

            foreach (var p in pedidos)
            {
                if (p.Estado == "En preparación")
                    lstPedidosPreparacion.Items.Add(p.Numero);

                if (p.Estado == "Listo")
                    lstPedidosListos.Items.Add(p.Numero);
            }

            if (pantallaPedidos != null)
                pantallaPedidos.ActualizarPedidos(pedidos);
        }

        //Marca como listo un pedido
        public void MarcarListo_Click(object sender, RoutedEventArgs e)
        {
            if (lstPedidosPreparacion.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un pedido", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string numero = lstPedidosPreparacion.SelectedItem.ToString();

            foreach (var p in pedidos)
            {
                if (p.Numero == numero)
                {
                    if (configApp.VozPedidos)
                    {
                        Hablar($"El pedido número {p.Numero} está listo");
                    }

                    p.Estado = "Listo";
                    break;
                }
            }

            ActualizarPantallas();
        }

        //Función para las voces de los pedidos
        private void Hablar(string texto)
        {
            if (!configApp.VozPedidos)
                return;

            synth.SpeakAsyncCancelAll();

            synth.Volume = (int)sliderVolumenVoz.Value;
            synth.Rate = (int)sliderVelocidadVoz.Value;

            if (cmbVoces.SelectedItem != null)
            {
                synth.SelectVoice(cmbVoces.SelectedItem.ToString());
            }

            synth.SpeakAsync(texto);
        }

        //Elimina un pedido de la lista de pedidos terminados
        private void Eliminar_Click(object sender, RoutedEventArgs e)
        {
            if (lstPedidosListos.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un pedido", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
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
        //  USUARIOS 
        // =====================================================


        private void agregarUsuarios(object sender, RoutedEventArgs e)
        {
            if (usuarioActual.Rol == "Cajero")
            {
                MessageBox.Show("No tienes los requisitos para realizar esta accion",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text))
            {
                MessageBox.Show("Rellena el nombre",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtConstraseña.Password))
            {
                MessageBox.Show("Rellena la contraseña",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }
            if (cmbRol.SelectedIndex == -1)
            {
                MessageBox.Show("Selecciona un rol",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }
            var res = CrearHash(txtConstraseña.Password);

            using (var db = new AppDbContext())
            {
                var nuevoUsuario = new Usuario
                {
                    Nombre = txtNombreUsuario.Text,
                    PasswordHash = res.hash,
                    Salt = res.salt,
                    Rol = ((ComboBoxItem)cmbRol.SelectedItem).Content.ToString()
                };

                db.Usuarios.Add(nuevoUsuario);
                db.SaveChanges();
            }

            CargarUsuariosDesdeBD(); // refrescar grid desde DB
        }
        private void CargarUsuariosDesdeBD()
        {
            using (var db = new AppDbContext())
            {
                usuarios = db.Usuarios.ToList();
                dgUsuarios.ItemsSource = usuarios;
            }
        }

        //Esta aplicacion utiliza un metodo de encriptacion con hash + salt para aumentar la seguridad de las contraseñas
        public static (string hash, string salt) CrearHash(string password)
        {
            // Creamos un array de 16 bytes que será el SALT (valor aleatorio)
            // El salt sirve para hacer el hash más seguro y evitar ataques por diccionario
            byte[] saltBytes = new byte[16];

            // Generador criptográficamente seguro de números aleatorios
            using (var rng = RandomNumberGenerator.Create())
            {
                // Rellena el array saltBytes con valores aleatorios
                rng.GetBytes(saltBytes);
            }

            // Creamos instancia del algoritmo SHA256 para generar el hash
            using (var sha256 = SHA256.Create())
            {
                // Convertimos la contraseña a bytes
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                // Creamos un array que contiene contraseña + salt
                byte[] combinado = new byte[passwordBytes.Length + saltBytes.Length];

                // Copiamos primero la contraseña al array combinado
                Buffer.BlockCopy(passwordBytes, 0, combinado, 0, passwordBytes.Length);

                // Copiamos después el salt al final del array
                Buffer.BlockCopy(saltBytes, 0, combinado, passwordBytes.Length, saltBytes.Length);

                // Generamos el hash final usando SHA256 sobre la combinación
                byte[] hash = sha256.ComputeHash(combinado);

                // Devuelve un hash en Base64 (para guardarlo en BD) y un salt en Base64 (para poder verificar luego la contraseña)
                return (
                    Convert.ToBase64String(hash),
                    Convert.ToBase64String(saltBytes)
                );
            }
        }
        private void modificarUsuario(object sender, RoutedEventArgs e)
        {
            if (usuarioActual.Rol == "Cajero")
            {
                MessageBox.Show("No posees los derechos de administrador", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text))
            {
                MessageBox.Show("Rellena el nombre",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtConstraseña.Password))
            {
                MessageBox.Show("Rellena la contraseña",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }
            if (cmbRol.SelectedIndex == -1)
            {
                MessageBox.Show("Selecciona un rol",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            if (dgUsuarios.SelectedItem == null) return;

            var seleccionado = (Usuario)dgUsuarios.SelectedItem;

            using (var db = new AppDbContext())
            {
                var u = db.Usuarios.FirstOrDefault(x => x.Id == seleccionado.Id);

                if (u == null) return;

                u.Nombre = txtNombreUsuario.Text;
                u.Rol = ((ComboBoxItem)cmbRol.SelectedItem).Content.ToString();

                // SOLO si cambia contraseña
                if (!string.IsNullOrWhiteSpace(txtConstraseña.Password))
                {
                    var res = CrearHash(txtConstraseña.Password);

                    u.PasswordHash = res.hash;
                    u.Salt = res.salt;
                }

                db.SaveChanges();
            }

            CargarUsuariosDesdeBD();
        }

        private void eliminarUsuario(object sender, RoutedEventArgs e)
        {
            if (usuarioActual.Rol == "Cajero")
            {
                MessageBox.Show("No posees los derechos de administrador", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (dgUsuarios.SelectedItem == null) return;

            var seleccionado = (Usuario)dgUsuarios.SelectedItem;

            using (var db = new AppDbContext())
            {
                var u = db.Usuarios.FirstOrDefault(x => x.Id == seleccionado.Id);

                if (u == null) return;

                db.Usuarios.Remove(u);
                db.SaveChanges();
            }

            CargarUsuariosDesdeBD();
        }

        private void dgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsuarios.SelectedItem == null) return;

            var u = (Usuario)dgUsuarios.SelectedItem;

            txtNombreUsuario.Text = u.Nombre;

            //  NUNCA mostrar hash en password
            txtConstraseña.Password = "";

            foreach (ComboBoxItem item in cmbRol.Items)
            {
                if ((string)item.Content == u.Rol)
                {
                    cmbRol.SelectedItem = item;
                    break;
                }
            }
        }
        //Verifica q la contraseña coincida con el hash generado
        public static bool VerificarPassword(string passwordIngresado, string hashGuardado, string saltGuardado)
        {
            // Convertimos el salt almacenado (Base64) de vuelta a bytes
            byte[] saltBytes = Convert.FromBase64String(saltGuardado);

            // Creamos instancia del algoritmo SHA256
            using (var sha256 = SHA256.Create())
            {
                // Convertimos la contraseña que introduce el usuario a bytes
                byte[] passwordBytes = Encoding.UTF8.GetBytes(passwordIngresado);

                // Creamos un array que contendrá contraseña + salt
                byte[] combinado = new byte[passwordBytes.Length + saltBytes.Length];

                // Copiamos la contraseña al inicio del array
                Buffer.BlockCopy(passwordBytes, 0, combinado, 0, passwordBytes.Length);

                // Copiamos el salt al final del array
                Buffer.BlockCopy(saltBytes, 0, combinado, passwordBytes.Length, saltBytes.Length);

                // Generamos el hash de la combinación (password + salt)
                byte[] hash = sha256.ComputeHash(combinado);

                // Convertimos el hash generado a Base64 para poder compararlo
                string hashNuevo = Convert.ToBase64String(hash);

                // Comparamos el hash generado con el hash guardado en la base de datos
                return hashNuevo == hashGuardado;
            }
        }
        // =====================================================
        //  TICKET 
        // =====================================================

        //La previsualización del ticket
        private void MostrarTicketTermico(Factura factura)
        {
            facturaActual = factura;

            tkEmpresa.Text = configTicket.Empresa;
            tkDireccion.Text = configTicket.Direccion;
            tkCIF.Text = $"CIF: {configTicket.CIF}";


            tkNumero.Text = $"Ticket: {factura.Numero}";
            tkFecha.Text = $"Fecha: {factura.Fecha:dd/MM/yyyy HH:mm}";
            tkUsuario.Text = $"Atendido por: {factura.Usuario}";

            tkProductos.ItemsSource = null;
            tkProductos.ItemsSource = factura.Productos.ToList();

            tkTotal.Text = $"{factura.Total:0.00} €";
            tkRecibido.Text = $"{factura.Importe:0.00} €";
            tkCambio.Text = $"{factura.Importe - factura.Total:0.00} €";

            ticketTermico.Visibility = Visibility.Visible;
        }

        private void AplicarCambiosTicket_Click(object sender, RoutedEventArgs e)
        {
            tkEmpresa.Text = txtEmpresa.Text;
            tkDireccion.Text = txtDireccion.Text;
            tkCIF.Text = $"CIF: {txtCIF.Text}";

            configTicket.Empresa = txtEmpresa.Text;
            configTicket.Direccion = txtDireccion.Text;
            configTicket.CIF = txtCIF.Text;



            GuardarConfiguracionTicket();
        }

        private async void ImprimirTicketTermico(Factura factura)
        {
            ticketTermico.Visibility = Visibility.Visible;

            // =========================
            // CARGAR PRODUCTOS DESDE FACTURA
            // =========================
            var lista = factura.Productos
                .Select(p => new
                {
                    p.Nombre,
                    p.Cantidad,
                    p.Precio,
                    PrecioTotal = p.Precio * p.Cantidad
                })
                .ToList();

            tkProductos.ItemsSource = lista;

            // =========================
            // FORZAR RENDERIZADO DEL TICKET
            // =========================
            tkProductos.ApplyTemplate();
            tkProductos.UpdateLayout();

            ticketTermico.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            ticketTermico.Arrange(new Rect(
                0,
                0,
                ticketTermico.DesiredSize.Width,
                ticketTermico.DesiredSize.Height));

            ticketTermico.UpdateLayout();

            // Pequeña espera para asegurar render final WPF
            await Task.Delay(100);
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
            await Task.Delay(300);

            // =========================
            // IMPRIMIR
            // =========================
            PrintDialog pd = new PrintDialog();
            string nombreTicket = $"Ticket_{factura.Numero}_{DateTime.Now:dd-MM-yyyy_HH-mm}";

            if (pd.ShowDialog() == true)
            {
                pd.PrintVisual(ticketTermico, nombreTicket);
            }
        }


        private string GenerarNumeroTicket()
        {
            using (var db = new AppDbContext())
            {
                var ultimaVenta = db.Ventas
                    .OrderByDescending(v => v.Id)
                    .FirstOrDefault();

                // PRIMER TICKET
                if (ultimaVenta == null)
                    return "A001";

                string ultimoNumero = ultimaVenta.Numero;

                // LETRA
                char letra = ultimoNumero[0];

                // NÚMERO
                int numero = int.Parse(ultimoNumero.Substring(1));

                numero++;

                // SI LLEGA A 999 CAMBIA LETRA
                if (numero > 999)
                {
                    numero = 1;
                    letra++;
                }

                return $"{letra}{numero:D3}";
            }
        }

        // =========================
        // DASHBOARD
        // =========================
        private void CargarDashboard()
        {
            using (var db = new AppDbContext())
            {

                // Carga todas las ventas ordenadas de más reciente a más antigua
                dgUltimasVentas.ItemsSource = db.Ventas
                    .Include(v => v.Usuario) // incluye el usuario que realizó la venta
                    .OrderByDescending(v => v.Fecha)
                    .ToList();


                // Suma total de todas las ventas realizadas
                txtTotalVentas.Text = db.Ventas
                    .Sum(v => v.Total)
                    .ToString("0.00 €");


                // Filtra ventas del día actual y suma su total
                txtVentasHoy.Text = db.Ventas
                    .Where(v => v.Fecha.Date == DateTime.Now.Date)
                    .Sum(v => v.Total)
                    .ToString("0.00 €");

                // Cuenta cuántas ventas se han hecho hoy
                txtPedidosHoy.Text = db.Ventas
                    .Count(v => v.Fecha.Date == DateTime.Now.Date)
                    .ToString();


                // TOP 10 PRODUCTOS MÁS VENDIDOS (GENERAL)
                var topProductos = db.VentaDetalles
                  .Include(vd => vd.Producto)
                  .GroupBy(vd => vd.Producto.Nombre) // agrupa por nombre de producto
                  .Select(g => new
                  {
                      Nombre = g.Key,
                      Cantidad = g.Sum(x => x.Cantidad) // suma unidades vendidas
                  })
                  .OrderByDescending(x => x.Cantidad)
                  .Take(10)
                  .ToList();

                // muestra ranking general
                lstTopProductos.ItemsSource = topProductos;

                // producto más vendido del ranking general
                txtProductoTopGrande.Text =
                    topProductos.FirstOrDefault()?.Nombre ?? "-";


                // PRODUCTO MÁS VENDIDO HOY
                var topHoy = db.VentaDetalles
                    .Include(vd => vd.Producto)
                    .Include(vd => vd.Venta)
                    .Where(vd => vd.Venta.Fecha.Date == DateTime.Now.Date) // solo ventas de hoy
                    .GroupBy(vd => vd.Producto.Nombre)
                    .Select(g => new
                    {
                        Nombre = g.Key,
                        Cantidad = g.Sum(x => x.Cantidad)
                    })
                    .OrderByDescending(x => x.Cantidad)
                    .FirstOrDefault();

                // muestra el producto más vendido hoy
                txtProductoTop.Text = topHoy?.Nombre ?? "-";
            }
        }

        // =========================
        // CONFIGURACION
        // =========================
        private void AplicarTema_Click(object sender, RoutedEventArgs e)
        {
            string tema = ((ComboBoxItem)cmbTema.SelectedItem).Content.ToString();
            string archivoTema;

            switch (tema)
            {
                case "Oscuro":
                    archivoTema = "Dark.xaml";
                    break;

                case "Azul":
                    archivoTema = "Blue.xaml";
                    break;

                case "Verde":
                    archivoTema = "Green.xaml";
                    break;

                case "Claro":
                    archivoTema = "Light.xaml";
                    break;

                case "Naranja":
                    archivoTema = "Orange.xaml";
                    break;

                case "Morado":
                    archivoTema = "Purple.xaml";
                    break;

                default:
                    archivoTema = "Light.xaml";
                    break;
            }

            Uri uri = new Uri($"Temas/{archivoTema}", UriKind.Relative);
            ResourceDictionary nuevoTema = new ResourceDictionary();
            nuevoTema.Source = uri;

            // Recorre todos los diccionarios de recursos cargados en la aplicación
            for (int i = Application.Current.Resources.MergedDictionaries.Count - 1; i >= 0; i--)
            {
                ResourceDictionary dict = Application.Current.Resources.MergedDictionaries[i];

                if (dict.Source != null && dict.Source.OriginalString.Contains("Temas/"))
                {
                    Application.Current.Resources.MergedDictionaries.Remove(dict);
                }
            }

            Application.Current.Resources.MergedDictionaries.Add(nuevoTema);
            RenderProductos();

        }
        private void AplicarPantallaPedidos_Click(object sender, RoutedEventArgs e)
        {
            if (chkPantallaPedidos.IsChecked == true)
            {
                if (pantallaPedidos == null)
                {
                    pantallaPedidos = new PantallaPedidos();
                    pantallaPedidos.Show();
                }
            }
            else
            {
                pantallaPedidos?.Close();
                pantallaPedidos = null;
            }

            // PANTALLA COMPLETA


            if (chkPantallaCompleta.IsChecked == true)
            {
                pantallaPedidos.WindowState = WindowState.Maximized;
                pantallaPedidos.WindowStyle = WindowStyle.None;
            }
            else
            {
                if (pantallaPedidos == null)
                {
                    return;
                }
                pantallaPedidos.WindowState = WindowState.Normal;
                pantallaPedidos.WindowStyle = WindowStyle.SingleBorderWindow;
            }


            // RELOJ

            lblHora.Visibility = chkMostrarReloj.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;

            MessageBox.Show(
              "Configuración aplicada correctamente.",
              "Información",
              MessageBoxButton.OK,
              MessageBoxImage.Information
            );
        }


        // ------------------------
        // BACKUP BASE DE DATOS
        // ------------------------

        private void CrearBackup_Click(object sender, RoutedEventArgs e)
        {
            // Verifica permisos: solo administradores pueden hacer backup
            if (usuarioActual.Rol == "Cajero")
            {
                MessageBox.Show(
                    "No posees los derechos de administrador",
                    "Advertencia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation
                );

                return;
            }

            try
            {
                // Lee la cadena de conexión desde App.config
                string connStr = ConfigurationManager
                    .ConnectionStrings["DefaultConnection"]
                    .ConnectionString;

                // Extrae los datos de conexión (servidor, usuario, contraseña, base de datos)
                var builder = new MySqlConnectionStringBuilder(connStr);

                string database = builder.Database;
                string user = builder.UserID;
                string password = builder.Password;
                string server = builder.Server;

                // Ruta de la carpeta donde se guardarán los backups
                string carpeta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups");

                // Si no existe la carpeta, la crea
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                // Genera un nombre de archivo con la fecha y hora actual
                string fecha = DateTime.Now.ToString("dd-MM-yyyy_HH-mm");
                string archivo = Path.Combine(carpeta, $"backup_{fecha}.sql");

                // Ruta del ejecutable mysqldump (debe estar en la carpeta de la app)
                string mysqldump = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "mysqldump.exe"
                );

                // Si mysqldump no existe, muestra error y sale
                if (!File.Exists(mysqldump))
                {
                    MessageBox.Show("No se encontró mysqldump.exe");
                    return;
                }

                // Configura el proceso para ejecutar mysqldump
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = mysqldump,
                    // Parámetros de conexión a MySQL
                    Arguments = $"-h {server} -u {user} -p{password} {database}",
                    RedirectStandardOutput = true, // captura la salida (SQL)
                    RedirectStandardError = true,  // captura errores
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process p = new Process())
                {
                    p.StartInfo = psi;
                    p.Start();

                    // Guarda la salida del dump en el archivo .sql
                    using (var fs = new FileStream(archivo, FileMode.Create, FileAccess.Write))
                    {
                        p.StandardOutput.BaseStream.CopyTo(fs);
                    }

                    // Lee posibles errores del proceso
                    string error = p.StandardError.ReadToEnd();
                    p.WaitForExit();

                    // Si falla el proceso, muestra el error
                    if (p.ExitCode != 0 || !string.IsNullOrWhiteSpace(error))
                    {
                        MessageBox.Show(error, "Error en backup",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Mensaje de éxito
                MessageBox.Show(
                    "Backup creado correctamente.",
                    "OK",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                // Captura cualquier error inesperado
                MessageBox.Show(ex.Message);
            }
        }

        // -----------------
        // RESTAURAR BACKUP
        // -----------------

        private void RestaurarBackup_Click(object sender, RoutedEventArgs e)
        {
            // =========================
            // VERIFICACIÓN DE PERMISOS
            // =========================
            // Solo administradores pueden restaurar backups
            if (usuarioActual.Rol == "Cajero")
            {
                MessageBox.Show(
                    "No posees los derechos de administrador",
                    "Advertencia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation
                );

                return;
            }

            // =========================
            // SELECCIÓN DE ARCHIVO
            // =========================
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "SQL (*.sql)|*.sql"
            };

            // Si el usuario cancela, se sale del método
            if (dlg.ShowDialog() != true)
                return;

            try
            {
                // =========================
                // LECTURA CONFIGURACIÓN BD
                // =========================
                string connStr = ConfigurationManager
                    .ConnectionStrings["DefaultConnection"]
                    .ConnectionString;

                // Extrae datos de conexión
                var builder = new MySqlConnectionStringBuilder(connStr);

                string database = builder.Database;
                string user = builder.UserID;
                string password = builder.Password;
                string server = builder.Server;

                // =========================
                // LOCALIZACIÓN MYSQL CLIENT
                // =========================
                string mysql = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "mysql.exe"
                );

                // Si no existe el ejecutable, error
                if (!File.Exists(mysql))
                {
                    MessageBox.Show("No se encontró mysql.exe");
                    return;
                }

                // =========================
                // CONFIGURACIÓN DEL PROCESO
                // =========================
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = mysql,
                    // Parámetros de conexión a la base de datos
                    Arguments = $"-h {server} -u {user} -p{password} {database}",
                    RedirectStandardInput = true,   // permite enviar el .sql al proceso
                    RedirectStandardError = true,   // captura errores
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process p = new Process())
                {
                    p.StartInfo = psi;
                    p.Start();

                    // =========================
                    // CARGA DEL ARCHIVO SQL
                    // =========================
                    // Se envía el contenido del .sql directamente al proceso mysql
                    using (var sr = new StreamReader(dlg.FileName))
                    {
                        sr.BaseStream.CopyTo(p.StandardInput.BaseStream);
                    }

                    // Cierra la entrada estándar para indicar fin del archivo
                    p.StandardInput.Close();

                    // Lee errores del proceso
                    string error = p.StandardError.ReadToEnd();
                    p.WaitForExit();

                    // Si hay error o código de salida incorrecto
                    if (p.ExitCode != 0 || !string.IsNullOrWhiteSpace(error))
                    {
                        MessageBox.Show(error, "Error al restaurar",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // =========================
                // MENSAJE DE ÉXITO
                // =========================
                MessageBox.Show(
                    "Backup restaurado correctamente.",
                    "OK",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // =========================
                // RECARGA DE DATOS EN MEMORIA
                // =========================
                using (var db = new AppDbContext())
                {
                    productos = db.Productos.ToList();
                    categorias = db.Categorias.ToList();
                    usuarios = db.Usuarios.ToList();
                }

                // Refresca toda la UI
                RefrescarTodo();
                RenderCategorias();
                RenderProductos();
            }
            catch (Exception ex)
            {
                // Captura cualquier error inesperado
                MessageBox.Show(ex.Message);
            }
        }



        private void CargarConfiguracionApp()
        {
            if (File.Exists("config_app.json"))
            {
                string json = File.ReadAllText("config_app.json");
                configApp = System.Text.Json.JsonSerializer.Deserialize<ConfigApp>(json);
            }
            else
            {
                configApp = new ConfigApp
                {
                    Tema = "Claro",
                    PantallaPedidos = true,
                    PantallaCompleta = false,
                    MostrarReloj = true,

                    VozPedidos = false,
                    ConfirmacionEliminar = true,
                    ImpresionAuto = false,


                };
            }

            // =========================
            // UI SYNC
            // =========================
            cmbTema.Text = configApp.Tema;

            chkPantallaPedidos.IsChecked = configApp.PantallaPedidos;
            chkPantallaCompleta.IsChecked = configApp.PantallaCompleta;
            chkMostrarReloj.IsChecked = configApp.MostrarReloj;

            chkVozPedidos.IsChecked = configApp.VozPedidos;
            chkConfirmacionEliminar.IsChecked = configApp.ConfirmacionEliminar;
            chkImpresionAuto.IsChecked = configApp.ImpresionAuto;

            // =========================
            // TEMA
            // =========================
            AplicarTema_Click(null, null);


        }

        private void ProbarVoz_Click(object sender, RoutedEventArgs e)
        {
            Hablar("Esta es una prueba de voz del sistema");
        }

        //Guarda las configuraciones en un JSON
        private void GuardarConfiguracionApp_Click(object sender, RoutedEventArgs e)
        {
            configApp.PantallaPedidos = chkPantallaPedidos.IsChecked == true;
            configApp.PantallaCompleta = chkPantallaCompleta.IsChecked == true;
            configApp.MostrarReloj = chkMostrarReloj.IsChecked == true;


            configApp.VozPedidos = chkVozPedidos.IsChecked == true;
            configApp.ConfirmacionEliminar = chkConfirmacionEliminar.IsChecked == true;
            configApp.ImpresionAuto = chkImpresionAuto.IsChecked == true;


            configApp.Tema = (cmbTema.SelectedItem as ComboBoxItem)?.Content.ToString();

            string json = System.Text.Json.JsonSerializer.Serialize(
                configApp,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText("config_app.json", json);

            MessageBox.Show(
                "Configuración guardada correctamente.",
                "Configuración",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }


        // =====================================================
        // CLASES
        // =====================================================


        public class Pedido
        {
            public string Numero { get; set; }

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

        public class ConfiguracionTicket
        {
            public string Empresa { get; set; }
            public string Direccion { get; set; }
            public string CIF { get; set; }

        }

        public class ConfigApp
        {
            public string Tema { get; set; }

            public bool PantallaPedidos { get; set; }
            public bool PantallaCompleta { get; set; }
            public bool MostrarReloj { get; set; }

            public bool VozPedidos { get; set; }
            public bool ConfirmacionEliminar { get; set; }
            public bool ImpresionAuto { get; set; }

        }
    }
}