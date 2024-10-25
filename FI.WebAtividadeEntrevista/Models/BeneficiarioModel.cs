using FI.WebAtividadeEntrevista.Language;
using FI.WebAtividadeEntrevista.Validation;
using System.ComponentModel.DataAnnotations;

namespace FI.WebAtividadeEntrevista.Models
{
    /// <summary>
    /// Classe de Modelo de Beneficiario
    /// </summary>
    public class BeneficiarioModel
    {
        public long? Id { get; set; }

        /// <summary>
        /// CPF
        /// </summary>
        [Required]
        [ValidationCPF(ErrorMessageResourceType = typeof(WebAtividadeEntrevistaMsg), ErrorMessageResourceName = "MSG01")]
        public string CPF { get; set; }

        /// <summary>
        /// Nome
        /// </summary>

        [Required]
        public string Nome { get; set; }
    }
}