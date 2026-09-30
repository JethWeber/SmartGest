using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartGest.Desktop.Services;

namespace SmartGest.Desktop.ViewModels;

public partial class InitialSetupViewModel : ViewModelBase
{
    private readonly ConfiguracoesService _configService;
    private readonly ContasBancariasService _bancosService;
    private readonly FirstRunService _firstRun;
    private readonly UiFeedbackService _feedback;

    public event Action? SetupCompleted;

    [ObservableProperty] private string _empresaNome = string.Empty;
    [ObservableProperty] private string _empresaNif = string.Empty;
    [ObservableProperty] private string _empresaMorada = string.Empty;
    [ObservableProperty] private string _empresaCidade = "Luanda";
    [ObservableProperty] private string _empresaTelefone = string.Empty;
    [ObservableProperty] private string _empresaEmail = string.Empty;
    [ObservableProperty] private string _banco = string.Empty;
    [ObservableProperty] private string _nib = string.Empty;
    [ObservableProperty] private string _titular = string.Empty;
    [ObservableProperty] private string _saldoInicial = "0";
    [ObservableProperty] private bool _isSaving;
    [ObservableProperty] private string _erro = string.Empty;

    public InitialSetupViewModel(ConfiguracoesService configService, ContasBancariasService bancosService, FirstRunService firstRun, UiFeedbackService feedback)
    {
        _configService = configService;
        _bancosService = bancosService;
        _firstRun = firstRun;
        _feedback = feedback;
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        if (IsSaving) return;
        Erro = string.Empty;
        if (string.IsNullOrWhiteSpace(EmpresaNome)) { Erro = "Informe o nome da empresa."; return; }
        if (string.IsNullOrWhiteSpace(Banco)) { Erro = "Informe o banco da empresa."; return; }

        IsSaving = true;
        try
        {
            decimal.TryParse(SaldoInicial.Replace(".", "").Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var saldo);

            await _configService.GuardarEmpresaAsync(
                EmpresaNome.Trim(), EmpresaNif.Trim(), EmpresaMorada.Trim(),
                EmpresaCidade.Trim(), "Angola", EmpresaTelefone.Trim(),
                EmpresaEmail.Trim(), "", 0, "");

            await _bancosService.CriarAsync(new ContasBancariasService.ContaBancariaRequest(
                Banco.Trim(), Nib.Trim(), "Conta à Ordem", "AOA", saldo, "", Titular.Trim(), "#1A2E5A"));

            _firstRun.MarkSetupCompleted();
            _feedback.ShowSuccess("Empresa e conta bancária configuradas com sucesso.");
            SetupCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            AppLogService.Error("Falha na configuração inicial da empresa e conta bancária.", ex);
            Erro = "Não foi possível concluir a configuração. Verifique os dados e tente novamente.";
            _feedback.ShowError(Erro);
        }
        finally { IsSaving = false; }
    }
}
