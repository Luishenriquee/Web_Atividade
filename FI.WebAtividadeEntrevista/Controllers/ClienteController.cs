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

            var beneficiariosDuplicado = model.Beneficiarios
                .GroupBy(b => b.CPF)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (beneficiariosDuplicado.Any())
            {
                StringBuilder mensagemErro = new StringBuilder(WebAtividadeEntrevistaMsg.MSG03);
                foreach (var cpf in beneficiariosDuplicado)
                    mensagemErro.AppendLine(cpf);

                Response.StatusCode = 400;
                return Json(mensagemErro.ToString());
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

            List<Beneficiario> beneficiariosExistente = boBeneficiario.Listar(model.Id);
            foreach (Beneficiario beneficiario in beneficiariosExistente)
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

            CadastrarOuAtualizarBeneficiario(model, boBeneficiario, beneficiariosExistente);

            RemoverBeneficiarios(boBeneficiario, beneficiariosExistente);

            return Json(WebAtividadeEntrevistaMsg.MSG05);
        }

        private void CadastrarOuAtualizarBeneficiario(ClienteModel clienteModel, BoBeneficiario boBeneficiarios, List<Beneficiario> beneficiariosExistente)
        {
            foreach (var beneficiarioModel in clienteModel.Beneficiarios)
            {
                if (beneficiarioModel.Id != null)
                {
                    boBeneficiarios.Alterar(new Beneficiario
                    {
                        Id = beneficiarioModel.Id.Value,
                        Nome = beneficiarioModel.Nome,
                        CPF = ExtrairNumeros(beneficiarioModel.CPF),
                        IdCliente = clienteModel.Id
                    });

                    beneficiariosExistente.RemoveAll(x => x.Id == beneficiarioModel.Id);
                }
                else
                {
                    boBeneficiarios.Incluir(new Beneficiario
                    {
                        Nome = beneficiarioModel.Nome,
                        CPF = ExtrairNumeros(beneficiarioModel.CPF),
                        IdCliente = clienteModel.Id
                    });
                }
            }
        }

        [HttpGet]
        public ActionResult Alterar(long id)
        {
            Cliente cliente = new BoCliente().Consultar(id);
            List<Beneficiario> beneficiarios = new BoBeneficiario().Listar(cliente.Id);
            ClienteModel clienteModel = null;

            if (cliente != null)
            {
                clienteModel = new ClienteModel()
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

            return View(clienteModel);
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

        private string ExtrairNumeros(string num)
        {
            return Regex.Replace(num, @"\D", "");
        }

        private void RemoverBeneficiarios(BoBeneficiario boBeneficiarios, List<Beneficiario> beneficiariosExistente)
        {
            foreach (var beneficiario in beneficiariosExistente)
                boBeneficiarios.Excluir(beneficiario.Id);
        }
    }
}