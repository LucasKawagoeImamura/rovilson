using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using POlimpicos.Data;
using POlimpicos.Models;
using System.Data.SqlTypes;

namespace POlimpicos.Controllers
{
    public class AtletasController : Controller
    {
        private readonly Database db = new Database();
        public IActionResult Index()
        {
            var lista = new List<Atletas>();

            using (var conn = db.GetConnection())
            using (var cmd = new MySqlCommand(@"
 SELECT DISTINCT 
        a.codAtleta, a.nomeAtleta,a.dataNascimento, a.sexo, a.altura,a.peso, a.codCidade,c.nomeCidade AS CidadeNascimento,e.nomeEstado AS EstadoNascimento,
        m.nomeModalidade AS modalidade
 FROM atletas a
 INNER JOIN resultadosatletas r ON a.codAtleta = r.codAtleta
 INNER JOIN provas p ON r.codProva = p.codProva
 INNER JOIN modalidades m ON p.codModalidade = m.codModalidade
 LEFT JOIN cidades c ON a.codCidade = c.codCidade
 LEFT JOIN estados e ON c.codEstado = e.codEstado
 ORDER BY a.nomeAtleta;", conn))
            using (var rd = cmd.ExecuteReader())
            {
                while (rd.Read())
                {
                    lista.Add(new Atletas
                    {
                        codAtleta = rd.GetInt32("codAtleta"),
                        nomeAtleta = rd["nomeAtleta"] as string,
                        dataNascimento = rd["dataNascimento"] as string,
                        sexo = rd.IsDBNull(rd.GetOrdinal("sexo")) ? (char?)null : rd.GetChar("sexo"),
                        altura = rd.IsDBNull(rd.GetOrdinal("altura")) ? (decimal?)null : rd.GetDecimal("altura"),
                        peso = rd.IsDBNull(rd.GetOrdinal("peso")) ? (decimal?)null : rd.GetDecimal("peso"),
                        codCidade = rd.IsDBNull(rd.GetOrdinal("codCidade")) ? (int?)null : rd.GetInt32("codCidade"),
                        CidadeNascimento = rd["CidadeNascimento"] as string,
                        EstadoNascimento = rd["EstadoNascimento"] as string,
                        modalidade = rd["modalidade"] as string
                    });
                }
            }

            return View(lista);
        }


        public IActionResult Criar()
        {
            ViewBag.Cidades = GetCidades();
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Atletas atletas)
        {
            using (var conn = db.GetConnection())
            {
                var sql = @"Insert into Atletas(nomeAtleta, dataNascimento, sexo, altura, peso, codCidade)
                            Values(@nome, @data, @sexo, @altura, @peso, @cidade)";

                var cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@nome", atletas.nomeAtleta);

                cmd.Parameters.AddWithValue("@data", atletas.dataNascimento);

                cmd.Parameters.AddWithValue("@sexo", atletas.sexo);

                cmd.Parameters.AddWithValue("@altura", atletas.altura);

                cmd.Parameters.AddWithValue("@peso", atletas.peso);

                cmd.Parameters.AddWithValue("@cidade", atletas.codCidade);

                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        private List<Cidade> GetCidades()
        {
            List<Cidade> cidades = new List<Cidade>();
            using (var conn = db.GetConnection())
            {
                var sql = "Select Distinct * FROM Cidades order by nomeCidade";

                var cmd = new MySqlCommand(sql, conn);

                var reader = cmd.ExecuteReader();

                while(reader.Read())
                {
                    cidades.Add(new Cidade
                    {
                        CodCidade = reader.GetInt32("codCidade"),
                        NomeCidade = reader.GetString("nomeCidade"),
                        CodEstado = reader.GetInt32("codEstado")
                    });
                }
            }
            return cidades;
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
