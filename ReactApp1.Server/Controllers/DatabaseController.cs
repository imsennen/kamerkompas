using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using DotNetEnv;

namespace ReactApp1.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly string _connectionString;

        public DatabaseController()
        {
            // Ensure .env is loaded (if not already done in Program.cs)
            DotNetEnv.Env.Load();

            var host = Environment.GetEnvironmentVariable("host");
            var user = Environment.GetEnvironmentVariable("user");
            var password = Environment.GetEnvironmentVariable("password");
            var database = Environment.GetEnvironmentVariable("database");

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(database))
            {
                throw new InvalidOperationException("Database credentials are not set in .env");
            }

            _connectionString = $"server={host};user={user};password={password};database={database};";
        }

        // Generic helper for queries returning a list of T
        private async Task<List<T>> QueryAsync<T>(string sql, Func<MySqlDataReader, T> map)
        {
            var results = new List<T>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync() as MySqlDataReader; // Explicitly cast to MySqlDataReader
            if (reader == null)
            {
                throw new InvalidOperationException("Failed to cast DbDataReader to MySqlDataReader.");
            }
            while (await reader.ReadAsync())
            {
                results.Add(map(reader));
            }
            return results;
        }

        // Overload of QueryAsync to support parameters
        private async Task<List<T>> QueryAsync<T>(string sql, Func<MySqlDataReader, T> map, params MySqlParameter[] parameters)
        {
            var results = new List<T>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            using var reader = await command.ExecuteReaderAsync() as MySqlDataReader;
            if (reader == null)
            {
                throw new InvalidOperationException("Failed to cast DbDataReader to MySqlDataReader.");
            }
            while (await reader.ReadAsync())
            {
                results.Add(map(reader));
            }
            return results;
        }

        [HttpGet("fracties")]
        public async Task<IActionResult> GetFracties()
        {
            var sql = @"
                SELECT NaamNL, Id, AantalZetels
                FROM tweede_kame4.fractie
                WHERE DatumInactief IS NULL
                ORDER BY AantalZetels DESC;
            ";
            var fracties = await QueryAsync(sql, r => new {
                NaamNL = r.GetString("NaamNL"),
                Id = r.GetString("Id"),
                AantalZetels = r.GetInt32("AantalZetels"),
                FotoLink = $"https://gegevensmagazijn.tweedekamer.nl/OData/v4/2.0/fractie/{r.GetString("Id")}/resource"
            });
            return Ok(fracties);
        }

        [HttpGet("fracties/{id}")]
        public async Task<IActionResult> GetFractieById(string id)
        {
            var sql = @"
                SELECT NaamNL, Id, AantalZetels
                FROM tweede_kame4.fractie
                WHERE Id = @id AND DatumInactief IS NULL
                LIMIT 1;
            ";
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync() as MySqlDataReader;
            if (reader == null || !await reader.ReadAsync())
            {
                return NotFound();
            }

            var fractie = new
            {
                NaamNL = reader.GetString("NaamNL"),
                Id = reader.GetString("Id"),
                AantalZetels = reader.GetInt32("AantalZetels"),
                FotoLink = $"https://gegevensmagazijn.tweedekamer.nl/OData/v4/2.0/fractie/{reader.GetString("Id")}/resource"
            };

            return Ok(fractie);
        }

        [HttpGet("huidige-functies")]
        public async Task<IActionResult> GetHuidigeFuncties()
        {
            var sql = @"
                SELECT
                  p.Achternaam  AS persoon_naam,
                  f.NaamNL      AS fractie_naam,
                  f.Afkorting   AS fractie_afkorting,
                  fzp.functie   AS functie,
                  fzp.van       AS functie_van,
                  fzp.TotEnMet  AS functie_tot
                FROM fractiezetelpersoon AS fzp
                JOIN fractiezetel AS fz
                  ON fzp.FractieZetel_Id = fz.Id
                JOIN fractie AS f
                  ON fz.Fractie_Id = f.Id
                JOIN persoon AS p
                  ON fzp.Persoon_Id = p.Id
                WHERE
                  fzp.TotEnMet IS NULL
                  AND fz.verwijderd = FALSE
                  AND f.verwijderd = FALSE
                ORDER BY
                  f.NaamNL,
                  p.Achternaam;
            ";

            var functies = await QueryAsync(sql, r => new {
                PersoonNaam = r.GetString("persoon_naam"),
                FractieNaam = r.GetString("fractie_naam"),
                FractieAfkorting = r.GetString("fractie_afkorting"),
                Functie = r.GetString("functie"),
                FunctieVan = r.GetDateTime("functie_van"),
                FunctieTot = r.IsDBNull(r.GetOrdinal("functie_tot")) ? (DateTime?)null : r.GetDateTime("functie_tot")
            });

            return Ok(functies);
        }

        [HttpGet("leden-van-fractie")]
        public async Task<IActionResult> GetLedenVanFractie([FromQuery] string partijNaam)
        {
            if (string.IsNullOrWhiteSpace(partijNaam))
            {
                return BadRequest("Parameter 'partijNaam' is required.");
            }

            var sql = @"
                SELECT
                  p.Achternaam  AS persoon_naam,
                  p.Voornamen   AS persoon_voornamen,
                  f.NaamNL      AS fractie_naam,
                  f.Afkorting   AS fractie_afkorting,
                  fzp.functie   AS functie,
                  fzp.van       AS functie_van,
                  fzp.TotEnMet  AS functie_tot
                FROM fractiezetelpersoon AS fzp
                JOIN fractiezetel AS fz
                  ON fzp.FractieZetel_Id = fz.Id
                JOIN fractie AS f
                  ON fz.Fractie_Id = f.Id
                JOIN persoon AS p
                  ON fzp.Persoon_Id = p.Id
                WHERE
                  fzp.TotEnMet IS NULL
                  AND fz.verwijderd = FALSE
                  AND f.verwijderd = FALSE
                  AND f.NaamNL = @partijNaam
                ORDER BY
                  p.Achternaam, p.Voornamen;
            ";

            var leden = await QueryAsync(sql, r => new {
                PersoonAchternaam = r.GetString("persoon_naam"),
                PersoonVoornamen = r.GetString("persoon_voornamen"),
                FractieNaam = r.GetString("fractie_naam"),
                FractieAfkorting = r.GetString("fractie_afkorting"),
                Functie = r.GetString("functie"),
                FunctieVan = r.GetDateTime("functie_van"),
                FunctieTot = r.IsDBNull(r.GetOrdinal("functie_tot")) ? (DateTime?)null : r.GetDateTime("functie_tot")
            }, new MySqlParameter("@partijNaam", partijNaam));

            return Ok(leden);
        }

        ////more apis
        //[HttpGet("members")]
        //public async Task<IActionResult> GetMembers()
        //{
        //    var sql = "SELECT id, name FROM members";
        //    var members = await QueryAsync(sql, r => new {
        //        Id = r.GetInt32("id"),
        //        Name = r.GetString("name")
        //    });
        //    return Ok(members);
        //}

        // Add more endpoints as needed, just provide SQL and mapping logic
    }
}