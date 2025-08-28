using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using POlimpicos.Data;
using POlimpicos.Models;

namespace POlimpicos.Controllers
{
    public class EstadosController : Controller
    {
        private readonly Database db = new Database();
        public IActionResult Index()
        {
            var lista = new List<Estado>();

            using (var conn = db.GetConnection())
            using (var cmd = new MySqlCommand("select * from Estados", conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    lista.Add(new Estado
                    {
                        codEstado = rd.GetInt32("codEstado"),
                        nomeEstado = rd["nomeEstado"] as string,
                       
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
        public IActionResult Cadastrar(Estado estado)
        {
            using (var conn = db.GetConnection())
            {
                var sql = @"Insert into Estados(nomeEstado)
                            Values(@nome)";

                var cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@nome", estado.nomeEstado);
               
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
    JOIN cidades c ON c.codCidade = a.codCidade
    LEFT JOIN modalidades m ON m.codModalidade = p.codModalidade
    WHERE c.codEstado = @id
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
