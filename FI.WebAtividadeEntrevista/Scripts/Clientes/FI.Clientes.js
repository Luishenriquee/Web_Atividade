$(document).ready(function () {
    configurarFormularioCadastro();
    configurarFormularioBeneficiario();
    configurarBotoesBeneficiarios();

    function configurarFormularioCadastro() {
        $('#formCadastro').submit(function (e) {
            e.preventDefault();
            const beneficiarios = obterBeneficiarios();

            $.ajax({
                url: urlPost,
                method: "POST",
                data: obterDadosFormulario(beneficiarios),
                error: function (response) {
                    tratarErro(response);
                },
                success: function (response) {
                    tratarSucesso(response);
                }
            });
        });
    }

    function obterBeneficiarios() {
        const beneficiarios = [];
        $('#tabelaBenficiario tbody tr').each(function () {
            const id = $(this).find('td:eq(0)').text();
            const cpf = $(this).find('td:eq(1)').text();
            const nome = $(this).find('td:eq(2)').text();
            beneficiarios.push({ Id: id, CPF: cpf, Nome: nome });
        });
        return beneficiarios;
    }

    function obterDadosFormulario(beneficiarios) {
        return {
            "NOME": $("#Nome").val(),
            "CEP": $("#CEP").val(),
            "Email": $("#Email").val(),
            "Sobrenome": $("#Sobrenome").val(),
            "Nacionalidade": $("#Nacionalidade").val(),
            "Estado": $("#Estado").val(),
            "Cidade": $("#Cidade").val(),
            "Logradouro": $("#Logradouro").val(),
            "Telefone": $("#Telefone").val(),
            "CPF": $("#CPF").val(),
            "Beneficiarios": beneficiarios
        };
    }

    function tratarErro(response) {
        const mensagem = response.status === 400 ? response.responseJSON : "Ocorreu um erro interno no servidor.";
        ModalDialog("Ocorreu um erro", mensagem);
    }

    function tratarSucesso(response) {
        ModalDialog("Sucesso!", response);
        $("#formCadastro")[0].reset();
    }

    function configurarFormularioBeneficiario() {
        $('#formIncluirBeneficiario').submit(function (e) {
            e.preventDefault();
            adicionarBeneficiario();
        });
    }

    function adicionarBeneficiario() {
        const cpf = $('#CpfBeneficiario').val();
        const nome = $('#NomeBeneficiario').val();

        const novaLinha = `
            <tr>
                <td class="hidden-xs hidden"></td>
                <td>${cpf}</td>
                <td>${nome}</td>
                <td class="text-center">
                    <button type="button" class="btn btn-sm btn-primary btn-alterar" style="margin-right: 0.4rem">Alterar</button>
                    <button type="button" class="btn btn-sm btn-primary btn-excluir">Excluir</button>
                </td>
            </tr>
        `;

        $('#tabelaBenficiario tbody').append(novaLinha);
        $('#CpfBeneficiario').val('');
        $('#NomeBeneficiario').val('');
    }

    function configurarBotoesBeneficiarios() {
        $('#btnBeneficiarios').click(function (event) {
            event.preventDefault();
            $('#beneficarios').modal('show');
        });

        $('#tabelaBenficiario').on('click', 'button.btn-excluir', function () {
            $(this).closest('tr').remove();
        });

        $('#tabelaBenficiario').on('click', 'button.btn-alterar', function () {
            alterarLinhaBeneficiario($(this));
        });
    }

    function alterarLinhaBeneficiario(botao) {
        const linha = botao.closest('tr');

        if (linha.hasClass('em-edicao')) {
            salvarEdicaoLinha(linha, botao);
        } else {
            editarLinha(linha, botao);
        }
    }

    function editarLinha(linha, botao) {
        const tdCPF = linha.find('td:eq(1)');
        const tdNome = linha.find('td:eq(2)');

        tdCPF.html(`<div class="input-group"><input id="beneficiario_Alt_CPF" type="text" class="form-control" style="width: 13rem;" value="${tdCPF.text()}"></div>`);
        tdNome.html(`<div class="input-group"><input type="text" class="form-control" style="width: 150px;" value="${tdNome.text()}"></div>`);
        $('#beneficiario_Alt_CPF').mask('000.000.000-00', { reverse: true });

        botao.text('Salvar').addClass('btn-success');
        linha.addClass('em-edicao');
    }

    function salvarEdicaoLinha(linha, botao) {
        const valores = linha.find('input').map(function () {
            return $(this).val();
        }).get();

        linha.find('td:eq(1)').text(valores[0]);
        linha.find('td:eq(2)').text(valores[1]);

        botao.text('Alterar').removeClass('btn-success');
        linha.removeClass('em-edicao');
    }
});

function ModalDialog(titulo, texto) {
    const random = Math.random().toString().replace('.', '');
    const modalHtml = `
        <div id="${random}" class="modal fade">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
                        <h4 class="modal-title">${titulo}</h4>
                    </div>
                    <div class="modal-body">
                        <p>${texto}</p>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-default" data-dismiss="modal">Fechar</button>
                    </div>
                </div>
            </div>
        </div>
    `;
    $('body').append(modalHtml);
    $('#' + random).modal('show');
}
