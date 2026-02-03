# PagueVeloz API

## Visão Geral
Desafio Técnico PagueVeloz: Sistema de Processamento de Transações Financeiras

    `OBS: O projeto é didático e por isso possui dados e credenciais sensíveis.`
---

## Decisões Técnicas e Arquiteturais

### Clean Architecture
O projeto segue uma adaptação da **Clean Architecture**, separando responsabilidades:

- **Domain**  
  Contém as entidades (`Account`, `Transaction`, `Event`, `Client`) e regras de negócio.

- **Application**  
  Casos de uso (ex: `Credit`, `Debit`, `Transfer`). Orquestram transações e controlam fluxo.

- **Infrastructure**  
  Implementações concretas: EF Core, SQL Server, RabbitMQ, repositórios, Unit of Work.

- **TransactionProcessor - API**  
  Camada de entrada: controllers e exposição REST.

- **Tests**  
  Camada de testes unitários.

---

### Transações e Consistência
- Todas as operações financeiras são **atômicas**
- Uso explícito de **Unit of Work** para controle de `Begin`, `Commit` e `Rollback`

---

### Concorrência
- Controle de concorrência no nível de banco (SQL Server)
- Estratégia preparada para **lock otimista** (versionamento / rowversion)
- Operações concorrentes na mesma conta não geram inconsistência

---

### Idempotência
Cada transação possui um `ReferenceId`:
- Índice único `(AccountId, ReferenceId)`
- Requisições repetidas não geram efeitos colaterais

---

### Eventos e Mensageria
- Publicação de eventos assíncronos via **RabbitMQ**

## Frameworks e Bibliotecas

### .NET 9
- Versão exigida no documento

### Entity Framework Core
- ORM robusto
- Integração direta com SQL Server

### SQL Server
- Amplamente usado com .NET

### RabbitMQ
- Comunicação assíncrona
- Garantia de entrega

### xUnit
- Padrão no .NET

---

## Infraestrutura com Docker

O projeto utiliza **Docker Compose** para subir todos os serviços necessários:
- API
- SQL Server
- RabbitMQ

### Subir o ambiente
```bash
docker compose up -d --build
```

### Parar o ambiente
```bash
docker compose down
```

---

## Compilação e Execução

### Executar via Docker
```bash
docker compose up -d --build
```

A API ficará disponível em:
```
http://localhost:5000
```

Swagger:
```
http://localhost:5000/swagger
```

---

## Execução dos Testes

### Testes Unitários
É necessário ter o .NET na máquina, dado que o container possui apenas o runtime do .NET
```bash
dotnet test
```

## Exemplos de Uso da API

### Criar Conta
**POST** `/accounts`
```json
{
  "clientId": "CLI-008",
  "creditLimit": 50000,
  "availableBalance": 0
}
```

```json
curl -X 'POST' \
  'https://localhost:5000/api/Accounts' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "clientId": "CLI-008",
  "creditLimit": 50000,
  "availableBalance": 0
}'
```

Resposta:
```json
{
  "data": {
    "accountId": "ACC-029",
    "clientId": "CLI-008"
  },
  "message": "Conta criado com sucesso"
}
```

### Criar Crédito
**POST** `/transactions`
```json
{
  "account_id": "ACC-001",
  "operation": "credit",
  "amount": 10000,
  "currency": "BRL",
  "reference_id": "TXN-001",
  "metadata": {
    "description": "Depósito inicial"
  }
}
```

```json
curl -X 'POST' \
  'https://localhost:5000/api/Transactions' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "operation": "credit",
  "account_id": "ACC-001",
  "amount": 10000,
  "currency": "BRL",
  "reference_id": "TXN-001",
  "metadata": {
    "description": "Depósito inicial"
  }
}'
```

Resposta:
```json
{
  "transaction_id": "TXN-001-PROCESSED",
  "status": "success",
  "balance": 1000,
  "reserved_balance": 0,
  "available_balance": 1000,
  "timestamp": "2026-02-02T12:00:00Z",
  "error_message": null
}
```

---

### Débito
```json
{
  "account_id": "ACC-001",
  "operation": "debit",
  "amount": 2000,
  "currency": "BRL",
  "reference_id": "TXN-002",
  "metadata": {
    "description": "Débito"
  }
}
```

```json
curl -X 'POST' \
  'https://localhost:5000/api/Transactions' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "operation": "debit",
  "account_id": "ACC-001",
  "amount": 3000,
  "currency": "BRL",
  "reference_id": "TXN-002",
  "metadata": {
    "description": "Débito"
  }
}'
```

---

### Reserva
```json
{
  "account_id": 1,
  "operation": "reserve",
  "amount": 300,
  "currency": "BRL",
  "reference_id": "TXN-003"
}
```
```json
curl -X 'POST' \
  'https://localhost:5000/api/Transactions' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "operation": "reserve",
  "account_id": "ACC-001",
  "amount": 1500,
  "currency": "BRL",
  "reference_id": "TXN-003",
  "metadata": {
    "description": "Reservar saldo"
  }
}'
```

---

### Transferência
```json
{
  "operation": "transfer",
  "Account_id": "ACC-001",
  "Destination_account_id": "ACC-002",
  "amount": 150,
  "currency": "BRL",
  "reference_id": "TXN-004"
}
```

```json
curl -X 'POST' \
  'https://localhost:5000/api/Transactions' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "operation": "transfer",
  "Account_id": "ACC-018",
  "Destination_account_id": "ACC-019",
  "amount": 50,
  "currency": "BRL",
  "reference_id": "TXN-005",
  "metadata": {
    "description": "Transferência entre contas"
  }
}'
```

### Captura
```json
{
  "operation": "capture",
  "account_id": "ACC-001",
  "amount": 150,
  "currency": "BRL",
  "reference_id": "TXN-005"
}
```

```json
curl -X 'POST' \
  'https://localhost:5000/api/Transactions' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "operation": "capture",
  "account_id": "ACC-029",
  "amount": 1500,
  "currency": "BRL",
  "reference_id": "TXN-005",
  "metadata": {
    "description": "Capturar saldo"
  }
}'
```

### Reverter transação
```json
{
  "operation": "reversal",
  "reference_id": "TXN-002",
  "metadata": {
    "description": "Reverter transação"
  }
}

```

```json
curl -X 'POST' \
  'https://localhost:5000/api/Transactions' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "operation": "reversal",
  "reference_id": "TXN-002-R",
  "metadata": {
    "description": "Reverter transação"
  }
}'
```
---