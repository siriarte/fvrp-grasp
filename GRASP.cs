using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metaheuristicas
{
    class GRASP
    {
        FVRP InstanciaProblema;
        BusquedasLocales busquedasLocales;
        MatrizAdy celdas;
        enum TipoAlfa { Fijo, Aleatorio, Variable, Reactivo };
        enum TipoTerminacion { Completa, IteracionesSinMejora, Epsilon }

        // Parametros generales GRASP.
        int cantidadCorridas = 10000;
        int cantidadDeIteracionesSinMejora = 1000;
        decimal epsilon = (decimal)0.1;
        int cantidadSolucionesMenorEpsilon = 1;
        TipoAlfa tipoAlfa = TipoAlfa.Reactivo;
        
        // Parametros retorno temprano.
        bool retornoTemprano = false;
        int probRetornoTemprano = 85;

        // Parametros GRASP Fijo.
        decimal alfaFijo = (decimal)0.5;

        // Parametros GRASP Aleatorio.
        decimal paso = (decimal)0.02;
        decimal alfaInicial = (decimal)0.1;
        decimal alfaFinal = (decimal)1.0;

        // Parametros GRASP Reactivo.
        Dictionary<int, List<decimal>> solucionesPorAlfa = new Dictionary<int, List<decimal>>();
        decimal[] probabilidad = new decimal[10];
        double[] alfas = new double[] { 0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9 };
        bool promedioSolCompleto = false;
        double delta = 50; // 1 no hace nada.
        int cantidadIteracionesActualizarPromedio = 100;

        public GRASP(FVRP problema)
        {
            InstanciaProblema = problema;

            celdas = new MatrizAdy(InstanciaProblema.Distancias.Count);
            for (int i = 0; i < InstanciaProblema.Distancias.Count(); i++)
            {
                for (int j = 0; j < InstanciaProblema.Distancias.Count(); j++)
                {
                    celdas.Get(i, j).distancia = InstanciaProblema.Distancias[i][j];
                }
            }

            var totalVisitas = InstanciaProblema.Familias.Select(x => x.cantidad_visitas).Sum();
            // La búsqueda local tiene que depender de la longitud del camino a evaluar y no del total de nodos
            busquedasLocales = new BusquedasLocales(totalVisitas, celdas, InstanciaProblema.Familias, InstanciaProblema.Demanda, InstanciaProblema.simetrico, totalVisitas * 2);

            // Inicializacion GRASP Reactivo.
            for (int i = 0; i < 10; i++)
                probabilidad[i] = (decimal)1 / (decimal)10;
        }

        public Solucion ProcedimientoGRASP()
        {
            // procedure GRASP(MaxIterations,Seed)
            // 1.  Set f∗ ← ∞;
            // 2.  for k = 1,...,MaxIterations do
            // 3.      S ← GreedyRandomizedAlgorithm(Seed);
            // 4.      if S is not feasible then
            // 5.          S ← RepairSolution(S);
            // 6.      end;
            // 7.      S ← LocalSearch(S);
            // 8.      if f(S) < f∗ then
            // 9.          S∗ ← S;
            // 10.         f∗ ← f(S);
            // 11.     end;
            // 12. end;
            // 13. return S∗;
            // end.

            switch (tipoAlfa)
            {
                case TipoAlfa.Fijo:
                    {
                        return GRASPFijo();
                    }
                case TipoAlfa.Aleatorio:
                    {
                        return GRASPAleatorio();
                    }
                case TipoAlfa.Variable:
                    {
                        return GRASPVariable();
                    }
                case TipoAlfa.Reactivo:
                    {
                        return GRASPReactivo();
                    }
            }

            throw new Exception("Imposible que llegue aca, pero chilla el compilador! :-)");
        }

        public Solucion GRASPFijo()
        {
            SolucionFVRP mejorSolucion = null;
            TimeSpan mejorTiempo = new TimeSpan();
            Stopwatch sw = new Stopwatch();
            TipoTerminacion terminacion = TipoTerminacion.Completa;
            int iteracionesSinMejora = 0;
            int solucionesMenorEpsilon = 0;
            sw.Start();

            for (var i = 0; i < cantidadCorridas && iteracionesSinMejora < cantidadDeIteracionesSinMejora; i++)
            {
                SolucionFVRP solucion = GolosoAleatorio(alfaFijo);
                RealizarBusquedaLocal(solucion);

                ValidarSolucion(solucion);

                if (mejorSolucion == null || mejorSolucion.Costo > solucion.Costo)
                {
                    sw.Stop();
                    decimal porcentajeDeMejora = (decimal)100;
                    if (mejorSolucion != null)
                        porcentajeDeMejora = 100 - ((solucion.Costo * 100) / mejorSolucion.Costo);
                    mejorSolucion = solucion;
                    mejorTiempo = sw.Elapsed;
                    sw.Start();
                    iteracionesSinMejora = 0;
                    if (porcentajeDeMejora < epsilon)
                    {
                        if (solucionesMenorEpsilon > cantidadSolucionesMenorEpsilon)
                        {
                            terminacion = TipoTerminacion.Epsilon;
                            break;
                        }
                        solucionesMenorEpsilon++;
                    }
                }
                else
                    iteracionesSinMejora++;
            }

            if (iteracionesSinMejora >= cantidadDeIteracionesSinMejora)
                terminacion = TipoTerminacion.IteracionesSinMejora;

            return new Solucion()
            {
                SolucionFVRP = mejorSolucion,
                MejorAlfa = alfaFijo,
                MejorTiempo = mejorTiempo,
                TipoTerminacion = terminacion.ToString()
            };
        }

        public Solucion GRASPAleatorio()
        {
            SolucionFVRP mejorSolucion = null;
            TimeSpan mejorTiempo = new TimeSpan();
            Stopwatch sw = new Stopwatch();
            TipoTerminacion terminacion = TipoTerminacion.Completa;
            int iteracionesSinMejora = 0;
            int solucionesMenorEpsilon = 0;
            sw.Start();

            decimal mejorAlfa = 0;
            for (var i = 0; i < cantidadCorridas && iteracionesSinMejora < cantidadDeIteracionesSinMejora; i++)
            {
                SolucionFVRP solucion = null;

                SolucionFVRP solParcial = GolosoAleatorio(0);
                RealizarBusquedaLocal(solParcial);
                solucion = solParcial;
                mejorAlfa = 0;

                ValidarSolucion(solucion);

                if (mejorSolucion == null || mejorSolucion.Costo > solucion.Costo)
                {
                    sw.Stop();
                    decimal porcentajeDeMejora = (decimal)100;
                    if (mejorSolucion != null)
                        porcentajeDeMejora = 100 - ((solucion.Costo * 100) / mejorSolucion.Costo);
                    mejorSolucion = solucion;
                    mejorTiempo = sw.Elapsed;
                    sw.Start();
                    iteracionesSinMejora = 0;
                    if (porcentajeDeMejora < epsilon)
                    {
                        if (solucionesMenorEpsilon > cantidadSolucionesMenorEpsilon)
                        {
                            terminacion = TipoTerminacion.Epsilon;
                            break;
                        }
                        solucionesMenorEpsilon++;
                    }
                }
                else
                    iteracionesSinMejora++;
            }

            if (iteracionesSinMejora >= cantidadDeIteracionesSinMejora)
                terminacion = TipoTerminacion.IteracionesSinMejora;

            return new Solucion()
            {
                SolucionFVRP = mejorSolucion,
                MejorAlfa = mejorAlfa,
                MejorTiempo = mejorTiempo,
                TipoTerminacion = terminacion.ToString()
            };
        }

        public Solucion GRASPVariable()
        {
            SolucionFVRP mejorSolucion = null;
            TimeSpan mejorTiempo = new TimeSpan();
            Stopwatch sw = new Stopwatch();
            TipoTerminacion terminacion = TipoTerminacion.Completa;
            int iteracionesSinMejora = 0;
            int solucionesMenorEpsilon = 0;
            sw.Start();

            decimal mejorAlfa = 0;
            for (var i = 0; i < cantidadCorridas && iteracionesSinMejora < cantidadDeIteracionesSinMejora; i++)
            {
                SolucionFVRP solucion = null;

                decimal alfa = alfaInicial;
                while (alfa < alfaFinal)
                {
                    SolucionFVRP solParcial = GolosoAleatorio(alfa);
                    RealizarBusquedaLocal(solParcial);
                    if (solucion == null || solucion.Costo > solParcial.Costo)
                    {
                        solucion = solParcial;
                        mejorAlfa = alfa;
                    }
                    alfa += paso;
                }
                ValidarSolucion(solucion);

                if (mejorSolucion == null || mejorSolucion.Costo > solucion.Costo)
                {
                    sw.Stop();
                    decimal porcentajeDeMejora = (decimal)100;
                    if (mejorSolucion != null)
                        porcentajeDeMejora = 100 - ((solucion.Costo * 100) / mejorSolucion.Costo);
                    mejorSolucion = solucion;
                    mejorTiempo = sw.Elapsed;
                    sw.Start();
                    iteracionesSinMejora = 0;
                    if (porcentajeDeMejora < epsilon)
                    {
                        if (solucionesMenorEpsilon > cantidadSolucionesMenorEpsilon)
                        {
                            terminacion = TipoTerminacion.Epsilon;
                            break;
                        }
                        solucionesMenorEpsilon++;
                    }
                }
                else
                    iteracionesSinMejora++;
            }

            if (iteracionesSinMejora >= cantidadDeIteracionesSinMejora)
                terminacion = TipoTerminacion.IteracionesSinMejora;

            return new Solucion()
            {
                SolucionFVRP = mejorSolucion,
                MejorAlfa = mejorAlfa,
                MejorTiempo = mejorTiempo,
                TipoTerminacion = terminacion.ToString()
            };
        }

        public Solucion GRASPReactivo()
        {
            SolucionFVRP mejorSolucion = null;
            TimeSpan mejorTiempo = new TimeSpan();
            Stopwatch sw = new Stopwatch();
            TipoTerminacion terminacion = TipoTerminacion.Completa;
            int iteracionesSinMejora = 0;
            int iteracionesHastaReincio = 0;
            int solucionesMenorEpsilon = 0;
            int iteracionesActualizar = 0;
            sw.Start();

            decimal mejorAlfa = 0;
            for (var i = 0; i < cantidadCorridas && iteracionesSinMejora < cantidadDeIteracionesSinMejora; i++)
            {
                SolucionFVRP solucion = null;

                // Reactive GRASP
                decimal alfa = (decimal)0.0;
                if (!promedioSolCompleto)
                {
                    // Se hace una primera corrida con cada alfa para empezar a completar la tabla promedioSoluciones.
                    for (int j = 0; j < 10; j++)
                    {
                        alfa = (decimal)alfas[j];
                        SolucionFVRP solParcial = GolosoAleatorio(alfa);
                        RealizarBusquedaLocal(solParcial);
                        solucionesPorAlfa[j] = new List<decimal>();
                        solucionesPorAlfa[j].Add(solParcial.Costo);
                        if (solucion == null || solucion.Costo > solParcial.Costo)
                        {
                            solucion = solParcial;
                            mejorAlfa = alfa;
                        }
                    }
                }
                else
                {
                    // A partir de que se cargan para todo alfa, arrancamos el algoritmo posta.
                    if (iteracionesActualizar >= cantidadIteracionesActualizarPromedio && mejorSolucion != null)
                    {
                        decimal sum = 0;
                        decimal[] normalizado = new decimal[10];
                        for (int j = 0; j < 10; j++)
                        {
                            decimal promedioSolucionAlfa = solucionesPorAlfa[j].Sum() / solucionesPorAlfa[j].Count;
                            normalizado[j] = (decimal)Math.Pow((double)(mejorSolucion.Costo / promedioSolucionAlfa), delta);
                            sum += normalizado[j];
                        }
                        for (int j = 0; j < 10; j++)
                        {
                            probabilidad[j] = normalizado[j] / sum;
                        }
                        iteracionesActualizar = 0;
                    }
                    else
                        iteracionesActualizar++;

                    var random = ((decimal)(new Random().Next(0, 100)) / 100);
                    var acum = (decimal)0.0;
                    var actual = -1;
                    while (acum < random)
                    {
                        actual++;
                        acum += probabilidad[actual];
                    }

                    if (actual == -1)
                        actual = 0;

                    alfa = (decimal)alfas[actual];

                    SolucionFVRP solParcial = GolosoAleatorio(alfa);
                    RealizarBusquedaLocal(solParcial);
                    if (solucion == null || solucion.Costo > solParcial.Costo)
                    {
                        solucion = solParcial;
                        mejorAlfa = alfa;
                    }

                    solucionesPorAlfa[actual].Add(solParcial.Costo);
                }

                // En la primer iteracion se completa... es horrible como me esta quedando, lo se.
                if (!promedioSolCompleto)
                    promedioSolCompleto = true;

                ValidarSolucion(solucion);

                if (mejorSolucion == null || mejorSolucion.Costo > solucion.Costo)
                {
                    sw.Stop();
                    decimal porcentajeDeMejora = (decimal)100;
                    if (mejorSolucion != null)
                        porcentajeDeMejora = 100 - ((solucion.Costo * 100) / mejorSolucion.Costo);
                    mejorSolucion = solucion;
                    mejorTiempo = sw.Elapsed;
                    sw.Start();
                    iteracionesSinMejora = 0;
                    if (porcentajeDeMejora < epsilon)
                    {
                        if (solucionesMenorEpsilon > cantidadSolucionesMenorEpsilon)
                        {
                            terminacion = TipoTerminacion.Epsilon;
                            break;
                        }
                        solucionesMenorEpsilon++;
                    }
                }
                else
                    iteracionesSinMejora++;

                iteracionesHastaReincio++;
            }

            if (iteracionesSinMejora >= cantidadDeIteracionesSinMejora)
                terminacion = TipoTerminacion.IteracionesSinMejora;

            return new Solucion()
            {
                SolucionFVRP = mejorSolucion,
                MejorAlfa = mejorAlfa,
                MejorTiempo = mejorTiempo,
                TipoTerminacion = terminacion.ToString()
            };
        }

        private SolucionFVRP GolosoAleatorio(decimal alfa)
        {
            // procedure GreedyRandomizedConstruction(α,Seed)
            // 1.  S ← empty;
            // 2.  Initialize the candidate set: C ← E;
            // 3.  Evaluate the incremental cost c(e) for all e ∈ C;
            // 4.  while C != empty do
            // 5.      c_min ← min{c(e) | e ∈C};
            // 6.      c_max ← max{c(e) | e ∈ C};
            // 7.      Build the restricted candidate list: RCL ← {e ∈ C | c(e) ≤ c_min +α(c_max − c_min)};
            // 8.      Choose s at random from RCL;
            // 9.      Incorporate s into solution: S ← S∪ {s};
            // 10.     Update the candidate set C;
            // 11.     Reevaluate the incremental cost c(e) for all e ∈ C;
            // 12. end;
            // 13. return S;
            // end.
            SolucionFVRP solucion = new SolucionFVRP(InstanciaProblema.Distancias.Count);

            solucion.Caminos = new List<Camino>();
            solucion.FamiliasPendientes = new List<Familia>();

            foreach (var familia in InstanciaProblema.Familias)
                if (!familia.nodos.Contains(0))
                    solucion.FamiliasPendientes.Add(new Familia()
                    {
                        cantidad_visitada = 0,
                        cantidad_visitas = familia.cantidad_visitas,
                        id = familia.id,
                        nodos = new List<int>(familia.nodos.ToArray())
                    });

            AgregarCaminoConNodoInicial(solucion);

            while (solucion.FamiliasPendientes.Count > 0)
            {
                if (!Mover(solucion, alfa))
                {
                    CerrarUltimoCamino(solucion);
                    AgregarCaminoConNodoInicial(solucion);
                }
            }
            CerrarUltimoCamino(solucion);

            return solucion;
        }
 
        void AgregarCaminoConNodoInicial(SolucionFVRP solucion)
        {
            solucion.Caminos.Add(new Camino() { Nodos = new List<int>(), Costo = 0, Capacidad = InstanciaProblema.capacidad });
            solucion.Caminos[solucion.Caminos.Count - 1].Nodos.Add(0);
        }

        void CerrarUltimoCamino(SolucionFVRP solucion)
        {
            // Sumar al costo la vuelta al último nodo
            var ultimo_camino = solucion.Caminos.Count - 1;

            solucion.Caminos[ultimo_camino].Costo +=
                celdas.Get(solucion.Caminos[ultimo_camino].Nodos[0],
                           solucion.Caminos[ultimo_camino].Nodos[solucion.Caminos[ultimo_camino].Nodos.Count - 1]).distancia;
        }

        bool Mover(SolucionFVRP solucion, decimal alfa)
        {
            var nro_camino_actual = solucion.Caminos.Count - 1;
            var camino_actual = solucion.Caminos[nro_camino_actual].Nodos;
            var nodo_actual = camino_actual[camino_actual.Count - 1];

            // No hace falta conocer los adyaecentes porque es un grafo completo, entonces hay que mirar todos.
            List<int> candidatos = new List<int>();
            for (int ady = 0; ady < InstanciaProblema.Distancias.Count; ady++)
            {
                if (ady == nodo_actual)
                    continue;

                // Se corrobora que podamos satisfacer la demanda del nodo adyacente.
                if (InstanciaProblema.Demanda[ady] > solucion.Caminos[nro_camino_actual].Capacidad)
                    continue;

                // Si el nodo adyacente fue utilizado no se puede volver a usar.
                if (solucion.NodosUtilizadosGrasp[ady])
                    continue;

                // La familia del nodo adyacente debe pertenecer a las familias pendientes.
                var familiaPendiente = solucion.FamiliasPendientes.Any(x => x.id == InstanciaProblema.FamiliaPorNodo[ady]);
                if (!familiaPendiente)
                    continue;

                candidatos.Add(ady);
            }

            if (candidatos.Count == 0)
                return false;

            var max = decimal.MinValue;
            var min = decimal.MaxValue;
            foreach (var ady in candidatos)
            {
                if (nodo_actual != ady)
                {
                    if (InstanciaProblema.Distancias[nodo_actual][ady] > max)
                        max = InstanciaProblema.Distancias[nodo_actual][ady];
                    if (InstanciaProblema.Distancias[nodo_actual][ady] < min)
                        min = InstanciaProblema.Distancias[nodo_actual][ady];
                }
            }

            // Retorno temprano. Si entre todos los posibles candidatos, el mas cercano esta mas lejos que el
            // deposito, se vuelve con una probabilidad determinada.
            if (retornoTemprano && (solucion.Caminos[nro_camino_actual].Nodos.Count > 1) &&
                (InstanciaProblema.Distancias[nodo_actual][0] <= min) &&
                (new Random().Next(0, 100) < probRetornoTemprano))
            {
                return false;
            }

            if (tipoAlfa == TipoAlfa.Aleatorio)
            {
                // En la version aleatoria, cambia el alfa por cada movimiento de manera random.
                alfa = ((decimal)(new Random().Next(0, 100)) / 100);
            }

            // Construccion de la lista de candidatos.
            List<int> RCL = new List<int>();
            foreach (var ady in candidatos)
            {
                if (InstanciaProblema.Distancias[nodo_actual][ady] <= (min + alfa * (max - min)))
                    RCL.Add(ady);
            }
            if (RCL.Count == 0)
                throw new Exception("Error, la RCL no puede ser nunca vacia!");

            // Seleccion random del candidato.
            Random rnd = new Random();
            int r = rnd.Next(RCL.Count);
            int vecino = RCL[r];
            int familia_id = InstanciaProblema.FamiliaPorNodo[vecino];

            // Se actualiza el camino y se mete el vecino en la solucion
            solucion.Caminos[nro_camino_actual].Costo += InstanciaProblema.Distancias[nodo_actual][vecino];
            solucion.Caminos[nro_camino_actual].Capacidad -= InstanciaProblema.Demanda[vecino];
            solucion.Caminos[nro_camino_actual].Nodos.Add(vecino);
            solucion.NodosUtilizadosGrasp[vecino] = true;

            // Se actualizan las familias y si ya se visitaron todos los nodos, se remueven.
            for (var i = 0; i < solucion.FamiliasPendientes.Count; i++)
                if (solucion.FamiliasPendientes[i].id == familia_id)
                {
                    solucion.FamiliasPendientes[i].nodos.Remove(vecino);
                    solucion.FamiliasPendientes[i].cantidad_visitada++;

                    if (solucion.FamiliasPendientes[i].cantidad_visitada == solucion.FamiliasPendientes[i].cantidad_visitas)
                        solucion.FamiliasPendientes.RemoveAt(i);

                    break;
                }
            return true;
        }

        void RealizarBusquedaLocal(SolucionFVRP solucionCompleta, int cant_iteraciones_busquedas_locales = 2)
        {
            for (var i = 0; i < cant_iteraciones_busquedas_locales; i++)
            {
                busquedasLocales.relocate(solucionCompleta.Caminos);
                busquedasLocales.exchange(solucionCompleta.Caminos);
                busquedasLocales.cross_exchange(solucionCompleta.Caminos);
                busquedasLocales.tail_exchange(solucionCompleta.Caminos);

                foreach (var solucion in solucionCompleta.Caminos)
                {
                    // Si son 6 nodos o menos conviene permutar todo a andar haciendo búsquedas locales heurísticas
                    if (solucion.Nodos.Count <= 6)
                        Utilidades.PermutarTodo(celdas, solucion);
                    else
                    {
                        busquedasLocales.relocate(solucion);
                        busquedasLocales.exchange(solucion);
                        busquedasLocales.or_opt(solucion);
                        busquedasLocales.four_opt(solucion);
                        busquedasLocales.two_opt(solucion);
                    }

                    // Hay que reacomodar porque estas búsquedas locales te mueven el depósito para cualquier lado
                    while (solucion.Nodos[0] != 0)
                    {
                        var valor = solucion.Nodos[0];
                        for (var cam = 0; cam < solucion.Nodos.Count; cam++)
                        {
                            var siguiente_posicion = cam + 1 == solucion.Nodos.Count ? 0 : cam + 1;
                            var _valor = solucion.Nodos[siguiente_posicion];
                            solucion.Nodos[siguiente_posicion] = valor;
                            valor = _valor;
                        }
                    }
                }

                busquedasLocales.family_exchange(solucionCompleta.Caminos);
            }

            // Por búsquedas locales puede ocurrir que quede algún vehículo solo con el depósito.
            // No afecta a la distancia, solo agrega un vehículo al pedo. Se lo sacamos y listo.
            var j = 0;
            while (j < solucionCompleta.Caminos.Count)
            {
                if (solucionCompleta.Caminos[j].Nodos.Count == 1)
                    solucionCompleta.Caminos.RemoveAt(j);
                else
                    j++;
            }
        }

        public void ValidarSolucion(SolucionFVRP solucion)
        {
            // Validar que solo aparezca una vez el 0 en cada camino y este se encuentre en el 1er lugar
            foreach (var camino in solucion.Caminos)
            {
                if (camino.Nodos.Count != camino.Nodos.Distinct().Count())
                    throw new Exception("Nodos repetidos");
                if (camino.Nodos[0] != 0)
                    throw new Exception("El 0 debe ir en el 1er lugar");
            }

            var familias = new List<Familia>();
            foreach (var familia in InstanciaProblema.Familias)
                familias.Add(new Familia()
                {
                    id = familia.id,
                    demanda = familia.demanda,
                    nodos = familia.nodos,
                    cantidad_visitada = 0,
                    cantidad_visitas = familia.cantidad_visitas
                });

            foreach (var camino in solucion.Caminos)
                foreach (var nodo in camino.Nodos)
                    foreach (var familia in familias)
                        if (familia.nodos.Contains(nodo))
                        {
                            familia.cantidad_visitada++;
                            break;
                        }

            foreach (var familia in familias)
                if (familia.cantidad_visitas != familia.cantidad_visitada && !familia.nodos.Contains(0))
                    throw new Exception("Familia no visitada");

            // Validar demandas
            foreach (var camino in solucion.Caminos)
            {
                var demanda_camino = camino.Nodos.Select(x => InstanciaProblema.Demanda[x]).Sum();
                if (demanda_camino > InstanciaProblema.capacidad)
                    throw new Exception("La demanda del camino supera la capacidad máxima");

                if (InstanciaProblema.capacidad - demanda_camino != camino.Capacidad)
                    throw new Exception("La capacidad del camino es distinta a la que debería ser");

                var costo_camino = Utilidades.CalcularCosto(celdas, camino.Nodos);
                if (Math.Round(costo_camino) != Math.Round(camino.Costo))
                    throw new Exception(string.Format("Costos distintos, calculado: {0}, valor manual: {1}",
                        Math.Round(costo_camino), Math.Round(camino.Costo)));
            }

            var costo = Utilidades.CalcularCosto(celdas, solucion.Caminos);
            if (Math.Round(costo) != Math.Round(solucion.Costo))
                throw new Exception(string.Format("Costos distintos, calculado: {0}, valor manual: {1}",
                    Math.Round(costo), Math.Round(solucion.Costo)));
        }
    }
}
