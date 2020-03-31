using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaheuristicas
{
    public class FVRP
    {
        public List<decimal> Demanda { get { return _demanda; } }
        List<decimal> _demanda;

        public decimal capacidad { get; set; }
        public decimal menor_demanda { get; set; }

        public FVRP(List<Familia> familias, List<List<decimal>> distancias, List<decimal> Demandas, decimal Capacidad, bool Simetrico)
        {
            _simetrico = simetrico;

            _distancias = distancias;
            _familias = familias;

            _nodos = new List<int>();
            foreach(var familia in familias)
                foreach (var nodo in familia.nodos)
                {
                    _nodos.Add(familia.id);
                }

            // Para VRP donde el nodo inicial no tiene familia
            _demanda = Demandas;
            capacidad = Capacidad;
        }

        bool _simetrico = true;
        public bool simetrico { get { return _simetrico; } set { _simetrico = value; } }

        public List<List<decimal>> Distancias { get { return _distancias; } }
        List<List<decimal>> _distancias;

        public List<Familia> Familias { get { return _familias; } }
        List<Familia> _familias;

        public List<int> FamiliaPorNodo { get { return _nodos; } }
        List<int> _nodos;

    }

    public class Familia
    {
        public List<int> nodos { get; set; }
        public int cantidad_visitas { get; set; }
        public int cantidad_visitada { get; set; }
        public decimal demanda { get; set; }
        public int id { get; set; }
    }
}
