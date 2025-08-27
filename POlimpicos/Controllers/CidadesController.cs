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
            return View();
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
    }

}
