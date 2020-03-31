using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Metaheuristicas
{
    public static class CargadorInstancias
    {
        public static FVRP CargarInstancia(string archivo)
        {
            var valores = archivo.Split(new string[] { " ", "    ", "\n", "\r\n", "\t" },
                StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToDecimal(x)).ToList();
            var cantidad_nodos = Convert.ToInt32(valores[0] + 1);
            var cantidad_familias = valores[1];
            var cantidad_visitas = valores[2];
            var capacidad_transporte = valores[3];

            var cant_nodos_familia = new List<int>();
            var familias = new List<Familia>();

            for (var i = 0; i < cantidad_familias; i++)
                cant_nodos_familia.Add(Convert.ToInt32(valores[4 + i]));
            for (var i = 0; i < cantidad_familias; i++)
                familias.Add(new Familia()
                {
                    id = i + 1,
                    cantidad_visitas = Convert.ToInt32(valores[4 + Convert.ToInt32(cantidad_familias) + i]),
                    cantidad_visitada = 0,
                    demanda = Convert.ToDecimal(valores[4 + Convert.ToInt32(cantidad_familias) * 2 + i]),
                    nodos = new List<int>()
                });

            var siguiente_valor = 1;
            for (var i = 0; i < cantidad_familias; i++)
            {
                for (var j = 0; j < cant_nodos_familia[i]; j++)
                    familias[i].nodos.Add(siguiente_valor + j);
                siguiente_valor += cant_nodos_familia[i];
            }

            familias.Insert(0, new Familia() { id = 0, nodos = new List<int>() { 0 }, cantidad_visitada = 0, cantidad_visitas = 0 });

            var demandas = new List<decimal>();
            for (var i = 0; i < cantidad_nodos; i++)
                demandas.Add(valores[4 + (int)cantidad_familias * 3 + i * 2 + 1]);

            var distancias = new List<List<decimal>>();
            var valor_suma_anterior = Convert.ToInt32(4 + cantidad_familias * 3 + cantidad_nodos * 2);
            for (var i = 0; i < cantidad_nodos; i++)
            {
                distancias.Add(new List<decimal>());
                for (var j = 0; j < cantidad_nodos; j++)
                    distancias[i].Add(valores[valor_suma_anterior + i * cantidad_nodos + j]);
            }

            return new FVRP(familias, distancias, demandas, capacidad_transporte, true);
        }
    }
}
