using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaheuristicas
{
    static class Utilidades
    {
        public static void PermutarTodo(MatrizAdy celdas, Camino solucion)
        {
            var permutaciones = GetPermutaciones(solucion.Nodos.Select(x => x).ToList(), 0, solucion.Nodos.Count - 1);
            var mejor_camino_local = solucion.Nodos;
            var mejor_costo_local = solucion.Costo;

            foreach (var modificacion in permutaciones)
            {
                var costoModificacion = CalcularCosto(celdas, modificacion);
                if (costoModificacion < mejor_costo_local)
                {
                    mejor_camino_local = modificacion.Select(x => x).ToList();
                    mejor_costo_local = costoModificacion;
                }
            }
        }

        public static IEnumerable<List<int>> GetPermutaciones(List<int> lista, int k, int m)
        {
            int i;
            if (k == m)
            {
                yield return lista;
            }
            else
                for (i = k; i <= m; i++)
                {
                    var tmp = lista[k];
                    lista[k] = lista[i];
                    lista[i] = tmp;

                    foreach (var valorRecursion in GetPermutaciones(lista, k + 1, m))
                        yield return valorRecursion;

                    lista[i] = lista[k];
                    lista[k] = tmp;
                }
        }

        public static decimal CalcularCosto(MatrizAdy celdas, List<int> camino)
        {
            decimal costoActual = 0;
            for (var i = 0; i < camino.Count - 1; i++)
                costoActual += celdas.Get(camino[i], camino[i + 1]).distancia;
            costoActual += celdas.Get(camino[camino.Count - 1], camino[0]).distancia; // Sumo la cola con el comienzo
            return costoActual;
        }

        public static decimal CalcularCosto(MatrizAdy celdas, List<Camino> caminos)
        {
            decimal costoActual = 0;
            for (var i = 0; i < caminos.Count; i++)
            {
                for (var j = 0; j < caminos[i].Nodos.Count - 1; j++)
                    costoActual += celdas.Get(caminos[i].Nodos[j], caminos[i].Nodos[j + 1]).distancia;

                costoActual += celdas.Get(caminos[i].Nodos[caminos[i].Nodos.Count - 1], caminos[i].Nodos[0]).distancia; // Sumo la cola con el comienzo
            }
            return costoActual;
        }
    }
}
