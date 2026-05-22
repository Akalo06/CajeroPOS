using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Collections.Generic;
using static CajeroPOS.MainWindow;
using System.Speech.Synthesis;



namespace CajeroPOS
{
    public partial class PantallaPedidos : Window
    {

        public PantallaPedidos()
        {
            InitializeComponent();


        }

        public void ActualizarPedidos(List<Pedido> pedidos)
        {
            lstPreparacion.Items.Clear();
            lstListos.Items.Clear();

            foreach (var p in pedidos)
            {
                if (p.Estado == "En preparación")
                    lstPreparacion.Items.Add(p.Numero);

                if (p.Estado == "Listo")
                {

                    lstListos.Items.Add(p.Numero);
                }


                if (p.Estado == "Eliminar")
                    lstListos.Items.Remove(p.Numero);
            }
        }

    }
}

