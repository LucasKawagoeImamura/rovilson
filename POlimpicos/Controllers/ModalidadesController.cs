using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using POlimpicos.Data;
using POlimpicos.Models;

namespace POlimpicos.Controllers
{
    public class ModalidadesController : Controller
    {
        private readonly Database db = new Database();

        public IActionResult Index()
        {
            var lista = new List<Modalidades>();

            using (var conn = db.GetConnection())
            using (var cmd = new MySqlCommand("select * from Modalidades", conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    lista.Add(new Modalidades
                    {
                        codModalidade = rd.GetInt32("codModalidade"),
                        nomeModalidade = rd["nomeModalidade"] as string,

                    });
                }
            }

            return View(lista);
        }


        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar(Modalidades modalidades)
        {
            using (var conn = db.GetConnection())
            {
                var sql = @"Insert into Modalidades (nomeModalidade)
                            Values(@nome)";

                var cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@nome", modalidades.nomeModalidade);


                cmd.ExecuteNonQuery();

            }
            return RedirectToAction("Index");

        }

        public IActionResult Atletas(int id)
        {
            List<Atletas> atletas = new List<Atletas>();
            int totalAtletas = 0;
            using (MySqlConnection conn = db.GetConnection())
            {
                string query = @"SELECT DISTINCT     
        a.codAtleta, 
        a.nomeAtleta, 
        a.dataNascimento, 
        a.sexo, 
        a.codCidade,
        m.codModalidade, 
        m.nomeModalidade
    FROM resultadosatletas r
    JOIN provas p ON p.codProva = r.codProva
    JOIN atletas a ON a.codAtleta = r.codAtleta
    LEFT JOIN modalidades m ON m.codModalidade = p.codModalidade
    WHERE p.codModalidade = @id
                    ";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        atletas.Add(new Atletas
                        {
                            codAtleta = reader.GetInt32(reader.GetOrdinal("codAtleta")),

                            nomeAtleta = reader.IsDBNull(reader.GetOrdinal("nomeAtleta")) ? null : reader.GetString(reader.GetOrdinal("nomeAtleta")),

                            dataNascimento = reader.IsDBNull(reader.GetOrdinal("dataNascimento")) ? null
                                : reader.GetString(reader.GetOrdinal("dataNascimento")),

                            sexo = reader.IsDBNull(reader.GetOrdinal("sexo"))
                                ? '\0'  // valor padrão para char
                                : reader.GetChar(reader.GetOrdinal("sexo")),

                            codCidade = reader.IsDBNull(reader.GetOrdinal("codCidade"))
                                ? 0  // ou (int?)null se for Nullable<int>
                                : reader.GetInt32(reader.GetOrdinal("codCidade")),

                            codModalidade = reader.IsDBNull(reader.GetOrdinal("codModalidade"))
                                ? 0  // ou (int?)null se sua propriedade for Nullable
                                : reader.GetInt32(reader.GetOrdinal("codModalidade")),

                            modalidade = reader.IsDBNull(reader.GetOrdinal("nomeModalidade"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("nomeModalidade"))
                        });
                    }

                }

                totalAtletas = atletas.Count;
            }

            ViewBag.EdicaoId = id;
            ViewBag.TotalAtletas = totalAtletas;
            return View(atletas);
        }


    }
}
