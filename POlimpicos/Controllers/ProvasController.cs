using Microsoft.AspNetCore.Mvc;
using POlimpicos.Data;
using MySql.Data.MySqlClient;
using POlimpicos.Models;
using System.Data.SqlTypes;



namespace POlimpicos.Controllers
{
    public class ProvasController : Controller
    {
        private readonly Database db = new Database();

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Cadastrar()
        {
            ViewBag.Modalidades = GetModalidades();
            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar(Provas provas)
        {
            using (var conn = db.GetConnection())
            {
                var sql = @"Insert into Provas(nomeProva, codModalidade)
                            Values(@nome,@cod)";

                var cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@nome", provas.nomeProva);

                cmd.Parameters.AddWithValue("@cod", provas.codModalidade);

                
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }



        private List<Modalidades> GetModalidades()
        {
            List<Modalidades> modalidades = new List<Modalidades>();
            using (var conn = db.GetConnection())
            {
                var sql = "Select Distinct * FROM Modalidades order by nomeModalidade";

                var cmd = new MySqlCommand(sql, conn);

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    modalidades.Add(new Modalidades
                    {
                        codModalidade = reader.GetInt32("codModalidade"),
                        nomeModalidade = reader.GetString("nomeModalidade")

                    });
                }
            }
            return modalidades;
        }


    }
}
