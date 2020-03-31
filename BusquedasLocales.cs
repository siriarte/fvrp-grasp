using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaheuristicas
{
    public class BusquedasLocales
    {
        int max_iteraciones_sin_mejora_busqueda_local;
        MatrizAdy celdas;
        List<Familia> familias;
        List<decimal> demandas;
        bool simetrico;
        bool explorar_completo;
        int longitud_camino;

        #region Contadores de iteraciones
        int _cantidad_iteraciones_two_opt;

        public int Cantidad_iteraciones_two_opt
        {
            get { return _cantidad_iteraciones_two_opt; }
            set { _cantidad_iteraciones_two_opt = value; }
        }
        int _cantidad_mejoras_two_opt;

        public int Cantidad_mejoras_two_opt
        {
            get { return _cantidad_mejoras_two_opt; }
            set { _cantidad_mejoras_two_opt = value; }
        }
        int _cantidad_iteraciones_relocate;

        public int Cantidad_iteraciones_relocate
        {
            get { return _cantidad_iteraciones_relocate; }
            set { _cantidad_iteraciones_relocate = value; }
        }
        int _cantidad_mejoras_relocate;

        public int Cantidad_mejoras_relocate
        {
            get { return _cantidad_mejoras_relocate; }
            set { _cantidad_mejoras_relocate = value; }
        }
        int _cantidad_iteraciones_exchange;

        public int Cantidad_iteraciones_exchange
        {
            get { return _cantidad_iteraciones_exchange; }
            set { _cantidad_iteraciones_exchange = value; }
        }
        int _cantidad_mejoras_exchange;

        public int Cantidad_mejoras_exchange
        {
            get { return _cantidad_mejoras_exchange; }
            set { _cantidad_mejoras_exchange = value; }
        }
        int _cantidad_iteraciones_or_opt;

        public int Cantidad_iteraciones_or_opt
        {
            get { return _cantidad_iteraciones_or_opt; }
            set { _cantidad_iteraciones_or_opt = value; }
        }
        int _cantidad_mejoras_or_opt;

        public int Cantidad_mejoras_or_opt
        {
            get { return _cantidad_mejoras_or_opt; }
            set { _cantidad_mejoras_or_opt = value; }
        }
        int _cantidad_iteraciones_four_opt;

        public int Cantidad_iteraciones_four_opt
        {
            get { return _cantidad_iteraciones_four_opt; }
            set { _cantidad_iteraciones_four_opt = value; }
        }
        int _cantidad_mejoras_four_opt;

        public int Cantidad_mejoras_four_opt
        {
            get { return _cantidad_mejoras_four_opt; }
            set { _cantidad_mejoras_four_opt = value; }
        }
        int _cantidad_iteraciones_family_exchange;

        public int Cantidad_iteraciones_family_exchange
        {
            get { return _cantidad_iteraciones_family_exchange; }
            set { _cantidad_iteraciones_family_exchange = value; }
        }
        int _cantidad_mejoras_family_exchange;

        public int Cantidad_mejoras_family_exchange
        {
            get { return _cantidad_mejoras_family_exchange; }
            set { _cantidad_mejoras_family_exchange = value; }
        }
        #endregion

        private void initialize(int longitudCamino, MatrizAdy _celdas, List<Familia> _familias, List<decimal> _demandas, bool _simetrico, int _max_iteraciones_sin_mejora_busqueda_local)
        {
            longitud_camino = longitudCamino;
            familias = _familias;
            demandas = _demandas;
            celdas = _celdas;
            simetrico = _simetrico;
            max_iteraciones_sin_mejora_busqueda_local = _max_iteraciones_sin_mejora_busqueda_local;
            explorar_completo = longitud_camino <= 64;

            _cantidad_iteraciones_exchange = 0;
            _cantidad_iteraciones_family_exchange = 0;
            _cantidad_iteraciones_four_opt = 0;
            _cantidad_iteraciones_or_opt = 0;
            _cantidad_iteraciones_relocate = 0;
            _cantidad_iteraciones_two_opt = 0;
            _cantidad_mejoras_exchange = 0;
            _cantidad_mejoras_family_exchange = 0;
            _cantidad_mejoras_four_opt = 0;
            _cantidad_mejoras_or_opt = 0;
            _cantidad_mejoras_relocate = 0;
            _cantidad_mejoras_two_opt = 0;
        }

        public BusquedasLocales(int longitudCamino, MatrizAdy _celdas, List<Familia> _familias, List<decimal> _demandas, bool _simetrico, int _max_iteraciones_sin_mejora_busqueda_local)
        {
            initialize(longitudCamino, _celdas, _familias, _demandas, _simetrico, _max_iteraciones_sin_mejora_busqueda_local);
        }

        public void two_opt(Camino solucion)
        {
            var max_w = solucion.Nodos.Count - 2;

            //if (explorar_completo)
            if (true)
            {
                for (var i = 0; i < solucion.Nodos.Count; i++)
                    for (var w = 1; w <= max_w; w++)
                    {
                        if (two_opt(solucion, i, w))
                            break;
                    }
            }
            else
            {
                var rnd = new Random();
                var cant_iteraciones = 0;
                while (cant_iteraciones < max_iteraciones_sin_mejora_busqueda_local)
                {
                    var w = rnd.Next(1, max_w + 1);
                    var i = rnd.Next(0, solucion.Nodos.Count);
                    if (two_opt(solucion, i, w))
                        cant_iteraciones = 0;
                    else
                        cant_iteraciones++;
                }
            }
        }

        bool two_opt(Camino solucion, int i, int w)
        {
            _cantidad_iteraciones_two_opt++;

            var j = i + w;

            var nodosAfectados = GetArray(i, j);

            for (var m = 0; m < nodosAfectados.Count; m++)
                nodosAfectados[m] -= nodosAfectados[m] >= solucion.Nodos.Count ? solucion.Nodos.Count : 0;

            if (j >= solucion.Nodos.Count)
                j -= solucion.Nodos.Count;

            var nodo_anterior_i = i - 1 < 0 ? solucion.Nodos.Count - 1 : i - 1;
            var nodo_siguiente_j = j + 1 >= solucion.Nodos.Count ? 0 : j + 1;

            // dar vuelta j nodos desde i
            var costo_restar_i = celdas.Get(solucion.Nodos[nodo_anterior_i], solucion.Nodos[i]).distancia;
            var costo_restar_j = celdas.Get(solucion.Nodos[j], solucion.Nodos[nodo_siguiente_j]).distancia;

            var costo_sumar_i = celdas.Get(solucion.Nodos[nodo_anterior_i], solucion.Nodos[j]).distancia;
            var costo_sumar_j = celdas.Get(solucion.Nodos[i], solucion.Nodos[nodo_siguiente_j]).distancia;

            var nuevo_costo = solucion.Costo - costo_restar_i - costo_restar_j + costo_sumar_i + costo_sumar_j;

            // Si no es simétrico hay que recalcular el costo de dar vuelta los nodos interiores
            if (!simetrico)
                for (var k = 0; k < nodosAfectados.Count - 1; k++)
                {
                    var costo_restar_k = celdas.Get(solucion.Nodos[nodosAfectados[k]], solucion.Nodos[nodosAfectados[k + 1]]).distancia;
                    var costo_sumar_k = celdas.Get(solucion.Nodos[nodosAfectados[k + 1]], solucion.Nodos[nodosAfectados[k]]).distancia;
                    nuevo_costo = nuevo_costo - costo_restar_k + costo_sumar_k;
                }

            if (nuevo_costo < solucion.Costo)
            {
                var nuevos_valores = new List<int>();
                for (var k = nodosAfectados.Count - 1; k >= 0; k--)
                    nuevos_valores.Add(solucion.Nodos[nodosAfectados[k]]);
                for (var k = 0; k < nodosAfectados.Count; k++)
                    solucion.Nodos[nodosAfectados[k]] = nuevos_valores[k];

                solucion.Costo = nuevo_costo;

                _cantidad_mejoras_two_opt++;
                return true;
            }
            return false;
        }

        public void relocate(Camino solucion)
        {
            //if (explorar_completo)
            if (true)
            {
                for (var i = 0; i < solucion.Nodos.Count; i++)
                    for (var j = 0; j < solucion.Nodos.Count; j++)
                    {
                        if ((i == j) || ((i == 0 && j == solucion.Nodos.Count - 1) || (i == solucion.Nodos.Count - 1 && j == 0)))
                            continue;

                        relocate(solucion, i, j);
                    }
            }
            else
            {
                var rnd = new Random();
                var cant_iteraciones = 0;
                while (cant_iteraciones < max_iteraciones_sin_mejora_busqueda_local)
                {
                    var i = rnd.Next(0, solucion.Nodos.Count);
                    var j = rnd.Next(0, solucion.Nodos.Count);

                    if ((i == j) || ((i == 0 && j == solucion.Nodos.Count - 1) || (i == solucion.Nodos.Count - 1 && j == 0)))
                        continue;

                    if (relocate(solucion, i, j))
                        cant_iteraciones = 0;
                    else
                        cant_iteraciones++;
                }
            }
        }

        bool relocate(Camino solucion, int i, int j)
        {
            _cantidad_iteraciones_relocate++;

            // Para no irnos del camino
            var celda_anterior_i = i - 1 == -1 ? solucion.Nodos.Count - 1 : i - 1;
            var celda_anterior_j = j - 1 == -1 ? solucion.Nodos.Count - 1 : j - 1;

            var celda_siguiente_i = i + 1 == solucion.Nodos.Count ? 0 : i + 1;
            var celda_siguiente_j = j + 1 == solucion.Nodos.Count ? 0 : j + 1;

            if (i < j)
                celda_anterior_j++;
            else
                celda_siguiente_j--;

            // Calculamos el nuevo costo, a ver si es mejor cambiar o no de posición 
            var _a = celdas.Get(solucion.Nodos[celda_anterior_i], solucion.Nodos[i]).distancia;
            var _b = celdas.Get(solucion.Nodos[i], solucion.Nodos[celda_siguiente_i]).distancia;
            var _C = celdas.Get(solucion.Nodos[celda_anterior_i], solucion.Nodos[celda_siguiente_i]).distancia;

            var _A = celdas.Get(solucion.Nodos[celda_anterior_j], solucion.Nodos[i]).distancia;
            var _B = celdas.Get(solucion.Nodos[i], solucion.Nodos[celda_siguiente_j]).distancia;
            var _c = celdas.Get(solucion.Nodos[celda_anterior_j], solucion.Nodos[celda_siguiente_j]).distancia;

            var nuevo_costo = solucion.Costo - _a - _b + _C + _A + _B - _c;
            if (nuevo_costo < solucion.Costo)
            {
                var valor = solucion.Nodos[i];
                solucion.Nodos.RemoveAt(i);
                solucion.Nodos.Insert(j, valor);

                solucion.Costo = nuevo_costo;
                _cantidad_mejoras_relocate++;
                return true;
            }
            return false;
        }

        public void exchange(Camino solucion)
        {
            if (explorar_completo)
            //if (true)
            {
                for (var i = 0; i < solucion.Nodos.Count; i++)
                    for (var j = 0; j < solucion.Nodos.Count; j++)
                    {
                        if ((i == j) || ((i == 0 && j == solucion.Nodos.Count - 1) || (i == solucion.Nodos.Count - 1 && j == 0)))
                            continue;

                        exchange(solucion, i, j);
                    }
            }
            else
            {
                var rnd = new Random();
                var cant_iteraciones = 0;
                while (cant_iteraciones < max_iteraciones_sin_mejora_busqueda_local)
                {
                    var i = rnd.Next(0, solucion.Nodos.Count);
                    var j = rnd.Next(0, solucion.Nodos.Count);

                    if (exchange(solucion, i, j))
                        cant_iteraciones = 0;
                    else
                        cant_iteraciones++;
                }
            }
        }

        bool exchange(Camino solucion, int i, int j)
        {
            _cantidad_iteraciones_exchange++;

            // Para no irnos del camino
            var celda_anterior_i = i - 1 == -1 ? solucion.Nodos.Count - 1 : i - 1;
            var celda_anterior_j = j - 1 == -1 ? solucion.Nodos.Count - 1 : j - 1;

            var celda_siguiente_i = i + 1 == solucion.Nodos.Count ? 0 : i + 1;
            var celda_siguiente_j = j + 1 == solucion.Nodos.Count ? 0 : j + 1;

            // Calculamos el nuevo costo, a ver si es mejor cambiar o no de posición 
            // Costos viejos
            var _distancia_i_izquierda = celdas.Get(solucion.Nodos[celda_anterior_i], solucion.Nodos[i]).distancia;
            var _distancia_i_derecha = celdas.Get(solucion.Nodos[i], solucion.Nodos[celda_siguiente_i]).distancia;

            var _distancia_j_izquierda = celdas.Get(solucion.Nodos[celda_anterior_j], solucion.Nodos[j]).distancia;
            var _distancia_j_derecha = celdas.Get(solucion.Nodos[j], solucion.Nodos[celda_siguiente_j]).distancia;

            decimal nuevo_costo = 0;
            // Costos nuevos
            decimal _distancia_nueva_i_izquierda, _distancia_nueva_i_derecha, _distancia_nueva_j_izquierda, _distancia_nueva_j_derecha;

            if (i == celda_siguiente_j)
            {
                _distancia_nueva_i_izquierda = celdas.Get(solucion.Nodos[celda_anterior_j], solucion.Nodos[i]).distancia;
                _distancia_nueva_i_derecha = celdas.Get(solucion.Nodos[i], solucion.Nodos[j]).distancia;

                _distancia_nueva_j_derecha = celdas.Get(solucion.Nodos[j], solucion.Nodos[celda_siguiente_i]).distancia;
                _distancia_nueva_j_izquierda = celdas.Get(solucion.Nodos[j], solucion.Nodos[i]).distancia;

                //_distancia_nueva_i_derecha = _distancia_nueva_j_izquierda 
                //    = celdas.Get(solucion.camino[i], solucion.camino[j]).distancia;
            }
            else if (j == celda_siguiente_i)
            {
                _distancia_nueva_i_derecha = celdas.Get(solucion.Nodos[i], solucion.Nodos[celda_siguiente_j]).distancia;
                _distancia_nueva_i_izquierda = celdas.Get(solucion.Nodos[j], solucion.Nodos[i]).distancia;

                _distancia_nueva_j_izquierda = celdas.Get(solucion.Nodos[celda_anterior_i], solucion.Nodos[j]).distancia;
                _distancia_nueva_j_derecha = celdas.Get(solucion.Nodos[i], solucion.Nodos[j]).distancia;

                //_distancia_nueva_i_izquierda = _distancia_nueva_j_derecha = 
                //    celdas.Get(solucion.camino[j], solucion.camino[i]).distancia;
            }
            else
            {
                _distancia_nueva_i_izquierda = celdas.Get(solucion.Nodos[celda_anterior_j], solucion.Nodos[i]).distancia;
                _distancia_nueva_i_derecha = celdas.Get(solucion.Nodos[i], solucion.Nodos[celda_siguiente_j]).distancia;

                _distancia_nueva_j_izquierda = celdas.Get(solucion.Nodos[celda_anterior_i], solucion.Nodos[j]).distancia;
                _distancia_nueva_j_derecha = celdas.Get(solucion.Nodos[j], solucion.Nodos[celda_siguiente_i]).distancia;
            }

            nuevo_costo = solucion.Costo - _distancia_i_izquierda - _distancia_i_derecha - _distancia_j_izquierda - _distancia_j_derecha +
            _distancia_nueva_i_izquierda + _distancia_nueva_i_derecha + _distancia_nueva_j_izquierda + _distancia_nueva_j_derecha;

            if (nuevo_costo < solucion.Costo)
            {
                var valor = solucion.Nodos[i];
                solucion.Nodos[i] = solucion.Nodos[j];
                solucion.Nodos[j] = valor;

                solucion.Costo = nuevo_costo;
                _cantidad_mejoras_exchange++;
                return true;
            }
            return false;
        }

        public void or_opt(Camino solucion)
        {
            var max_w = solucion.Nodos.Count - 3;

            if (explorar_completo)
            {
                for (var i = 0; i < solucion.Nodos.Count; i++)
                    for (var w = 1; w <= max_w; w++)
                    {
                        var obtener_k = new Func<List<int>, IEnumerable<int>>((x) => x);
                        if (or_opt(solucion, i, w, obtener_k))
                            break;
                    }
            }
            else
            {
                var rnd = new Random();
                var cant_iteraciones = 0;
                while (cant_iteraciones < max_iteraciones_sin_mejora_busqueda_local)
                {
                    var w = rnd.Next(1, max_w + 1);
                    var i = rnd.Next(0, solucion.Nodos.Count);
                    var obtener_k = new Func<List<int>, IEnumerable<int>>((x) => new List<int>() { x[rnd.Next(0, x.Count())] });
                    if (or_opt(solucion, i, w, obtener_k))
                        cant_iteraciones = 0;
                    else
                        cant_iteraciones++;
                }
            }
        }

        bool or_opt(Camino solucion, int i, int w, Func<List<int>, IEnumerable<int>> obtener_k)
        {
            var j = i + w;
            var nodosAfectados = GetArray(i, j);

            for (var m = 0; m < nodosAfectados.Count; m++)
                nodosAfectados[m] -= nodosAfectados[m] >= solucion.Nodos.Count ? solucion.Nodos.Count : 0;

            if (j >= solucion.Nodos.Count)
                j -= solucion.Nodos.Count;

            var nodosRestantes = GetArray(0, solucion.Nodos.Count - 1).Except(nodosAfectados).Except(new int[] { i == 0 ? solucion.Nodos.Count - 1 : i - 1 }).ToList();

            var hubo_mejora = false;
            foreach(var k in obtener_k(nodosRestantes))
            {
                _cantidad_iteraciones_or_opt++;

                // Costos
                var nodo_anterior_i = i - 1 < 0 ? solucion.Nodos.Count - 1 : i - 1;
                var nodo_siguiente_j = j + 1 >= solucion.Nodos.Count ? 0 : j + 1;
                var nodo_siguiente_k = k + 1 >= solucion.Nodos.Count ? 0 : k + 1;

                // Costos de remoción, ambos extremos del camino a recortar se juntan
                decimal costo_restar_i, costo_restar_j, costo_desarmar_relacion_k, costo_agregar_i, costo_agregar_j, costo_armar_relacion_nueva;
                decimal nuevo_costo = 0;

                costo_restar_i = celdas.Get(solucion.Nodos[nodo_anterior_i], solucion.Nodos[i]).distancia;
                costo_restar_j = celdas.Get(solucion.Nodos[j], solucion.Nodos[nodo_siguiente_j]).distancia;
                costo_desarmar_relacion_k = celdas.Get(solucion.Nodos[k], solucion.Nodos[nodo_siguiente_k]).distancia;

                costo_agregar_i = celdas.Get(solucion.Nodos[k], solucion.Nodos[i]).distancia;
                costo_agregar_j = celdas.Get(solucion.Nodos[j], solucion.Nodos[nodo_siguiente_k]).distancia;
                costo_armar_relacion_nueva = celdas.Get(solucion.Nodos[nodo_anterior_i], solucion.Nodos[nodo_siguiente_j]).distancia;

                nuevo_costo = solucion.Costo - costo_restar_i - costo_restar_j - costo_desarmar_relacion_k + costo_agregar_i + costo_agregar_j + costo_armar_relacion_nueva;

                if (nuevo_costo < solucion.Costo)
                {
                    // Armar el camino
                    var nuevo_camino = new List<int>();

                    #region Armado del camino
                    // Anteriores a K
                    if (k < i)
                    {
                        if (k < j)
                        {
                            // Iniciales
                            for (var m = 0; m <= k; m++)
                                nuevo_camino.Add(solucion.Nodos[m]);

                            // Ruta a mover
                            foreach (var m in nodosAfectados)
                                nuevo_camino.Add(solucion.Nodos[m]);

                            // Siguientes hasta la ruta
                            for (var m = nodo_siguiente_k; m <= nodo_anterior_i; m++)
                                nuevo_camino.Add(solucion.Nodos[m]);

                            // Desde la ruta hasta el final del camino
                            for (var m = nuevo_camino.Count; m < solucion.Nodos.Count; m++) // Nodo siguiente j
                                nuevo_camino.Add(solucion.Nodos[m]);
                        }
                        else
                        {
                            // Iniciales
                            for (var m = nodo_siguiente_j; m <= k; m++)
                                nuevo_camino.Add(solucion.Nodos[m]);

                            // Ruta a mover
                            foreach (var m in nodosAfectados)
                                nuevo_camino.Add(solucion.Nodos[m]);

                            // Siguientes hasta la ruta
                            for (var m = nodo_siguiente_k; m <= nodo_anterior_i; m++)
                                nuevo_camino.Add(solucion.Nodos[m]);
                        }
                    }
                    else
                    {
                        // Iniciales
                        if (i != 0)
                            for (var m = 0; m <= (nodo_anterior_i == solucion.Nodos.Count - 1 ? 0 : nodo_anterior_i); m++)
                                nuevo_camino.Add(solucion.Nodos[m]);

                        // Siguientes hasta la ruta
                        for (var m = nodo_siguiente_j; m <= k; m++)
                            nuevo_camino.Add(solucion.Nodos[m]);

                        // Ruta a mover
                        foreach (var m in nodosAfectados)
                            nuevo_camino.Add(solucion.Nodos[m]);

                        // Desde la ruta hasta el final del camino
                        for (var m = nuevo_camino.Count; m < solucion.Nodos.Count; m++)
                            nuevo_camino.Add(solucion.Nodos[m]);
                    }
                    #endregion

                    solucion.Nodos = nuevo_camino;
                    solucion.Costo = nuevo_costo;

                    hubo_mejora = true;
                    _cantidad_mejoras_or_opt++;
                    break;
                }
            }
            return hubo_mejora;
        }

        public void four_opt(Camino solucion)
        {
            var max_w_local = (int)Math.Floor((solucion.Nodos.Count - 2) / (double)2) - 1;

            if (explorar_completo)
            {
                for (var i = 0; i < solucion.Nodos.Count; i++)
                    for (var w = 1; w <= max_w_local; w++)
                    {
                        var obtener_k = new Func<List<int>, IEnumerable<int>>((x) => x);
                        if (four_opt(solucion, i, w, obtener_k))
                            break;
                    }
            }
            else
            {
                var rnd = new Random();
                var cant_iteraciones = 0;
                while (cant_iteraciones < max_iteraciones_sin_mejora_busqueda_local)
                {
                    var w = rnd.Next(1, max_w_local + 1);
                    var i = rnd.Next(0, solucion.Nodos.Count);
                    var obtener_k = new Func<List<int>, IEnumerable<int>>((x) => new List<int> { x[rnd.Next(0, x.Count())] });
                    if (four_opt(solucion, i, w, obtener_k))
                        cant_iteraciones = 0;
                    else
                        cant_iteraciones++;
                }
            }
        }

        bool four_opt(Camino solucion, int i, int w, Func<List<int>, IEnumerable<int>> obtener_k)
        {
            var j = i + w;
            var nodosAfectados = GetArray(i, j);

            for (var m = 0; m < nodosAfectados.Count; m++)
                nodosAfectados[m] -= nodosAfectados[m] >= solucion.Nodos.Count ? solucion.Nodos.Count : 0;

            if (j >= solucion.Nodos.Count)
                j -= solucion.Nodos.Count;

            var nodosRestantes = GetArray(0, solucion.Nodos.Count - 1).Except(nodosAfectados).ToList();

            var hubo_mejora = false;
            foreach (var k in obtener_k(nodosRestantes))
            {
                var l = k + w;
                if (nodosAfectados.Contains(l >= solucion.Nodos.Count ? l - solucion.Nodos.Count : l))
                    continue;

                _cantidad_iteraciones_four_opt++;

                var nodosAReemplazar = GetArray(k, l);

                for (var m = 0; m < nodosAReemplazar.Count; m++)
                    nodosAReemplazar[m] -= nodosAReemplazar[m] >= solucion.Nodos.Count ? solucion.Nodos.Count : 0;

                if (l >= solucion.Nodos.Count)
                    l -= solucion.Nodos.Count;

                // Para no irnos del camino
                var celda_anterior_i = i - 1 <= -1 ? solucion.Nodos.Count - 1 : i - 1;
                var celda_anterior_j = j - 1 <= -1 ? solucion.Nodos.Count - 1 : j - 1;
                var celda_anterior_k = k - 1 <= -1 ? solucion.Nodos.Count - 1 : k - 1;
                var celda_anterior_l = l - 1 <= -1 ? solucion.Nodos.Count - 1 : l - 1;

                var celda_siguiente_i = i + 1 >= solucion.Nodos.Count ? 0 : i + 1;
                var celda_siguiente_j = j + 1 >= solucion.Nodos.Count ? 0 : j + 1;
                var celda_siguiente_k = k + 1 >= solucion.Nodos.Count ? 0 : k + 1;
                var celda_siguiente_l = l + 1 >= solucion.Nodos.Count ? 0 : l + 1;

                // Calculamos el nuevo costo, a ver si es mejor cambiar o no de posición 
                // Costos viejos
                decimal nuevo_costo = 0;
                decimal _distancia_izquierda_a, _distancia_derecha_a, _distancia_izquierda_b, _distancia_derecha_b,
                    _distancia_izquierda_nueva_a, _distancia_derecha_nueva_a, _distancia_izquierda_nueva_b, _distancia_derecha_nueva_b;

                #region Calculo Distancia
                if (j == celda_anterior_k)
                {
                    _distancia_izquierda_a = celdas.Get(solucion.Nodos[celda_anterior_i], solucion.Nodos[i]).distancia;
                    _distancia_derecha_a = celdas.Get(solucion.Nodos[j], solucion.Nodos[celda_siguiente_j]).distancia;
                    _distancia_derecha_b = celdas.Get(solucion.Nodos[l], solucion.Nodos[celda_siguiente_l]).distancia;

                    // Costos nuevos
                    _distancia_izquierda_nueva_a = celdas.Get(solucion.Nodos[celda_anterior_i], solucion.Nodos[k]).distancia;
                    _distancia_derecha_nueva_a = celdas.Get(solucion.Nodos[l], solucion.Nodos[i]).distancia;
                    _distancia_derecha_nueva_b = celdas.Get(solucion.Nodos[j], solucion.Nodos[celda_siguiente_l]).distancia;

                    nuevo_costo = solucion.Costo - _distancia_izquierda_a - _distancia_derecha_a - _distancia_derecha_b +
                        _distancia_izquierda_nueva_a + _distancia_derecha_nueva_a + _distancia_derecha_nueva_b;
                }
                else if (l == celda_anterior_i)
                {
                    _distancia_izquierda_a = celdas.Get(solucion.Nodos[celda_anterior_k], solucion.Nodos[k]).distancia;
                    _distancia_derecha_a = celdas.Get(solucion.Nodos[l], solucion.Nodos[celda_siguiente_l]).distancia;
                    _distancia_derecha_b = celdas.Get(solucion.Nodos[j], solucion.Nodos[celda_siguiente_j]).distancia;

                    // Costos nuevos
                    _distancia_izquierda_nueva_a = celdas.Get(solucion.Nodos[celda_anterior_k], solucion.Nodos[i]).distancia;
                    _distancia_derecha_nueva_a = celdas.Get(solucion.Nodos[j], solucion.Nodos[k]).distancia;
                    _distancia_derecha_nueva_b = celdas.Get(solucion.Nodos[l], solucion.Nodos[celda_siguiente_j]).distancia;

                    nuevo_costo = solucion.Costo - _distancia_izquierda_a - _distancia_derecha_a - _distancia_derecha_b +
                        _distancia_izquierda_nueva_a + _distancia_derecha_nueva_a + _distancia_derecha_nueva_b;
                }
                else
                {
                    _distancia_izquierda_a = celdas.Get(solucion.Nodos[celda_anterior_i], solucion.Nodos[i]).distancia;
                    _distancia_derecha_a = celdas.Get(solucion.Nodos[j], solucion.Nodos[celda_siguiente_j]).distancia;

                    _distancia_izquierda_b = celdas.Get(solucion.Nodos[celda_anterior_k], solucion.Nodos[k]).distancia;
                    _distancia_derecha_b = celdas.Get(solucion.Nodos[l], solucion.Nodos[celda_siguiente_l]).distancia;

                    // Costos nuevos
                    _distancia_izquierda_nueva_a = celdas.Get(solucion.Nodos[celda_anterior_i], solucion.Nodos[k]).distancia;
                    _distancia_derecha_nueva_a = celdas.Get(solucion.Nodos[l], solucion.Nodos[celda_siguiente_j]).distancia;

                    _distancia_izquierda_nueva_b = celdas.Get(solucion.Nodos[celda_anterior_k], solucion.Nodos[i]).distancia;
                    _distancia_derecha_nueva_b = celdas.Get(solucion.Nodos[j], solucion.Nodos[celda_siguiente_l]).distancia;

                    nuevo_costo = solucion.Costo - _distancia_izquierda_a - _distancia_derecha_a - _distancia_izquierda_b - _distancia_derecha_b +
                        _distancia_izquierda_nueva_a + _distancia_derecha_nueva_a + _distancia_izquierda_nueva_b + _distancia_derecha_nueva_b;
                }
                #endregion
                var nuevo_camino = solucion.Nodos.Select(x => x).ToList();
                for (var tmp = 0; tmp < nodosAReemplazar.Count; tmp++)
                {
                    var valor = nuevo_camino[nodosAReemplazar[tmp]];
                    nuevo_camino[nodosAReemplazar[tmp]] = nuevo_camino[nodosAfectados[tmp]];
                    nuevo_camino[nodosAfectados[tmp]] = valor;
                }

                if (nuevo_costo < solucion.Costo)
                {
                    for (var tmp = 0; tmp < nodosAReemplazar.Count; tmp++)
                    {
                        var valor = solucion.Nodos[nodosAReemplazar[tmp]];
                        solucion.Nodos[nodosAReemplazar[tmp]] = solucion.Nodos[nodosAfectados[tmp]];
                        solucion.Nodos[nodosAfectados[tmp]] = valor;
                    }

                    solucion.Costo = nuevo_costo;
                    hubo_mejora = true;
                    _cantidad_mejoras_four_opt++;
                    break;
                }
            }

            return hubo_mejora;
        }

        public void family_exchange(Camino solucion)
        {
            foreach (var familia in familias)
            {
                if (familia.cantidad_visitas < familia.nodos.Count)
                {
                    var nodosVisitados = solucion.Nodos.Intersect(familia.nodos).ToList();
                    var nodosNoVisitados = familia.nodos.Except(nodosVisitados).ToList();

                    foreach (var nodoNoVisitado in nodosNoVisitados)
                    {
                        var contador = 0;
                        var cantidad_nodos_visitados = nodosVisitados.Count();

                        while (contador < cantidad_nodos_visitados)
                        {
                            _cantidad_iteraciones_family_exchange++;

                            var nodoVisitado = nodosVisitados[contador];

                            int i = -1;
                            do { i++; } while (solucion.Nodos[i] != nodoVisitado);

                            var anterior_i = i == 0 ? solucion.Nodos.Count - 1 : i - 1;
                            var siguiente_i = i == solucion.Nodos.Count - 1 ? 0 : i + 1;

                            // COSTOS
                            var removerIzquierda = celdas.Get(solucion.Nodos[anterior_i], solucion.Nodos[i]).distancia;
                            var removerDerecha = celdas.Get(solucion.Nodos[i], solucion.Nodos[siguiente_i]).distancia;

                            var agregarIzquierda = celdas.Get(solucion.Nodos[anterior_i], nodoNoVisitado).distancia;
                            var agregarDerecha = celdas.Get(nodoNoVisitado, solucion.Nodos[siguiente_i]).distancia;

                            var nuevo_costo = solucion.Costo - removerIzquierda - removerDerecha + agregarIzquierda + agregarDerecha;

                            if (nuevo_costo < solucion.Costo)
                            {
                                solucion.Nodos[i] = nodoNoVisitado;
                                solucion.Costo = nuevo_costo;

                                // Así ya no intenta reemplazarlo
                                nodosVisitados.Remove(nodoVisitado);
                                cantidad_nodos_visitados--;

                                _cantidad_mejoras_family_exchange++;

                                break;
                            }

                            contador++;
                        }
                    }
                }
            }
        }

        public void family_exchange(List<Camino> soluciones)
        {
            foreach (var familia in familias)
            {
                if (familia.cantidad_visitas < familia.nodos.Count)
                {
                    var nodosVisitados = new List<int>();
                    foreach(var unCamino in soluciones.Select(x => x.Nodos))
                        nodosVisitados.AddRange(unCamino);
                    nodosVisitados = nodosVisitados.Intersect(familia.nodos).ToList();

                    var nodosNoVisitados = familia.nodos.Except(nodosVisitados).ToList();

                    foreach (var nodoNoVisitado in nodosNoVisitados)
                    {
                        var contador = 0;
                        var cantidad_nodos_visitados = nodosVisitados.Count();

                        while (contador < cantidad_nodos_visitados)
                        {
                            _cantidad_iteraciones_family_exchange++;

                            var nodoVisitado = nodosVisitados[contador];

                            // Busco la solución 
                            var solucion = soluciones.Where(x => x.Nodos.Contains(nodoVisitado)).First();

                            // Si es FVRP hay que validar que la demanda no se sobrepase
                            if (((Camino)solucion).Capacidad + demandas[nodoVisitado] - demandas[nodoNoVisitado] < 0)
                            {
                                contador++;
                                continue;
                            }

                            int i = -1;
                            do { i++; } while (solucion.Nodos[i] != nodoVisitado);

                            var anterior_i = i == 0 ? solucion.Nodos.Count - 1 : i - 1;
                            var siguiente_i = i == solucion.Nodos.Count - 1 ? 0 : i + 1;

                            // COSTOS
                            var removerIzquierda = celdas.Get(solucion.Nodos[anterior_i], solucion.Nodos[i]).distancia;
                            var removerDerecha = celdas.Get(solucion.Nodos[i], solucion.Nodos[siguiente_i]).distancia;

                            var agregarIzquierda = celdas.Get(solucion.Nodos[anterior_i], nodoNoVisitado).distancia;
                            var agregarDerecha = celdas.Get(nodoNoVisitado, solucion.Nodos[siguiente_i]).distancia;

                            var nuevo_costo = solucion.Costo - removerIzquierda - removerDerecha + agregarIzquierda + agregarDerecha;

                            if (nuevo_costo < solucion.Costo)
                            {
                                solucion.Nodos[i] = nodoNoVisitado;
                                solucion.Costo = nuevo_costo;

                                // Si es FVRP actualizamos la demanda
                                if (demandas != null)
                                    ((Camino)solucion).Capacidad = ((Camino)solucion).Capacidad + demandas[nodoVisitado] - demandas[nodoNoVisitado];

                                // Así ya no intenta reemplazarlo
                                nodosVisitados.Remove(nodoVisitado);
                                cantidad_nodos_visitados--;

                                _cantidad_mejoras_family_exchange++;

                                break;
                            }

                            contador++;
                        }
                    }
                }
            }
        }

        public void relocate(List<Camino> soluciones)
        {
            for (var i = 0; i < soluciones.Count; i++)
                // empieza en 1 porque el 0 no se puede intercambiar
                for (var j = 1; j < soluciones[i].Nodos.Count; j++)
                {
                    var nodo = soluciones[i].Nodos[j];
                    var posicion_anterior_nodo = j == 0 ? soluciones[i].Nodos.Count - 1 : j - 1;
                    var posicion_siguiente_nodo = j == soluciones[i].Nodos.Count - 1 ? 0 : j + 1;
                    var nodo_anterior = soluciones[i].Nodos[posicion_anterior_nodo];
                    var nodo_siguiente = soluciones[i].Nodos[posicion_siguiente_nodo];

                    var costo_remover_nodo = - celdas.Get(nodo_anterior, nodo).distancia 
                                             - celdas.Get(nodo, nodo_siguiente).distancia 
                                             + celdas.Get(nodo_anterior, nodo_siguiente).distancia;

                    for (var k = 0; k < soluciones.Count; k++)
                    {
                        var reemplazado = false;
                        if ((k != i)&&(soluciones[k].Capacidad >= demandas[nodo]))
                            for (var l = 0; l < soluciones[k].Nodos.Count; l++)
                            {
                                var segundo_nodo = soluciones[k].Nodos[l];
                                var posicion_siguiente_segundo_nodo = l == soluciones[k].Nodos.Count - 1 ? 0 : l + 1;
                                var segundo_nodo_siguiente = soluciones[k].Nodos[posicion_siguiente_segundo_nodo];

                                var costo_insertar_nodo = - celdas.Get(segundo_nodo, segundo_nodo_siguiente).distancia
                                                          + celdas.Get(segundo_nodo, nodo).distancia
                                                          + celdas.Get(nodo, segundo_nodo_siguiente).distancia;

                                var nuevo_costo_total = soluciones[k].Costo + soluciones[i].Costo + costo_remover_nodo + costo_insertar_nodo;

                                if (nuevo_costo_total < soluciones[k].Costo + soluciones[i].Costo)
                                {
                                    // 1) Remover el nodo de aquel camino
                                    soluciones[i].Nodos.RemoveAt(j);
                                    // 2) Insertar el nodo en este nuevo camino
                                    soluciones[k].Nodos.Insert(l + 1, nodo);
                                    // 3) Actualizar costos
                                    soluciones[i].Costo += costo_remover_nodo;
                                    soluciones[k].Costo += costo_insertar_nodo;
                                    // 4) Actualizar demanda
                                    soluciones[k].Capacidad -= demandas[nodo];
                                    soluciones[i].Capacidad += demandas[nodo];

                                    // Iteradores
                                    // Para salir del de arriba
                                    reemplazado = true;
                                    // Para que j apunte al siguiente nodo del que reemplazamos y no saltee ninguno
                                    j--;
                                    // Para salir del actual
                                    break;
                                }
                            }

                        if (reemplazado)
                            break;
                    }
                }
        }

        public void exchange(List<Camino> soluciones)
        {
            exchange(soluciones, (x, y) => 0);
        }

        public void cross_exchange(List<Camino> soluciones)
        {
            var max_w = 4;

            // No nos podemos pasar para el otro lado del array
            exchange(soluciones, (x, y) => Math.Min(new Random().Next(1, max_w + 1), y - x - 1));
        }

        public void tail_exchange(List<Camino> soluciones)
        {
            // hasta el final del array
            //exchange(soluciones, (x, y) => y - x - 1);

            for (var i = 0; i < soluciones.Count; i++)
                // empieza en 1 porque el 0 no se puede intercambiar
                for (var j = 1; j < soluciones[i].Nodos.Count; j++)
                {
                    var longitud = soluciones[i].Nodos.Count - j - 1;

                    #region Variables
                    var nodo = soluciones[i].Nodos[j];
                    var posicion_anterior_nodo = j == 0 ? soluciones[i].Nodos.Count - 1 : j - 1;
                    var posicion_siguiente_nodo = j == soluciones[i].Nodos.Count - 1 ? 0 : j + 1;
                    var nodo_anterior = soluciones[i].Nodos[posicion_anterior_nodo];
                    var nodo_siguiente = soluciones[i].Nodos[posicion_siguiente_nodo];

                    var ultima_posicion = j + longitud;
                    var ultimo_nodo = soluciones[i].Nodos[ultima_posicion];
                    var posicion_anterior_ultimo_nodo = ultima_posicion == 0 ? soluciones[i].Nodos.Count - 1 : ultima_posicion - 1;
                    var posicion_siguiente_ultimo_nodo = ultima_posicion == soluciones[i].Nodos.Count - 1 ? 0 : ultima_posicion + 1;
                    var ultimo_nodo_anterior = soluciones[i].Nodos[posicion_anterior_ultimo_nodo];
                    var ultimo_nodo_siguiente = soluciones[i].Nodos[posicion_siguiente_ultimo_nodo];

                    var posiciones_afectadas = GetArray(j, ultima_posicion);
                    var nodos_afectados = posiciones_afectadas.Select(x => soluciones[i].Nodos[x]).ToList();
                    var total_demanda_nodos_afectados = nodos_afectados.Select(x => demandas[x]).Sum();
                    #endregion

                    var reemplazado = false;
                    for (var k = 0; k < soluciones.Count; k++)
                    {
                        if (k != i)
                        {
                            if (longitud + 1 >= soluciones[k].Nodos.Count)
                                continue;

                            #region Variables y calculo de costo
                            var l = soluciones[k].Nodos.Count - longitud - 1;

                            var segundo_nodo = soluciones[k].Nodos[l];
                            var posicion_anterior_segundo_nodo = l == 0 ? soluciones[k].Nodos.Count - 1 : l - 1;
                            var posicion_siguiente_segundo_nodo = l == soluciones[k].Nodos.Count - 1 ? 0 : l + 1;
                            var segundo_nodo_anterior = soluciones[k].Nodos[posicion_anterior_segundo_nodo];
                            var segundo_nodo_siguiente = soluciones[k].Nodos[posicion_siguiente_segundo_nodo];

                            var segunda_ultima_posicion = l + longitud;
                            var ultimo_segundo_nodo = soluciones[k].Nodos[segunda_ultima_posicion];
                            var posicion_anterior_ultimo_segundo_nodo = segunda_ultima_posicion == 0 ? soluciones[k].Nodos.Count - 1 : segunda_ultima_posicion - 1;
                            var posicion_siguiente_ultimo_segundo_nodo = segunda_ultima_posicion == soluciones[k].Nodos.Count - 1 ? 0 : segunda_ultima_posicion + 1;
                            var ultimo_segundo_nodo_anterior = soluciones[k].Nodos[posicion_anterior_ultimo_segundo_nodo];
                            var ultimo_segundo_nodo_siguiente = soluciones[k].Nodos[posicion_siguiente_ultimo_segundo_nodo];

                            var segundas_posiciones_afectadas = GetArray(l, segunda_ultima_posicion);
                            var segundos_nodos_afectados = segundas_posiciones_afectadas.Select(x => soluciones[k].Nodos[x]).ToList();
                            var segundos_total_demanda_nodos_afectados = segundos_nodos_afectados.Select(x => demandas[x]).Sum();

                            if ((soluciones[i].Capacidad + total_demanda_nodos_afectados - segundos_total_demanda_nodos_afectados < 0) ||
                                (soluciones[k].Capacidad - total_demanda_nodos_afectados + segundos_total_demanda_nodos_afectados < 0))
                                continue;

                            var costo_camino_a = -celdas.Get(nodo_anterior, nodo).distancia
                                         - celdas.Get(ultimo_nodo, ultimo_nodo_siguiente).distancia
                                         + celdas.Get(nodo_anterior, segundo_nodo).distancia
                                         + celdas.Get(ultimo_segundo_nodo, ultimo_nodo_siguiente).distancia;

                            var costo_camino_b = -celdas.Get(segundo_nodo_anterior, segundo_nodo).distancia
                                         - celdas.Get(ultimo_segundo_nodo, ultimo_segundo_nodo_siguiente).distancia
                                         + celdas.Get(segundo_nodo_anterior, nodo).distancia
                                         + celdas.Get(ultimo_nodo, ultimo_segundo_nodo_siguiente).distancia;

                            var variacion_costo = costo_camino_a + costo_camino_b;
                            #endregion

                            if (variacion_costo < 0)
                            {
                                // 1) Cambiar de nodos
                                for (var x = 0; x < posiciones_afectadas.Count(); x++)
                                    soluciones[i].Nodos[posiciones_afectadas[x]] = segundos_nodos_afectados[x];
                                for (var x = 0; x < segundas_posiciones_afectadas.Count(); x++)
                                    soluciones[k].Nodos[segundas_posiciones_afectadas[x]] = nodos_afectados[x];
                                // 2) Actualizar costos
                                decimal costo_interno_a = 0;
                                for (var x = 0; x < nodos_afectados.Count - 1; x++)
                                    costo_interno_a += celdas.Get(nodos_afectados[x], nodos_afectados[x + 1]).distancia;
                                decimal costo_interno_b = 0;
                                for (var x = 0; x < segundos_nodos_afectados.Count - 1; x++)
                                    costo_interno_b += celdas.Get(segundos_nodos_afectados[x], segundos_nodos_afectados[x + 1]).distancia;

                                soluciones[i].Costo += costo_camino_a - costo_interno_a + costo_interno_b;
                                soluciones[k].Costo += costo_camino_b + costo_interno_a - costo_interno_b;

                                var costo_i = Utilidades.CalcularCosto(celdas, soluciones[i].Nodos);
                                var costo_k = Utilidades.CalcularCosto(celdas, soluciones[k].Nodos);

                                // 4) Actualizar demanda
                                soluciones[i].Capacidad += total_demanda_nodos_afectados;
                                soluciones[i].Capacidad -= segundos_total_demanda_nodos_afectados;
                                soluciones[k].Capacidad += segundos_total_demanda_nodos_afectados;
                                soluciones[k].Capacidad -= total_demanda_nodos_afectados;

                                // Iteradores
                                // Para salir del de arriba
                                reemplazado = true;
                                // Para salir del actual
                                break;
                            }
                        }
                    }
                    if (reemplazado)
                        break;
                }
        }

        void exchange(List<Camino> soluciones, Func<int, int, int> obtener_longitud)
        {
            for (var i = 0; i < soluciones.Count; i++)
                // empieza en 1 porque el 0 no se puede intercambiar
                for (var j = 1; j < soluciones[i].Nodos.Count; j++)
                {
                    var longitud = obtener_longitud(j, soluciones[i].Nodos.Count);

                    var nodo = soluciones[i].Nodos[j];
                    var posicion_anterior_nodo = j == 0 ? soluciones[i].Nodos.Count - 1 : j - 1;
                    var posicion_siguiente_nodo = j == soluciones[i].Nodos.Count - 1 ? 0 : j + 1;
                    var nodo_anterior = soluciones[i].Nodos[posicion_anterior_nodo];
                    var nodo_siguiente = soluciones[i].Nodos[posicion_siguiente_nodo];

                    var ultima_posicion = j + longitud;
                    var ultimo_nodo = soluciones[i].Nodos[ultima_posicion];
                    var posicion_anterior_ultimo_nodo = ultima_posicion == 0 ? soluciones[i].Nodos.Count - 1 : ultima_posicion - 1;
                    var posicion_siguiente_ultimo_nodo = ultima_posicion == soluciones[i].Nodos.Count - 1 ? 0 : ultima_posicion + 1;
                    var ultimo_nodo_anterior = soluciones[i].Nodos[posicion_anterior_ultimo_nodo];
                    var ultimo_nodo_siguiente = soluciones[i].Nodos[posicion_siguiente_ultimo_nodo];

                    var posiciones_afectadas = GetArray(j, ultima_posicion);
                    var nodos_afectados = posiciones_afectadas.Select(x => soluciones[i].Nodos[x]).ToList();
                    var total_demanda_nodos_afectados = nodos_afectados.Select(x => demandas[x]).Sum();

                    for (var k = 0; k < soluciones.Count; k++)
                    {
                        var reemplazado = false;
                        if (k != i)
                            // empieza en 1 porque el 0 no se puede intercambiar
                            for (var l = 1; l < soluciones[k].Nodos.Count; l++)
                            {
                                if (l + longitud >= soluciones[k].Nodos.Count)
                                    continue;

                                var segundo_nodo = soluciones[k].Nodos[l];
                                var posicion_anterior_segundo_nodo = l == 0 ? soluciones[k].Nodos.Count - 1 : l - 1;
                                var posicion_siguiente_segundo_nodo = l == soluciones[k].Nodos.Count - 1 ? 0 : l + 1;
                                var segundo_nodo_anterior = soluciones[k].Nodos[posicion_anterior_segundo_nodo];
                                var segundo_nodo_siguiente = soluciones[k].Nodos[posicion_siguiente_segundo_nodo];

                                var segunda_ultima_posicion = l + longitud;
                                var ultimo_segundo_nodo = soluciones[k].Nodos[segunda_ultima_posicion];
                                var posicion_anterior_ultimo_segundo_nodo = segunda_ultima_posicion == 0 ? soluciones[k].Nodos.Count - 1 : segunda_ultima_posicion - 1;
                                var posicion_siguiente_ultimo_segundo_nodo = segunda_ultima_posicion == soluciones[k].Nodos.Count - 1 ? 0 : segunda_ultima_posicion + 1;
                                var ultimo_segundo_nodo_anterior = soluciones[k].Nodos[posicion_anterior_ultimo_segundo_nodo];
                                var ultimo_segundo_nodo_siguiente = soluciones[k].Nodos[posicion_siguiente_ultimo_segundo_nodo];

                                var segundas_posiciones_afectadas = GetArray(l, segunda_ultima_posicion);
                                var segundos_nodos_afectados = segundas_posiciones_afectadas.Select(x => soluciones[k].Nodos[x]).ToList();
                                var segundos_total_demanda_nodos_afectados = segundos_nodos_afectados.Select(x => demandas[x]).Sum();

                                if ((soluciones[i].Capacidad + total_demanda_nodos_afectados - segundos_total_demanda_nodos_afectados < 0) ||
                                    (soluciones[k].Capacidad - total_demanda_nodos_afectados + segundos_total_demanda_nodos_afectados < 0))
                                    continue;

                                var costo_camino_a = -celdas.Get(nodo_anterior, nodo).distancia
                                             - celdas.Get(ultimo_nodo, ultimo_nodo_siguiente).distancia
                                             + celdas.Get(nodo_anterior, segundo_nodo).distancia
                                             + celdas.Get(ultimo_segundo_nodo, ultimo_nodo_siguiente).distancia;

                                var costo_camino_b = -celdas.Get(segundo_nodo_anterior, segundo_nodo).distancia
                                             - celdas.Get(ultimo_segundo_nodo, ultimo_segundo_nodo_siguiente).distancia
                                             + celdas.Get(segundo_nodo_anterior, nodo).distancia
                                             + celdas.Get(ultimo_nodo, ultimo_segundo_nodo_siguiente).distancia;

                                var variacion_costo = costo_camino_a + costo_camino_b;

                                if (variacion_costo < 0)
                                {
                                    // 1) Cambiar de nodos
                                    for (var x = 0; x < posiciones_afectadas.Count(); x++)
                                        soluciones[i].Nodos[posiciones_afectadas[x]] = segundos_nodos_afectados[x];
                                    for (var x = 0; x < segundas_posiciones_afectadas.Count(); x++)
                                        soluciones[k].Nodos[segundas_posiciones_afectadas[x]] = nodos_afectados[x];
                                    // 2) Actualizar costos
                                    decimal costo_interno_a = 0;
                                    for (var x = 0; x < nodos_afectados.Count - 1; x++)
                                        costo_interno_a += celdas.Get(nodos_afectados[x], nodos_afectados[x + 1]).distancia;
                                    decimal costo_interno_b = 0;
                                    for (var x = 0; x < segundos_nodos_afectados.Count - 1; x++)
                                        costo_interno_b += celdas.Get(segundos_nodos_afectados[x], segundos_nodos_afectados[x + 1]).distancia;

                                    soluciones[i].Costo += costo_camino_a - costo_interno_a + costo_interno_b;
                                    soluciones[k].Costo += costo_camino_b + costo_interno_a - costo_interno_b;

                                    var costo_i = Utilidades.CalcularCosto(celdas, soluciones[i].Nodos);
                                    var costo_k = Utilidades.CalcularCosto(celdas, soluciones[k].Nodos);

                                    // 4) Actualizar demanda
                                    soluciones[i].Capacidad += total_demanda_nodos_afectados;
                                    soluciones[i].Capacidad -= segundos_total_demanda_nodos_afectados;
                                    soluciones[k].Capacidad += segundos_total_demanda_nodos_afectados;
                                    soluciones[k].Capacidad -= total_demanda_nodos_afectados;

                                    // Iteradores
                                    // Para salir del de arriba
                                    reemplazado = true;
                                    // Para salir del actual
                                    break;
                                }
                            }

                        if (reemplazado)
                            break;
                    }
                }
        }

        public static List<int> GetArray(int i, int j)
        {
            int min_value;
            int max_value;

            if (i < j)
            {
                min_value = i;
                max_value = j;
            }
            else
            {
                min_value = j;
                max_value = i;
            }

            var tmp = new List<int>();
            for (var k = min_value; k <= max_value; k++)
            {
                tmp.Add(k);
            }
            return tmp;
        }
    }
}
