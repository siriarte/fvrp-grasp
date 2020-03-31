using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Metaheuristicas
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"..\..\..\Tests\";
            string pathResultados = @"..\..\..\Tests\Solutions";

            Stopwatch swTotal = new Stopwatch();
            swTotal.Start();
            Console.WriteLine("Iniciando resolución...");

            var files = Directory.GetFiles(path).Select(x => new FileInfo(x)).OrderBy(x => x.Length).Select(x => Path.Combine(path, x.Name)).ToList();
            foreach (var archivo in files)
            {
                var nombreInstancia = Path.GetFileNameWithoutExtension(archivo);
                Console.WriteLine("Resolviendo la instancia: " + nombreInstancia);
                var instancia = CargadorInstancias.CargarInstancia(File.ReadAllText(archivo));

                Stopwatch sw = new Stopwatch();
                sw.Start();

                Solucion solucion = new GRASP(instancia).ProcedimientoGRASP();

                sw.Stop();

                Console.WriteLine("Mejor costo fue: " + solucion.SolucionFVRP.Costo.ToString("N4"));
                Console.WriteLine("Alfa del mejor costo: " + solucion.MejorAlfa.ToString("N4"));
                Console.WriteLine("Tiempo de ejecucion: " + sw.Elapsed);
                Console.WriteLine("Tiempo mejor solucion: " + solucion.MejorTiempo);
                Console.WriteLine("Tipo de terminacion: " + solucion.TipoTerminacion);
                Console.WriteLine("");

                var horaActual = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                string resultados = Path.Combine(pathResultados, "resultados.txt");
                var linea = nombreInstancia + "\t" + horaActual + "\t" + solucion.SolucionFVRP.Costo.ToString("N2") +
                            "\t" + solucion.MejorAlfa + "\t" + sw.Elapsed + "\t" + solucion.MejorTiempo + "\t" + solucion.TipoTerminacion;

                File.AppendAllLines(resultados, new string[] { linea });
            }

            swTotal.Stop();
            Console.WriteLine("Procedimiento finalizado, tiempo total: " + swTotal.Elapsed);

            Console.ReadKey();
        }
    }
}
