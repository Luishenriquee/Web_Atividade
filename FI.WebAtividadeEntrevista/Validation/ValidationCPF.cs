using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace FI.WebAtividadeEntrevista.Validation
{
    public class ValidationCPF : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            bool result = false;

            if (value == null)
                return true;

            var _result = Regex.Match(value.ToString(), "[^0-9/.-]");

            if (_result.Success == true)
                return false;

            value = Regex.Replace(value.ToString(), "[^0-9]+", "");
            if (value.ToString().Count() != 11 && value.ToString().Count() != 14)
                return false;

            if (value.ToString().Count() == 11)
                result = IsCpf(value.ToString());

            return result;
        }

        public static bool IsCpf(string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
                return false;

            if (cpf == "00000000000" ||
                cpf == "11111111111" ||
                cpf == "22222222222" ||
                cpf == "33333333333" ||
                cpf == "44444444444" ||
                cpf == "55555555555" ||
                cpf == "66666666666" ||
                cpf == "77777777777" ||
                cpf == "88888888888" ||
                cpf == "99999999999")
                return false;

            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;
            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "");
            if (cpf.Length != 11) return false;
            tempCpf = cpf.Substring(0, 9); soma = 0;
            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i]; resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
            digito = digito + resto.ToString();
            return cpf.EndsWith(digito);
        }
    }
}