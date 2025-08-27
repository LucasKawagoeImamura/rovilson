namespace POlimpicos.Models
{
    public class Atletas
    {
        public int codAtleta { get; set; }

        public string? nomeAtleta { get; set; }

        public string? dataNascimento { get; set; }

        public char? sexo { get; set; }

        public decimal? altura { get; set; }

        public decimal? peso { get; set; }

        public int? codCidade { get; set; }

        public int? codModalidade { get; set; }

        public string? modalidade { get; set; }
        public string? CidadeNascimento { get; set; }
        public string? EstadoNascimento { get; set; }
    }
}
