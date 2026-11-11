# SmartGest — Plano de Implementação: Configuração, Onboarding e Licenciamento

> **Estado:** Proposta de implementação  
> **Produto:** SmartGest Desktop  
> **Repositório:** `JethWeber/SmartGest`  
> **Prioridade:** Desktop local, autónomo e offline-first  
> **Moeda/região padrão:** AOA (Kz) / Angola

Este documento define o plano de trabalho para consolidar a experiência inicial do SmartGest Desktop, introduzir o ficheiro de configuração próprio `.sg` e integrar o sistema de licenciamento **WeberTechLicensing** com o mesmo comportamento funcional já utilizado no KiVenda.

O documento é um plano técnico. Os itens só devem ser marcados como concluídos depois de implementados, integrados e testados.

---

## 1. Objectivos e decisões de arquitectura

### 1.1 Objectivos

- Fazer o SmartGest Desktop funcionar localmente, sem depender da API para as operações normais de gestão e contabilidade.
- Exigir o registo da empresa na primeira inicialização.
- Guardar as configurações da aplicação e os dados cadastrais da empresa num ficheiro JSON com extensão `.sg`.
- Manter os dados transaccionais numa base de dados SQLite.
- Integrar o SDK partilhado WeberTechLicensing, sem criar uma implementação paralela ou incompatível.
- Replicar o comportamento de licenciamento do KiVenda: ativação, validação offline, avisos, tolerância, bloqueio e renovação.
- Preservar dados da empresa, base de dados e licença durante atualizações.

### 1.2 Decisões obrigatórias

| Tema | Decisão |
|---|---|
| Experiência principal | Desktop local, offline-first |
| Registo da empresa | Obrigatório na primeira inicialização |
| Ficheiro de configuração | `SmartGest.sg` |
| Formato | JSON válido, com extensão `.sg` |
| Dados operacionais | SQLite |
| API | Mantida como projeto independente; não deve bloquear os fluxos locais do Desktop |
| Licenciamento | SDK WeberTechLicensing partilhado com o KiVenda |
| Identificador do produto | `smartgest.desktop_v01` (confirmar no registo oficial de produtos antes da emissão de licenças) |
| Ficheiro de licença | `.wta` |
| Aviso de expiração | A partir de 14 dias antes do fim da licença |
| Tolerância após expiração | 7 dias corridos |
| Bloqueio | Depois de terminar a tolerância |
| Recuperação | Importar uma licença válida deve desbloquear imediatamente a aplicação |

### 1.3 Separação de responsabilidades

O ficheiro `.sg` não substitui a base de dados. Cada armazenamento tem uma finalidade:

- **`SmartGest.sg`:** dados cadastrais da empresa, preferências, opções da aplicação e configurações não transaccionais.
- **`smartgest.db`:** utilizadores locais, contas, lançamentos, movimentos, categorias e restantes dados operacionais.
- **Licença `.wta`:** artefacto de licenciamento assinado, armazenado e gerido pelo sistema WeberTechLicensing.
- **Backups:** cópias consistentes da base de dados e, quando apropriado, do ficheiro de configuração. A licença deve seguir as regras de armazenamento e recuperação definidas pelo SDK.

Não guardar passwords, tokens de sessão, chaves privadas ou segredos no ficheiro `.sg`.

---

## 2. Fase 0 — Auditoria e preparação

**Objectivo:** confirmar o estado real do código antes de alterar os fluxos existentes.

- [ ] Mapear os fluxos actuais de arranque, login e configuração inicial.
- [ ] Identificar todos os pontos que leem ou escrevem dados da empresa.
- [ ] Identificar a utilização actual de `FirstRunService` e do ficheiro `onboarding.json`.
- [ ] Identificar as configurações persistidas actualmente em SQLite.
- [ ] Mapear as dependências HTTP/API ainda utilizadas pelo Desktop.
- [ ] Identificar acessos directos ao SQLite fora da camada Infrastructure.
- [ ] Localizar a implementação efectiva do SDK WeberTechLicensing utilizada pelo KiVenda.
- [ ] Confirmar como o SDK é distribuído e referenciado (project reference, pacote ou outra forma oficial).
- [ ] Confirmar o ProductId oficial do SmartGest e a chave pública correspondente.
- [ ] Identificar componentes antigos de licenciamento RSA no SmartGest/KiVenda que não devem ser reutilizados.
- [ ] Definir uma estratégia de migração para instalações que já tenham dados.

**Critério de conclusão:** existe um mapa dos fluxos actuais, das dependências e dos dados que precisam de ser preservados. Nenhuma chave privada ou mecanismo de assinatura é copiado para o Desktop.

---

## 3. Fase 1 — Configuração `.sg` e registo obrigatório da empresa

**Objectivo:** estabelecer uma configuração local, versionada e recuperável.

### 3.1 Localização e estrutura de dados

Localização proposta no Windows:

```text
%LOCALAPPDATA%/SmartGest/
├── SmartGest.sg
├── smartgest.db
├── Backups/
├── Logs/
└── Licenciamento/
    └── (armazenamento gerido pelo SDK, se aplicável)
```

A localização final deve ser resolvida por um serviço central de caminhos da aplicação. Não espalhar caminhos absolutos pelos ViewModels.

### 3.2 Estrutura inicial sugerida para `SmartGest.sg`

Exemplo ilustrativo de conteúdo JSON:

```json
{
  "schemaVersion": 1,
  "application": {
    "name": "SmartGest",
    "language": "pt-AO",
    "firstRunCompleted": true
  },
  "company": {
    "name": "Nome da Empresa",
    "nif": "",
    "address": "",
    "city": "Luanda",
    "country": "Angola",
    "phone": "",
    "email": "",
    "website": "",
    "logoPath": ""
  },
  "preferences": {
    "theme": "System",
    "currency": "AOA",
    "dateFormat": "dd/MM/yyyy"
  },
  "notifications": {
    "application": true,
    "lowBalance": true,
    "licenseExpiration": true,
    "backup": true
  }
}
```

O exemplo não é um contrato definitivo. Os campos devem ser ajustados aos modelos reais e validados. A moeda padrão deve ser AOA, com apresentação em Kz.

### 3.3 Implementação

- [ ] Criar modelos tipados para a configuração.
- [ ] Criar `IConfigurationService` e implementação em Infrastructure.
- [ ] Implementar leitura e escrita JSON com serialização consistente.
- [ ] Validar `schemaVersion` e campos obrigatórios.
- [ ] Implementar gravação atómica (ficheiro temporário + substituição).
- [ ] Criar cópia de segurança antes de alterações relevantes.
- [ ] Tratar ficheiro ausente, inválido ou corrompido sem apagar dados silenciosamente.
- [ ] Definir migrações de esquema para futuras versões do `.sg`.
- [ ] Garantir que uma actualização da aplicação não substitui as configurações do cliente.
- [ ] Centralizar a localização do ficheiro e evitar duplicação de caminhos.

### 3.4 Primeiro arranque: wizard da empresa

Quando o SmartGest iniciar:

1. Verificar se existe `SmartGest.sg`.
2. Se não existir, iniciar o wizard obrigatório de registo da empresa.
3. Recolher os dados essenciais: nome legal, NIF (quando aplicável), endereço, município/cidade, província, telefone e e-mail.
4. Validar os campos e apresentar uma confirmação antes de gravar.
5. Guardar a configuração em `SmartGest.sg`.
6. Inicializar ou actualizar o SQLite e executar as migrações necessárias.
7. Criar o primeiro utilizador administrador, se ainda não existir.
8. Encaminhar o utilizador para a etapa de ativação da licença.
9. Após a configuração e ativação necessárias, permitir o acesso normal à aplicação.

O fluxo deve permitir recuperar uma configuração interrompida sem criar empresas ou administradores duplicados.

### 3.5 Critérios de aceitação

- [ ] Instalação nova apresenta o wizard e não permite ignorar o registo da empresa.
- [ ] Dados válidos são gravados em `SmartGest.sg`.
- [ ] Reiniciar a aplicação carrega os dados sem pedir novo registo.
- [ ] Ficheiro inválido apresenta recuperação controlada e não apaga o SQLite.
- [ ] Actualização da aplicação preserva o ficheiro e os dados existentes.
- [ ] Alterações de esquema são migradas sem perda de campos suportados.

---

## 4. Fase 2 — Integração WeberTechLicensing

**Objectivo:** implementar no SmartGest o mesmo comportamento funcional de licenciamento do KiVenda, utilizando a biblioteca oficial partilhada.

### 4.1 Regra fundamental

Usar o **SDK WeberTechLicensing real e partilhado**. Não reutilizar implementações antigas ou de referência que tenham um formato criptográfico diferente.

A referência funcional do KiVenda utiliza:

- Assinaturas RSA-3072 com RSA-PSS e SHA-256.
- Ficheiros de licença `.wta`.
- Validação de assinatura, ProductId, MachineId, validade e funcionalidades autorizadas.
- Chave privada apenas no ambiente seguro de emissão/geração de licenças.
- Chave pública no cliente para validar licenças.
- Pedido de ativação em QR com o formato `WTAREQ1:<base64(json)>`.
- Validação local/offline depois de a licença ser emitida e instalada.

**Atenção:** os detalhes acima descrevem o comportamento de referência conhecido do KiVenda. Antes da implementação, confirmar as APIs, versões e contratos directamente no SDK. Não inventar métodos nem implementar criptografia manualmente.

### 4.2 Identidade do produto e máquina

- [ ] Registar/confirmar o ProductId exclusivo `smartgest.desktop_v01`.
- [ ] Confirmar a chave pública oficial associada ao produto.
- [ ] Confirmar o algoritmo e os parâmetros de assinatura exigidos pelo SDK.
- [ ] Utilizar o mecanismo oficial de MachineId do SDK.
- [ ] Confirmar o comportamento em Windows e Linux, caso ambos sejam distribuídos.
- [ ] Garantir que uma licença do KiVenda não é aceite pelo SmartGest.
- [ ] Garantir que uma licença emitida para outra máquina não é aceite.

Não gerar nem armazenar a chave privada no Desktop, no repositório público ou no instalador.

### 4.3 Organização proposta no Desktop

A estrutura exacta deve respeitar a organização já existente, mas a responsabilidade deve ficar centralizada:

```text
SmartGest.Desktop/
└── Licensing/
    ├── WeberTechLicensingService.cs
    ├── LicenseMonitorService.cs
    ├── LicenseNotificationService.cs
    ├── LicensePolicy.cs
    ├── LicenseStatusViewModel.cs
    └── Views/
        └── LicenseSettingsView.axaml
```

Responsabilidades:

- **WeberTechLicensingService:** inicialização do SDK, importação, validação e consulta do estado.
- **LicenseMonitorService:** verificação periódica e transições de estado.
- **LicenseNotificationService:** avisos visuais e notificações ao utilizador.
- **LicensePolicy:** regras centralizadas para validade, tolerância e bloqueio.
- **LicenseStatusViewModel/View:** apresentação do estado e fluxo de ativação/renovação.

Os nomes são propostas; devem ser adaptados às convenções reais do projecto. Não duplicar regras de licença em vários ViewModels.

### 4.4 Fluxo de ativação

1. O SmartGest inicializa o SDK com o ProductId e a configuração pública do produto.
2. O SDK verifica se existe uma licença instalada.
3. Se não existir licença válida, apresentar o ecrã de ativação.
4. Gerar e apresentar o QR de pedido de ativação através do SDK.
5. O cliente envia o pedido ao canal oficial Weber Tech.
6. A licença assinada `.wta` é emitida fora da aplicação.
7. O cliente importa o ficheiro `.wta`.
8. O SDK valida assinatura, produto, máquina, validade e permissões.
9. Se a licença for válida, actualizar o estado e desbloquear as funcionalidades autorizadas.
10. Se a licença for inválida, apresentar uma mensagem específica e manter o acesso limitado ao fluxo de ativação/configuração permitido.

A aplicação não deve exigir ligação à Internet em cada arranque para validar uma licença já instalada, salvo se o contrato oficial do SDK definir explicitamente outra regra.

### 4.5 Avisos, expiração e tolerância

Política funcional a replicar do KiVenda:

| Estado | Comportamento |
|---|---|
| Mais de 14 dias para expirar | Funcionamento normal |
| 14 dias ou menos | Aviso de expiração e indicação dos dias restantes |
| Licença expirada, dentro dos 7 dias de tolerância | Aplicação continua funcional, com aviso persistente |
| Mais de 7 dias após expiração | Bloquear módulos de negócio |
| Licença renovada e válida | Desbloquear imediatamente |

Regras adicionais:

- [ ] Mostrar aviso persistente no shell quando a licença estiver próxima da expiração ou em tolerância.
- [ ] Emitir notificações recorrentes, com intervalo de 3 dias, sem duplicar notificações já apresentadas.
- [ ] Verificar o estado periodicamente e também em momentos críticos: arranque, login, regresso à aplicação e importação de licença.
- [ ] Durante a tolerância, deixar claro que a licença já expirou e informar a data limite.
- [ ] Depois da tolerância, bloquear Dashboard, Caixa, Contabilidade, lançamentos e restantes módulos de negócio.
- [ ] Manter acessível a área de Configurações → Licença para importar uma renovação.
- [ ] Após importar uma licença válida, revalidar e desbloquear sem exigir reinstalação.

A contagem dos sete dias deve ser definida com precisão no código: o período de tolerância termina após sete dias completos desde a expiração. Testar limites de data/hora e fuso horário para evitar um bloqueio antecipado ou tardio.

### 4.6 Segurança e resistência a manipulação

- [ ] Usar exclusivamente validação criptográfica do SDK.
- [ ] Não confiar em estados de licença guardados apenas em JSON ou SQLite.
- [ ] Aplicar as protecções de alteração/retrocesso do relógio disponibilizadas pelo SDK.
- [ ] Não permitir que um erro de leitura seja interpretado como licença válida.
- [ ] Registar eventos relevantes sem expor dados sensíveis.
- [ ] Definir comportamento seguro quando o armazenamento da licença estiver corrompido.
- [ ] Testar reinstalação, cópia de ficheiros, alteração do relógio e restauração de backups.

A protecção local contra manipulação do relógio tem limites: não deve ser apresentada como inviolável. A implementação deve usar os mecanismos do SDK e documentar os riscos residuais.

---

## 5. Fase 3 — Autenticação e utilizadores locais

**Objectivo:** garantir que a autenticação do Desktop não depende da API para as operações locais.

- [ ] Confirmar e consolidar autenticação local.
- [ ] Criar o primeiro administrador durante o onboarding, caso não exista.
- [ ] Usar hashing de passwords adequado (nunca texto simples).
- [ ] Definir papéis e permissões.
- [ ] Implementar bloqueio temporário após tentativas falhadas, se aplicável.
- [ ] Definir expiração e encerramento de sessão.
- [ ] Criar fluxo seguro de recuperação de acesso.
- [ ] Registar eventos de autenticação e alterações críticas.
- [ ] Remover dependências HTTP que sejam exclusivamente necessárias à autenticação local.

O licenciamento e a autenticação são mecanismos diferentes: uma licença válida não substitui o login, e um utilizador autenticado não contorna uma licença inválida.

---

## 6. Fase 4 — Consolidação da arquitectura Desktop

**Objectivo:** tornar o fluxo local consistente e reduzir acoplamento.

Direcção arquitectural:

```text
SmartGest.Desktop
        |
        v
SmartGest.Application
        |
        v
SmartGest.Domain
        ^
        |
SmartGest.Infrastructure
        |
        v
      SQLite
```

A API permanece como aplicação independente para cenários futuros, mas não deve ser requisito para o Desktop executar as operações locais.

- [ ] Fazer os ViewModels consumirem casos de uso/serviços da Application.
- [ ] Mover acessos directos a SQLite/EF Core para Infrastructure.
- [ ] Uniformizar repositórios e Unit of Work onde fizer sentido.
- [ ] Rever `LancamentoService` e `DashboardService` para eliminar acessos directos inconsistentes à base de dados.
- [ ] Rever `AuthService`, `ApiClient` e `TokenStore` para separar autenticação local de integração remota.
- [ ] Remover ou limitar o uso de Service Locator legado.
- [ ] Evitar instanciação directa de ViewModels a partir de outros ViewModels.
- [ ] Centralizar registo de dependências e ciclo de vida dos serviços.
- [ ] Garantir transacções nas operações financeiras críticas.
- [ ] Manter a API isolada dos fluxos que devem funcionar offline.

---

## 7. Fase 5 — Módulos financeiros e consistência funcional

**Objectivo:** validar que os módulos financeiros apresentam dados reais e regras contabilísticas consistentes.

- [ ] Rever Dashboard para usar indicadores reais.
- [ ] Substituir metas artificiais por metas configuráveis ou remover a métrica quando não houver configuração.
- [ ] Validar Caixa e movimentos.
- [ ] Validar Contas Bancárias.
- [ ] Validar categorias e lançamentos.
- [ ] Validar Balancete.
- [ ] Validar Balanço.
- [ ] Validar Demonstração de Resultados (DRE).
- [ ] Garantir consistência entre lançamentos, saldos e relatórios.
- [ ] Rever arredondamentos, precisão decimal e apresentação de valores em AOA/Kz.
- [ ] Validar datas, períodos contabilísticos e operações de fecho.

Os testes devem incluir casos reais e dados de referência, não apenas verificar se os ecrãs abrem.

---

## 8. Fase 6 — Notificações, auditoria e recuperação

- [ ] Centralizar notificações da aplicação.
- [ ] Integrar notificações de expiração da licença.
- [ ] Implementar avisos de backup e integridade.
- [ ] Rever logs para não guardar passwords, tokens ou dados sensíveis desnecessários.
- [ ] Registar operações críticas e alterações de configuração.
- [ ] Garantir backups automáticos e manuais.
- [ ] Garantir que o backup da base de dados é consistente com SQLite/WAL.
- [ ] Definir política de retenção de backups.
- [ ] Testar restauração completa.
- [ ] Definir o que acontece quando `SmartGest.sg` está ausente, inválido ou incompatível com a versão actual.
- [ ] Garantir que recuperação de configuração não substitui nem elimina a base de dados do cliente.

---

## 9. Fase 7 — Instalador e actualizações

- [ ] Rever o instalador Windows x64.
- [ ] Criar/confirmar directórios de dados fora da pasta de instalação.
- [ ] Não incluir qualquer chave privada no instalador.
- [ ] Preservar `SmartGest.sg`, SQLite, backups e licença durante actualizações.
- [ ] Definir comportamento de desinstalação: por padrão, não apagar os dados empresariais sem consentimento explícito.
- [ ] Testar instalação limpa.
- [ ] Testar actualização de uma versão anterior.
- [ ] Testar arranque sem Internet.
- [ ] Testar importação e renovação da licença.
- [ ] Testar recuperação após falha de energia ou encerramento inesperado.

---

## 10. Fase 8 — Testes e critérios de aceitação

### 10.1 Onboarding e configuração

- [ ] Primeira execução abre o registo obrigatório da empresa.
- [ ] Dados da empresa são gravados em `SmartGest.sg`.
- [ ] Reinício carrega a configuração.
- [ ] Configuração corrompida é tratada sem perda silenciosa da base de dados.
- [ ] Actualização preserva configuração e dados.
- [ ] Migração de versão do esquema é testada.

### 10.2 Licenciamento

- [ ] Sem licença, o utilizador só acede ao fluxo permitido de ativação/configuração.
- [ ] QR de ativação é gerado pelo SDK.
- [ ] Licença SmartGest válida e da máquina correcta é aceite.
- [ ] Licença do KiVenda é rejeitada.
- [ ] Licença de outra máquina é rejeitada.
- [ ] Licença adulterada é rejeitada.
- [ ] Licença expirada entra em tolerância durante sete dias completos.
- [ ] Aviso é apresentado a partir dos 14 dias anteriores à expiração.
- [ ] Notificações recorrentes não são duplicadas.
- [ ] Depois da tolerância, os módulos de negócio ficam bloqueados.
- [ ] Configurações → Licença continua acessível após o bloqueio.
- [ ] Renovação válida desbloqueia a aplicação imediatamente.
- [ ] Validação de uma licença instalada funciona sem Internet.

### 10.3 Operação e recuperação

- [ ] Operações locais essenciais funcionam offline.
- [ ] Backups são criados e restaurados.
- [ ] Actualização não perde dados.
- [ ] Desinstalação não elimina dados sem consentimento.
- [ ] Erros de configuração, base de dados e licença têm mensagens claras.
- [ ] Testes automatizados cobrem regras de configuração e política de licenciamento.

---

## 11. Estrutura de projecto proposta

A estrutura final deve adaptar-se à organização actual, evitando criar pastas ou serviços duplicados:

```text
SmartGest.Desktop/
├── Configuration/
│   ├── SmartGestConfiguration.cs
│   ├── ConfigurationService.cs
│   └── ConfigurationMigration.cs
├── Licensing/
│   ├── WeberTechLicensingService.cs
│   ├── LicenseMonitorService.cs
│   ├── LicenseNotificationService.cs
│   ├── LicensePolicy.cs
│   └── Views/
├── Services/
├── ViewModels/
└── Views/

SmartGest.Tests/
├── Configuration/
├── Licensing/
├── Application/
└── Infrastructure/

SmartGest.Doc/
├── ROADMAP.md
└── PLANO_IMPLEMENTACAO_CONFIGURACAO_LICENCIAMENTO.md
```

---

## 12. Ordem recomendada de execução

A ordem abaixo reduz risco e evita misturar alterações de armazenamento, onboarding e licenciamento numa única entrega.

| Ordem | Entrega | Resultado verificável |
|---:|---|---|
| 1 | Auditoria | Fluxos e dependências mapeados |
| 2 | Serviço de configuração | Leitura/escrita segura do `.sg` |
| 3 | Wizard da empresa | Primeiro arranque completo e persistente |
| 4 | Migração dos dados existentes | Instalações actuais preservadas |
| 5 | Integração do SDK | Inicialização e validação reais |
| 6 | Ecrã de ativação | QR e importação de `.wta` |
| 7 | Política de licença | Avisos, sete dias de tolerância e bloqueio |
| 8 | Consolidação local | Desktop sem dependência da API para operações locais |
| 9 | Testes e recuperação | Cenários críticos validados |
| 10 | Distribuição | Instalador e actualização seguros |

### Primeira entrega recomendada

A primeira entrega de código deve limitar-se a:

1. Criar o serviço de configuração `.sg`.
2. Implementar o wizard obrigatório da empresa.
3. Persistir e recarregar a configuração.
4. Inicializar SQLite sem duplicar dados existentes.
5. Garantir que reiniciar a aplicação mantém o estado.

Só depois desta etapa estar estável deve começar a integração do licenciamento.

---

## 13. Riscos e pontos que exigem confirmação

- **SDK:** confirmar a referência exacta, versão e API do WeberTechLicensing antes de escrever código de integração.
- **ProductId:** `smartgest.desktop_v01` é uma proposta e deve ser confirmado no registo oficial de produtos.
- **Chave pública:** obter a chave pública correcta para o ProductId; nunca gerar uma chave privada dentro do cliente.
- **Migração:** instalações existentes podem ter dados de empresa em SQLite ou em `onboarding.json`; migrar sem perda e sem duplicação.
- **Relógio local:** mecanismos locais de detecção de retrocesso do relógio reduzem manipulação, mas não tornam o cliente inviolável.
- **Plataformas:** confirmar se o licenciamento e o cálculo de MachineId serão suportados em todos os sistemas operativos distribuídos.
- **API:** preservar o projecto API, mas impedir que indisponibilidade do servidor bloqueie o Desktop local.
- **Contabilidade:** alterações aos relatórios exigem validação das regras contabilísticas e dos dados de origem.

---

## 14. Regra de conclusão

Uma fase só pode ser marcada como concluída quando:

1. O código está implementado e integrado.
2. A aplicação compila.
3. Os testes relevantes passam.
4. Os cenários de aceitação foram executados.
5. A persistência e a recuperação foram verificadas.
6. A documentação foi actualizada.

**Compilar não significa concluir. Abrir um ecrã não significa validar uma funcionalidade.**
