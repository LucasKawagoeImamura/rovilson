using Microsoft.AspNetCore.Mvc;
using POlimpicos.Data;
using MySql.Data.MySqlClient;
using POlimpicos.Models;
using Microsoft.AspNetCore.Mvc.Rendering;



namespace POlimpicos.Controllers
{
    public class ResultadosAtletasController : Controller
    {
        private readonly Database db = new Database();

        public IActionResult Index()
        {
            return View();
        }

       


        [HttpPost]
        public IActionResult Cadastrar(Resultadosatletas resultados)
        {
            using (var conn = db.GetConnection())
            {
                var sql = @"Insert into Resultadosatletas(codAtleta, codProva, codEdicao, resultado, medalha)
                            Values(@ca, @cp, @ce, @res, @med)";

                var cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@ca", resultados.codAtleta);
                cmd.Parameters.AddWithValue("@cp", resultados.codProva);
                cmd.Parameters.AddWithValue("@ce", resultados.codEdicao);
                cmd.Parameters.AddWithValue("@res", resultados.resultado);
                cmd.Parameters.AddWithValue("@med", resultados.medalha);

                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }


        private List<Provas> GetProvas()
        {
            List<Provas> provas = new List<Provas>();
            using (var conn = db.GetConnection())
            {
                var sql = "Select Distinct * FROM Provas order by nomeProva";

                var cmd = new MySqlCommand(sql, conn);

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    provas.Add(new Provas
                    {
                        codProva = reader.GetInt32("codProva"),
                        nomeProva = reader.GetString("nomeProva"),
                        codModalidade = reader.GetInt32("codModalidade")
                    });
                }
            }
            return provas;
        }

        private List<Atletas> GetAtletas()
        {
            List<Atletas> atletas = new List<Atletas>();
            using (var conn = db.GetConnection())
            {
                var sql = "Select Distinct * FROM Atletas order by nomeAtleta";

                var cmd = new MySqlCommand(sql, conn);

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    atletas.Add(new Atletas
                    {
                        codAtleta = reader.GetInt32("codAtleta"),
                        nomeAtleta = reader.GetString("nomeAtleta"),
                        dataNascimento = reader.GetString("dataNascimento"),
                        sexo = reader.GetChar("sexo"),
                        codCidade = reader.GetInt32("codCidade")
                    });
                }
            }
            return atletas;
        }

        public IActionResult Cad()
        {
            ViewBag.Atletas = GetAtletas();
            ViewBag.Provas = GetProvas();
            ViewBag.Edi = GetEdi();
            return View();
        }


        private IEnumerable<SelectListItem> GetEdi()
        {
            var itens = new List<SelectListItem>();
            using (var conn = db.GetConnection())
            {
                var sql = "SELECT codedicao, ano, sede FROM Edicao ORDER BY ano";
                using var cmd = new MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var codigo = reader.GetInt32("codedicao");
                    var ano = reader.GetInt32("ano");
                    var sede = reader.GetString("sede");

                    itens.Add(new SelectListItem
                    {
                        Value = codigo.ToString(),
                        Text = $"{ano} - {sede}"
                    });
                }
            }
            return itens;
        }

    }
}
