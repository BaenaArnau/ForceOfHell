using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ForceOfHell.Scripts.Weapons
{
    /// <summary>
    /// Nodo responsable de cargar las definiciones de armas desde un CSV y exponerlas al resto del juego.
    /// Se asegura de leer el archivo una sola vez durante la sesión.
    /// </summary>
    public partial class ReadWeapons
    {
        /// <summary>
        /// Ruta absoluta al CSV dentro del sistema de archivos del proyecto (res://).
        /// </summary>
        private static string FilePath = ProjectSettings.GlobalizePath("res://Files/Weapon/weapons.csv");

        /// <summary>
        /// Diccionario con todas las armas, indexadas por su identificador único.
        /// </summary>
        private static Dictionary<int, Weapons> WeaponsById = new();

        /// <summary>
        /// Vista de solo lectura del diccionario para evitar escrituras externas.
        /// </summary>
        public static IReadOnlyDictionary<int, Weapons> Weapons => WeaponsById;

        static ReadWeapons()
        {
            // Si ya se cargaron las armas (por ejemplo, porque otro nodo lo hizo antes) no repetimos el trabajo.
            if (WeaponsById.Count > 0)
                return;

            try
            {
                LoadWeaponsFromCsv();
                GD.Print($"[Weapons] Cargadas {WeaponsById.Count} armas desde '{FilePath}'.");
            }
            catch (Exception ex)
            {
                // Mostramos el error en la consola de Godot para facilitar el diagnóstico.
                GD.PushError($"[Weapons] Error al cargar armas: {ex.Message}");
            }
        }

        /// <summary>
        /// Lee el archivo CSV completo, parsea cada línea y popula el diccionario de armas.
        /// </summary>
        private static void LoadWeaponsFromCsv()
        {
            if (!File.Exists(FilePath))
                throw new FileNotFoundException("No se ha encontrado el archivo de armas.", FilePath);

            using var reader = new StreamReader(FilePath);
            var headerRead = false;

            // Recorremos el archivo línea a línea para minimizar memoria y obtener feedback detallado.
            while (!reader.EndOfStream)
            {
                var rawLine = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(rawLine))
                    continue; // Ignora líneas vacías para permitir comentarios o espacios en el archivo.

                // Saltamos la primera línea (cabecera de columnas).
                if (!headerRead)
                {
                    headerRead = true;
                    continue;
                }

                var columns = SplitCsv(rawLine);
                if (columns.Length < 8)
                {
                    GD.PushWarning($"[Weapons] Línea ignorada (número de columnas inesperado): {rawLine}");
                    continue;
                }

                // Validamos cada columna crítica; si falla, damos una advertencia y seguimos con la siguiente línea.
                if (!int.TryParse(columns[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                {
                    GD.PushWarning($"[Weapons] Línea ignorada (id inválido): {rawLine}");
                    continue;
                }

                if (!float.TryParse(columns[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var damage))
                    damage = 0f;

                if (!int.TryParse(columns[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var cost))
                    cost = 0;

                if (!float.TryParse(columns[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var fireRate))
                    fireRate = 0f;

                if (!bool.TryParse(columns[6], out var isMeelee))
                    isMeelee = false;

                var weapon = new Weapons
                {
                    Id = id,
                    Name = columns[1],
                    Damage = damage,
                    Cost = cost,
                    FireRate = fireRate,
                    Description = columns[5],
                    IsMeelee = isMeelee,
                    Bullet = NormalizeNull(columns[7])
                };

                // Si se repite el id, la última línea prevalece (permite overrides simples).
                WeaponsById[id] = weapon;
            }
        }

        /// <summary>
        /// Devuelve un arma específica si existe en el diccionario.
        /// </summary>
        public static bool TryGetWeapon(int id, out Weapons weapon) => WeaponsById.TryGetValue(id, out weapon);

        /// <summary>
        /// Normaliza valores vacíos o con la palabra "null" a null real para evitar cadenas sin contenido.
        /// </summary>
        private static string? NormalizeNull(string value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrEmpty(trimmed) || trimmed.Equals("null", StringComparison.OrdinalIgnoreCase) ? null : trimmed;
        }

        /// <summary>
        /// Divide una línea CSV respetando comillas dobles para campos con comas internas.
        /// </summary>
        private static string[] SplitCsv(string line)
        {
            var values = new List<string>();
            var current = new StringBuilder();
            var insideQuotes = false;

            foreach (var c in line)
            {
                switch (c)
                {
                    case '"':
                        insideQuotes = !insideQuotes;
                        break;
                    case ',' when !insideQuotes:
                        values.Add(current.ToString().Trim());
                        current.Clear();
                        break;
                    default:
                        current.Append(c);
                        break;
                }
            }

            values.Add(current.ToString().Trim());
            return values.ToArray();
        }
    }
}
