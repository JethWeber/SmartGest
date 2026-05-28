# SmartGest Desktop — Integração Login + Novo Lançamento

## Ficheiros entregues

```
Services/
  TokenStore.cs              ← singleton JWT + dados do utilizador
  ApiClient.cs               ← wrapper HttpClient (Bearer automático)
  AuthService.cs             ← POST /api/auth/login  |  POST /api/auth/alterar-senha
  ContasBancariasService.cs  ← GET  /api/contas-bancarias
  LancamentoService.cs       ← POST /api/lancamentos

ViewModels/
  LoginViewModel.cs          ← integrado com AuthService
  NovoLancamentoViewModel.cs ← integrado com LancamentoService + ContasBancariasService
  MainWindowViewModel.cs     ← recebe TokenStore via DI

ServiceLocator.cs            ← registo de DI (IServiceCollection)
README.md                    ← este ficheiro
```

## API Base URL

Todos os serviços apontam para:
```
http://localhost:5275/
```
Para mudar, edita a constante em `Services/ApiClient.cs`:
```csharp
public const string BaseUrl = "http://localhost:5275/";
```

---

## 1 — Instalar dependências NuGet (Desktop)

```
dotnet add package Microsoft.Extensions.DependencyInjection
```
(O `Microsoft.Extensions.Http` é opcional — o `ApiClient` usa `new HttpClient()` directamente.)

---

## 2 — Configurar DI em App.axaml.cs

Substitui o teu `OnFrameworkInitializationCompleted` por:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SmartGest.Desktop.Views;   // ajusta ao teu namespace de Views

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ServiceLocator.Register(services);
        ServiceLocator.Provider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Mostra o SplashScreen (ou LoginWindow directamente)
            desktop.MainWindow = new SplashWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
```

---

## 3 — Ligar LoginViewModel ao LoginWindow

No code-behind da tua `LoginWindow.axaml.cs`:

```csharp
public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();

        var vm = ServiceLocator.Get<LoginViewModel>();
        DataContext = vm;

        vm.LoginSucceeded += () =>
        {
            var mainVm = ServiceLocator.Get<MainWindowViewModel>();
            new MainWindow { DataContext = mainVm }.Show();
            Close();
        };
    }
}
```

---

## 4 — Ligar NovoLancamentoViewModel ao modal

No code-behind do modal de novo lançamento:

```csharp
public partial class NovoLancamentoWindow : Window
{
    public NovoLancamentoWindow()
    {
        InitializeComponent();

        var vm = ServiceLocator.Get<NovoLancamentoViewModel>();
        vm.OwnerWindow = this;
        DataContext    = vm;

        vm.DialogClosed      += () => Close();
        vm.LancamentoCriado  += lanc =>
        {
            // Opcional: notifica a página de Caixa para recarregar a lista
            Console.WriteLine($"Lançamento {lanc.Id} criado: {lanc.Descricao}");
        };
    }
}
```

---

## 5 — Habilitar CORS na API

Em `Program.cs` da API SmartGest.API, **antes** de `builder.Build()`:

```csharp
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader()));
```

**Após** `builder.Build()` e **antes** dos middlewares de routing:

```csharp
app.UseCors();
```

---

## 6 — Credencial de teste (seed da API)

| Campo    | Valor       |
|----------|-------------|
| Telefone | `900000000` |
| Password | Ver hash no SeedData — cria um utilizador de teste com BCrypt |

Para gerar um hash de teste, adiciona temporariamente à `Program.cs` da API:
```csharp
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("smartgest2025"));
```
E actualiza o campo `PasswordHash` no `SeedData` do `AppDbContext`.

---

## Tratamento de erros implementado

| Código HTTP | Situação                        | Comportamento               |
|-------------|---------------------------------|-----------------------------|
| 401         | Credenciais erradas / token expirado | Mensagem ao utilizador  |
| 400         | Dados inválidos (validação API) | Mostra detalhe do erro      |
| 409         | NIB duplicado / email já existe | Mensagem ao utilizador      |
| Timeout     | Servidor inactivo               | Mensagem com URL da API     |

---

## Próximos passos (fase 2)

- Dashboard → `GET /api/dashboard`
- Caixa → `GET /api/lancamentos` com paginação
- Contas & Bancos → `GET /api/contas-bancarias/{id}/movimentos`
- Configurações → `GET/PUT /api/configuracoes`
