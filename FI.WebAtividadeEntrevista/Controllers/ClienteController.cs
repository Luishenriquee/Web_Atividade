using FI.AtividadeEntrevista.BLL;
using WebAtividadeEntrevista.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FI.AtividadeEntrevista.DML;
using System.Text;
using FI.WebAtividadeEntrevista.Models;
using FI.WebAtividadeEntrevista.Language;
using System.Text.RegularExpressions;

namespace WebAtividadeEntrevista.Controllers
{
    public class ClienteController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Incluir()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Incluir(ClienteModel model)
        {
            BoCliente boCliente = new BoCliente();
            BoBeneficiario boBeneficiario = new BoBeneficiario();

            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }

            if (boCliente.VerificarExistencia(model.CPF))
            {
                Response.StatusCode = 400;
                return Json(WebAtividadeEntrevistaMsg.MSG02);
            }

            var beneficiariosDuplicados = model.Beneficiarios
                .GroupBy(b => b.CPF)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (beneficiariosDuplicados.Any())
            {
                StringBuilder errorMessage = new StringBuilder(WebAtividadeEntrevistaMsg.MSG03);
                foreach (var cpf in beneficiariosDuplicados)
                    errorMessage.AppendLine(cpf);

                Response.StatusCode = 400;
                return Json(errorMessage.ToString());
            }

            model.Id = boCliente.Incluir(new Cliente()
            {
                CEP = model.CEP,
                Cidade = model.Cidade,
                Email = model.Email,
                Estado = model.Estado,
                Logradouro = model.Logradouro,
                Nacionalidade = model.Nacionalidade,
                Nome = model.Nome,
                Sobrenome = model.Sobrenome,
                Telefone = model.Telefone,
                CPF = ExtrairNumeros(model.CPF)
            });

            for (int i = 0; i < model.Beneficiarios.Count; i++)
            {
                BeneficiarioModel beneficiario = model.Beneficiarios[i];
                boBeneficiario.Incluir(new Beneficiario
                {
                    Nome = beneficiario.Nome,
                    CPF = beneficiario.CPF,
                    IdCliente = model.Id
                });
            }

            return Json(WebAtividadeEntrevistaMsg.MSG04);
        }

        private string ExtrairNumeros(string num)
        {
            return Regex.Replace(num, @"\D", "");
        }

        [HttpPost]
        public JsonResult Alterar(ClienteModel model)
        {
            BoCliente boCliente = new BoCliente();
            BoBeneficiario boBeneficiario = new BoBeneficiario();

            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }

            List<Beneficiario> beneficiariosExistentes = boBeneficiario.Listar(model.Id);
            foreach (Beneficiario beneficiario in beneficiariosExistentes)
            {
                if (model.Beneficiarios.Any(b => b.CPF == beneficiario.CPF && b.Id != beneficiario.Id))
                {
                    Response.StatusCode = 400;
                    return Json(string.Format(WebAtividadeEntrevistaMsg.MSG06, beneficiario.CPF));
                }
            }

            boCliente.Alterar(new Cliente()
            {
                Id = model.Id,
                CEP = model.CEP,
                Cidade = model.Cidade,
                Email = model.Email,
                Estado = model.Estado,
                Logradouro = model.Logradouro,
                Nacionalidade = model.Nacionalidade,
                Nome = model.Nome,
                Sobrenome = model.Sobrenome,
                Telefone = model.Telefone,
                CPF = ExtrairNumeros(model.CPF)
            });

            CadastrarOuAtualizarBeneficiario(model, boBeneficiario, beneficiariosExistentes);

            RemoverBeneficiariosExcluidos(boBeneficiario, beneficiariosExistentes);

            return Json(WebAtividadeEntrevistaMsg.MSG05);
        }

        private void CadastrarOuAtualizarBeneficiario(ClienteModel model, BoBeneficiario boBeneficiarios, List<Beneficiario> beneficiariosExistentes)
        {
            foreach (var beneficiarioModel in model.Beneficiarios)
            {
                if (beneficiarioModel.Id != null)
                {
                    boBeneficiarios.Alterar(new Beneficiario
                    {
                        Id = beneficiarioModel.Id.Value,
                        Nome = beneficiarioModel.Nome,
                        CPF = ExtrairNumeros(beneficiarioModel.CPF),
                        IdCliente = model.Id
                    });

                    beneficiariosExistentes.RemoveAll(x => x.Id == beneficiarioModel.Id);
                }
                else
                {
                    boBeneficiarios.Incluir(new Beneficiario
                    {
                        Nome = beneficiarioModel.Nome,
                        CPF = ExtrairNumeros(beneficiarioModel.CPF),
                        IdCliente = model.Id
                    });
                }
            }
        }

        private void RemoverBeneficiariosExcluidos(BoBeneficiario boBeneficiarios, List<Beneficiario> beneficiariosExistentes)
        {
            foreach (var beneficiario in beneficiariosExistentes)
                boBeneficiarios.Excluir(beneficiario.Id);
        }

        [HttpGet]
        public ActionResult Alterar(long id)
        {
            Cliente cliente = new BoCliente().Consultar(id);
            List<Beneficiario> beneficiarios = new BoBeneficiario().Listar(cliente.Id);
            ClienteModel model = null;

            if (cliente != null)
            {
                model = new ClienteModel()
                {
                    Id = cliente.Id,
                    CEP = cliente.CEP,
                    Cidade = cliente.Cidade,
                    Email = cliente.Email,
                    Estado = cliente.Estado,
                    Logradouro = cliente.Logradouro,
                    Nacionalidade = cliente.Nacionalidade,
                    Nome = cliente.Nome,
                    Sobrenome = cliente.Sobrenome,
                    Telefone = cliente.Telefone,
                    CPF = ExtrairNumeros(cliente.CPF),
                    Beneficiarios = beneficiarios
                    .Select(beneficiario => new BeneficiarioModel
                    {
                        Id = beneficiario.Id,
                        Nome = beneficiario.Nome,
                        CPF = ExtrairNumeros(beneficiario.CPF)
                    })
                .ToList()
                };
            }

            return View(model);
        }

        [HttpPost]
        public JsonResult ClienteList(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            try
            {
                int qtd = 0;
                string campo = string.Empty;
                string crescente = string.Empty;
                string[] array = jtSorting.Split(' ');

                if (array.Length > 0)
                    campo = array[0];

                if (array.Length > 1)
                    crescente = array[1];

                List<Cliente> clientes = new BoCliente().Pesquisa(jtStartIndex, jtPageSize, campo, crescente.Equals("ASC", StringComparison.InvariantCultureIgnoreCase), out qtd);

                //Return result to jTable
                return Json(new { Result = "OK", Records = clientes, TotalRecordCount = qtd });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }
    }
}