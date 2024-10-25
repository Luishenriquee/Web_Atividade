using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace FI.WebAtividadeEntrevista.Validation
{
    public class ValidationCPF : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            string cpfFull = value.ToString();

            if (Regex.IsMatch(cpfFull, @"[^0-9/.-]"))
                return false;

            string cpf = Regex.Replace(cpfFull, @"[^0-9]+", "");

            if (cpf.Length != 11)
                return false;

            return IsCpf(cpf);
        }

        private static bool IsCpf(string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
                return false;

            if (new[] { "00000000000", "11111111111", "22222222222", "33333333333",
                        "44444444444", "55555555555", "66666666666", "77777777777",
                        "88888888888", "99999999999" }.Contains(cpf))
                return false;

            int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            string digito = CalculateDigit(tempCpf, multiplicador1);

            tempCpf += digito;
            digito += CalculateDigit(tempCpf, multiplicador2);

            return cpf.EndsWith(digito);
        }

        private static string CalculateDigit(string cpf, int[] multiplicadores)
        {
            int soma = cpf.Select((t, i) => int.Parse(t.ToString()) * multiplicadores[i]).Sum();
            int resto = soma % 11;
            return resto < 2 ? "0" : (11 - resto).ToString();
        }
    }
}