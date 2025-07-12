# GitActDashNet

Dashboard para GitHub Actions desenvolvido em Blazor Server (.NET 9).

## Configuração do Servidor

A porta e outras configurações do servidor podem ser configuradas via appsettings.json:
{
  "Server": {
    "Port": 3001,
    "UseHttps": false,
    "Host": "localhost"
  }
}
### Configurações Disponíveis:

- **Port**: Porta do servidor (padrão: 5000)
- **UseHttps**: Se deve usar HTTPS (padrão: false)
- **Host**: Host do servidor (padrão: localhost)

### ⚠️ Importante sobre HTTPS Redirection

O sistema agora respeita a configuração `UseHttps`:
- Se `UseHttps: true` → Força redirecionamento para HTTPS
- Se `UseHttps: false` → Não força redirecionamento HTTPS
- Cookies são configurados como `Secure` apenas quando HTTPS está habilitado

## Configuração do GitHub OAuth

Para usar a autenticação com GitHub, você precisa:

### 1. Criar uma GitHub OAuth App

1. Acesse [GitHub Developer Settings](https://github.com/settings/developers)
2. Clique em "New OAuth App"
3. Preencha os campos:
   - **Application name**: GitActDashNet
   - **Homepage URL**: `http://localhost:3001` (ou sua URL configurada)
   - **Authorization callback URL**: `http://localhost:3001/api/auth/callback` (ou sua URL configurada)
4. Clique em "Register application"
5. Copie o **Client ID** e **Client Secret**

### 2. Configurar o appsettings.json

No arquivo `appsettings.json` e `appsettings.Development.json`, configure:
{
  "Server": {
    "Port": 3001,
    "UseHttps": false,
    "Host": "localhost"
  },
  "GitHubOAuth": {
    "ClientId": "seu_client_id_aqui",
    "ClientSecret": "seu_client_secret_aqui"
  }
}
### 3. Executar o projeto
dotnet run
O projeto estará disponível na URL configurada (ex: `http://localhost:3001`)

## Funcionalidades

- ✅ Autenticação com GitHub OAuth
- ✅ Redirecionamento automático baseado no status de autenticação
- ✅ Detecção dinâmica de porta do servidor
- ✅ Configuração de servidor via appsettings.json
- ✅ Controle inteligente de HTTPS redirection
- 🚧 Dashboard (em desenvolvimento)

## Resolução de Problemas

### Erro 404 ou Redirecionamento Incorreto

Se você estiver enfrentando problemas de redirecionamento:

1. **Verifique o appsettings.Development.json**: Certifique-se de que a porta está correta
2. **Verifique UseHttps**: Se usar HTTP, defina `UseHttps: false`
3. **Verifique a GitHub OAuth App**: A callback URL deve corresponder à sua configuração
4. **Limpe o cache**: Às vezes o navegador pode estar fazendo cache de redirecionamentos

### Configuração HTTP vs HTTPS

**Para desenvolvimento com HTTP:**{
  "Server": {
    "Port": 3001,
    "UseHttps": false,
    "Host": "localhost"
  }
}
**Para desenvolvimento com HTTPS:**{
  "Server": {
    "Port": 3001,
    "UseHttps": true,
    "Host": "localhost"
  }
}
## Exemplos de Configuração

### Desenvolvimento HTTP (appsettings.Development.json){
  "Server": {
    "Port": 3001,
    "UseHttps": false,
    "Host": "localhost"
  }
}
### Desenvolvimento HTTPS (appsettings.Development.json){
  "Server": {
    "Port": 3001,
    "UseHttps": true,
    "Host": "localhost"
  }
}
### Produção (appsettings.json){
  "Server": {
    "Port": 80,
    "UseHttps": false,
    "Host": "0.0.0.0"
  }
}
### Produção com HTTPS (appsettings.json){
  "Server": {
    "Port": 443,
    "UseHttps": true,
    "Host": "0.0.0.0"
  }
}