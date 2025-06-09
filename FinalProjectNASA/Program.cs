using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinalProjectNASA
{
    class Program
    {
        private const string api = "https://api.nasa.gov/planetary/apod";
        private const string key = "dlI71b7WG1efSkePYdr6zw61QSrjIMqqDQDn7GTQ";
        private static readonly HttpClient _httpClient = new();
        private static List<string[]> dataset = new();
        private static string[] headers;

        static async Task Main()
        {
            bool salir = false;

            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("=== Menu ===");
                Console.WriteLine("1. NASA API");
                Console.WriteLine("2. DATA SET");
                Console.WriteLine("0. EXIT");
                Console.Write("Select an option: ");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        Console.Clear(); // Borra antes de mostrar la API
                        await MostrarNasaApiAsync();
                        break;
                    case "2":
                        MostrarDataSet();
                        break;
                    case "0":
                        Console.Clear();
                        Console.WriteLine("Exiting the program...");
                        salir = true;
                        break;
                    default:
                        Console.Clear();
                        Console.WriteLine("Invalid option, try again.");
                        EsperarTecla();
                        break;
                }
            }
        }

        static async Task MostrarNasaApiAsync()
        {
            string[] months = {
                "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December"
            };

            for (int i = 0; i < months.Length; i++)
                Console.WriteLine($"{i + 1}. {months[i]}");

            Console.Write("Select a month (1-12): ");
            if (!int.TryParse(Console.ReadLine(), out int selectedMonth) || selectedMonth < 1 || selectedMonth > 12)
            {
                Console.WriteLine("Invalid month.");
                EsperarTecla();
                return;
            }

            int year = DateTime.Now.Year;
            DateTime start = new DateTime(year, selectedMonth, 1);
            DateTime end = start.AddMonths(1).AddDays(-1);

            string url = $"{api}?api_key={key}&start_date={start:yyyy-MM-dd}&end_date={end:yyyy-MM-dd}";

            try
            {
                string json = await _httpClient.GetStringAsync(url);
                var result = JsonSerializer.Deserialize<List<ApodResponse>>(json);

                if (result == null || result.Count == 0)
                {
                    Console.WriteLine("No data found.");
                }
                else
                {
                    foreach (var item in result)
                    {
                        Console.WriteLine($"Date: {item.date}");
                        Console.WriteLine($"Title: {item.title}");
                        Console.WriteLine($"URL: {item.url}");
                        Console.WriteLine(new string('-', 40));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching data: " + ex.Message);
            }

            EsperarTecla();
        }

        static void MostrarDataSet()
        {
            string ruta = @"C:\Users\odeth\OneDrive\Escritorio\Riojas\stars\skydatos.csv";


            if (!File.Exists(ruta))
            {
                Console.WriteLine("CSV file not found.");
                EsperarTecla();
                return;
            }

            dataset = File.ReadAllLines(ruta)
                          .Select(line => line.Split(','))
                          .ToList();

            headers = dataset[0];
            dataset.RemoveAt(0); // Remove headers from data

            bool volver = false;
            while (!volver)
            {
                Console.Clear();
                Console.WriteLine("=== DATA SET MENU ===");
                Console.WriteLine("1. Mostrar datos");
                Console.WriteLine("2. Buscador");
                Console.WriteLine("3. Ordenar por Redshift");
                Console.WriteLine("4. Filtrar por tipo de objeto");
                Console.WriteLine("5. Exportar resultados");
                Console.WriteLine("6. Graficar");
                Console.WriteLine("0. Volver al menú principal");
                Console.Write("Seleccione una opción: ");
                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        MostrarDatos(dataset, headers);
                        break;
                    case "2":
                        Buscador();
                        break;
                    case "3":
                        OrdenarPorRedshift(dataset.ToArray(), headers);
                        break;
                    case "4":
                        FiltrarTipoObjeto();
                        break;
                    case "5":
                        ExportarResultados();
                        break;
                    case "6":
                        MenuGraficas();
                        break;
                    case "0":
                        volver = true;
                        break;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
        }

        static void MostrarDatos(List<string[]> dataset, string[] headers)
        {
            Console.Clear();
            Console.WriteLine("=== Primeros 10 registros ===\n");

            // Mostrar encabezados
            Console.WriteLine(string.Join(" | ", headers));

            // Mostrar las primeras 10 filas del dataset
            for (int i = 0; i < Math.Min(10, dataset.Count); i++)
            {
                Console.WriteLine(string.Join(" | ", dataset[i]));
            }

            EsperarTecla();
        }

        static void Buscador()
        {
            Console.Clear();
            Console.Write("Ingrese texto para buscar: ");
            string texto = Console.ReadLine().ToLower();

            var resultados = dataset.Where(fila =>
                fila.Any(columna => columna.ToLower().Contains(texto))
            );

            for (int i = 0; i < Math.Min(10, dataset.Count); i++)
            {
                Console.WriteLine(string.Join(" | ", dataset[i]));
            }


            EsperarTecla();
        }

        static void OrdenarPorRedshift(string[][] dataset, string[] headers)
        {
            Console.Clear();
            Console.WriteLine("Ordenar por Redshift:");
            Console.WriteLine("1. Near objects");
            Console.WriteLine("2. Far objects");
            Console.Write("Seleccione una opción: ");
            string opcion = Console.ReadLine();

            int redshiftIndex = Array.IndexOf(headers, "redshift");

            if (redshiftIndex == -1)
            {
                Console.WriteLine("La columna 'redshift' no fue encontrada.");
                EsperarTecla();
                return;
            }

            IEnumerable<string[]> datosOrdenados = dataset
                .Where(fila => double.TryParse(fila[redshiftIndex], out _))
                .OrderBy(fila => double.Parse(fila[redshiftIndex], System.Globalization.CultureInfo.InvariantCulture));

            if (opcion == "2")
                datosOrdenados = datosOrdenados.Reverse();

            foreach (var fila in datosOrdenados)
                Console.WriteLine(string.Join(" | ", fila));

            EsperarTecla();
        }


        static void FiltrarTipoObjeto()
        {
            Console.Clear();
            Console.WriteLine("1. STAR");
            Console.WriteLine("2. GALAXY");
            Console.WriteLine("3. QSO");
            Console.Write("Seleccione tipo: ");
            string tipo = Console.ReadLine()?.Trim().ToUpper();

            string tipoFiltro = tipo switch
            {
                "1" => "STAR",
                "2" => "GALAXY",
                "3" => "QSO",
                _ => null
            };

            if (tipoFiltro == null)
            {
                Console.WriteLine("Opción no válida.");
                EsperarTecla();
                return;
            }

            int classIndex = Array.FindIndex(headers, h => h.ToLower().Contains("class"));

            var filtrado = dataset.Where(fila => fila[classIndex].ToUpper() == tipoFiltro);
            foreach (var fila in filtrado)
                Console.WriteLine(string.Join(" | ", fila));

            EsperarTecla();
        }

        static void ExportarResultados()
        {
            string path = "exported_data.csv";
            File.WriteAllLines(path, dataset.Select(row => string.Join(",", row)));
            Console.WriteLine($"Datos exportados a {path}");
            EsperarTecla();
        }
        //a

        static void MenuGraficas()
        {
            Console.Clear();
            Console.WriteLine("1. Gráfica de barras (STAR, GALAXY, QSO)");
            Console.Write("Seleccione una opción: ");
            string op = Console.ReadLine();

            if (op == "1") GraficarBarraAscii();
            else Console.WriteLine("Opción inválida");

            EsperarTecla();
        }

        static void GraficarBarraAscii()
        {
            int classIndex = Array.FindIndex(headers, h => h.ToLower().Contains("class"));

            var conteo = dataset
                .GroupBy(fila => fila[classIndex].ToUpper())
                .ToDictionary(g => g.Key, g => g.Count());

            Console.WriteLine("\nGráfica de barras (ASCII):\n");

            int maxValor = conteo.Values.Max();
            int maxLong = 50; // largo máximo visual

            foreach (var (key, value) in conteo)
            {
                int longitud = (int)((value / (double)maxValor) * maxLong);
                Console.WriteLine($"{key.PadRight(7)} | {new string('#', longitud)} ({value})");
            }
        }

        static void EsperarTecla()
        {
            Console.WriteLine("\nPresione una tecla para continuar...");
            Console.ReadKey();
        }

        public class ApodResponse
        {
            public string title { get; set; }
            public string date { get; set; }
            public string explanation { get; set; }
            public string url { get; set; }
            public string media_type { get; set; }
        }
    }
}

