using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaheuristicas
{
    public class Camino
    {
        public List<int> Nodos { get; set; }
        public decimal Costo { get; set; }
        public decimal Capacidad { get; set; }
    }

    public class SolucionFVRP
    {
        public SolucionFVRP(int cantidadNodos)
        {
            NodosUtilizadosGrasp = new bool[cantidadNodos];
        }
        public List<Camino> Caminos { get; set; }
        public bool[] NodosUtilizadosGrasp;
        public decimal Costo { get { return Caminos.Select(x => x.Costo).Sum(); } }
        public List<Familia> FamiliasPendientes { get; set; }
    }

    public class Solucion
    {
        public SolucionFVRP SolucionFVRP {get;set;}
        public decimal MejorAlfa { get; set; }
        public TimeSpan MejorTiempo { get; set; }
        public string TipoTerminacion { get; set; }
    }
}
