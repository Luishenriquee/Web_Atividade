$(document).ready(function () {
    preencherFormularioSeExistirObjeto();
    configurarEventos();

    function preencherFormularioSeExistirObjeto() {
        if (!obj) return;

        preencherCamposFormulario();
        preencherTabelaBeneficiarios();
    }

    function preencherCamposFormulario() {
        $('#formCadastro #Nome').val(obj.Nome);
        $('#formCadastro #CEP').val(obj.CEP);
        $('#formCadastro #Email').val(obj.Email);
        $('#formCadastro #Sobrenome').val(obj.Sobrenome);
        $('#formCadastro #Nacionalidade').val(obj.Nacionalidade);
        $('#formCadastro #Estado').val(obj.Estado);
        $('#formCadastro #Cidade').val(obj.Cidade);
        $('#formCadastro #Logradouro').val(obj.Logradouro);
        $('#formCadastro #Telefone').val(obj.Telefone);
        $('#formCadastro #CPF').val(obj.CPF);
    }

    function preencherTabelaBeneficiarios() {
        $('#tabelaBenficiario tbody').empty();

        $.each(obj.Beneficiarios, function (_, beneficiario) {
            const cpfFormatado = beneficiario.CPF.replace(/^(\d{3})(\d{3})(\d{3})(\d{2})$/, "$1.$2.$3-$4");
            const linha = criarLinhaTabelaBeneficiario(beneficiario.Id, cpfFormatado, beneficiario.Nome);
            $('#tabelaBenficiario tbody').append(linha);
        });
    }

    function criarLinhaTabelaBeneficiario(id, cpf, nome) {
        return `
            <tr>
                <td class="hidden-xs hidden">${id}</td>
                <td>${cpf}</td>
                <td>${nome}</td>
                <td class="text-center">
                    <button type="button" class="btn btn-sm btn-primary btn-alterar" style="margin-right: 0.4rem">Alterar</button>
                    <button type="button" class="btn btn-sm btn-primary btn-excluir">Excluir</button>
                </td>
            </tr>
        `;
    }

    function configurarEventos() {
        $('#btnBeneficiarios').click(function (e) {
            e.preventDefault();
            $('#beneficarios').modal('show');
        });

        $('#formCadastro').submit(enviarFormularioCadastro);
        $('#formIncluirBeneficiario').submit(adicionarBeneficiario);
        $('#tabelaBenficiario').on('click', 'button.btn-excluir', excluirLinha);
        $('#tabelaBenficiario').on('click', 'button.btn-alterar', alternarEdicaoLinha);
    }

    function enviarFormularioCadastro(e) {
        e.preventDefault();

        const beneficiarios = obterBeneficiarios();

        $.ajax({
            url: urlPost,
            method: "POST",
            data: obterDadosFormulario(beneficiarios),
            error: tratarErro,
            success: tratarSucesso
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
        window.location.href = urlRetorno;
    }

    function adicionarBeneficiario(e) {
        e.preventDefault();

        const cpf = $('#CpfBeneficiario').val();
        const nome = $('#NomeBeneficiario').val();
        const novaLinha = criarLinhaTabelaBeneficiario('', cpf, nome);

        $('#tabelaBenficiario tbody').append(novaLinha);
        $('#CpfBeneficiario').val('');
        $('#NomeBeneficiario').val('');
    }

    function excluirLinha() {
        $(this).closest('tr').remove();
    }

    function alternarEdicaoLinha() {
        const linha = $(this).closest('tr');

        if (linha.hasClass('em-edicao')) {
            salvarEdicaoLinha(linha, $(this));
        } else {
            iniciarEdicaoLinha(linha, $(this));
        }
    }

    function iniciarEdicaoLinha(linha, botao) {
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
    var random = Math.random().toString().replace('.', '');
    var texto = '<div id="' + random + '" class="modal fade">                                                               ' +
        '        <div class="modal-dialog">                                                                                 ' +
        '            <div class="modal-content">                                                                            ' +
        '                <div class="modal-header">                                                                         ' +
        '                    <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>         ' +
        '                    <h4 class="modal-title">' + titulo + '</h4>                                                    ' +
        '                </div>                                                                                             ' +
        '                <div class="modal-body">                                                                           ' +
        '                    <p>' + texto + '</p>                                                                           ' +
        '                </div>                                                                                             ' +
        '                <div class="modal-footer">                                                                         ' +
        '                    <button type="button" class="btn btn-default" data-dismiss="modal">Fechar</button>             ' +
        '                                                                                                                   ' +
        '                </div>                                                                                             ' +
        '            </div><!-- /.modal-content -->                                                                         ' +
        '  </div><!-- /.modal-dialog -->                                                                                    ' +
        '</div> <!-- /.modal -->                                                                                        ';

    $('body').append(texto);
    $('#' + random).modal('show');
}
