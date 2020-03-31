using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaheuristicas
{
    public class Celda
    {
        public decimal distancia { get; set; }
    }

    public class MatrizAdy
    {
        List<List<Celda>> _contenido;
        public MatrizAdy(int cantidad)
        {
            _contenido = new List<List<Celda>>();
            for (var i = 0; i < cantidad; i++)
            {
                _contenido.Add(new List<Celda>());
                for (var j = 0; j < cantidad; j++)
                    _contenido[i].Add(new Celda());
            }
        }
        public int Count
        {
            get
            {
                return _contenido.Count;
            }
        }
        public List<Celda> Get(int nodo)
        {
            return _contenido[nodo];
        }
        public Celda Get(int i, int j)
        {
            return _contenido[i][j];
        }
        public void Set(int i, int j, Celda celda)
        {
            var _celda = Get(i, j);
            if (_celda == null)
            {
                _contenido[i][j] = celda;
            }
            else
            {
                _celda.distancia = celda.distancia;
            }
        }
    }
}
