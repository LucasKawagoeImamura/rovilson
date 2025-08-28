using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using POlimpicos.Data;
using POlimpicos.Models;

namespace POlimpicos.Controllers
{
    public class CidadesController : Controller
    {
        private readonly Database db = new Database();
        public IActionResult Index()
        {
            List<Cidade> cidades = new List<Cidade>();
            using (MySqlConnection conn = db.GetConnection())
            {
                string sql = "SELECT * FROM Cidades";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cidades.Add(new Cidade
                        {
                            CodCidade = reader.GetInt32("CodCidade"),
                            NomeCidade = reader.GetString("NomeCidade"),
                            CodEstado = reader.GetInt32("CodEstado")

                        });

                    }
                }
            }
            return View(cidades);
        }

        public IActionResult Atletas(int id)
        {
            List<Atletas> atletas = new List<Atletas>();
            string nomeCidade = "";
            int totalAtletas = 0;
            using (MySqlConnection conn = db.GetConnection())
            {
                string query = @"
                SELECT DISTINCT 
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
                    WHERE a.codCidade = @id
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

            System.Diagnostics.Debug.WriteLine(nomeCidade);
            ViewBag.NomeCidade = nomeCidade;
            ViewBag.TotalAtletas = totalAtletas;
            return View(atletas);
        }

        public IActionResult Cadastrar()
        {
            ViewBag.Estados = GetEstados();
            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar(Cidade cidade)
        {
          using (var conn = db.GetConnection())
            {
                var sql = @"Insert into Cidades (nomeCidade, codEstado)
                            Values(@nome, @estado)";

                var cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@nome", cidade.NomeCidade);
                cmd.Parameters.AddWithValue("@estado", cidade.CodEstado);

                cmd.ExecuteNonQuery();

            }
            return RedirectToAction("Index");
        }


        List<Estado> GetEstados()
        {
            List<Estado> estados = new List<Estado>();

            using (var conn = db.GetConnection())
            {
                var sql = "Select Distinct * from Estados order by nomeEstado";

                var cmd = new MySqlCommand(sql, conn);

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    estados.Add(new Estado
                    {
                        codEstado = reader.GetInt32("codEstado"),
                        nomeEstado = reader.GetString("nomeEstado")
                    });
                }
            }
            return estados;
        }
        public IActionResult Detalhes(int id)
        {
            Atletas atleta = null;
            List<(string Prova, string Edicao, string Resultado, string Medalha)> participacoes = new();

            using (var conn = db.GetConnection())
            {
                string query = @"
               SELECT 
             a.codAtleta,a.nomeAtleta,a.dataNascimento,a.sexo,c.codCidade, c.nomeCidade,e.nomeEstado,
             m.codModalidade, m.nomeModalidade,p.nomeProva,r.resultado,r.medalha 
                 FROM atletas a
                 JOIN cidades c ON c.codCidade = a.codCidade
                 JOIN estados e ON e.codEstado = c.codEstado
                 JOIN resultadosatletas r ON r.codAtleta = a.codAtleta
                 JOIN provas p ON p.codProva = r.codProva
                 JOIN modalidades m ON m.codModalidade = p.codModalidade
                 WHERE a.codAtleta = @id";

                var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        atleta = new Atletas
                        {
                            codAtleta = reader.GetInt32("codAtleta"),
                            nomeAtleta = reader.GetString("nomeAtleta"),
                            dataNascimento = reader.GetString("dataNascimento"),
                            sexo = reader.GetChar("sexo"),
                            CidadeNascimento = reader.GetString("nomeCidade"),
                            codModalidade = reader.GetInt32("codModalidade"),
                            modalidade = reader.GetString("nomeModalidade"),
                            EstadoNascimento = reader.GetString("nomeEstado"),
                            codCidade = reader.GetInt32("codCidade")
                        };
                    }
                }

                // Buscar participações
                string participacaoQuery = @"
                        SELECT p.nomeProva, e.ano, e.sede, r.resultado, r.medalha
                        FROM resultadosatletas r
                        JOIN provas p ON p.codProva = r.codProva
                        JOIN edicao e ON e.codEdicao = r.codEdicao
                        WHERE r.codAtleta = @id";

                var cmd2 = new MySqlCommand(participacaoQuery, conn);
                cmd2.Parameters.AddWithValue("@id", id);
                using (var reader = cmd2.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        participacoes.Add((
                            reader.IsDBNull(reader.GetOrdinal("nomeProva"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("nomeProva")),

                            $"{(reader.IsDBNull(reader.GetOrdinal("ano"))
                                ? "?"
                                : reader.GetInt32(reader.GetOrdinal("ano")).ToString())} - {(reader.IsDBNull(reader.GetOrdinal("sede"))
                                ? "?"
                                : reader.GetString(reader.GetOrdinal("sede")))}",

                            reader.IsDBNull(reader.GetOrdinal("resultado"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("resultado")),

                            reader.IsDBNull(reader.GetOrdinal("medalha"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("medalha"))
                        ));
                    }

                }
            }

            ViewBag.Participacoes = participacoes;
            return View(atleta);
        }
    }

}
